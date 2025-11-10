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

    }

    public override void UpdateState()
    {
        // ========== BOTÃO DIREITO DO MOUSE (MOVIMENTO/ATAQUE) ==========
        // Detecta tanto clique quanto botão segurado
        if (player.Mouse.RightMouseButtonDown || player.Mouse.RightMouseButtonHeld)
        {
            HandleRightClick();
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
        if (player.Mouse.IsMouseOverEnemy(out CharacterManager enemyCharacter))
        {
            if (enemyCharacter.Data.IsAlive)
            {
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
                SwitchState(new PlayerMovingToTargetState(stateMachine, player, eventComponent.transform.position));
                return;
            }
        }

        // Clicou em chão/terreno - move até posição
        Vector3 destination = player.Mouse.GetMousePosition();
        if (destination != Vector3.zero)
        {
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
