using UnityEngine;
using System;

[System.Serializable]
public class PlayerAnimationController
{
    public Animator animator;

    // Hash de parâmetros do Animator (pré-computados para performance)
    private static readonly int HashIdle = Animator.StringToHash("Idle");
    private static readonly int HashMove = Animator.StringToHash("Move");
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashCast = Animator.StringToHash("Cast");
    private static readonly int HashDie = Animator.StringToHash("Die");
    private static readonly int HashStun = Animator.StringToHash("Stun");
    private static readonly int HashMoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int HashAttackSpeed = Animator.StringToHash("AttackSpeed");
    private static readonly int HashIsGrounded = Animator.StringToHash("IsGrounded");

    // Cache do estado atual
    private StateBase _currentState;
    private bool _isMoving = false;
    private bool _isGrounded = true;

    /// <summary>
    /// Inicializa o controlador de animação
    /// </summary>
    public void Initialize(Animator anim)
    {
        animator = anim;
        if (animator != null)
        {
            // Define valores iniciais
            animator.SetFloat(HashMoveSpeed, 0f);
            animator.SetFloat(HashAttackSpeed, 1f);
            animator.SetBool(HashIsGrounded, true);
        }
    }

    /// Atualiza animações baseado no estado atual da state machine
    /// Chamado no Update do PlayerManager
    public void UpdateAnimation(StateBase currentState, CharacterManager character, bool isMoving)
    {
        if (animator == null) return;

        _isMoving = isMoving;

        // Atualiza parâmetros de movimento
        animator.SetFloat(HashMoveSpeed, isMoving ? 1f : 0f);

        // Atualiza attack speed se houver personagem
        if (character != null)
        {
            animator.SetFloat(HashAttackSpeed, character.Data.TotalAttackSpeed);
        }

        // Se mudou de estado, dispara trigger apropriado
        if (currentState != _currentState)
        {
            TransitionToState(currentState);
            _currentState = currentState;
        }
    }

    /// Transição entre estados - dispara os triggers corretos
    private void TransitionToState(StateBase newState)
    {
        if (newState == null || animator == null) return;

        // Reseta todos os triggers para evitar conflitos
        ResetAllTriggers();

        // Dispara trigger baseado no tipo do estado
        if (newState is IdleState)
        {
            animator.SetTrigger(HashIdle);
        }
        else if (newState is MoveState)
        {
            animator.SetTrigger(HashMove);
        }
        else if (newState is AttackState)
        {
            animator.SetTrigger(HashAttack);
        }
        else if (newState is CastState)
        {
            animator.SetTrigger(HashCast);
        }
        else if (newState is StunState)
        {
            animator.SetTrigger(HashStun);
        }
        else if (newState is DeadState)
        {
            animator.SetTrigger(HashDie);
        }
    }

    /// Reseta todos os triggers do animator
    private void ResetAllTriggers()
    {
        if (animator == null) return;

        animator.ResetTrigger(HashIdle);
        animator.ResetTrigger(HashMove);
        animator.ResetTrigger(HashAttack);
        animator.ResetTrigger(HashCast);
        animator.ResetTrigger(HashStun);
        animator.ResetTrigger(HashDie);
    }

    /// Atualiza estado de grounded
    public void SetGrounded(bool grounded)
    {
        _isGrounded = grounded;
        if (animator != null)
        {
            animator.SetBool(HashIsGrounded, grounded);
        }
    }

    /// Força animação específica (útil para debugging ou eventos especiais)
    public void ForceAnimation(string triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }
}
