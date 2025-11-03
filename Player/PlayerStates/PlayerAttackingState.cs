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
        // Pega a duração configurada no PlayerManager
        attackDuration = player.AttackDuration;
    }

    public override void EnterState()
    {
        attackTimer = 0f;
        attackExecuted = false;

        // Para o movimento
        player.Motor.Stop();

        // Rotaciona para a direção do mouse
        Vector3 mouseWorldPos = player.Mouse.GetMousePosition();
        Vector3 attackDirection = (mouseWorldPos - player.transform.position);
        attackDirection.y = 0;

        if (attackDirection.magnitude > 0.1f)
        {
            player.Motor.Rotate(attackDirection);
        }

        // Verifica se a habilidade do slot 0 (ataque básico) tem cast time
        var attackAbility = player.Ability.SkillSlots[0]?.AssignedAbility;
        
        if (attackAbility != null && attackAbility.castTime > 0f)
        {
            // Habilidade tem cast time - vai para CastingState ao invés de executar direto
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

        // Ataque instantâneo (sem cast time)
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

        // Permite rotacionar durante ataque (se configurado)
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

        // Quando a animação de ataque terminar, volta para Idle
        if (attackTimer >= attackDuration)
        {
            // Verifica se há input de movimento para ir direto para Moving
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
        Debug.Log("[AttackingState] Saiu do estado Attacking");
    }

    // ========== Permissões ==========
    // Durante ataque: NÃO pode mover, NÃO pode atacar de novo (por enquanto, sem combos)

    public override bool CanMove() => false;
    public override bool CanAttack() => false;
    public override bool CanUseAbility() => false;
    public override bool CanInteract() => false;
    public override bool CanDodge() => false;
}
