using UnityEngine;

/// <summary>
/// Estado de stun/atordoamento
/// - Impede movimento, rotação e uso de skills
/// - Retorna para Idle após duração do stun
/// </summary>
public class StunState : StateBase
{
    private float stunDuration;
    private float stunTimer = 0f;

    public StunState(float duration)
    {
        this.stunDuration = Mathf.Max(0.1f, duration);
    }

    public override void EnterState(PlayerManager playerManager)
    {
        // Para qualquer movimento ao entrar em stun
        if (playerManager != null && playerManager._playerMotor != null)
        {
            playerManager._playerMotor.Stop();
        }

        stunTimer = 0f;
    }

    public override void UpdateState(PlayerManager playerManager)
    {
        if (playerManager == null) return;

        stunTimer += Time.deltaTime;

        // Quando o stun acabar, volta para Idle
        if (stunTimer >= stunDuration)
        {
            playerManager._playerStateMachine.SwitchState(new IdleState());
        }
    }

    public override void ExitState(PlayerManager playerManager)
    {
        // Nada especial ao sair
    }

    // Stun impede todas as ações
    public override bool CanMove => false;
    public override bool CanRotate => false;
    public override bool CanCasting => false;
}
