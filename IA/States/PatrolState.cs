using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : State
{
    private bool hasReachedPoint = false;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    public override void EnterState(IAManager ia)
    {
        if (ia.CharacterManager.characterType == CharacterType.Ally)
        {
            Debug.LogWarning($"[PatrolState] Aliados não devem usar estado de patrulha!");
            
            if (ia.CanUseState(IAState.Idle))
            {
                ia.SwitchState(ia.GetStateByIAState(IAState.Idle));
            }
            return;
        }

        ia.animator?.SetState(IAState.Patrol);
        ia.currentSpeed = ia.walkSpeed;
        
        hasReachedPoint = false;
        isWaiting = false;
        waitTimer = 0f;
        
        Debug.Log($"[PatrolState] {ia.CharacterManager.characterType} iniciou patrulha no ponto {ia.currentPatrolIndex}");
    }

    public override void UpdateState(IAManager ia)
    {
        if (ia.patrolPoints == null || ia.patrolPoints.Length == 0)
        {
            Debug.LogWarning("[PatrolState] Sem pontos de patrulha definidos! Mudando para Idle.");
            
            if (ia.CanUseState(IAState.Idle))
            {
                ia.SwitchState(ia.GetStateByIAState(IAState.Idle));
            }
            return;
        }

        if (isWaiting)
        {
            HandleWaiting(ia);
            return;
        }

        MoveTowardsPatrolPoint(ia);
    }

    public override void ClearState(IAManager ia)
    {
        ia.animator?.ExitState(IAState.Patrol);
        ia.currentSpeed = 0f;
        ia.moveDirection = Vector3.zero;
        
        hasReachedPoint = false;
        isWaiting = false;
        waitTimer = 0f;
        
        Debug.Log($"[PatrolState] {ia.CharacterManager.characterType} saiu de Patrol");
    }

    private void MoveTowardsPatrolPoint(IAManager ia)
    {
        Vector3 targetPoint = ia.patrolPoints[ia.currentPatrolIndex];
        Vector3 currentPosition = ia.transform.position;
        
        Vector3 direction = targetPoint - currentPosition;
        direction.y = 0f;
        
        float distanceToPoint = direction.magnitude;

        if (distanceToPoint <= ia.patrolPointRadius)
        {
            if (!hasReachedPoint)
            {
                OnReachPatrolPoint(ia);
            }
            return;
        }

        direction.Normalize();
        
        ia.RotateTowards(targetPoint, 5f);
        
        ia.moveDirection = direction;
        Vector3 movement = ia.moveDirection * ia.currentSpeed * Time.deltaTime;
        ia.controller?.Move(movement);
        
        if (ia.animator != null)
        {
            ia.animator.targetSpeedNormalized = Mathf.Clamp01(ia.currentSpeed / ia.runSpeed);
        }
    }

    private void OnReachPatrolPoint(IAManager ia)
    {
        hasReachedPoint = true;
        
        Debug.Log($"[PatrolState] {ia.CharacterManager.characterType} chegou no ponto {ia.currentPatrolIndex}");
        
        ia.currentSpeed = 0f;
        ia.moveDirection = Vector3.zero;
        
        if (ia.animator != null)
        {
            ia.animator.targetSpeedNormalized = 0f;
        }
        
        if (ia.CanUseState(IAState.Idle))
        {
            ia.SwitchState(ia.GetStateByIAState(IAState.Idle));
        }
        else
        {
            isWaiting = true;
            waitTimer = 0f;
        }
    }

    private void HandleWaiting(IAManager ia)
    {
        waitTimer += Time.deltaTime;
        
        ia.currentSpeed = 0f;
        ia.moveDirection = Vector3.zero;
        
        if (ia.animator != null)
        {
            ia.animator.targetSpeedNormalized = 0f;
        }

        if (waitTimer >= ia.waitTimeAtPoint)
        {
            MoveToNextPatrolPoint(ia);
            isWaiting = false;
            hasReachedPoint = false;
            waitTimer = 0f;
        }
    }

    private void MoveToNextPatrolPoint(IAManager ia)
    {
        ia.currentPatrolIndex++;
        
        if (ia.currentPatrolIndex >= ia.patrolPoints.Length)
        {
            ia.currentPatrolIndex = 0;
        }
        
        Debug.Log($"[PatrolState] {ia.CharacterManager.characterType} indo para próximo ponto: {ia.currentPatrolIndex}");
        
        ia.currentSpeed = ia.walkSpeed;
    }
}