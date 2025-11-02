public class FleeState : State
{
    private float _fleeStartTime;
    private UnityEngine.Vector3 _fleeDirection;
    private UnityEngine.Vector3 _fleeTargetPosition;
    private bool _hasReachedSafety;

    public override void EnterState(IAManager ia)
    {
        _fleeStartTime = UnityEngine.Time.time;
        _hasReachedSafety = false;

        if (ia.iaType == IaType.Ally)
        {
            HandleAllyFlee(ia);
        }
        else
        {
            HandleEnemyNeutralFlee(ia);
        }

        ia.currentSpeed = ia.runSpeed;
        ia.animator.targetSpeedNormalized = 1.0f;

        UnityEngine.Debug.Log($"[FleeState] {ia.Data.characterName} ({ia.iaType}) iniciou fuga! HP: {ia.Data.currentHealth}/{ia.Data.TotalMaxHealth}");
    }

    public override void UpdateState(IAManager ia)
    {
        if (!ia.IsAlive)
        {
            ReturnToIdle(ia);
            return;
        }

        if (ia.iaType == IaType.Ally)
        {
            UpdateAllyFlee(ia);
        }
        else
        {
            UpdateEnemyNeutralFlee(ia);
        }
    }

    public override void ClearState(IAManager ia)
    {
        ia.currentSpeed = 0f;
        ia.moveDirection = UnityEngine.Vector3.zero;
        ia.animator.targetSpeedNormalized = 0f;

        UnityEngine.Debug.Log($"[FleeState] {ia.Data.characterName} saiu da fuga. HP: {ia.Data.currentHealth}/{ia.Data.TotalMaxHealth}");
    }

    private void HandleAllyFlee(IAManager ia)
    {
        if (ia.playerToFollow == null)
        {
            UnityEngine.Debug.LogWarning($"[FleeState] Aliado {ia.Data.characterName} não tem playerToFollow configurado!");
            HandleEnemyNeutralFlee(ia);
            return;
        }

        _fleeTargetPosition = ia.playerToFollow.position;
        _fleeDirection = (_fleeTargetPosition - ia.transform.position).normalized;

        UnityEngine.Debug.Log($"[FleeState] Aliado {ia.Data.characterName} fugindo em direção ao player");
    }

    private void HandleEnemyNeutralFlee(IAManager ia)
    {
        if (ia.currentTarget != null)
        {
            _fleeDirection = (ia.transform.position - ia.currentTarget.position).normalized;
        }
        else
        {
            _fleeDirection = -ia.transform.forward;
        }

        _fleeDirection.y = 0f;
        _fleeDirection.Normalize();

        _fleeTargetPosition = ia.transform.position + (_fleeDirection * ia.stateManager.fleeDistance);

        UnityEngine.Debug.Log($"[FleeState] {ia.iaType} {ia.Data.characterName} fugindo da luta");
    }

    private void UpdateAllyFlee(IAManager ia)
    {
        if (ia.playerToFollow == null)
        {
            ReturnToIdle(ia);
            return;
        }

        float distanceToPlayer = UnityEngine.Vector3.Distance(ia.transform.position, ia.playerToFollow.position);

        if (distanceToPlayer <= ia.stateManager.allyReachDistance)
        {
            UnityEngine.Debug.Log($"[FleeState] Aliado {ia.Data.characterName} alcançou o player");
            ReturnToIdle(ia);
            return;
        }

        _fleeTargetPosition = ia.playerToFollow.position;
        _fleeDirection = (_fleeTargetPosition - ia.transform.position).normalized;
        _fleeDirection.y = 0f;

        MoveTowardsTarget(ia);
    }

    private void UpdateEnemyNeutralFlee(IAManager ia)
    {
        float timeElapsed = UnityEngine.Time.time - _fleeStartTime;

        float healthPercent = (float)ia.Data.currentHealth / ia.Data.TotalMaxHealth;

        if (healthPercent >= ia.stateManager.healthRegenTarget && _hasReachedSafety)
        {
            UnityEngine.Debug.Log($"[FleeState] {ia.Data.characterName} recuperou vida suficiente ({healthPercent * 100:F0}%)");
            ReturnToIdle(ia);
            return;
        }

        if (timeElapsed >= ia.stateManager.fleeDuration)
        {
            _hasReachedSafety = true;
        }

        if (!_hasReachedSafety)
        {
            MoveTowardsTarget(ia);
        }
        else
        {
            ia.currentSpeed = 0f;
            ia.moveDirection = UnityEngine.Vector3.zero;
            ia.animator.targetSpeedNormalized = 0f;

            if (healthPercent >= ia.stateManager.healthRegenTarget)
            {
                ReturnToIdle(ia);
            }
        }
    }

    private void MoveTowardsTarget(IAManager ia)
    {
        ia.RotateTowards(_fleeTargetPosition, 10f);

        ia.moveDirection = _fleeDirection;
        ia.currentSpeed = ia.runSpeed;

        UnityEngine.Vector3 movement = ia.moveDirection * ia.currentSpeed * UnityEngine.Time.deltaTime;
        ia.controller?.Move(movement);

        ia.animator.targetSpeedNormalized = 1.0f;

        UnityEngine.Debug.DrawLine(ia.transform.position + UnityEngine.Vector3.up, _fleeTargetPosition + UnityEngine.Vector3.up, UnityEngine.Color.cyan);
    }

    private void ReturnToIdle(IAManager ia)
    {
        ia.currentTarget = null;

        State idleState = ia.GetStateByIAState(IAState.Idle);
        if (idleState != null && ia.CanUseState(IAState.Idle))
        {
            ia.SwitchState(idleState);
        }
    }
}