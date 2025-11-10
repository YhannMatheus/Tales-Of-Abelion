using UnityEngine;

public class StateManager : MonoBehaviour
{
    [Header("Estados Disponíveis")]
    public IAState activeStates;

    [Header("Estado Inicial")]
    public IAState initialState = IAState.Idle;

    [Header("Configurações Idle State")]
    public float minIdleTime = 2f;
    public float maxIdleTime = 5f;

    [Header("Configurações Combat State")]
    public float attackCheckInterval = 0.3f;
    [Range(0f, 1f)]
    public float basicAttackBias = 0.6f;

    [Header("Configurações Flee State")]
    public float fleeDuration = 5f;
    public float fleeDistance = 15f;
    [Range(0f, 1f)]
    public float healthRegenTarget = 0.25f;
    public float allyReachDistance = 2f;

    private State _idleState;
    private State _patrolState;
    private State _chaseState;
    private State _combatState;
    private State _fleeState;
    private State _deadState;
    private State _fallingState;

    private State _currentState;
    private State _lastState;

    private IAManager _iaManager;

    public State CurrentState => _currentState;
    public State LastState => _lastState;

    public State IdleState => _idleState;
    public State PatrolState => _patrolState;
    public State ChaseState => _chaseState;
    public State CombatState => _combatState;
    public State FleeState => _fleeState;
    public State DeadState => _deadState;
    public State FallingState => _fallingState;

    public void Initialize(IAManager iaManager)
    {
        _iaManager = iaManager;

        CreateStates();

        State startState = GetStateByType(initialState);
        
        if (startState != null && CanUseState(initialState))
        {
            _currentState = startState;
        }
        else
        {
            _currentState = GetFirstAvailableState();
        }

        _currentState?.EnterState(_iaManager);

        Debug.Log($"[StateManager] Iniciado no estado: {GetStateType(_currentState)}");
    }

    public void UpdateState()
    {
        _currentState?.UpdateState(_iaManager);
    }

    public void VerifyStates(IAManager ia)
    {
        if (ia == null || _currentState == null) return;

        // Verifica morte - prioridade máxima
        if (!ia.IsAlive && _currentState != _deadState)
        {
            if (CanUseState(IAState.Dead))
            {
                SwitchState(_deadState);
                return;
            }
        }

        // Verifica queda - segunda maior prioridade (após morte)
        if (!ia.controller.isGrounded && _currentState != _deadState && _currentState != _fallingState)
        {
            if (CanUseState(IAState.Falling))
            {
                SwitchState(_fallingState);
                return;
            }
        }

        // Verifica ressurreição do DeadState
        if (ia.IsAlive && _currentState == _deadState)
        {
            SwitchState(_idleState);
            return;
        }

        // Verifica vida baixa para Flee (exceto se já estiver em Flee ou Dead)
        if (_currentState != _fleeState && _currentState != _deadState && _currentState != _fallingState)
        {
            float healthPercent = (float)ia.Data.currentHealth / ia.Data.TotalMaxHealth;
            if (healthPercent < 0.3f && CanUseState(IAState.Flee))
            {
                SwitchState(_fleeState);
                return;
            }
        }

        // Detecção de alvo - transição para Chase
        if (ia.currentTarget != null && (_currentState == _idleState || _currentState == _patrolState))
        {
            if (CanUseState(IAState.Chase))
            {
                SwitchState(_chaseState);
                return;
            }
        }

        // Em Chase: verifica se deve entrar em Combat
        if (_currentState == _chaseState && ia.currentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(ia.transform.position, ia.currentTarget.position);
            if (distanceToTarget <= ia.attackRange && CanUseState(IAState.Attack))
            {
                SwitchState(_combatState);
                return;
            }
        }

        // Em Combat: verifica se deve voltar para Chase
        if (_currentState == _combatState && ia.currentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(ia.transform.position, ia.currentTarget.position);
            if (distanceToTarget > ia.attackRange && CanUseState(IAState.Chase))
            {
                SwitchState(_chaseState);
                return;
            }
        }

        // Perdeu o alvo - volta para Patrol ou Idle
        if (ia.currentTarget == null && (_currentState == _chaseState || _currentState == _combatState))
        {
            if (ia.patrolPoints != null && ia.patrolPoints.Length > 0 && CanUseState(IAState.Patrol))
            {
                SwitchState(_patrolState);
            }
            else if (CanUseState(IAState.Idle))
            {
                SwitchState(_idleState);
            }
        }
    }

    public void SwitchState(State newState)
    {
        if (newState == null || newState == _currentState) return;

        IAState stateType = GetStateType(newState);

        if (stateType != IAState.None && !CanUseState(stateType))
        {
            Debug.LogWarning($"[StateManager] Tentou mudar para {stateType} mas não tem permissão. activeStates: {activeStates}");
            return;
        }

        _lastState = _currentState;
        _currentState?.ClearState(_iaManager);

        _currentState = newState;
        _currentState?.EnterState(_iaManager);

        Debug.Log($"[StateManager] Mudou de estado: {GetStateType(_lastState)} → {stateType}");
    }

    public bool CanUseState(IAState state)
    {
        return (activeStates & state) == state;
    }

    public State GetStateByType(IAState stateType)
    {
        switch (stateType)
        {
            case IAState.Idle: return _idleState;
            case IAState.Patrol: return _patrolState;
            case IAState.Chase: return _chaseState;
            case IAState.Attack: return _combatState;
            case IAState.Flee: return _fleeState;
            case IAState.Dead: return _deadState;
            case IAState.Falling: return _fallingState;
            default: return null;
        }
    }

    public IAState GetStateType(State state)
    {
        if (state == null) return IAState.None;
        if (state == _idleState) return IAState.Idle;
        if (state == _patrolState) return IAState.Patrol;
        if (state == _chaseState) return IAState.Chase;
        if (state == _combatState) return IAState.Attack;
        if (state == _fleeState) return IAState.Flee;
        if (state == _deadState) return IAState.Dead;
        if (state == _fallingState) return IAState.Falling;
        return IAState.None;
    }

    private void CreateStates()
    {
        _idleState = new IdleState();
        _patrolState = new PatrolState();
        _chaseState = new ChaseState();
        _combatState = new CombatState();
        _fleeState = new FleeState();
        _deadState = new DeadState();
        _fallingState = new FallingState();

        Debug.Log("[StateManager] Estados criados");
    }

    private State GetFirstAvailableState()
    {
        if (CanUseState(IAState.Idle)) return _idleState;
        if (CanUseState(IAState.Patrol)) return _patrolState;
        if (CanUseState(IAState.Chase)) return _chaseState;
        if (CanUseState(IAState.Attack)) return _combatState;
        if (CanUseState(IAState.Flee)) return _fleeState;
        if (CanUseState(IAState.Falling)) return _fallingState;
        if (CanUseState(IAState.Dead)) return _deadState;

        Debug.LogError("[StateManager] Nenhum estado disponível!");
        return _idleState;
    }

    private void OnDestroy()
    {
        _currentState?.ClearState(_iaManager);
    }
}
