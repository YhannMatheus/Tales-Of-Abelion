using System.Linq;
using UnityEngine;

public class CharacterAnimatorController : MonoBehaviour
{
    [Header("References")]
    private Animator animator;
    private Character character;
    
    [Header("Movement")]
    [SerializeField] private float movementSmoothTime = 8f;
    
    [Header("Animation Parameters")]
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsDeathHash = Animator.StringToHash("isDeath");
    private static readonly int TakeDamageHash = Animator.StringToHash("TakeDamage");
    private static readonly int IsStunnedHash = Animator.StringToHash("isStunned");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int LevelUpHash = Animator.StringToHash("LevelUp");
    private static readonly int AbilityIndexHash = Animator.StringToHash("AbilityIndex");
    private static readonly int AbilityActiveHash = Animator.StringToHash("AbilityActive");
    
    // Estados do Player (para controlar animações por estado)
    private static readonly int IsIdleHash = Animator.StringToHash("isIdle");
    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    private static readonly int IsAttackingHash = Animator.StringToHash("isAttacking");
    private static readonly int IsCastingHash = Animator.StringToHash("isCasting");

    private float currentSpeed = 0f;
    private bool isGrounded = true;
    
    // Timeout para auto-reset de AbilityActive (segurança contra travamentos)
    private float abilityActiveTimer = 0f;
    private const float ABILITY_TIMEOUT = 5f;
    
    [Header("Debug")]
    [SerializeField] private bool debugAnimator = false;
    private bool hasSpeedParameter = true;

    private void Awake()
    {

        animator = GetComponentInChildren<Animator>();
        character = GetComponent<Character>();

        if (animator == null)
        {
            Debug.LogWarning($"[CharacterAnimatorController] No Animator found on {gameObject.name}");
            enabled = false;
            return;
        }

        if (character == null)
        {
            Debug.LogWarning($"[CharacterAnimatorController] No Character component found on {gameObject.name}");
            enabled = false;
            return;
        }

        // Verifica se o Animator possui o parâmetro "Speed" — se não, avisa para ajudar debugging
        var paramNames = animator.parameters.Select(p => p.name).ToArray();
        hasSpeedParameter = paramNames.Any(n => n == "Speed");
        if (!hasSpeedParameter)
        {
            Debug.LogWarning($"[CharacterAnimatorController] Animator on {gameObject.name} does not contain parameter 'Speed'. Available: {string.Join(", ", paramNames)}");
        }
    }

    private void OnEnable()
    {

        if (character != null)
        {
            character.OnDeath += HandleDeath;
            character.OnRevive += HandleRevive;
            character.OnLevelUp += HandleLevelUp;
        }
    }

    private void OnDisable()
    {

        if (character != null)
        {
            character.OnDeath -= HandleDeath;
            character.OnRevive -= HandleRevive;
            character.OnLevelUp -= HandleLevelUp;
        }
    }

    private void Update()
    {
        // Auto-reset de AbilityActive se passar do timeout (segurança contra travamentos)
        if (animator != null && animator.GetBool(AbilityActiveHash))
        {
            abilityActiveTimer += Time.deltaTime;
            if (abilityActiveTimer > ABILITY_TIMEOUT)
            {
                Debug.LogWarning($"[CharacterAnimatorController] AbilityActive timeout após {ABILITY_TIMEOUT}s! Forçando reset em {gameObject.name}");
                EndAbility();
            }
        }
        else
        {
            abilityActiveTimer = 0f;
        }
    }

