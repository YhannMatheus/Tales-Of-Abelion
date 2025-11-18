using UnityEngine;


[System.Serializable]
public class PlayerStateMachine
{
    public StateBase currentState;
    private PlayerManager playerManager;

    public void UpdateState()
    {
        if (currentState != null)
        {
            currentState.UpdateState(playerManager);
        }
    }

    public void SwitchState(StateBase newState)
    {
        if (currentState != null)
        {
            currentState.ExitState(playerManager);
        }
        currentState = newState;
        currentState.EnterState(playerManager);
    }

    public void SetPlayerManager(PlayerManager manager)
    {
        playerManager = manager;
    }
}