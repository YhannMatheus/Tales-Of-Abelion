public class DeadState : State
{
    public override void EnterState(IAManager ia)
    {
        ia.currentSpeed = 0f;
        ia.moveDirection = UnityEngine.Vector3.zero;

        if (ia.animator != null)
        {
            ia.animator.targetSpeedNormalized = 0f;
            ia.animator.IsDead = true;
        }

        if (ia.controller != null)
        {
            ia.controller.enabled = false;
        }

        UnityEngine.Debug.Log($"[DeadState] {ia.Data.characterName} entrou no estado de morte");
    }

    public override void UpdateState(IAManager ia)
    {
        if (ia.IsAlive)
        {
            UnityEngine.Debug.Log($"[DeadState] {ia.Data.characterName} foi revivido! Saindo do DeadState");
            ExitDeadState(ia);
        }
    }

    public override void ClearState(IAManager ia)
    {
        if (ia.animator != null)
        {
            ia.animator.IsDead = false;
        }

        if (ia.controller != null)
        {
            ia.controller.enabled = true;
        }

        UnityEngine.Debug.Log($"[DeadState] {ia.Data.characterName} saiu do estado de morte");
    }

    private void ExitDeadState(IAManager ia)
    {
        State idleState = ia.GetStateByIAState(IAState.Idle);
        if (idleState != null && ia.CanUseState(IAState.Idle))
        {
            ia.SwitchState(idleState);
        }
    }
}