using UnityEngine;

public class PlayerMovingState : PlayerStateBase
{
    public PlayerMovingState(PlayerStateMachine stateMachine, PlayerManager player) : base(stateMachine, player){}

    public override void EnterState()
    {
        // Define estado de animação como Moving
        // MovingState controla TODAS as animações de movimento via Speed (0.0-1.0)
        player.Animator?.SetMovingState();
        
        Debug.Log("[MovingState] Entrou no estado Moving - controlando Speed do blend tree");
    }

    public override void UpdateState()
    {
        // ========== BOTÃO DIREITO DO MOUSE (NOVO DESTINO/ALVO) ==========
        if (player.Mouse.RightMouseButtonDown)
        {
            HandleRightClick();
            return;
        }

        // ⚠️ MOVIMENTO WASD REMOVIDO - Agora usa apenas clique do mouse
        // Se chegou aqui sem destino, volta para Idle
        if (!player.Motor.HasDestination)
        {
            SwitchState(new PlayerIdleState(stateMachine, player));
            return;
        }

        // ========== HABILIDADES ==========
        int abilitySlot = CheckAbilityInputs();
        if (abilitySlot != -1)
        {
            TryUseAbility(abilitySlot);
            return;
        }

        // ========== INTERAÇÃO ==========
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
        // Zera velocidade ao sair - garante que blend tree volte para threshold 0.0
        // Isso faz transição suave para Idle dentro do próprio blend tree
        player.Animator?.UpdateMovementSpeed(0f, player.Motor.IsGrounded);
        player.Motor.Stop();
        
        Debug.Log("[MovingState] Saiu do estado Moving - Speed zerado para transição suave");
    }

    // Processa clique com botão direito do mouse (cancela movimento WASD)
    private void HandleRightClick()
    {
        // Verifica se clicou em inimigo
        if (player.Mouse.IsMouseOverEnemy(out CharacterManager enemyCharacter))
        {
            if (enemyCharacter.Data.IsAlive)
            {
                Debug.Log($"[MovingState] Cancelou movimento WASD, atacando: {enemyCharacter.name}");
                SwitchState(new PlayerMovingToTargetState(stateMachine, player, enemyCharacter));
                return;
            }
        }

        // Clicou em chão - troca para movimento por clique
        Vector3 destination = player.Mouse.GetMousePosition();
        if (destination != Vector3.zero)
        {
            Debug.Log("[MovingState] Trocou para movimento por clique");
            SwitchState(new PlayerMovingToTargetState(stateMachine, player, destination));
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
        
        return -1; // Nenhuma habilidade pressionada
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
    public override bool CanInteract() => true;
    public override bool CanDodge() => true;
}
