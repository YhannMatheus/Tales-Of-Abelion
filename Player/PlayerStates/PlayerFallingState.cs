using UnityEngine;

// ========================================
// PlayerFallingState - Estado de Queda do Player
// ========================================
// Estado ativo quando o personagem está no ar (isGrounded = false)
// - Bloqueia TODAS as ações (movimento, ataque, habilidades, interação)
// - Aplica gravidade via PlayerMotor
// - Retorna automaticamente para Idle quando tocar no chão
// ========================================

public class PlayerFallingState : PlayerStateBase
{
    private bool debugLogs = true;

    public PlayerFallingState(PlayerStateMachine stateMachine, PlayerManager player) 
        : base(stateMachine, player)
    {
    }

    public override void EnterState()
    {
        // Para o movimento horizontal
        player.Motor.Stop();

        // Define animação de queda
        player.Animator?.SetFallingState();

        if (debugLogs)
        {
            Debug.Log("[FallingState] Personagem está caindo - ações bloqueadas");
        }
    }

    public override void UpdateState()
    {
        // Aplica gravidade (PlayerMotor já faz isso no ApplyGravity)
        // Apenas verifica se tocou no chão para sair do estado

        if (player.Motor.IsGrounded)
        {
            // Tocou no chão - volta para Idle
            if (debugLogs)
            {
                Debug.Log("[FallingState] Tocou no chão - retornando para Idle");
            }
            SwitchState(new PlayerIdleState(stateMachine, player));
        }
    }

    public override void ExitState()
    {
        if (debugLogs)
        {
            Debug.Log("[FallingState] Saindo do estado de queda");
        }
    }

    // ========== Permissões de Ações (TODAS BLOQUEADAS) ==========
    public override bool CanMove() => false;
    public override bool CanAttack() => false;
    public override bool CanUseAbility() => false;
    public override bool CanInteract() => false;
    public override bool CanDodge() => false;
}
