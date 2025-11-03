using UnityEngine;

/// <summary>
/// Estado Attacking - Player executando ataque.
/// NÃO permite movimento durante ataque (por enquanto).
/// </summary>
public class PlayerAttackingState : PlayerStateBase
{
    private float attackDuration;
    private float attackTimer = 0f;
    private bool attackExecuted = false;

    public PlayerAttackingState(PlayerStateMachine stateMachine, PlayerManager player) 
        : base(stateMachine, player)
    {
        attackDuration = player.AttackDuration;
    }

    public override void EnterState()
    {
        attackTimer = 0f;
        attackExecuted = false;

        player.Motor.Stop();

        Vector3 mouseWorldPos = player.Mouse.GetMousePosition();
        Vector3 attackDirection = (mouseWorldPos - player.transform.position);
        attackDirection.y = 0;

        if (attackDirection.magnitude > 0.1f)
        {
            player.Motor.Rotate(attackDirection);
        }

        var attackAbility = player.Ability.SkillSlots[0]?.AssignedAbility;
        
        if (attackAbility != null && attackAbility.castTime > 0f)
        {
            var context = new AbilityContext
            {
                Caster = player.gameObject,
                Target = player.Mouse.GetClickedObject(),
                TargetPosition = player.Mouse.GetMousePosition(),
                CastStartPosition = player.transform.position
            };
            
            SwitchState(new PlayerCastingState(stateMachine, player, 0, context, attackAbility.castTime));
            return;
        }

        player.Animator?.TriggerAbility(0);
        
        var instantContext = new AbilityContext
        {
            Caster = player.gameObject,
            Target = player.Mouse.GetClickedObject(),
            TargetPosition = player.Mouse.GetMousePosition(),
            CastStartPosition = player.transform.position
        };
        
        bool success = player.Ability.TryUseAbilityInSlot(0, instantContext);
        attackExecuted = success;

        if (success)
        {
            Debug.Log("[AttackingState] Ataque básico executado com sucesso (instantâneo)");
        }
        else
        {
            Debug.LogWarning("[AttackingState] Falha ao executar ataque básico (cooldown ou energia insuficiente?)");
        }
    }

    public override void UpdateState()
    {
        attackTimer += Time.deltaTime;

        if (player.CanRotateDuringAttack)
        {
            Vector3 mouseWorldPos = player.Mouse.GetMousePosition();
            Vector3 attackDirection = (mouseWorldPos - player.transform.position);
            attackDirection.y = 0;

            if (attackDirection.magnitude > player.MovementThreshold)
            {
                player.Motor.Rotate(attackDirection);
            }
        }

        if (attackTimer >= attackDuration)
        {
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
        
        Debug.Log("[AttackingState] Saiu do estado Attacking");
    }

    // ========== Permissões ==========
    public override bool CanMove() => false;
    public override bool CanAttack() => false;
    public override bool CanUseAbility() => false;
    public override bool CanInteract() => false;
    public override bool CanDodge() => false;
}
