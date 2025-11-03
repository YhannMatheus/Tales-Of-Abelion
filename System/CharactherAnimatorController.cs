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
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int LevelUpHash = Animator.StringToHash("LevelUp");
    private static readonly int AbilityIndexHash = Animator.StringToHash("AbilityIndex");
    private static readonly int AbilityActiveHash = Animator.StringToHash("AbilityActive");

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

    public void UpdateMovementSpeed(float targetSpeed, bool grounded = true)
    {

        if (animator == null) return;
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

    public void TriggerHit()
    {

        if (animator == null || !character.Data.IsAlive) return;
        animator.SetTrigger(HitHash);
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

    /// <summary>
    /// Reseta todos os triggers pendentes (evita animações tocando em momentos errados)
    /// </summary>
    public void ResetAllTriggers()
    {
        if (animator == null) return;
        
        animator.ResetTrigger(AttackHash);
        animator.ResetTrigger(HitHash);
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