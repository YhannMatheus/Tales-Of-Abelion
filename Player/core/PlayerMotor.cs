using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [Header("Components")]
    private CharacterController controller;

    [Header("Movement")]
    private Vector3 playerVelocity;
    private float gravity = -9.81f;

    [Header("Pathfinding/Destination Settings")]
    [SerializeField] private float destinationReachedThreshold = 0.5f; // Distância para considerar que chegou
    private Vector3? targetDestination = null; // Posição de destino alvo
    private Transform targetTransform = null; // Transform de alvo móvel (inimigo)

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    public bool IsMoving { get; private set; }
    public bool IsGrounded { get; private set; }
    public float CurrentSpeedNormalized { get; private set; }
    
    // Propriedades para pathfinding
    public bool HasDestination => targetDestination.HasValue || targetTransform != null;
    public Vector3? CurrentDestination => targetTransform != null ? targetTransform.position : targetDestination;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    // Movimento direto com input (WASD)
    public void Move(Vector3 direction, float speed)
    {
        controller.Move(direction.normalized * Time.deltaTime * speed);

        IsMoving = direction.magnitude > 0.1f;
        
        if (IsMoving)
        {
            float currentSpeed = controller.velocity.magnitude;
            CurrentSpeedNormalized = Mathf.Clamp01(currentSpeed / speed);
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
            transform.forward = direction;
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
        // Ground check
        IsGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (IsGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        // Aplica gravidade
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
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