    /// <summary>
    /// Atualiza velocidade de movimento no animator
    /// IMPORTANTE: Só funciona quando isMoving=true (MovingState controla)
    /// Idle não usa Speed - é estado puro
    /// </summary>
    public void UpdateMovementSpeed(float targetSpeed, bool grounded = true)
    {
        if (animator == null) return;
        
        // CRÍTICO: Só atualiza Speed se estiver em MOVING state
        // Idle é estado puro e não deve receber updates de Speed
        if (!animator.GetBool(IsMovingHash))
        {
            if (debugAnimator)
            {
                Debug.Log("[CharacterAnimatorController] UpdateMovementSpeed ignorado - não está em MovingState");
            }
            return;
        }
        
        if (isGrounded != grounded)
        {
            isGrounded = grounded;
            animator.SetBool(IsGroundedHash, isGrounded);
        }

        float newSpeed;
        if (isGrounded)
        {
            newSpeed = Mathf.Lerp(currentSpeed, targetSpeed, movementSmoothTime * Time.deltaTime);
        }
        else
        {
            newSpeed = Mathf.Lerp(currentSpeed, 0f, movementSmoothTime * Time.deltaTime);
        }

        if (debugAnimator)
        {
            Debug.Log($"[CharacterAnimatorController] UpdateMovementSpeed: target={targetSpeed:F2}, grounded={grounded}, current(before)={currentSpeed:F2}, new={newSpeed:F2}");
        }

        currentSpeed = newSpeed;

        if (hasSpeedParameter)
        {
            animator.SetFloat(SpeedHash, currentSpeed);
        }
    }

