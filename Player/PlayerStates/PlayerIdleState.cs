using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    public PlayerIdleState(PlayerStateMachine stateMachine, PlayerManager player) 
        : base(stateMachine, player)
    {
    }

    public override void EnterState()
    {
        player.Motor.Stop();

        // Define estado de animação como Idle PURO (sem controle de Speed)
        // Idle = apenas parado, MovingState controla todas as animações de movimento
        player.Animator?.SetIdleState();

        Debug.Log("[IdleState] Entrou no estado Idle (estado puro - sem Speed control)");
    }

    public override void UpdateState()
    {
        // ========== BOTÃO DIREITO DO MOUSE (PRIORIDADE MÁXIMA) ==========
        if (player.Mouse.RightMouseButtonDown)
        {
            HandleRightClick();
            return;
        }

        // ========== MOVIMENTO WASD (LEGADO - OPCIONAL) ==========
        Vector3 moveInput = new Vector3(player.Input.horizontalInput, 0, player.Input.verticalInput);
        
        if (moveInput.magnitude > player.MovementThreshold)
        {
            SwitchState(new PlayerMovingState(stateMachine, player));
            return;
        }

        // ========== ATAQUE BÁSICO (LEGADO - OPCIONAL) ==========
        if (player.Input.attackInput)
        {
            SwitchState(new PlayerAttackingState(stateMachine, player));
            return;
        }

        // ========== HABILIDADES (Q, E, R, T, 1-4) ==========
        int abilitySlot = CheckAbilityInputs();
        if (abilitySlot != -1)
        {
            TryUseAbility(abilitySlot);
            return;
        }

        // ========== INTERAÇÃO COM EVENTOS (E) ==========
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

    // Processa clique com botão direito do mouse (estilo MOBA/ARPG)
    private void HandleRightClick()
    {
        // Verifica se clicou em inimigo
        if (player.Mouse.IsMouseOverEnemy(out Character enemyCharacter))
        {
            if (enemyCharacter.Data.IsAlive)
            {
                Debug.Log($"[IdleState] Clicou em inimigo: {enemyCharacter.name}, iniciando perseguição");
                SwitchState(new PlayerMovingToTargetState(stateMachine, player, enemyCharacter));
                return;
            }
        }

        // Verifica se clicou em evento interativo
        if (player.Mouse.IsMouseOverInteractable(out Event eventComponent))
        {
            float distance = Vector3.Distance(player.transform.position, eventComponent.transform.position);
            
            if (distance <= eventComponent.minDistanceToTrigger)
            {
                // Já está na distância, interage imediatamente
                if (player.AutoRotateOnInteract)
                {
                    player.Motor.RotateToPosition(eventComponent.transform.position);
                }
                eventComponent.OnClick();
                return;
            }
            else
            {
                // Precisa se mover até o objeto
                Debug.Log($"[IdleState] Movendo até objeto interativo: {eventComponent.name}");
                SwitchState(new PlayerMovingToTargetState(stateMachine, player, eventComponent.transform.position));
                return;
            }
        }

        // Clicou em chão/terreno - move até posição
        Vector3 destination = player.Mouse.GetMousePosition();
        if (destination != Vector3.zero)
        {
            Debug.Log($"[IdleState] Movendo para posição: {destination}");
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
        
        return -1; // Nenhuma habilidade pressionada
    }

    private void TryUseAbility(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= player.Ability.SkillSlots.Length) return;

        var slot = player.Ability.SkillSlots[slotIndex];
        if (slot == null || slot.AssignedAbility == null) return;
        if (!slot.CanUse()) return;

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
        }
    }

    public override void ExitState()
    {
        Debug.Log("[IdleState] Saiu do estado Idle");
    }

    // ========== Permissões ==========
    public override bool CanMove() => true;
    public override bool CanAttack() => true;
    public override bool CanUseAbility() => true;
    public override bool CanInteract() => true;
    public override bool CanDodge() => true;
}
