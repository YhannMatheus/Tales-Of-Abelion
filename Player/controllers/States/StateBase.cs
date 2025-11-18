[System.Serializable]
public class StateBase
{
    public virtual void EnterState(PlayerManager playerManager)
    {
    }

    public virtual void UpdateState(PlayerManager playerManager)
    {
    }

    public virtual void ExitState(PlayerManager playerManager)
    {
    }


    public virtual bool CanMove => true;
    public virtual bool CanRotate => true;
    public virtual bool CanCasting => true;

}