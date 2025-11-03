using UnityEngine;

/// <summary>
/// Estado Casting - Player conjurando habilidade com cast time.
/// Não permite movimento durante cast (configurável).
/// </summary>
public class PlayerCastingState : PlayerStateBase
{
    private float castTime;
    private float castTimer = 0f;
    private int abilitySlotIndex;
    private AbilityContext abilityContext;
    private bool castCompleted = false;

    public PlayerCastingState(PlayerStateMachine stateMachine, PlayerManager player, int slotIndex, AbilityContext context, float duration) 
        : base(stateMachine, player)
    {
        abilitySlotIndex = slotIndex;
        abilityContext = context;
        castTime = duration;
    }

    public override void EnterState()
    {
        castTimer = 0f;
        castCompleted = false;

        // Para o movimento
        player.Motor.Stop();

        // Rotaciona para o alvo (se tiver)
        if (abilityContext.Target != null)
        {
            Vector3 targetDir = abilityContext.Target.transform.position - player.transform.position;
            targetDir.y = 0;
            if (targetDir.magnitude > player.MovementThreshold)
            {
                player.Motor.Rotate(targetDir);
            }
        }
        else if (abilityContext.TargetPosition.HasValue && abilityContext.TargetPosition.Value != Vector3.zero)
        {
            Vector3 targetDir = abilityContext.TargetPosition.Value - player.transform.position;
            targetDir.y = 0;
            if (targetDir.magnitude > player.MovementThreshold)
            {
                player.Motor.Rotate(targetDir);
            }
        }

        // Toca animação de cast
        player.Animator?.TriggerAbility(abilitySlotIndex);

        Debug.Log($"[CastingState] Iniciando cast da habilidade slot {abilitySlotIndex} (duração: {castTime}s)");
    }

    public override void UpdateState()
    {
        castTimer += Time.deltaTime;

        // Se configurado para cancelar ao mover
        if (player.CanCancelCastByMoving)
        {
            Vector3 moveInput = new Vector3(player.Input.horizontalInput, 0, player.Input.verticalInput);
            
            if (moveInput.magnitude > player.MovementThreshold)
            {
                CancelCast();
                return;
            }
        }

        // Permite rotacionar durante cast (se configurado)
        if (player.CanRotateDuringCast)
        {
            if (abilityContext.Target != null)
            {
                Vector3 targetDir = abilityContext.Target.transform.position - player.transform.position;
                targetDir.y = 0;
                if (targetDir.magnitude > player.MovementThreshold)
                {
                    player.Motor.Rotate(targetDir);
                }
            }
        }

        // Cast completo
        if (castTimer >= castTime && !castCompleted)
        {
            castCompleted = true;
            ExecuteAbility();
            
            // Volta para Idle após cast
            Vector3 moveInput = new Vector3(player.Input.horizontalInput, 0, player.Input.verticalInput);
            if (moveInput.magnitude > player.MovementThreshold)
            {
                SwitchState(new PlayerMovingState(stateMachine, player));
            }
            else
            {
                SwitchState(new PlayerIdleState(stateMachine, player));
            }
        }
    }

    public override void ExitState()
    {
        // Finaliza animação de habilidade
        player.Animator?.EndAbility();

        Debug.Log($"[CastingState] Cast finalizado. Completado: {castCompleted}");
    }

    private void ExecuteAbility()
    {
        // Executa a habilidade via AbilityManager
        // Nota: Não precisa verificar energyCost novamente, o AbilityManager já faz isso
        bool success = player.Ability.TryUseAbilityInSlot(abilitySlotIndex, abilityContext);
        
        if (success)
        {
            Debug.Log($"[CastingState] Habilidade slot {abilitySlotIndex} executada com sucesso após cast!");
        }
        else
        {
            Debug.LogWarning($"[CastingState] Falha ao executar habilidade slot {abilitySlotIndex} (energia insuficiente?)");
        }
    }

    private void CancelCast()
    {
        Debug.Log($"[CastingState] Cast cancelado pelo jogador!");
        
        // Volta para Moving (já está se movendo)
        SwitchState(new PlayerMovingState(stateMachine, player));
    }

    // ========== Permissões ==========
    // Durante cast: NÃO pode mover (a menos que cancele), NÃO pode atacar

    public override bool CanMove() => false;
    public override bool CanAttack() => false;
    public override bool CanUseAbility() => false;
    public override bool CanInteract() => false;
    public override bool CanDodge() => false;
}
