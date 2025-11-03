using UnityEngine;

public class PlayerMovingState : PlayerStateBase
{
    public PlayerMovingState(PlayerStateMachine stateMachine, PlayerManager player) : base(stateMachine, player){}

    public override void EnterState()
    {
        Debug.Log("[MovingState] Entrou no estado Moving");
    }

    public override void UpdateState()
    {
        // Lê input de movimento
        Vector3 moveDirection = new Vector3(player.Input.horizontalInput, 0, player.Input.verticalInput);

        if (moveDirection.magnitude <= player.MovementThreshold)
        {
            SwitchState(new PlayerIdleState(stateMachine, player));
            return;
        }

        player.Motor.Move(moveDirection, player.Character.Data.TotalSpeed);
        player.Motor.Rotate(moveDirection);

        player.Animator?.UpdateMovementSpeed(player.Motor.CurrentSpeedNormalized, player.Motor.IsGrounded);

        if (player.Input.attackInput)
        {
            SwitchState(new PlayerAttackingState(stateMachine, player));
            return;
        }

        int abilitySlot = CheckAbilityInputs();
        if (abilitySlot != -1)
        {
            TryUseAbility(abilitySlot);
            return;
        }

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
                        player.Motor.Stop();
                        
                        if (player.AutoRotateOnInteract)
                        {
                            player.Motor.RotateToPosition(player.Mouse.GetMousePosition());
                        }
                        
                        eventComponent.OnClick();
                        
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
    public override bool CanInteract() => true;
    public override bool CanDodge() => true;
}
