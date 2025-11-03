using UnityEngine;

/// <summary>
/// Classe base abstrata para todos os estados do player.
/// Define o contrato que todos os estados devem seguir.
/// </summary>
public abstract class PlayerStateBase
{
    protected PlayerStateMachine stateMachine;
    protected PlayerManager player;

    /// <summary>
    /// Construtor que recebe a referência da state machine
    /// </summary>
    public PlayerStateBase(PlayerStateMachine stateMachine, PlayerManager player)
    {
        this.stateMachine = stateMachine;
        this.player = player;
    }

    /// <summary>
    /// Chamado UMA VEZ quando entra no estado.
    /// Use para configurações iniciais, tocar animações, etc.
    /// </summary>
    public abstract void EnterState();

    /// <summary>
    /// Chamado TODO FRAME enquanto está neste estado.
    /// Lógica contínua do estado vai aqui.
    /// </summary>
    public abstract void UpdateState();

    /// <summary>
    /// Chamado UMA VEZ quando sai do estado.
    /// Use para cleanup, resetar variáveis, etc.
    /// </summary>
    public abstract void ExitState();

    // ========== Permissões de Ações ==========
    // Cada estado define quais ações são permitidas

    /// <summary>
    /// Este estado permite movimentação?
    /// </summary>
    public virtual bool CanMove() => false;

    /// <summary>
    /// Este estado permite atacar?
    /// </summary>
    public virtual bool CanAttack() => false;

    /// <summary>
    /// Este estado permite usar habilidades?
    /// </summary>
    public virtual bool CanUseAbility() => false;

    /// <summary>
    /// Este estado permite interagir com objetos/NPCs?
    /// </summary>
    public virtual bool CanInteract() => false;

    /// <summary>
    /// Este estado permite dodge/roll?
    /// </summary>
    public virtual bool CanDodge() => false;

    // ========== Métodos Auxiliares ==========

    /// <summary>
    /// Facilita transição para outro estado
    /// </summary>
    protected void SwitchState(PlayerStateBase newState)
    {
        stateMachine.SwitchState(newState);
    }

    /// <summary>
    /// Retorna o nome do estado (útil para debug)
    /// </summary>
    public virtual string GetStateName()
    {
        return GetType().Name;
    }
}
