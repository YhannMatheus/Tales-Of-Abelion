using UnityEngine;
using System;

/// <summary>
/// Gerenciador de estados do player.
/// Controla transições entre estados e garante que apenas um estado esteja ativo por vez.
/// </summary>
public class PlayerStateMachine
{
    // Estado atual ativo
    private PlayerStateBase currentState;

    // Referência ao PlayerManager
    private PlayerManager player;

    // Eventos para notificar quando estado muda (útil para UI/Debug)
    public event Action<string, string> OnStateChanged; // (estadoAnterior, estadoNovo)

    // Propriedade pública para ler estado atual
    public PlayerStateBase CurrentState => currentState;

    /// <summary>
    /// Construtor da State Machine
    /// </summary>
    public PlayerStateMachine(PlayerManager player)
    {
        this.player = player;
    }

    /// <summary>
    /// Inicializa a state machine com um estado inicial
    /// </summary>
    public void Initialize(PlayerStateBase initialState)
    {
        currentState = initialState;
        currentState.EnterState();

        Debug.Log($"[PlayerStateMachine] Inicializado no estado: {currentState.GetStateName()}");
    }

    /// <summary>
    /// Atualiza o estado atual (chamado no Update do PlayerManager)
    /// </summary>
    public void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateState();
        }
    }

    /// <summary>
    /// Troca para um novo estado com transição segura
    /// </summary>
    public void SwitchState(PlayerStateBase newState)
    {
        if (newState == null)
        {
            Debug.LogError("[PlayerStateMachine] Tentativa de trocar para estado nulo!");
            return;
        }

        // Se já está neste estado, não faz nada
        if (currentState != null && currentState.GetType() == newState.GetType())
        {
            return;
        }

        string previousStateName = currentState?.GetStateName() ?? "Nenhum";
        string newStateName = newState.GetStateName();

        // 1. Sai do estado anterior (cleanup)
        if (currentState != null)
        {
            currentState.ExitState();
        }

        // 2. Atualiza referência
        currentState = newState;

        // 3. Entra no novo estado (setup)
        currentState.EnterState();

        // 4. Dispara evento de mudança
        OnStateChanged?.Invoke(previousStateName, newStateName);

        Debug.Log($"[PlayerStateMachine] Transição: {previousStateName} → {newStateName}");
    }

    // ========== Métodos de Permissão (delegates para o estado atual) ==========

    public bool CanMove() => currentState?.CanMove() ?? false;
    public bool CanAttack() => currentState?.CanAttack() ?? false;
    public bool CanUseAbility() => currentState?.CanUseAbility() ?? false;
    public bool CanInteract() => currentState?.CanInteract() ?? false;
    public bool CanDodge() => currentState?.CanDodge() ?? false;

    /// <summary>
    /// Retorna o nome do estado atual (útil para debug/UI)
    /// </summary>
    public string GetCurrentStateName()
    {
        return currentState?.GetStateName() ?? "Nenhum";
    }
}
