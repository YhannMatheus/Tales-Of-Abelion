using UnityEngine;

// Estado MovingToTarget - Player se movendo até um destino ou inimigo
// Usado para movimento com botão direito do mouse (estilo MOBA/ARPG)
public class PlayerMovingToTargetState : PlayerStateBase
{
    private Vector3 targetPosition;
    private Transform targetTransform; // Para inimigos móveis
    private CharacterManager targetCharacter; // Se for inimigo
    private bool isTargetEnemy;
    private float attackRange = 2.0f; // Distância mínima para atacar

    // Construtor para movimento até posição fixa
    public PlayerMovingToTargetState(PlayerStateMachine stateMachine, PlayerManager player, Vector3 destination) 
        : base(stateMachine, player)
    {
        targetPosition = destination;
        targetTransform = null;
        targetCharacter = null;
        isTargetEnemy = false;
    }

    // Construtor para movimento até inimigo (ataque automático)
    public PlayerMovingToTargetState(PlayerStateMachine stateMachine, PlayerManager player, CharacterManager enemyTarget) 
        : base(stateMachine, player)
    {
        targetCharacter = enemyTarget;
        targetTransform = enemyTarget.transform;
        targetPosition = targetTransform.position;
        isTargetEnemy = true;
    }

    public override void EnterState()
    {
        // Define estado de animação como Moving
        player.Animator?.SetMovingState();
    }

    public override void UpdateState()
    {
        // ========== ATUALIZA DESTINO CONTINUAMENTE ENQUANTO BOTÃO PRESSIONADO ==========
        if (player.Mouse.RightMouseButtonHeld && !isTargetEnemy)
        {
            Vector3 newDestination = player.Mouse.GetMousePosition();
            if (newDestination != Vector3.zero)
            {
                targetPosition = newDestination;
                player.Motor.ClearDestination(); // Força atualização no próximo frame
            }
        }
        
        // ========== VERIFICA NOVO CLIQUE DE BOTÃO DIREITO ==========
        if (player.Mouse.RightMouseButtonDown)
        {
            HandleRightClick();
            return;
        }

        // ========== VERIFICA SE ALVO MORREU ==========
        if (isTargetEnemy && (targetCharacter == null || !targetCharacter.Data.IsAlive))
        {
            SwitchState(new PlayerIdleState(stateMachine, player));
            return;
        }

        // ========== ATUALIZA DESTINO SE FOR INIMIGO MÓVEL ==========
        if (isTargetEnemy && targetTransform != null)
        {
            targetPosition = targetTransform.position;
        }

        // ========== VERIFICA DISTÂNCIA ATÉ ALVO ==========
        float distanceToTarget = Vector3.Distance(player.transform.position, targetPosition);

        // Se é inimigo e chegou na distância de ataque
        if (isTargetEnemy && distanceToTarget <= attackRange)
        {
            SwitchState(new PlayerAutoAttackingState(stateMachine, player, targetCharacter));
            return;
        }

        // ========== MOVIMENTO ==========
        if (targetTransform != null)
        {
            // Segue transform móvel
            player.Motor.MoveToTarget(targetTransform, player.Character.Data.TotalSpeed);
        }
        else
        {
            // Move até posição fixa
            player.Motor.MoveToDestination(targetPosition, player.Character.Data.TotalSpeed);
        }

        // Verifica se chegou ao destino (APÓS setar o destino no Motor)
        if (!isTargetEnemy && player.Motor.HasReachedDestination())
        {
            SwitchState(new PlayerIdleState(stateMachine, player));
            return;
        }

        // Rotaciona na direção do movimento
        Vector3 direction = (targetPosition - player.transform.position);
        direction.y = 0;
        
        if (direction.magnitude > 0.1f)
        {
            player.Motor.Rotate(direction);
        }

        // Atualiza animação de movimento
        player.Animator?.UpdateMovementSpeed(player.Motor.CurrentSpeedNormalized, player.Motor.IsGrounded);

        // ========== PERMITE USAR HABILIDADES DURANTE MOVIMENTO ==========
        int abilitySlot = CheckAbilityInputs();
        if (abilitySlot != -1)
        {
            TryUseAbility(abilitySlot);
            return;
        }
    }

    public override void ExitState()
    {
        player.Animator?.UpdateMovementSpeed(0f, player.Motor.IsGrounded);
        player.Motor.Stop();
    }

    // Processa novo clique com botão direito
    private void HandleRightClick()
    {
        // Verifica se clicou em inimigo
        if (player.Mouse.IsMouseOverEnemy(out CharacterManager enemyCharacter))
        {
            if (enemyCharacter.Data.IsAlive)
            {
                // Atualiza destino para o novo inimigo
                targetCharacter = enemyCharacter;
                targetTransform = enemyCharacter.transform;
                targetPosition = targetTransform.position;
                isTargetEnemy = true;
                
                // Limpa destino fixo anterior no Motor
                player.Motor.ClearDestination();
                
                return;
            }
        }

        // Clicou em chão/posição - atualiza destino direto
        Vector3 destination = player.Mouse.GetMousePosition();
        if (destination != Vector3.zero)
        {
            // Atualiza para nova posição fixa
            targetPosition = destination;
            targetTransform = null;
            targetCharacter = null;
            isTargetEnemy = false;
            
            // Limpa destino anterior no Motor (será setado no próximo Update)
            player.Motor.ClearDestination();
        }
    }

    private int CheckAbilityInputs()
    {
        // Slots 1-6: Q, W, E, A, S, D (Habilidades principais)
        if (player.Input.ability1Input) return 1; // Q
        if (player.Input.ability2Input) return 2; // W
        if (player.Input.ability3Input) return 3; // E
        if (player.Input.ability4Input) return 4; // A
        if (player.Input.ability5Input) return 5; // S
        if (player.Input.ability6Input) return 6; // D
        
        // Slots 7-8: Z, X (Itens/Consumíveis)
        if (player.Input.ability7Input) return 7; // Z
        if (player.Input.ability8Input) return 8; // X
        
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
            Target = player.Mouse.GetClickedObject()?.GetComponent<CharacterManager>(),
            OriginPosition = player.transform.position,
            TargetPosition = player.Mouse.GetMousePosition()
        };

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

    // ========== Permissões ==========
    public override bool CanMove() => true;
    public override bool CanAttack() => true;
    public override bool CanUseAbility() => true;
    public override bool CanInteract() => false; // Focado em movimento
    public override bool CanDodge() => true;
}
