[System.Serializable]
public abstract class StateBase
{
    public abstract void EnterState(PlayerManager playerManager);

    public abstract void UpdateState(PlayerManager playerManager);

    public abstract void ExitState(PlayerManager playerManager);


    public virtual bool CanMove => true;
    public virtual bool CanRotate => true;
    public virtual bool CanCasting => true;

}