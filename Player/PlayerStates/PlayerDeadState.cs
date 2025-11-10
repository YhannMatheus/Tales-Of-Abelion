using UnityEngine;

public class PlayerDeadState : PlayerStateBase
{
    public PlayerDeadState(PlayerStateMachine stateMachine, PlayerManager player) 
        : base(stateMachine, player)
    {
    }

    public override void EnterState()
    {
        player.Motor.Stop();

        var characterController = player.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        
        Debug.Log("[DeadState] Player morreu! Entrou no estado Dead");
    }

    public override void UpdateState()
    {
        // Estado de morte não faz nada - aguarda respawn externo
        // A transição para Idle só acontece quando CharacterManager.Revive() for chamado
        // ainda vou criar um evento para isso
    }

    public override void ExitState()
    {
        var characterController = player.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = true;
        }

        
        Debug.Log("[DeadState] Player reviveu! Saindo do estado Dead");
    }

    // ========== Permissões ==========
    public override bool CanMove() => false;
    public override bool CanAttack() => false;
    public override bool CanUseAbility() => false;
    public override bool CanInteract() => false;
    public override bool CanDodge() => false;
}
