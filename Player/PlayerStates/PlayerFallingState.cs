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
    private bool debugLogs = false;

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
            Debug.Log($"[FallingState] ✈️ ENTROU NO ESTADO DE QUEDA | IsGrounded = {player.Motor.IsGrounded} | Y Velocity = {player.transform.position.y:F2}");
        }
    }

    public override void UpdateState()
    {
        // ✅ CORRIGIDO: Usa o valor REAL do Motor.IsGrounded ao invés de forçar false
        // Isso permite que o Animator detecte quando tocou no chão ANTES de mudar de estado
        bool motorGrounded = player.Motor.IsGrounded;
        
        Debug.Log($"[FallingState] 🔄 UPDATE CHAMADO | Motor.IsGrounded = {motorGrounded} | Y Pos = {player.transform.position.y:F2}");
        
        player.Animator?.UpdateMovementSpeed(0f, motorGrounded);

        // Verifica se tocou no chão para sair do estado
        if (motorGrounded)
        {
            // Tocou no chão - volta para Idle
            Debug.Log($"[FallingState] ✅ DETECTOU CHÃO - TENTANDO MUDAR PARA IDLE | Y Pos = {player.transform.position.y:F2}");
            SwitchState(new PlayerIdleState(stateMachine, player));
            Debug.Log($"[FallingState] ✅ SwitchState CHAMADO");
        }
        else
        {
            Debug.Log($"[FallingState] ⏳ AINDA NO AR | IsGrounded = {motorGrounded}");
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
