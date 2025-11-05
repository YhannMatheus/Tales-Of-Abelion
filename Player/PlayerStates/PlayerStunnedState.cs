using UnityEngine;

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

        player.Motor.Stop();

        // Define estado de stun (bool, não trigger)
        player.Animator?.SetStunned(true);

        Debug.Log($"[StunnedState] Player atordoado por {stunDuration}s");
    }

    public override void UpdateState()
    {
        stunTimer += Time.deltaTime;

        if (stunTimer >= stunDuration)
        {
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
        // Remove estado de stun
        player.Animator?.SetStunned(false);
        
        Debug.Log("[StunnedState] Player recuperou do atordoamento");
    }

    // ========== Permissões ==========
    public override bool CanMove() => false;
    public override bool CanAttack() => false;
    public override bool CanUseAbility() => false;
    public override bool CanInteract() => false;
    public override bool CanDodge() => false;
}
