using UnityEngine;

public class ChaseState : State
{
    public override void EnterState(IAManager ia)
    {
        ia.animator?.SetState(IAState.Chase);
        ia.currentSpeed = ia.runSpeed;
        
        Debug.Log($"[ChaseState] {ia.iaType} começou perseguição do alvo: {ia.currentTarget?.name}");
    }

    public override void UpdateState(IAManager ia)
    {
        if (!HasValidTarget(ia))
        {
            HandleNoTarget(ia);
            return;
        }

        if (ia.IsInAttackRange(ia.currentTarget))
        {
            SwitchToAttack(ia);
            return;
        }

        ChaseTarget(ia);
    }

    public override void ClearState(IAManager ia)
    {
        ia.animator?.ExitState(IAState.Chase);
        ia.currentSpeed = 0f;
        ia.moveDirection = Vector3.zero;
        
        if (ia.animator != null)
        {
            ia.animator.targetSpeedNormalized = 0f;
        }
        
        Debug.Log($"[ChaseState] {ia.iaType} parou de perseguir");
    }

    private bool HasValidTarget(IAManager ia)
    {
        if (ia.currentTarget == null)
        {
            return false;
        }

        Character targetChar = ia.currentTarget.GetComponent<Character>();
        if (targetChar != null)
        {
            if (!targetChar.Data.IsAlive)
            {
                return false;
            }
        }

        IAManager targetIA = ia.currentTarget.GetComponent<IAManager>();
        if (targetIA != null)
        {
            if (!targetIA.IsAlive)
            {
                return false;
            }
        }

        if (ia.detectSystem != null && ia.detectSystem.currentTarget != ia.currentTarget)
        {
            return false;
        }

        return true;
    }

    private void ChaseTarget(IAManager ia)
    {
        Vector3 targetPosition = ia.currentTarget.position;
        Vector3 currentPosition = ia.transform.position;

        Vector3 direction = targetPosition - currentPosition;
        direction.y = 0f;
        
        direction.Normalize();

        ia.RotateTowards(targetPosition, 8f);

        ia.moveDirection = direction;
        ia.currentSpeed = ia.runSpeed;
        Vector3 movement = ia.moveDirection * ia.currentSpeed * Time.deltaTime;
        ia.controller?.Move(movement);

        if (ia.animator != null)
        {
            ia.animator.targetSpeedNormalized = 1.0f;
        }

        Debug.DrawLine(currentPosition + Vector3.up, targetPosition + Vector3.up, Color.red);
    }

    private void HandleNoTarget(IAManager ia)
    {
        Debug.Log($"[ChaseState] {ia.iaType} perdeu alvo válido");

        ia.currentTarget = null;

        if (ia.CanUseState(IAState.Patrol) && ia.patrolPoints != null && ia.patrolPoints.Length > 0)
        {
            ia.SwitchState(ia.GetStateByIAState(IAState.Patrol));
        }
        else if (ia.CanUseState(IAState.Idle))
        {
            ia.SwitchState(ia.GetStateByIAState(IAState.Idle));
        }
    }

    private void SwitchToAttack(IAManager ia)
    {
        Debug.Log($"[ChaseState] {ia.iaType} alcançou o alvo! Mudando para Attack");

        if (ia.CanUseState(IAState.Attack))
        {
            ia.SwitchState(ia.GetStateByIAState(IAState.Attack));
        }
        else
        {
            Debug.LogWarning($"[ChaseState] {ia.iaType} não pode usar AttackState!");
        }
    }
}