using UnityEngine;

/// <summary>
/// Estado Idle - Player parado, aguardando input.
/// Permite todas as ações (mover, atacar, interagir).
/// </summary>
public class PlayerIdleState : PlayerStateBase
{
    public PlayerIdleState(PlayerStateMachine stateMachine, PlayerManager player) 
        : base(stateMachine, player)
    {
    }

    public override void EnterState()
    {
        // Para o movimento
        player.Motor.Stop();

        // Toca animação de idle (se tiver)
        player.Animator?.UpdateMovementSpeed(0f, player.Motor.IsGrounded);

        Debug.Log("[IdleState] Entrou no estado Idle");
    }

    public override void UpdateState()
    {
        // Transição para Moving se houver input de movimento
        Vector3 moveInput = new Vector3(player.Input.horizontalInput, 0, player.Input.verticalInput);
        
        if (moveInput.magnitude > player.MovementThreshold)
        {
            SwitchState(new PlayerMovingState(stateMachine, player));
            return;
        }

        // Transição para Attacking se apertar botão de ataque
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

        // Transição para Interacting se apertar botão de interação
        if (player.Input.interactButton)
        {
            // Verifica se há algo para interagir
            GameObject clickedObject = player.Mouse.GetClickedObject();
            if (clickedObject != null)
            {
                Event eventComponent = clickedObject.GetComponent<Event>();
                if (eventComponent != null)
                {
                    float distance = Vector3.Distance(player.transform.position, clickedObject.transform.position);
                    
                    if (distance <= eventComponent.minDistanceToTrigger)
                    {
                        // Rotaciona para o objeto antes de interagir (se configurado)
                        if (player.AutoRotateOnInteract)
                        {
                            player.Motor.RotateToPosition(player.Mouse.GetMousePosition());
                        }
                        eventComponent.OnClick();
                    }
                }
            }
        }
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
            // Habilidade instantânea - executa direto
            player.Animator?.TriggerAbility(slotIndex);
            player.Ability.TryUseAbilityInSlot(slotIndex, context);
        }
    }

    public override void ExitState()
    {
        Debug.Log("[IdleState] Saiu do estado Idle");
    }

    // ========== Permissões ==========
    // No estado Idle, player pode fazer tudo

    public override bool CanMove() => true;
    public override bool CanAttack() => true;
    public override bool CanUseAbility() => true;
    public override bool CanInteract() => true;
    public override bool CanDodge() => true;
}
