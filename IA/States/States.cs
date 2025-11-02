using UnityEngine;

public abstract class State
{
    public abstract void EnterState(IAManager ia);

    public abstract void UpdateState(IAManager ia);

    public abstract void ClearState(IAManager ia);
}