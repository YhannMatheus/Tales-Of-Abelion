using UnityEngine;

// Estado MovingToTarget - Player se movendo até um destino ou inimigo
// Usado para movimento com botão direito do mouse (estilo MOBA/ARPG)
public class PlayerMovingToTargetState : PlayerStateBase
{
    private Vector3 targetPosition;
    private Transform targetTransform; // Para inimigos móveis
    private Character targetCharacter; // Se for inimigo
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
    public PlayerMovingToTargetState(PlayerStateMachine stateMachine, PlayerManager player, Character enemyTarget) 
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
        
        if (isTargetEnemy)
        {
            Debug.Log($"[MovingToTargetState] Movendo para atacar inimigo: {targetCharacter.name}");
        }
        else
        {
            Debug.Log($"[MovingToTargetState] Movendo para posição: {targetPosition}");
        }
    }

    public override void UpdateState()
    {
        // ========== VERIFICA NOVO INPUT DE BOTÃO DIREITO ==========
        if (player.Mouse.RightMouseButtonDown)
        {
            HandleRightClick();
            return;
        }

        // ========== VERIFICA SE ALVO MORREU ==========
        if (isTargetEnemy && (targetCharacter == null || !targetCharacter.Data.IsAlive))
        {
            Debug.Log("[MovingToTargetState] Alvo morreu, voltando para Idle");
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
            Debug.Log("[MovingToTargetState] Na distância de ataque, iniciando auto-ataque");
            SwitchState(new PlayerAutoAttackingState(stateMachine, player, targetCharacter));
            return;
        }

        // Se é posição fixa e chegou
        if (!isTargetEnemy && player.Motor.HasReachedDestination())
        {
            Debug.Log("[MovingToTargetState] Chegou ao destino");
            SwitchState(new PlayerIdleState(stateMachine, player));
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
        
        Debug.Log("[MovingToTargetState] Saiu do estado MovingToTarget");
    }

    // Processa novo clique com botão direito
    private void HandleRightClick()
    {
        // Verifica se clicou em inimigo
        if (player.Mouse.IsMouseOverEnemy(out Character enemyCharacter))
        {
            if (enemyCharacter.Data.IsAlive)
            {
                Debug.Log($"[MovingToTargetState] Novo alvo de ataque: {enemyCharacter.name}");
                SwitchState(new PlayerMovingToTargetState(stateMachine, player, enemyCharacter));
                return;
            }
        }

        // Clicou em chão/posição
        Vector3 destination = player.Mouse.GetMousePosition();
        if (destination != Vector3.zero)
        {
            Debug.Log($"[MovingToTargetState] Novo destino: {destination}");
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
        if (slotIndex < 0 || slotIndex >= player.Ability.SkillSlots.Length) return;

        var slot = player.Ability.SkillSlots[slotIndex];
        if (slot == null || slot.AssignedAbility == null) return;
        if (!slot.CanUse()) return;

        player.Motor.Stop();

        var context = new AbilityContext
        {
            Caster = player.gameObject,
            Target = player.Mouse.GetClickedObject(),
            TargetPosition = player.Mouse.GetMousePosition(),
            CastStartPosition = player.transform.position
        };

        if (slot.AssignedAbility.castTime > 0f)
        {
            SwitchState(new PlayerCastingState(stateMachine, player, slotIndex, context, slot.AssignedAbility.castTime));
        }
        else
        {
            player.Animator?.TriggerAbility(slotIndex);
            player.Ability.TryUseAbilityInSlot(slotIndex, context);
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