    public void TriggerAttack()
    {
        if (animator == null || !character.Data.IsAlive) return;
        
        // Valida se não está em outra ação
        if (animator.GetBool(AbilityActiveHash))
        {
            if (debugAnimator)
            {
                Debug.LogWarning("[CharacterAnimatorController] Cannot attack: ability is active");
            }
            return;
        }
        
        // Zera velocidade para evitar deslizamento durante ataque
        currentSpeed = 0f;
        animator.SetFloat(SpeedHash, 0f);
        
        animator.SetTrigger(AttackHash);
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] Attack triggered, speed reset to 0");
        }
    }

    public void TriggerAbility(int abilityIndex)
    {
        if (animator == null || !character.Data.IsAlive) return;
        
        // Valida se não está em outra habilidade
        if (animator.GetBool(AbilityActiveHash))
        {
            if (debugAnimator)
            {
                Debug.LogWarning($"[CharacterAnimatorController] Cannot use ability {abilityIndex}: another ability is active");
            }
            return;
        }
        
        // Zera velocidade para evitar deslizamento durante habilidade
        currentSpeed = 0f;
        animator.SetFloat(SpeedHash, 0f);
        
        animator.SetInteger(AbilityIndexHash, abilityIndex);
        animator.SetBool(AbilityActiveHash, true);
        abilityActiveTimer = 0f; // Reset do timer de timeout
        
        if (debugAnimator)
        {
            Debug.Log($"[CharacterAnimatorController] Ability {abilityIndex} triggered, speed reset to 0");
        }
    }

    /// <summary>
    /// Dispara animação de receber dano (diferente de stun)
    /// </summary>
    public void TriggerTakeDamage()
    {
        if (animator == null || !character.Data.IsAlive) return;
        animator.SetTrigger(TakeDamageHash);
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] Take damage animation triggered");
        }
    }

    /// <summary>
    /// Define estado de stun (bool, não trigger)
    /// </summary>
    public void SetStunned(bool stunned)
    {
        if (animator == null) return;
        animator.SetBool(IsStunnedHash, stunned);
        
        if (debugAnimator)
        {
            Debug.Log($"[CharacterAnimatorController] Stunned state set to {stunned}");
        }
    }

    public void EndAbility()
    {
        if (animator == null) return;
        animator.SetBool(AbilityActiveHash, false);
        abilityActiveTimer = 0f;
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] Ability ended, AbilityActive reset to false");
        }
    }

    // ========== Controle de Estados ==========

    /// <summary>
    /// Ativa estado Idle (desativa outros estados)
    /// IMPORTANTE: Idle é estado PURO - não controla Speed, apenas para
    /// </summary>
    public void SetIdleState()
    {
        if (animator == null) return;
        
        animator.SetBool(IsIdleHash, true);
        animator.SetBool(IsMovingHash, false);
        animator.SetBool(IsAttackingHash, false);
        animator.SetBool(IsCastingHash, false);
        
        // NÃO zera Speed aqui - Idle é estado puro sem controle de velocidade
        // MovingState é responsável por controlar Speed
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] State set to IDLE (estado puro, sem Speed control)");
        }
    }

    /// <summary>
    /// Ativa estado Moving (desativa outros estados)
    /// </summary>
    public void SetMovingState()
    {
        if (animator == null) return;
        
        animator.SetBool(IsIdleHash, false);
        animator.SetBool(IsMovingHash, true);
        animator.SetBool(IsAttackingHash, false);
        animator.SetBool(IsCastingHash, false);
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] State set to MOVING");
        }
    }

    /// <summary>
    /// Ativa estado Attacking (desativa outros estados, zera velocidade)
    /// </summary>
    public void SetAttackingState()
    {
        if (animator == null) return;
        
        animator.SetBool(IsIdleHash, false);
        animator.SetBool(IsMovingHash, false);
        animator.SetBool(IsAttackingHash, true);
        animator.SetBool(IsCastingHash, false);
        
        // Zera velocidade para evitar deslizamento
        currentSpeed = 0f;
        animator.SetFloat(SpeedHash, 0f);
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] State set to ATTACKING, speed forced to 0");
        }
    }

    /// <summary>
    /// Ativa estado Casting (desativa outros estados, zera velocidade)
    /// </summary>
    public void SetCastingState()
    {
        if (animator == null) return;
        
        animator.SetBool(IsIdleHash, false);
        animator.SetBool(IsMovingHash, false);
        animator.SetBool(IsAttackingHash, false);
        animator.SetBool(IsCastingHash, true);
        
        // Zera velocidade para evitar deslizamento
        currentSpeed = 0f;
        animator.SetFloat(SpeedHash, 0f);
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] State set to CASTING, speed forced to 0");
        }
    }

    // ========== Métodos Utilitários ==========

    /// <summary>
    /// Reseta todos os triggers pendentes (evita animações tocando em momentos errados)
    /// </summary>
    public void ResetAllTriggers()
    {
        if (animator == null) return;
        
        animator.ResetTrigger(AttackHash);
        animator.ResetTrigger(TakeDamageHash);
        animator.ResetTrigger(LevelUpHash);
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] Todos os triggers resetados");
        }
    }

    /// <summary>
    /// Reseta completamente o animator para estado inicial limpo
    /// Útil após morte, revive, teleporte ou bugs
    /// </summary>
    public void ResetAnimatorState()
    {
        if (animator == null) return;
        
        // Reseta bools
        animator.SetBool(IsDeathHash, false);
        animator.SetBool(AbilityActiveHash, false);
        animator.SetBool(IsGroundedHash, true);
        
        // Reseta floats
        currentSpeed = 0f;
        animator.SetFloat(SpeedHash, 0f);
        
        // Reseta ints
        animator.SetInteger(AbilityIndexHash, -1);
        
        // Reseta triggers pendentes
        ResetAllTriggers();
        
        // Reseta timers
        abilityActiveTimer = 0f;
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] Animator state completamente resetado");
        }
    }

    private void HandleDeath()
    {
        if (animator == null) return;
        
        // Reseta todos os triggers pendentes para evitar animações estranhas
        ResetAllTriggers();
        
        // Para todas as ações
        animator.SetBool(AbilityActiveHash, false);
        animator.SetBool(IsDeathHash, true);
        
        // Zera velocidade
        currentSpeed = 0f;
        animator.SetFloat(SpeedHash, 0f);
        
        // Reseta timers
        abilityActiveTimer = 0f;
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] HandleDeath: animator resetado para estado de morte");
        }
    }

    private void HandleRevive()
    {
        if (animator == null) return;
        
        // Reseta completamente o animator para estado limpo
        ResetAnimatorState();
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] HandleRevive: animator completamente resetado");
        }
    }

    private void HandleLevelUp(int newLevel)
    {
        if (animator == null) return;
        animator.SetTrigger(LevelUpHash);
    }
}