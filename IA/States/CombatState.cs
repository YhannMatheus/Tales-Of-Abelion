
public class CombatState : State
{
    private float _lastAttackCheckTime;

    public override void EnterState(IAManager ia)
    {
        ia.currentSpeed = 0f;
        ia.animator.targetSpeedNormalized = 0f;
        _lastAttackCheckTime = UnityEngine.Time.time;
        
        UnityEngine.Debug.Log($"[CombatState] {ia.Data.characterName} entrou em combate");
    }

    public override void UpdateState(IAManager ia)
    {
        if (!ia.IsAlive || ia.currentTarget == null)
        {
            HandleNoTarget(ia);
            return;
        }

        if (!HasValidTarget(ia))
        {
            HandleNoTarget(ia);
            return;
        }
        ia.RotateTowards(ia.currentTarget.position, 10f);

        float distanceToTarget = UnityEngine.Vector3.Distance(ia.transform.position, ia.currentTarget.position);
        
        if (distanceToTarget > ia.attackRange)
        {
            State chaseState = ia.GetStateByIAState(IAState.Chase);
            if (chaseState != null && ia.CanUseState(IAState.Chase))
            {
                ia.SwitchState(chaseState);
                return;
            }
        }

        if (UnityEngine.Time.time >= _lastAttackCheckTime + ia.stateManager.attackCheckInterval)
        {
            _lastAttackCheckTime = UnityEngine.Time.time;
            TryCombat(ia, distanceToTarget);
        }
    }

    public override void ClearState(IAManager ia)
    {
        _lastAttackCheckTime = 0f;
        UnityEngine.Debug.Log($"[CombatState] {ia.Data.characterName} saiu de combate");
    }

    private bool HasValidTarget(IAManager ia)
    {
        if (ia.currentTarget == null) return false;

        Character targetCharacter = ia.currentTarget.GetComponent<Character>();
        if (targetCharacter != null)
        {
            return targetCharacter.Data.IsAlive;
        }

        IAManager targetIA = ia.currentTarget.GetComponent<IAManager>();
        if (targetIA != null)
        {
            return targetIA.IsAlive;
        }

        return false;
    }

    private void TryCombat(IAManager ia, float distanceToTarget)
    {
        if (ia.skillManager == null) return;

        Character targetChar = ia.currentTarget != null ? ia.currentTarget.GetComponent<Character>() : null;
        if (targetChar == null) return;

        SkillSlot highestDamageSkill = ia.skillManager.GetHighestDamageSkill(distanceToTarget, includeBasicAttack: true);

        if (highestDamageSkill == null)
        {
            UnityEngine.Debug.Log($"[CombatState] {ia.Data.characterName} não tem skills disponíveis");
            return;
        }

        bool isBasicAttack = highestDamageSkill == ia.skillManager.BasicAttackSlot;

        if (isBasicAttack)
        {
            if (ia.skillManager.TryUseBasicAttack(targetChar))
            {
                UnityEngine.Debug.Log($"[CombatState] {ia.Data.characterName} usou ataque básico");
            }
        }
        else
        {
            float randomChance = UnityEngine.Random.value;

            if (randomChance < ia.stateManager.basicAttackBias && ia.skillManager.BasicAttackSlot != null)
            {
                if (ia.skillManager.TryUseBasicAttack(targetChar))
                {
                    UnityEngine.Debug.Log($"[CombatState] {ia.Data.characterName} escolheu ataque básico (bias)");
                    return;
                }
            }

            bool skillUsed = ia.skillManager.TryUseSkill(highestDamageSkill, targetChar);
            
            if (skillUsed)
            {
                UnityEngine.Debug.Log($"[CombatState] {ia.Data.characterName} usou skill: {highestDamageSkill.AssignedSkill.skillName}");
            }
            else if (ia.skillManager.BasicAttackSlot != null && ia.skillManager.TryUseBasicAttack(targetChar))
            {
                UnityEngine.Debug.Log($"[CombatState] {ia.Data.characterName} usou ataque básico (fallback)");
            }
        }
    }

    private void HandleNoTarget(IAManager ia)
    {
        if (ia.iaType == IaType.Enemy || ia.iaType == IaType.Neutral)
        {
            if (ia.CanUseState(IAState.Patrol) && ia.patrolPoints != null && ia.patrolPoints.Length > 0)
            {
                State patrolState = ia.GetStateByIAState(IAState.Patrol);
                if (patrolState != null)
                {
                    ia.SwitchState(patrolState);
                    return;
                }
            }
        }

        State idleState = ia.GetStateByIAState(IAState.Idle);
        if (idleState != null && ia.CanUseState(IAState.Idle))
        {
            ia.SwitchState(idleState);
        }
    }
}