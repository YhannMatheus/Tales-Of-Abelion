using UnityEngine;

// Estado AutoAttacking - Player executando ataques automáticos repetidos em um inimigo
// Ataca continuamente até que o inimigo morra, saia de alcance, ou player clique em outro lugar
public class PlayerAutoAttackingState : PlayerStateBase
{
    private Character targetEnemy;
    private float attackRange = 2.5f; // Range de ataque (um pouco maior que range de movimento)
    private float attackDuration;
    private float attackTimer = 0f;
    private bool canAttackAgain = true;

    // Tempo mínimo antes de poder cancelar o ataque com movimento
    private const float MINIMUM_ATTACK_TIME = 0.2f;

    public PlayerAutoAttackingState(PlayerStateMachine stateMachine, PlayerManager player, Character target) 
        : base(stateMachine, player)
    {
        targetEnemy = target;
        attackDuration = player.AttackDuration;
    }

    public override void EnterState()
    {
        if (targetEnemy == null || !targetEnemy.Data.IsAlive)
        {
            SwitchState(new PlayerIdleState(stateMachine, player));
            return;
        }

        attackTimer = 0f;
        canAttackAgain = false;

        // Para movimento
        player.Motor.Stop();

        // Rotaciona para o inimigo
        player.Motor.RotateToPosition(targetEnemy.transform.position);

        // Inicia primeiro ataque
        ExecuteAttack();

        Debug.Log($"[AutoAttackingState] Iniciou auto-ataque em {targetEnemy.name}");
    }

    public override void UpdateState()
    {
        if (targetEnemy == null || !targetEnemy.Data.IsAlive)
        {
            Debug.Log("[AutoAttackingState] Inimigo morreu, parando auto-ataque");
            SwitchState(new PlayerIdleState(stateMachine, player));
            return;
        }

        if (player.Mouse.RightMouseButtonDown)
        {
            HandleRightClick();
            return;
        }

        float distanceToEnemy = Vector3.Distance(player.transform.position, targetEnemy.transform.position);
        
        if (distanceToEnemy > attackRange)
        {
            Debug.Log("[AutoAttackingState] Inimigo saiu de alcance, perseguindo");
            SwitchState(new PlayerMovingToTargetState(stateMachine, player, targetEnemy));
            return;
        }

        attackTimer += Time.deltaTime;

        // Rotaciona continuamente para o inimigo (tracking)
        if (attackTimer < MINIMUM_ATTACK_TIME || canAttackAgain)
        {
            player.Motor.RotateToPosition(targetEnemy.transform.position);
        }

        if (attackTimer >= attackDuration && canAttackAgain)
        {
            // Reseta timer e executa próximo ataque
            attackTimer = 0f;
            canAttackAgain = false;
            ExecuteAttack();
        }
        else if (attackTimer >= MINIMUM_ATTACK_TIME && !canAttackAgain)
        {
            // Marca que pode atacar novamente após cooldown
            canAttackAgain = true;
        }

        int abilitySlot = CheckAbilityInputs();
        if (abilitySlot != -1)
        {
            TryUseAbility(abilitySlot);
            return;
        }
    }

    public override void ExitState()
    {
        player.Animator?.EndAbility();
        
        Debug.Log("[AutoAttackingState] Saiu do estado AutoAttacking");
    }

    // Executa um ataque básico no inimigo
    private void ExecuteAttack()
    {
        // Define animação de ataque
        player.Animator?.SetAttackingState();
        player.Animator?.TriggerAbility(0);

        // Usa Skill System
        player.SkillManager.UseBasicAttack(targetEnemy);
        Debug.Log($"[AutoAttackingState] Atacou {targetEnemy.name}");
    }

    // Processa novo clique com botão direito (cancela auto-ataque)
    private void HandleRightClick()
    {
        // Verifica se clicou em OUTRO inimigo
        if (player.Mouse.IsMouseOverEnemy(out Character enemyCharacter))
        {
            if (enemyCharacter.Data.IsAlive && enemyCharacter != targetEnemy)
            {
                Debug.Log($"[AutoAttackingState] Trocando alvo para: {enemyCharacter.name}");
                SwitchState(new PlayerMovingToTargetState(stateMachine, player, enemyCharacter));
                return;
            }
            else if (enemyCharacter == targetEnemy)
            {
                // Clicou no mesmo inimigo, continua atacando
                return;
            }
        }

        // Clicou em chão/posição - cancela auto-ataque e move
        Vector3 destination = player.Mouse.GetMousePosition();
        if (destination != Vector3.zero)
        {
            Debug.Log("[AutoAttackingState] Cancelou auto-ataque, movendo para nova posição");
            SwitchState(new PlayerMovingToTargetState(stateMachine, player, destination));
        }
    }

    private int CheckAbilityInputs()
    {
        if (player.Input.ability1Input) return 1;
        if (player.Input.ability2Input) return 2;
        if (player.Input.ability3Input) return 3;
        if (player.Input.ability4Input) return 4;
        if (player.Input.ability5Input) return 5;
        if (player.Input.ability6Input) return 6;
        if (player.Input.ability7Input) return 7;
        if (player.Input.ability8Input) return 8;
        
        return -1;
    }

    private void TryUseAbility(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= player.SkillManager.SkillSlots.Length) return;

        var slot = player.SkillManager.SkillSlots[slotIndex];
        if (slot == null || slot.AssignedSkill == null) return;
        if (!slot.CanUse(player.Character)) return;

        player.Motor.Stop();

        var context = new SkillContext
        {
            Caster = player.Character,
            Target = player.Mouse.GetClickedObject()?.GetComponent<Character>(),
            OriginPosition = player.transform.position,
            TargetPosition = player.Mouse.GetMousePosition()
        };

        Debug.Log("[AutoAttackingState] Cancelou auto-ataque para usar habilidade");

        if (slot.AssignedSkill.castTime > 0f)
        {
            SwitchState(new PlayerCastingState(stateMachine, player, slotIndex, context, slot.AssignedSkill.castTime));
        }
        else
        {
            player.Animator?.TriggerAbility(slotIndex);
            player.SkillManager.TryUseSkillInSlot(slotIndex, context);
            SwitchState(new PlayerIdleState(stateMachine, player));
        }
    }

    public override bool CanMove() => false; // Travado no auto-ataque
    public override bool CanAttack() => false; // Já está atacando
    public override bool CanUseAbility() => true; // Pode usar habilidades (cancela auto-ataque)
    public override bool CanInteract() => false;
    public override bool CanDodge() => true; // Pode dar dodge (futuro)
}
