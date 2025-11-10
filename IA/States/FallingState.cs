using UnityEngine;

// ========================================
// FallingState - Estado de Queda da IA
// ========================================
// Estado ativo quando a IA está no ar (isGrounded = false)
// - Bloqueia TODAS as ações (movimento, ataque, patrulha, perseguição)
// - Aplica gravidade
// - Retorna automaticamente para Idle quando tocar no chão
// ========================================

public class FallingState : State
{
    private bool debugLogs = true;

    public override void EnterState(IAManager ia)
    {
        // Para o movimento horizontal
        ia.currentSpeed = 0f;
        ia.moveDirection = Vector3.zero;

        // Define animação de queda (usa idle por enquanto)
        ia.animator?.SetState(IAState.Falling);

        if (debugLogs)
        {
            Debug.Log($"[FallingState] {ia.CharacterManager.characterType} está caindo - ações bloqueadas");
        }
    }

    public override void UpdateState(IAManager ia)
    {
        // Verifica se tocou no chão para sair do estado
        // CharacterController.isGrounded é atualizado automaticamente
        
        if (ia.controller.isGrounded)
        {
            // Tocou no chão - volta para Idle
            if (debugLogs)
            {
                Debug.Log($"[FallingState] {ia.CharacterManager.characterType} tocou no chão - retornando para Idle");
            }
            ia.SwitchState(ia.GetStateByIAState(IAState.Idle));
        }
    }

    public override void ClearState(IAManager ia)
    {
        ia.currentSpeed = 0f;
        ia.moveDirection = Vector3.zero;

        if (debugLogs)
        {
            Debug.Log($"[FallingState] {ia.CharacterManager.characterType} saiu do estado de queda");
        }
    }
}
