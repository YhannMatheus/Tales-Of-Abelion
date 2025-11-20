using UnityEngine;
using System;

[System.Serializable]
public class PlayerAnimationController
{
    public Animator animator;
    
    // Hash de parâmetros do Animator (pré-computados para performance)
    public static readonly int HashIdle = Animator.StringToHash("Idle");
    public static readonly int HashMove = Animator.StringToHash("Move");
    public static readonly int HashAttack = Animator.StringToHash("Attack");
    public static readonly int HashCast = Animator.StringToHash("Cast");
    public static readonly int HashDie = Animator.StringToHash("Die");
    public static readonly int HashStun = Animator.StringToHash("Stun");
    public static readonly int HashMoveSpeed = Animator.StringToHash("MoveSpeed");
    public static readonly int HashAttackSpeed = Animator.StringToHash("AttackSpeed");
    public static readonly int HashIsGrounded = Animator.StringToHash("IsGrounded");
    public static readonly int HashTakeDamage = Animator.StringToHash("TakeDamage");

    // Cache do estado atual
    private StateBase _currentState;
    private bool _isMoving = false;
    private bool _isGrounded = true;

    /// Inicializa o controlador de animação
    public void Initialize(Animator anim)
    {
        animator = anim;
        if (animator != null)
        {
            // cache de parâmetros existentes para evitar chamadas em params inexistentes
            _animParamNames = new System.Collections.Generic.HashSet<string>();
            foreach (var p in animator.parameters)
                _animParamNames.Add(p.name);

            // Define valores iniciais somente se os parâmetros existirem
            SafeSetFloat("MoveSpeed", HashMoveSpeed, 0f);
            SafeSetFloat("AttackSpeed", HashAttackSpeed, 1f);
            SafeSetBool("IsGrounded", HashIsGrounded, true);

            // Verifica se há um RuntimeAnimatorController atribuído
            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogWarning("[PlayerAnimationController] Animator não tem RuntimeAnimatorController atribuído.");
            }
            else
            {
                // Rebind para garantir inicialização correta do runtime
                animator.enabled = true;
                animator.Rebind();
                animator.Update(0f);

                // Se existir o trigger Idle, dispare para garantir que o state inicial seja acionado
                if (HasParam("Idle"))
                    SafeSetTrigger("Idle", HashIdle);
            }
        }
    }

    /// Atualiza animações baseado no estado atual da state machine
    /// Chamado no Update do PlayerManager
    public void UpdateAnimation(StateBase currentState, CharacterManager character, bool isMoving)
    {
        if (animator == null) return;

        _isMoving = isMoving;

        // Atualiza parâmetros de movimento
        SafeSetFloat("MoveSpeed", HashMoveSpeed, isMoving ? 1f : 0f);

        // Atualiza attack speed se houver personagem
        if (character != null)
        {
            SafeSetFloat("AttackSpeed", HashAttackSpeed, character.Data.TotalAttackSpeed);
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

        // Dispara trigger baseado no tipo do estado (somente se o trigger existir)
        if (newState is IdleState) SafeSetTrigger("Idle", HashIdle);
        else if (newState is MoveState) SafeSetTrigger("Move", HashMove);
        else if (newState is AttackState) SafeSetTrigger("Attack", HashAttack);
        else if (newState is CastState) SafeSetTrigger("Cast", HashCast);
        else if (newState is StunState) SafeSetTrigger("Stun", HashStun);
        else if (newState is DeadState) SafeSetTrigger("Die", HashDie);
    }

    /// Reseta todos os triggers do animator
    private void ResetAllTriggers()
    {
        if (animator == null) return;

        SafeResetTrigger("Idle", HashIdle);
        SafeResetTrigger("Move", HashMove);
        SafeResetTrigger("Attack", HashAttack);
        SafeResetTrigger("Cast", HashCast);
        SafeResetTrigger("Stun", HashStun);
        SafeResetTrigger("Die", HashDie);
    }

    /// Atualiza estado de grounded
    public void SetGrounded(bool grounded)
    {
        _isGrounded = grounded;
        if (animator != null)
        {
            SafeSetBool("IsGrounded", HashIsGrounded, grounded);
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

    //aumento gradual do float Speed na animação
    public void IncreaseSpeed(float amount, float maxSpeed)
    {
        if (animator == null) return;

        float currentSpeed = SafeGetFloat("MoveSpeed", HashMoveSpeed);
        float newSpeed = Mathf.Min(currentSpeed + amount, maxSpeed);
        SafeSetFloat("MoveSpeed", HashMoveSpeed, newSpeed);
    }

    // cache dos nomes de parâmetros existentes no Animator
    private System.Collections.Generic.HashSet<string> _animParamNames;

    private bool HasParam(string name)
    {
        return _animParamNames != null && _animParamNames.Contains(name);
    }

    private void SafeSetFloat(string name, int hash, float value)
    {
        if (animator == null) return;
        if (!HasParam(name)) return;
        animator.SetFloat(hash, value);
    }

    private float SafeGetFloat(string name, int hash)
    {
        if (animator == null) return 0f;
        if (!HasParam(name)) return 0f;
        return animator.GetFloat(hash);
    }

    private void SafeSetBool(string name, int hash, bool value)
    {
        if (animator == null) return;
        if (!HasParam(name)) return;
        animator.SetBool(hash, value);
    }

    private void SafeSetTrigger(string name, int hash)
    {
        if (animator == null) return;
        if (!HasParam(name)) return;
        animator.SetTrigger(hash);
    }

    private void SafeResetTrigger(string name, int hash)
    {
        if (animator == null) return;
        if (!HasParam(name)) return;
        animator.ResetTrigger(hash);
    }
}
