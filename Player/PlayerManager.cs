using UnityEngine;
[RequireComponent(typeof(CharacterManager))]
public class PlayerManager : MonoBehaviour
{
    public CharacterManager sheet;
    public PlayerSkillManager skillManager;
    
    [Header("Controller")]
    public PlayerMotor playerMotor;
    [HideInInspector]public PlayerStateMachine playerStateMachine{get; private set;}
    [HideInInspector]public PlayerMouseController playerMouseController{get; private set;}
    [HideInInspector]public PlayerAnimationController playerAnimationController;
    [HideInInspector] public CharacterManager characterManager;

    public Animator animator;

    private bool _isMoving = false;

    protected void Awake()
    {
        sheet = GetComponent<CharacterManager>();
        skillManager = GetComponent<PlayerSkillManager>();
        
        // Inicializa controle de mouse do player (mantém alvo atualizado)
        if (playerMouseController == null)
            playerMouseController = new PlayerMouseController();
        playerMouseController.Initialize();
        
        // Inicializa controlador de animação
        if (playerAnimationController == null) playerAnimationController = new PlayerAnimationController();
        
        // inicialização do controller do animator
        if(animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);
        }
        
        // Tenta aplicar o RuntimeAnimatorController definido na ficha do Character (pode ser nulo no Awake)
        ApplyAnimatorControllerFromSheet();

        // Inicializa controlador de animação (pode ser chamado novamente no Start caso a Controller seja aplicada depois)
        playerAnimationController.Initialize(animator);

        // Subscreve eventos de movimento do motor (se existir)
        if (playerMotor != null)
        {
            playerMotor.IsMoving += OnMovementChanged;
            playerMotor.IsGrounded += OnGroundedChanged;
        }

        // Garante que a state machine exista
        if (playerStateMachine == null)
            playerStateMachine = new PlayerStateMachine();
        playerStateMachine.SetPlayerManager(this);
        // Se não houver estado inicial definido no inspector, usa IdleState por padrão
        if (playerStateMachine.currentState == null)
            playerStateMachine.currentState = new IdleState();


    }
    private void Start()
    {
        // Caso o CharacterManager tenha inicializado sua ficha no Start e tenha definido o
        // RuntimeAnimatorController, reaplique-o aqui e reinicialize o controlador de animação.
        ApplyAnimatorControllerFromSheet();

        if (playerStateMachine != null && playerStateMachine.currentState != null)
            playerStateMachine.currentState.EnterState(this);
    }

    /// Aplica o RuntimeAnimatorController presente em `sheet.Data` ao `animator` local,
    /// e força rebind/reativação caso a Controller tenha sido adicionada depois.
    private void ApplyAnimatorControllerFromSheet()
    {
        if (sheet == null || sheet.Data == null) return;
        if (sheet.Data.animatorController == null) return;
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>(true);
        }

        if (animator == null) return;

        // Se já estiver aplicado, não faz nada
        if (animator.runtimeAnimatorController == sheet.Data.animatorController) return;

        animator.runtimeAnimatorController = sheet.Data.animatorController;
        animator.enabled = true;
        animator.Rebind();
        animator.Update(0f);

        // Re-inicializa o controller de animação para garantir cache de parâmetros
        playerAnimationController?.Initialize(animator);
    }

    public void Update()
    {
        // Verifica se o player morreu
        if (sheet != null && sheet.Health != null && sheet.Health.CurrentHealth <= 0)
        {
            // Se não está morto, transita para DeadState
            if (!(playerStateMachine.currentState is DeadState))
            {
                playerStateMachine.SwitchState(new DeadState());
                return; // não processa mais nada após morrer
            }
        }

        // Atualiza posição/objeto alvo do mouse antes dos sistemas de input/skill
        playerMouseController?.UpdateMouseTarget();

        // Entrada nativa do Unity:
        // Movimento: clique direito (inicia MoveState)
        if (Input.GetMouseButtonDown(1))
        {
            // atualiza alvo imediatamente e manda para MoveState ou AttackState se for inimigo
            playerMouseController?.UpdateMouseTarget();

            Vector3 pos = playerMouseController != null ? playerMouseController.GetTargetPosition() : Vector3.zero;
            GameObject targetObj = playerMouseController != null ? playerMouseController.GetTargetObject() : null;
            
            if (targetObj != null)
            {
                var targetChar = targetObj.GetComponent<CharacterManager>();
                if (targetChar != null && (sheet != null && (sheet.characterType == CharacterType.Player || sheet.characterType == CharacterType.Ally) ? targetChar.characterType == CharacterType.Enemy : false))
                {
                    playerStateMachine?.SwitchState(new AttackState(targetChar));
                }
                else
                {
                    playerStateMachine?.SwitchState(new MoveState(pos));
                }
            }
            else
            {
                playerStateMachine?.SwitchState(new MoveState(pos));
            }
        }

        // Ataque básico: clique esquerdo
        if (Input.GetMouseButtonDown(0))
        {
            if (skillManager != null && skillManager.basicAttackSkill != null)
            {
                if (skillManager.basicAttackSkill.CanUse())
                {
                    skillManager.UseBasicAttack(skillManager.CreateContext(skillManager.basicAttackSkill));
                }
            }
        }

        // Atualiza state machine e física
        playerStateMachine?.UpdateState();
        playerMotor?.ApplyGravity();

        // Atualiza animações baseado no estado atual
        playerAnimationController?.UpdateAnimation(playerStateMachine != null ? playerStateMachine.currentState : null, sheet, _isMoving);
    }

    private void OnMovementChanged(bool moving)
    {
        _isMoving = moving;
    }

    private void OnGroundedChanged(bool grounded)
    {
        playerAnimationController?.SetGrounded(grounded);
    }

    private void OnDestroy()
    {
        // Desinscreve eventos para evitar memory leaks
        if (playerMotor != null)
        {
            playerMotor.IsMoving -= OnMovementChanged;
            playerMotor.IsGrounded -= OnGroundedChanged;
        }
    }


}