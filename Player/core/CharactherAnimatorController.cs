using System.Collections;
using System.Linq;
using UnityEngine;

// Controlador de animações - gerenciado pelo PlayerManager/IAManager
// NÃO usar independentemente - requer inicialização via Initialize()
public class CharacterAnimatorController : MonoBehaviour
{
    [Header("References")]
    private Animator animator;
    private CharacterManager CharacterManager;
    
    // Configurações injetadas pelo PlayerManager/IAManager
    private float movementSmoothTime = 8f;
    private float slowThreshold = 0.7f;
    private float buffedThreshold = 1.3f;
    private bool debugAnimator = false;
    
    [Header("Animation Parameters")]
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int SpeedMultiplierHash = Animator.StringToHash("SpeedMultiplier");
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
    private static readonly int IsFallingHash = Animator.StringToHash("isFalling");

    private float currentSpeed = 0f;
    private bool isGrounded = true;
    
    // Timeout para auto-reset de AbilityActive (segurança contra travamentos)
    private float abilityActiveTimer = 0f;
    private const float ABILITY_TIMEOUT = 5f;
    
    // Flags para verificar existência de parâmetros no Animator
    private bool hasSpeedParameter = true;
    private bool hasSpeedMultiplierParameter = true;
    private bool hasAbilityActiveParameter = true;
    private bool hasIsMovingParameter = true;
    private bool hasIsFallingParameter = true;
    
    // Cache da velocidade base do personagem (para calcular multiplicador)
    private float baseSpeed = 0f;

    // Inicializado pelo PlayerManager ou IAManager - componentes são injetados
    public void Initialize(Animator anim, CharacterManager charComponent)
    {
        animator = anim;
        CharacterManager = charComponent;

        if (animator == null)
        {
            Debug.LogWarning($"[CharacterAnimatorController] No Animator found on {gameObject.name}");
            enabled = false;
            return;
        }

        if (CharacterManager == null)
        {
            Debug.LogWarning($"[CharacterAnimatorController] No CharacterManager component found on {gameObject.name}");
            enabled = false;
            return;
        }

        // Verifica quais parâmetros existem no Animator
        var paramNames = animator.parameters.Select(p => p.name).ToArray();
        hasSpeedParameter = paramNames.Any(n => n == "Speed");
        hasSpeedMultiplierParameter = paramNames.Any(n => n == "SpeedMultiplier");
        hasAbilityActiveParameter = paramNames.Any(n => n == "AbilityActive");
        hasIsMovingParameter = paramNames.Any(n => n == "isMoving");
        hasIsFallingParameter = paramNames.Any(n => n == "isFalling");
        
        if (!hasSpeedParameter)
        {
            Debug.LogWarning($"[CharacterAnimatorController] Animator on {gameObject.name} does not contain parameter 'Speed'. Available: {string.Join(", ", paramNames)}");
        }
        if (!hasSpeedMultiplierParameter && debugAnimator)
        {
            Debug.Log($"[CharacterAnimatorController] Animator on {gameObject.name} does not contain parameter 'SpeedMultiplier' (opcional)");
        }
        if (!hasAbilityActiveParameter && debugAnimator)
        {
            Debug.Log($"[CharacterAnimatorController] Animator on {gameObject.name} does not contain parameter 'AbilityActive' (normal para NPCs/inimigos)");
        }
        if (!hasIsMovingParameter && debugAnimator)
        {
            Debug.Log($"[CharacterAnimatorController] Animator on {gameObject.name} does not contain parameter 'isMoving' (opcional)");
        }
        if (!hasIsFallingParameter && debugAnimator)
        {
            Debug.Log($"[CharacterAnimatorController] Animator on {gameObject.name} does not contain parameter 'isFalling' (opcional)");
        }
        
        // Cacheia velocidade base
        if (CharacterManager != null && CharacterManager.Data != null)
        {
            baseSpeed = CharacterManager.Data.speed;
            if (debugAnimator)
            {
                Debug.Log($"[CharacterAnimatorController] Base speed capturada: {baseSpeed}");
            }
        }
    }
    
    // Configurações injetadas pelo PlayerManager/IAManager
    public void ConfigureSettings(float smoothTime, float slowThr, float buffedThr, bool debug)
    {
        movementSmoothTime = smoothTime;
        slowThreshold = slowThr;
        buffedThreshold = buffedThr;
        debugAnimator = debug;
    }

    // Inscreve em eventos do CharacterManager - chamado após Initialize()
    public void SubscribeToCharacterEvents()
    {
        if (CharacterManager != null)
        {
            CharacterManager.OnDeath += HandleDeath;
            CharacterManager.OnRevive += HandleRevive;
            CharacterManager.OnLevelUp += HandleLevelUp;
        }
    }

