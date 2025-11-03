using UnityEngine;

/// <summary>
/// Estado Stunned - Player atordoado/incapacitado.
/// Não pode fazer NADA durante a duração do stun.
/// </summary>
public class PlayerStunnedState : PlayerStateBase
{
    private float stunDuration;
    private float stunTimer = 0f;

    public PlayerStunnedState(PlayerStateMachine stateMachine, PlayerManager player, float duration) 
        : base(stateMachine, player)
    {
        stunDuration = Mathf.Clamp(duration, player.MinStunDuration, player.MaxStunDuration);
    }

    public override void EnterState()
    {
        stunTimer = 0f;

        // Para todo movimento
        player.Motor.Stop();

        // Toca animação de stun/hit
        player.Animator?.TriggerHit();

        Debug.Log($"[StunnedState] Player atordoado por {stunDuration}s");
    }

    public override void UpdateState()
    {
        stunTimer += Time.deltaTime;

        // Quando o stun acabar, volta para Idle
        if (stunTimer >= stunDuration)
        {
            // Verifica se há input de movimento para ir direto para Moving
            Vector3 moveInput = new Vector3(player.Input.horizontalInput, 0, player.Input.verticalInput);
            
            if (moveInput.magnitude > player.MovementThreshold)
            {
                SwitchState(new PlayerMovingState(stateMachine, player));
            }
            else
            {
                SwitchState(new PlayerIdleState(stateMachine, player));
            }
        }
    }

    public override void ExitState()
    {
        Debug.Log("[StunnedState] Player recuperou do atordoamento");
    }

    // ========== Permissões ==========
    // Stunned: NÃO pode fazer NADA

    public override bool CanMove() => false;
    public override bool CanAttack() => false;
    public override bool CanUseAbility() => false;
    public override bool CanInteract() => false;
    public override bool CanDodge() => false;
}
