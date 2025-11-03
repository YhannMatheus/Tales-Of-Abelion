using UnityEngine;
using System;

public class PlayerStateMachine
{
    private PlayerStateBase currentState;

    private PlayerManager player;

    public event Action<string, string> OnStateChanged; // (estadoAnterior, estadoNovo)

    public PlayerStateBase CurrentState => currentState;

    public PlayerStateMachine(PlayerManager player)
    {
        this.player = player;
    }

    public void Initialize(PlayerStateBase initialState)
    {
        currentState = initialState;
        currentState.EnterState();

        Debug.Log($"[PlayerStateMachine] Inicializado no estado: {currentState.GetStateName()}");
    }

    public void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateState();
        }
    }

    public void SwitchState(PlayerStateBase newState)
    {
        if (newState == null)
        {
            Debug.LogError("[PlayerStateMachine] Tentativa de trocar para estado nulo!");
            return;
        }

        if (currentState != null && currentState.GetType() == newState.GetType())
        {
            return;
        }

        string previousStateName = currentState?.GetStateName() ?? "Nenhum";
        string newStateName = newState.GetStateName();

        if (currentState != null)
        {
            currentState.ExitState();
        }

        currentState = newState;

        currentState.EnterState();

        OnStateChanged?.Invoke(previousStateName, newStateName);

        Debug.Log($"[PlayerStateMachine] Transição: {previousStateName} → {newStateName}");
    }

    // ========== Métodos de Permissão (delegates para o estado atual) ==========

    public bool CanMove() => currentState?.CanMove() ?? false;
    public bool CanAttack() => currentState?.CanAttack() ?? false;
    public bool CanUseAbility() => currentState?.CanUseAbility() ?? false;
    public bool CanInteract() => currentState?.CanInteract() ?? false;
    public bool CanDodge() => currentState?.CanDodge() ?? false;

    public string GetCurrentStateName()
    {
        return currentState?.GetStateName() ?? "Nenhum";
    }
}
