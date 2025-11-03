using UnityEngine;

public abstract class PlayerStateBase
{
    protected PlayerStateMachine stateMachine;
    protected PlayerManager player;
    public PlayerStateBase(PlayerStateMachine stateMachine, PlayerManager player)
    {
        this.stateMachine = stateMachine;
        this.player = player;
    }

    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();

    // ========== Permissões de Ações ==========
    public virtual bool CanMove() => false;
    public virtual bool CanAttack() => false;
    public virtual bool CanUseAbility() => false;
    public virtual bool CanInteract() => false;
    public virtual bool CanDodge() => false;

    // ========== Métodos Auxiliares ==========
    protected void SwitchState(PlayerStateBase newState)
    {
        stateMachine.SwitchState(newState);
    }
    public virtual string GetStateName()
    {
        return GetType().Name;
    }
}
