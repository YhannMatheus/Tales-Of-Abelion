using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    [Header("Components")]
    private CharacterController controller;
    private CharacterManager CharacterManager;

    [Header("Movement")]
    private Vector3 playerVelocity;
    private float gravity = -9.81f;

    // Configurações injetadas pelo PlayerManager
    private float rotationSpeed = 720f;
    private bool useSmoothRotation = true;
    private float slowThreshold = 0.7f;
    private float buffedThreshold = 1.3f;
    private float baseSpeed;
    
    // Pathfinding
    private float destinationReachedThreshold = 0.5f;
    private Vector3? targetDestination = null;
    private Transform targetTransform = null;
    
    // Ground Check
    private Transform groundCheck;
    private GroundCheckRaycast groundCheckRaycast; // ✅ NOVO: Componente de Raycast
    private float groundDistance = 0.4f;
    private LayerMask groundMask;

    public bool IsMoving { get; private set; }
    public bool IsGrounded { get; private set; }
    public float CurrentSpeedNormalized { get; private set; }
    public float SpeedMultiplier { get; private set; }
    
    public bool HasDestination => targetDestination.HasValue || targetTransform != null;
    public Vector3? CurrentDestination => targetTransform != null ? targetTransform.position : targetDestination;

    // Inicializado pelo PlayerManager - componentes são injetados
    public void Initialize(CharacterController charController, CharacterManager charComponent)
    {
        controller = charController;
        CharacterManager = charComponent;
        
        // Cacheia velocidade base
        if (CharacterManager != null)
        {
            baseSpeed = CharacterManager.Data.speed;
        }
    }
    
    // Configurações injetadas pelo PlayerManager
    public void ConfigureSettings(float rotSpeed, bool smoothRot, float slowThr, float buffedThr,
                                   float destThreshold, Transform groundChk, float groundDist, LayerMask groundMsk)
    {
        rotationSpeed = rotSpeed;
        useSmoothRotation = smoothRot;
        slowThreshold = slowThr;
        buffedThreshold = buffedThr;
        destinationReachedThreshold = destThreshold;
        groundCheck = groundChk;
        groundDistance = groundDist;
        groundMask = groundMsk;
        
        // ✅ Tenta obter o componente GroundCheckRaycast (se existir)
        groundCheckRaycast = GetComponent<GroundCheckRaycast>();
        if (groundCheckRaycast != null)
        {
            Debug.Log("[PlayerMotor] ✅ GroundCheckRaycast component encontrado e configurado!");
        }
    }

    // Movimento direto com input (WASD)
    public void Move(Vector3 direction, float speed)
    {
        // Usa TotalSpeed do CharacterManager se disponível (considera buffs/debuffs)
        float effectiveSpeed = CharacterManager != null ? CharacterManager.Data.TotalSpeed : speed;
        
        controller.Move(direction.normalized * Time.deltaTime * effectiveSpeed);

        IsMoving = direction.magnitude > 0.1f;
        
        // Calcula multiplicador de velocidade
        CalculateSpeedMultiplier(effectiveSpeed);
        
        if (IsMoving)
        {
            float currentSpeed = controller.velocity.magnitude;
            CurrentSpeedNormalized = Mathf.Clamp01(currentSpeed / effectiveSpeed);
        }
        else
        {
            CurrentSpeedNormalized = 0f;
        }
    }

    // Movimento até destino fixo
    public void MoveToDestination(Vector3 destination, float speed)
    {
        targetDestination = destination;
        targetTransform = null;

        Vector3 direction = GetDirectionToDestination();
        
        if (direction != Vector3.zero)
        {
            Move(direction, speed);
        }
    }

    // Movimento seguindo um Transform (inimigo móvel)
    public void MoveToTarget(Transform target, float speed)
    {
        if (target == null)
        {
            ClearDestination();
            return;
        }

        targetTransform = target;
        targetDestination = null;

        Vector3 direction = GetDirectionToDestination();
        
        if (direction != Vector3.zero)
        {
            Move(direction, speed);
        }
    }
    
    /// <summary>
    /// Calcula o multiplicador de velocidade baseado em buffs/debuffs
    /// </summary>
    private void CalculateSpeedMultiplier(float currentSpeed)
    {
        if (CharacterManager == null || baseSpeed <= 0f)
        {
            SpeedMultiplier = 1f;
            return;
        }

        SpeedMultiplier = currentSpeed / baseSpeed;
    }
    
    /// <summary>
    /// Recalcula a velocidade base. Use quando equipamentos ou nível mudarem.
    /// </summary>
    public void RecalculateBaseSpeed()
    {
        if (CharacterManager == null) return;
        baseSpeed = CharacterManager.Data.speed;
    }
    
    /// <summary>
    /// Retorna o tipo de velocidade atual (Slow/Normal/Buffed)
    /// </summary>
    public string GetSpeedType()
    {
        if (SpeedMultiplier < slowThreshold)
            return "SLOW";
        else if (SpeedMultiplier > buffedThreshold)
            return "BUFFED";
        else
            return "NORMAL";
    }

    // Verifica se chegou ao destino
    public bool HasReachedDestination()
    {
        if (!HasDestination) return true;

        Vector3 destination = CurrentDestination.Value;
        float distance = Vector3.Distance(transform.position, destination);
        
        return distance <= destinationReachedThreshold;
    }

    // Retorna distância até o destino atual
    public float GetDistanceToDestination()
    {
        if (!HasDestination) return 0f;
        return Vector3.Distance(transform.position, CurrentDestination.Value);
    }

    // Limpa destino (para de seguir)
    public void ClearDestination()
    {
        targetDestination = null;
        targetTransform = null;
    }

    // Retorna direção normalizada para o destino
    private Vector3 GetDirectionToDestination()
    {
        if (!HasDestination) return Vector3.zero;

        Vector3 destination = CurrentDestination.Value;
        Vector3 direction = (destination - transform.position);
        direction.y = 0; // Ignora diferença de altura

        return direction.normalized;
    }

    public void Rotate(Vector3 direction)
    {
        if (direction.magnitude > 0.1f)
        {
            if (useSmoothRotation)
            {
                // Rotação suave usando RotateTowards
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
            }
            else
            {
                // Rotação instantânea (comportamento antigo)
                transform.forward = direction;
            }
        }
    }

    public void RotateToPosition(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Mantém rotação apenas no plano horizontal
        
        Rotate(direction);
    }

    public void ApplyGravity()
    {
        bool wasGrounded = IsGrounded;
        
        // ✅ PRIORIDADE 1: Usa GroundCheckRaycast se disponível (MAIS PRECISO)
        if (groundCheckRaycast != null)
        {
            IsGrounded = groundCheckRaycast.IsGrounded;
            
            Debug.Log($"[PlayerMotor] 🎯 RAYCAST CHECK | IsGrounded = {IsGrounded} | WasGrounded = {wasGrounded} | Y Pos = {transform.position.y:F2}");
            
            if (wasGrounded != IsGrounded)
            {
                Debug.Log($"[PlayerMotor] 🚨 MUDANÇA DETECTADA (Raycast) | {wasGrounded} → {IsGrounded}");
            }
        }
        // PRIORIDADE 2: Usa Transform externo com CheckSphere
        else if (groundCheck != null)
        {
            IsGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            
            if (wasGrounded != IsGrounded)
            {
                Debug.Log($"[PlayerMotor] 🌍 GROUND CHECK (External Sphere) | IsGrounded = {IsGrounded} | CheckPos = {groundCheck.position} | Distance = {groundDistance}");
            }
        }
        // PRIORIDADE 3: Fallback - usa SphereCast privado
        else
        {
            // Fallback: usa SphereCast do próprio CharacterController
            IsGrounded = CheckGroundPrivate();
            
            if (wasGrounded != IsGrounded)
            {
                Debug.Log($"[PlayerMotor] 🔍 GROUND CHECK (Private Sphere) | IsGrounded = {IsGrounded}");
            }
        }

        if (IsGrounded && playerVelocity.y < 0)
        {
            // ✅ Quando no chão, aplica pequena força para baixo (mantém contato)
            // -2f é o padrão Unity para CharacterController grounded
            playerVelocity.y = -2f;
            
            if (wasGrounded != IsGrounded)
            {
                Debug.Log($"[PlayerMotor] ✅ ATERRISSOU | Y Velocity agora = -2f (grounded) | Y Pos = {transform.position.y:F2}");
            }
        }
        else
        {
            // ⬇️ APENAS aplica gravidade quando NO AR
            playerVelocity.y += gravity * Time.deltaTime;
        }
        
        // ✅ SEMPRE move (mesmo que seja Vector3.zero) para manter física ativa
        // Isso garante que CharacterController atualize colisões e IsGrounded funcione
        controller.Move(playerVelocity * Time.deltaTime);
        
        // Log detalhado de gravidade (REMOVIDO - muito spam)
        // if (Mathf.Abs(playerVelocity.y) > 0.5f)
        // {
        //     Debug.Log($"[PlayerMotor] ⬇️ GRAVITY | Y Velocity = {playerVelocity.y:F2} | IsGrounded = {IsGrounded} | Y Pos = {transform.position.y:F2}");
        // }
    }
    
    /// <summary>
    /// Ground check privado quando groundCheck Transform não está atribuído.
    /// Usa SphereCast a partir do CharacterController para detectar chão.
    /// </summary>
    private bool CheckGroundPrivate()
    {
        if (controller == null) return false;
        
        // Origem: ligeiramente acima do centro do controller
        Vector3 origin = transform.position + Vector3.up * (controller.radius + 0.05f);
        float rayDistance = controller.height / 2f + groundDistance;
        
        // SphereCast para baixo
        bool hitGround = Physics.SphereCast(origin, controller.radius * 0.9f, Vector3.down, 
                                           out RaycastHit hit, rayDistance, groundMask);
        
        Debug.Log($"[PlayerMotor] 🔍 CHECK GROUND PRIVATE | Origin = {origin} | RayDist = {rayDistance:F2} | HitGround = {hitGround} | LayerMask = {groundMask.value}");
        
        if (hitGround)
        {
            Debug.Log($"[PlayerMotor] ✅ PRIVATE CHECK HIT | Distance = {hit.distance:F2} | Point = {hit.point} | Normal = {hit.normal}");
        }
        
        return hitGround;
    }

    public void Stop()
    {
        IsMoving = false;
        CurrentSpeedNormalized = 0f;
        ClearDestination();
    }

    // ========== Debug Gizmos ==========
    private void OnDrawGizmosSelected()
    {
        if (!HasDestination) return;

        // Desenha linha até destino
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, CurrentDestination.Value);

        // Desenha esfera no destino
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(CurrentDestination.Value, destinationReachedThreshold);
    }
}