    // Desinscreve de eventos do CharacterManager
    public void UnsubscribeFromCharacterEvents()
    {
        if (CharacterManager != null)
        {
            CharacterManager.OnDeath -= HandleDeath;
            CharacterManager.OnRevive -= HandleRevive;
            CharacterManager.OnLevelUp -= HandleLevelUp;
        }
    }

    private void Update()
    {
        // Auto-reset de AbilityActive se passar do timeout (segurança contra travamentos)
        // Apenas verifica se o parâmetro existe no Animator
        if (animator != null && hasAbilityActiveParameter && animator.GetBool(AbilityActiveHash))
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
        
        // Verifica se o parâmetro isMoving existe antes de usá-lo
        if (hasIsMovingParameter && !animator.GetBool(IsMovingHash))
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

        currentSpeed = newSpeed;

        // Atualiza parâmetro Speed (normalizado 0-1)
        if (hasSpeedParameter)
        {
            animator.SetFloat(SpeedHash, currentSpeed);
        }
        
        // Calcula e atualiza SpeedMultiplier para diferentes animações de corrida
        if (hasSpeedMultiplierParameter && baseSpeed > 0f)
        {
            // Pega a velocidade total atual do personagem (pode ser afetada por buffs/debuffs)
            float currentTotalSpeed = CharacterManager.Data.TotalSpeed;
            
            // Calcula multiplicador (1.0 = velocidade normal, <1.0 = slow, >1.0 = buffed)
            float speedMultiplier = currentTotalSpeed / baseSpeed;
            
            animator.SetFloat(SpeedMultiplierHash, speedMultiplier);
            
            if (debugAnimator)
            {
                string speedType = "NORMAL";
                if (speedMultiplier < slowThreshold) speedType = "SLOW";
                else if (speedMultiplier > buffedThreshold) speedType = "BUFFED";
                
                Debug.Log($"[CharacterAnimatorController] Speed: {currentSpeed:F2} | Total: {currentTotalSpeed:F2} | Base: {baseSpeed:F2} | Multiplier: {speedMultiplier:F2} ({speedType})");
            }
        }
        else if (debugAnimator && hasSpeedMultiplierParameter)
        {
            Debug.LogWarning($"[CharacterAnimatorController] UpdateMovementSpeed: target={targetSpeed:F2}, grounded={grounded}, Speed={currentSpeed:F2}");
        }
    }

