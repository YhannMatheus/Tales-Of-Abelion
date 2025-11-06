using UnityEngine;

public class PlayerAttackingState : PlayerStateBase
{
    private float attackDuration;
    private float attackTimer = 0f;
    private bool damageDealt = false;
    private const float MINIMUM_ATTACK_TIME = 0.2f;

    public PlayerAttackingState(PlayerStateMachine stateMachine, PlayerManager player) 
        : base(stateMachine, player)
    {
        attackDuration = player.AttackDuration;
    }

    public override void EnterState()
    {
        attackTimer = 0f;
        damageDealt = false;
        damageDealt = false;

        player.Motor.Stop();

        // Rotaciona APENAS NO INÍCIO para direção do mouse
        Vector3 mouseWorldPos = player.Mouse.GetMousePosition();
        Vector3 attackDirection = (mouseWorldPos - player.transform.position);
        attackDirection.y = 0;

        if (attackDirection.magnitude > 0.1f)
        {
            player.Motor.Rotate(attackDirection);
        }

        var attackSkill = player.SkillManager.BasicAttackSlot?.AssignedSkill;
        
        if (attackSkill != null && attackSkill.castTime > 0f)
        {
            var context = new SkillContext
            {
                Caster = player.Character,
                Target = player.Mouse.GetClickedObject()?.GetComponent<Character>(),
                OriginPosition = player.transform.position,
                TargetPosition = player.Mouse.GetMousePosition()
            };
            
            SwitchState(new PlayerCastingState(stateMachine, player, 0, context, attackSkill.castTime));
            return;
        }

        // Define estado de animação como Attacking
        player.Animator?.SetAttackingState();
        player.Animator?.TriggerAbility(0);
        
        // IMPORTANTE: Dano só será aplicado após MINIMUM_ATTACK_TIME
        
        Debug.Log("[AttackingState] Ataque básico iniciado - dano será aplicado após tempo mínimo");
    }

    public override void UpdateState()
    {
        attackTimer += Time.deltaTime;

        // Verifica se deve aplicar o dano (após tempo mínimo)
        if (!damageDealt && attackTimer >= MINIMUM_ATTACK_TIME)
        {
            var context = new SkillContext
            {
                Caster = player.Character,
                Target = player.Mouse.GetClickedObject()?.GetComponent<Character>(),
                OriginPosition = player.transform.position,
                TargetPosition = player.Mouse.GetMousePosition()
            };
            
            bool success = player.SkillManager.TryUseBasicAttack(context);
            damageDealt = true;

            if (success)
            {
                Debug.Log("[AttackingState] Dano do ataque básico aplicado");
            }
            else
            {
                Debug.LogWarning("[AttackingState] Falha ao aplicar dano (cooldown ou energia insuficiente?)");
            }
        }

        // Verifica input de movimento para cancelar ataque
        Vector3 moveInput = new Vector3(player.Input.horizontalInput, 0, player.Input.verticalInput);
        
        if (moveInput.magnitude > player.MovementThreshold)
        {
            // Se tentar mover ANTES do tempo mínimo, cancela o ataque
            if (attackTimer < MINIMUM_ATTACK_TIME)
            {
                Debug.Log("[AttackingState] Ataque cancelado por movimento antes do tempo mínimo!");
                
                // Cancela animação de ataque
                player.Animator?.EndAbility();
                player.Animator?.ResetAllTriggers();
                
                SwitchState(new PlayerMovingState(stateMachine, player));
                return;
            }
            // Se tentar mover DEPOIS do tempo mínimo mas antes de terminar, sai para movimento
            else if (attackTimer < attackDuration)
            {
                Debug.Log("[AttackingState] Saindo para movimento após aplicar dano");
                SwitchState(new PlayerMovingState(stateMachine, player));
                return;
            }
        }

        // Ataque completo
        if (attackTimer >= attackDuration)
        {
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
