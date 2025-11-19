using UnityEngine;

/// <summary>
/// Estado de morte
/// - Impede todas as ações
/// - Permanece até que seja ressuscitado externamente ou jogo seja reiniciado
/// - Pode ser expandido para incluir lógica de respawn, penalidades, etc.
/// </summary>
public class DeadState : StateBase
{
    public override void EnterState(PlayerManager playerManager)
    {
        // Para qualquer movimento
        if (playerManager != null && playerManager._playerMotor != null)
        {
            playerManager._playerMotor.Stop();
        }

        // Aqui você pode adicionar:
        // - Trigger de animação de morte
        // - Desabilitar colliders
        // - Iniciar timer de respawn
        // - Mostrar UI de morte
        // - etc.
    }

    public override void UpdateState(PlayerManager playerManager)
    {
        if (playerManager == null) return;

        // Estado morto não faz nada sozinho
        // Precisa ser trocado externamente (por sistema de respawn, por exemplo)

        // Exemplo de auto-respawn após X segundos (descomente se quiser):
        // respawnTimer += Time.deltaTime;
        // if (respawnTimer >= respawnDelay)
        // {
        //     Respawn(playerManager);
        // }
    }

    public override void ExitState(PlayerManager playerManager)
    {
        // Ao sair do estado morto (respawn):
        // - Restaurar vida
        // - Reabilitar colliders
        // - Trigger animação de idle
        // - etc.
    }

    // Morte impede todas as ações
    public override bool CanMove => false;
    public override bool CanRotate => false;
    public override bool CanCasting => false;
}