    public void TriggerAttack()
    {
        if (animator == null || !CharacterManager.Data.IsAlive) return;
        
        // Valida se não está em outra ação (apenas se parâmetro existir)
        if (hasAbilityActiveParameter && animator.GetBool(AbilityActiveHash))
        {
            if (debugAnimator)
            {
                Debug.LogWarning("[CharacterAnimatorController] Cannot attack: ability is active");
            }
            return;
        }
        
        // Zera velocidade para evitar deslizamento durante ataque
        currentSpeed = 0f;
        if (hasSpeedParameter)
        {
            animator.SetFloat(SpeedHash, 0f);
        }
        
        animator.SetTrigger(AttackHash);
        
        // 📌 SINCRONIZA velocidade da animação com attack speed do personagem
        StartCoroutine(SyncAttackAnimationSpeed());
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] Attack triggered, speed reset to 0");
        }
    }
    
    /// <summary>
    /// Sincroniza a velocidade da animação de ataque com a velocidade de ataque do personagem.
    /// Se o personagem ataca mais rápido que a animação, acelera a animação para evitar bugs visuais.
    /// 
    /// EXEMPLO: Se attackSpeed = 2 (2 atks/seg = 0.5s por ataque) e animação dura 1.5s,
    /// a animação deve acelerar 3x (1.5s / 0.5s = 3.0) para caber no tempo correto.
    /// </summary>
    private System.Collections.IEnumerator SyncAttackAnimationSpeed()
    {
        // Aguarda 1 frame para o Animator processar o Trigger
        yield return null;
        
        // Busca o estado de ataque no Animator (layer 0 = Base Layer)
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // Verifica se entrou em um estado de ataque (pode ter vários estados com "attack" no nome)
        if (!stateInfo.IsName("Attack") && !stateInfo.IsTag("Attack"))
        {
            // Se não encontrou por nome/tag, tenta procurar por hash
            // (Adapte conforme seus estados no Animator - pode ser "BasicAttack", "Attack01", etc.)
            if (debugAnimator)
            {
                Debug.LogWarning($"[CharacterAnimatorController] Estado de ataque não encontrado. Estado atual: {stateInfo.fullPathHash}");
            }
            yield break;
        }
        
        // Pega a duração da animação atual (em segundos)
        float animationDuration = stateInfo.length;
        
        // Pega a velocidade de ataque do personagem (ataques por segundo)
        float attackSpeed = CharacterManager.Data.TotalAttackSpeed;
        
        // Calcula o tempo desejado por ataque (inverso da velocidade)
        // Ex: 2 atks/seg = 0.5 segundos por ataque
        float desiredAttackDuration = 1f / attackSpeed;
        
        // Calcula o multiplicador de velocidade necessário
        // Ex: animação de 1.5s com desiredDuration de 0.5s = multiplier de 3.0
        float speedMultiplier = animationDuration / desiredAttackDuration;
        
        // Limita o multiplicador para evitar animações muito lentas ou muito rápidas
        // Min 0.5x (metade da velocidade) até 5.0x (5 vezes mais rápido)
        speedMultiplier = Mathf.Clamp(speedMultiplier, 0.5f, 5.0f);
        
        // Aplica a velocidade apenas se for diferente de 1.0 (normal)
        if (Mathf.Abs(speedMultiplier - 1f) > 0.01f)
        {
            animator.speed = speedMultiplier;
            
            if (debugAnimator)
            {
                Debug.Log($"[CharacterAnimatorController] ATTACK SYNC: AnimDuration={animationDuration:F2}s | AttackSpeed={attackSpeed:F2}/s | DesiredDuration={desiredAttackDuration:F2}s | Multiplier={speedMultiplier:F2}x");
            }
            
            // Aguarda a duração ajustada da animação
            yield return new WaitForSeconds(animationDuration / speedMultiplier);
            
            // Restaura velocidade normal do Animator
            animator.speed = 1f;
        }
        else if (debugAnimator)
        {
            Debug.Log($"[CharacterAnimatorController] Attack speed match - sem necessidade de ajuste (multiplier={speedMultiplier:F2})");
        }
    }

    public void TriggerAbility(int abilityIndex)
    {
        if (animator == null || !CharacterManager.Data.IsAlive) return;
        
        // Se o Animator não tem suporte a habilidades, ignora
        if (!hasAbilityActiveParameter)
        {
            if (debugAnimator)
            {
                Debug.LogWarning($"[CharacterAnimatorController] Animator on {gameObject.name} does not support abilities (missing AbilityActive parameter)");
            }
            return;
        }
        
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
        if (hasSpeedParameter)
        {
            animator.SetFloat(SpeedHash, 0f);
        }
        
        animator.SetInteger(AbilityIndexHash, abilityIndex);
        animator.SetBool(AbilityActiveHash, true);
        abilityActiveTimer = 0f; // Reset do timer de timeout
        
        if (debugAnimator)
        {
            Debug.Log($"[CharacterAnimatorController] Ability {abilityIndex} triggered, speed reset to 0");
        }
    }

    public void TriggerTakeDamage()
    {
        if (animator == null || !CharacterManager.Data.IsAlive) return;
        animator.SetTrigger(TakeDamageHash);
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] Take damage animation triggered");
        }
    }

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
        
        // Apenas reseta se o parâmetro existir
        if (hasAbilityActiveParameter)
        {
            animator.SetBool(AbilityActiveHash, false);
        }
        abilityActiveTimer = 0f;
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] Ability ended, reset ability active flag");
        }
    }

    /// <summary>
    /// Recalcula a velocidade base do personagem. Use quando equipamentos ou nível mudarem.
    /// </summary>
    public void RecalculateBaseSpeed()
    {
        if (CharacterManager == null) return;
        baseSpeed = CharacterManager.Data.speed;
        
        if (debugAnimator)
        {
            Debug.Log($"[CharacterAnimatorController] Base speed recalculated: {baseSpeed}");
        }
    }

    // ========== Controle de Estados ==========
    public void SetIdleState()
    {
        if (animator == null) return;
        
        animator.SetBool(IsIdleHash, true);
        animator.SetBool(IsMovingHash, false);
        animator.SetBool(IsAttackingHash, false);
        animator.SetBool(IsCastingHash, false);
        
        // Limpa flag de queda ao retornar para idle
        if (hasIsFallingParameter)
        {
            animator.SetBool(IsFallingHash, false);
        }
        
        // NÃO zera Speed aqui - Idle é estado puro sem controle de velocidade
        // MovingState é responsável por controlar Speed
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] State set to IDLE (estado puro, sem Speed control)");
        }
    }

    public void SetMovingState()
    {
        if (animator == null) return;
        
        animator.SetBool(IsIdleHash, false);
        animator.SetBool(IsMovingHash, true);
        animator.SetBool(IsAttackingHash, false);
        animator.SetBool(IsCastingHash, false);
        
        // Limpa flag de queda ao começar a mover
        if (hasIsFallingParameter)
        {
            animator.SetBool(IsFallingHash, false);
        }
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] State set to MOVING");
        }
    }

    public void SetAttackingState()
    {
        if (animator == null) return;
        
        animator.SetBool(IsIdleHash, false);
        animator.SetBool(IsMovingHash, false);
        animator.SetBool(IsAttackingHash, true);
        animator.SetBool(IsCastingHash, false);
        
        // Limpa flag de queda ao atacar
        if (hasIsFallingParameter)
        {
            animator.SetBool(IsFallingHash, false);
        }
        
        // Zera velocidade para evitar deslizamento
        currentSpeed = 0f;
        animator.SetFloat(SpeedHash, 0f);
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] State set to ATTACKING, speed forced to 0");
        }
    }

    public void SetCastingState()
    {
        if (animator == null) return;
        
        animator.SetBool(IsIdleHash, false);
        animator.SetBool(IsMovingHash, false);
        animator.SetBool(IsAttackingHash, false);
        animator.SetBool(IsCastingHash, true);
        
        // Limpa flag de queda ao iniciar cast
        if (hasIsFallingParameter)
        {
            animator.SetBool(IsFallingHash, false);
        }
        
        // Zera velocidade para evitar deslizamento
        currentSpeed = 0f;
        animator.SetFloat(SpeedHash, 0f);
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] State set to CASTING, speed forced to 0");
        }
    }

    public void SetFallingState()
    {
        if (animator == null) return;
        
        animator.SetBool(IsIdleHash, false);
        animator.SetBool(IsMovingHash, false);
        animator.SetBool(IsAttackingHash, false);
        animator.SetBool(IsCastingHash, false);
        
        // Ativa flag de queda se existir
        if (hasIsFallingParameter)
        {
            animator.SetBool(IsFallingHash, true);
        }
        
        // Zera velocidade horizontal
        currentSpeed = 0f;
        if (hasSpeedParameter)
        {
            animator.SetFloat(SpeedHash, 0f);
        }
        
        if (debugAnimator)
        {
            Debug.Log("[CharacterAnimatorController] State set to FALLING, speed forced to 0");
        }
    }

    // ========== Métodos Utilitários ==========

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

    public void ResetAnimatorState()
    {
        if (animator == null) return;
        
        // Reseta bools
        animator.SetBool(IsDeathHash, false);
        if (hasAbilityActiveParameter)
        {
            animator.SetBool(AbilityActiveHash, false);
        }
        animator.SetBool(IsGroundedHash, true);
        
        if (hasIsFallingParameter)
        {
            animator.SetBool(IsFallingHash, false);
        }
        
        // Reseta floats
        currentSpeed = 0f;
        if (hasSpeedParameter)
        {
            animator.SetFloat(SpeedHash, 0f);
        }
        
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

    // Event handler para morte - assinatura EventHandler<DeathEventArgs>
    private void HandleDeath(object sender, DeathEventArgs e)
    {
        if (animator == null) return;
        
        // Reseta todos os triggers pendentes para evitar animações estranhas
        ResetAllTriggers();
        
        // Para todas as ações
        if (hasAbilityActiveParameter)
        {
            animator.SetBool(AbilityActiveHash, false);
        }
        animator.SetBool(IsDeathHash, true);
        
        // Zera velocidade
        currentSpeed = 0f;
        if (hasSpeedParameter)
        {
            animator.SetFloat(SpeedHash, 0f);
        }
        
        // Reseta timers
        abilityActiveTimer = 0f;
        
        if (debugAnimator)
        {
            Debug.Log($"[CharacterAnimatorController] HandleDeath: {e.CharacterName} morreu (Tipo: {e.CharacterType})");
        }
    }

    // Event handler para revive - assinatura EventHandler<ReviveEventArgs>
    private void HandleRevive(object sender, ReviveEventArgs e)
    {
        if (animator == null) return;
        
        // Reseta completamente o animator para estado limpo
        ResetAnimatorState();
        
        if (debugAnimator)
        {
            Debug.Log($"[CharacterAnimatorController] HandleRevive: restaurado {e.RestoredHealth} HP e {e.RestoredEnergy} energia");
        }
    }

    // Event handler para level up - assinatura EventHandler<LevelUpEventArgs>
    private void HandleLevelUp(object sender, LevelUpEventArgs e)
    {
        if (animator == null) return;
        animator.SetTrigger(LevelUpHash);
        
        if (debugAnimator)
        {
            Debug.Log($"[CharacterAnimatorController] HandleLevelUp: Level {e.PreviousLevel} → {e.NewLevel}");
        }
    }
}