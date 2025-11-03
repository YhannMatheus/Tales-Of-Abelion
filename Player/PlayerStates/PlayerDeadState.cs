using UnityEngine;

/// <summary>
/// Estado Dead - Player morto.
/// Desabilita TODOS os inputs e aguarda respawn.
/// 
/// Responsabilidades:
/// - Desabilitar inputs do player (via permissions)
/// - Desabilitar física (CharacterController)
/// - Aguardar evento de revive para transição
/// 
/// Fluxo:
/// 1. Character.Die() dispara OnDeath event
/// 2. PlayerManager.OnCharacterDeath() transiciona para DeadState
/// 3. DeadState.EnterState() desabilita inputs/física
/// 4. CheckpointManager.RespawnPlayer() chama Character.Revive()
/// 5. Character.Revive() dispara OnRevive event
/// 6. PlayerManager.OnCharacterRevive() transiciona para IdleState
/// 7. DeadState.ExitState() reabilita física
/// </summary>
public class PlayerDeadState : PlayerStateBase
{
    private bool deathAnimationPlayed = false;

    public PlayerDeadState(PlayerStateMachine stateMachine, PlayerManager player) 
        : base(stateMachine, player)
    {
    }

    public override void EnterState()
    {
        // Para todo movimento
        player.Motor.Stop();

        // Desabilita controle do CharacterController (não pode mais se mover)
        var characterController = player.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        // A animação de morte é disparada automaticamente pelo CharacterAnimatorController
        // via evento Character.OnDeath (HandleDeath() já configura isDeath=true)

        deathAnimationPlayed = true;

        Debug.Log("[DeadState] Player morreu! Entrou no estado Dead");
    }

    public override void UpdateState()
    {
        // Estado de morte não faz nada - aguarda respawn externo
        // A transição para Idle só acontece quando Character.Revive() for chamado
    }

    public override void ExitState()
    {
        // Reabilita CharacterController ao reviver
        var characterController = player.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = true;
        }

        Debug.Log("[DeadState] Player reviveu! Saindo do estado Dead");
    }

    // ========== Permissões ==========
    // Morto: NÃO pode fazer NADA

    public override bool CanMove() => false;
    public override bool CanAttack() => false;
    public override bool CanUseAbility() => false;
    public override bool CanInteract() => false;
    public override bool CanDodge() => false;
}
