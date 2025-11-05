using UnityEngine;

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

        player.Motor.Stop();
        
        // Define estado de animação como Casting
        player.Animator?.SetCastingState();

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

        player.Animator?.TriggerAbility(abilitySlotIndex);

        Debug.Log($"[CastingState] Iniciando cast da habilidade slot {abilitySlotIndex} (duração: {castTime}s)");
    }

    public override void UpdateState()
    {
        castTimer += Time.deltaTime;

        if (player.CanCancelCastByMoving)
        {
            Vector3 moveInput = new Vector3(player.Input.horizontalInput, 0, player.Input.verticalInput);
            
            if (moveInput.magnitude > player.MovementThreshold)
            {
                CancelCast();
                return;
            }
        }

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

        if (castTimer >= castTime && !castCompleted)
        {
            castCompleted = true;
            ExecuteAbility();
            
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
        player.Animator?.EndAbility();

        Debug.Log($"[CastingState] Cast finalizado. Completado: {castCompleted}");
    }

    private void ExecuteAbility()
    {
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
        
        SwitchState(new PlayerMovingState(stateMachine, player));
    }

    // ========== Permissões ==========
    
    public override bool CanMove() => false;
    public override bool CanAttack() => false;
    public override bool CanUseAbility() => false;
    public override bool CanInteract() => false;
    public override bool CanDodge() => false;
}
