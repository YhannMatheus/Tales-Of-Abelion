using UnityEngine;

public class IdleState : State
{
    private float idleTimer = 0f;

    public override void EnterState(IAManager ia)
    {
        ia.animator?.SetState(IAState.Idle);
        ia.currentSpeed = 0f;
        ia.moveDirection = Vector3.zero;
        idleTimer = 0f;
        
        Debug.Log($"[IdleState] {ia.CharacterManager.characterType} entrou em Idle");
    }

    public override void UpdateState(IAManager ia)
    {
        idleTimer += Time.deltaTime;
        
        switch (ia.CharacterManager.characterType)
        {
            case CharacterType.Neutral:
                HandleNeutralIdle(ia);
                break;
                
            case CharacterType.Enemy:
                HandleEnemyIdle(ia);
                break;
                
            case CharacterType.Ally:
                HandleAllyIdle(ia);
                break;
        }
    }

    public override void ClearState(IAManager ia)
    {
        ia.currentSpeed = 0f;
        idleTimer = 0f;
        
        Debug.Log($"[IdleState] {ia.CharacterManager.characterType} saiu de Idle");
    }

    private void HandleNeutralIdle(IAManager ia)
    {
        ia.currentSpeed = 0f;
                
        if (ShouldStartPatrol(ia))
        {
            if (ia.CanUseState(IAState.Patrol) && 
                ia.patrolPoints != null && 
                ia.patrolPoints.Length > 0)
            {
                ia.SwitchState(ia.GetStateByIAState(IAState.Patrol));
            }
        }
    }

    private void HandleEnemyIdle(IAManager ia)
    {
        ia.currentSpeed = 0f;
        
        if (ShouldStartPatrol(ia))
        {
            if (ia.CanUseState(IAState.Patrol) && 
                ia.patrolPoints != null && 
                ia.patrolPoints.Length > 0)
            {
                ia.SwitchState(ia.GetStateByIAState(IAState.Patrol));
            }
        }
    }

    private void HandleAllyIdle(IAManager ia)
    {
        if (ia.playerToFollow == null)
        {
            ia.currentSpeed = 0f;
            Debug.LogWarning("[IdleState] Aliado sem player para seguir!");
            return;
        }

        Vector3 targetPosition = ia.playerToFollow.position + ia.followOffset;
        float distanceToTarget = Vector3.Distance(ia.transform.position, targetPosition);

        if (distanceToTarget > ia.maxFollowDistance)
        {
            Vector3 direction = (targetPosition - ia.transform.position).normalized;
            ia.moveDirection = direction;
            ia.currentSpeed = ia.walkSpeed;
            
            ia.controller.Move(ia.moveDirection * ia.currentSpeed * Time.deltaTime);
            ia.RotateTowards(targetPosition, 5f);
        }
        else if (distanceToTarget < ia.minFollowDistance)
        {
            Vector3 direction = (ia.transform.position - targetPosition).normalized;
            ia.moveDirection = direction;
            ia.currentSpeed = ia.walkSpeed * 0.5f;
            
            ia.controller.Move(ia.moveDirection * ia.currentSpeed * Time.deltaTime);
        }
        else
        {
            ia.currentSpeed = 0f;
            ia.moveDirection = Vector3.zero;
            
            ia.RotateTowards(ia.playerToFollow.position, 3f);
        }
    }

    private bool ShouldStartPatrol(IAManager ia)
    {
        if (idleTimer >= ia.stateManager.maxIdleTime)
        {
            return true;
        }
        
        if (idleTimer >= ia.stateManager.minIdleTime)
        {
            float chancePerSecond = 0.1f;
            float chance = chancePerSecond * Time.deltaTime;
            
            if (Random.value < chance)
            {
                return true;
            }
        }
        
        return false;
    }
}