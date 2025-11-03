using UnityEngine;

/// <summary>
/// Estado Moving - Player se movimentando.
/// Permite atacar e interagir enquanto se move.
/// </summary>
public class PlayerMovingState : PlayerStateBase
{
    public PlayerMovingState(PlayerStateMachine stateMachine, PlayerManager player) 
        : base(stateMachine, player)
    {
    }

    public override void EnterState()
    {
        Debug.Log("[MovingState] Entrou no estado Moving");
    }

    public override void UpdateState()
    {
        // Lê input de movimento
        Vector3 moveDirection = new Vector3(player.Input.horizontalInput, 0, player.Input.verticalInput);

        // Se parou de se mover, volta para Idle
        if (moveDirection.magnitude <= player.MovementThreshold)
        {
            SwitchState(new PlayerIdleState(stateMachine, player));
            return;
        }

        // Movimenta o player
        player.Motor.Move(moveDirection, player.Character.Data.TotalSpeed);
        player.Motor.Rotate(moveDirection);

        // Atualiza animação de movimento
        player.Animator?.UpdateMovementSpeed(player.Motor.CurrentSpeedNormalized, player.Motor.IsGrounded);

        // Pode atacar enquanto se move
        if (player.Input.attackInput)
        {
            SwitchState(new PlayerAttackingState(stateMachine, player));
            return;
        }

        // Verifica inputs de habilidades (Q, E, R, etc.)
        int abilitySlot = CheckAbilityInputs();
        if (abilitySlot != -1)
        {
            TryUseAbility(abilitySlot);
            return;
        }

        // Pode interagir enquanto se move (para quando interagir)
        if (player.Input.interactButton)
        {
            GameObject clickedObject = player.Mouse.GetClickedObject();
            if (clickedObject != null)
            {
                Event eventComponent = clickedObject.GetComponent<Event>();
                if (eventComponent != null)
                {
                    float distance = Vector3.Distance(player.transform.position, clickedObject.transform.position);
                    
                    if (distance <= eventComponent.minDistanceToTrigger)
                    {
                        // Para e rotaciona antes de interagir (se configurado)
                        player.Motor.Stop();
                        
                        if (player.AutoRotateOnInteract)
                        {
                            player.Motor.RotateToPosition(player.Mouse.GetMousePosition());
                        }
                        
                        eventComponent.OnClick();
                        
                        // Volta para Idle após interagir
                        SwitchState(new PlayerIdleState(stateMachine, player));
                        return;
                    }
                }
            }
        }
    }

    public override void ExitState()
    {
        Debug.Log("[MovingState] Saiu do estado Moving");
    }

    /// <summary>
    /// Verifica quais teclas de habilidade foram pressionadas
    /// </summary>
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
        
        return -1; // Nenhuma habilidade pressionada
    }

    /// <summary>
    /// Tenta usar habilidade no slot especificado
    /// </summary>
    private void TryUseAbility(int slotIndex)
    {
        // Valida slot
        if (slotIndex < 0 || slotIndex >= player.Ability.SkillSlots.Length) return;

        var slot = player.Ability.SkillSlots[slotIndex];
        if (slot == null || slot.AssignedAbility == null) return;

        // Verifica se pode usar (cooldown, charges, etc.)
        if (!slot.CanUse()) return;

        // Para o movimento ao usar habilidade
        player.Motor.Stop();

        // Cria contexto da habilidade
        var context = new AbilityContext
        {
            Caster = player.gameObject,
            Target = player.Mouse.GetClickedObject(),
            TargetPosition = player.Mouse.GetMousePosition(),
            CastStartPosition = player.transform.position
        };

        // Se tem cast time, vai para CastingState
        if (slot.AssignedAbility.castTime > 0f)
        {
            SwitchState(new PlayerCastingState(stateMachine, player, slotIndex, context, slot.AssignedAbility.castTime));
        }
        else
        {
            // Habilidade instantânea - executa direto e volta para Idle
            player.Animator?.TriggerAbility(slotIndex);
            player.Ability.TryUseAbilityInSlot(slotIndex, context);
            SwitchState(new PlayerIdleState(stateMachine, player));
        }
    }

    // ========== Permissões ==========
    // Enquanto se move, pode atacar e interagir

    public override bool CanMove() => true;
    public override bool CanAttack() => true;
    public override bool CanUseAbility() => true;
    public override bool CanInteract() => true;
    public override bool CanDodge() => true;
}
