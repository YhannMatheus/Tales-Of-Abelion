using UnityEngine;
[RequireComponent(typeof(CharacterManager))]
public class PlayerManager : MonoBehaviour
{
    public CharacterManager sheet;
    public PlayerSkillManager skillManager;
    
    [Header("Controller")]
    public PlayerMotor _playerMotor;
    public PlayerStateMachine _playerStateMachine;
    public PlayerMouseController _playerMouseController;
    public PlayerAnimationController _playerAnimationController;

    [Header("Animation")]
    public Animator animator;

    private bool _isMoving = false;

    protected void Awake()
    {
        sheet = GetComponent<CharacterManager>();
        skillManager = GetComponent<PlayerSkillManager>();
        
        // Se animator não foi atribuído, tenta pegar do GameObject
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        
        // Inicializa controle de mouse do player (mantém alvo atualizado)
        _playerMouseController?.Initialize();
        
        // Inicializa controlador de animação
        if (_playerAnimationController == null)
            _playerAnimationController = new PlayerAnimationController();
        _playerAnimationController.Initialize(animator);
        
        // Subscreve eventos de movimento do motor
        if (_playerMotor != null)
        {
            _playerMotor.IsMoving += OnMovementChanged;
            _playerMotor.IsGrounded += OnGroundedChanged;
        }
        
        _playerStateMachine.SetPlayerManager(this);
        // Se não houver estado inicial definido no inspector, usa IdleState por padrão
        if (_playerStateMachine.currentState == null)
            _playerStateMachine.currentState = new IdleState();
    }
    private void Start()
    {
        _playerStateMachine.currentState.EnterState(this);
    }

    public void Update()
    {
        // Verifica se o player morreu
        if (sheet != null && sheet.Health != null && sheet.Health.CurrentHealth <= 0)
        {
            // Se não está morto, transita para DeadState
            if (!(_playerStateMachine.currentState is DeadState))
            {
                _playerStateMachine.SwitchState(new DeadState());
                return; // não processa mais nada após morrer
            }
        }

        // Atualiza posição/objeto alvo do mouse antes dos sistemas de input/skill
        _playerMouseController?.UpdateMouseTarget();
        _playerStateMachine.UpdateState();
        _playerMotor.ApplyGravity();
        
        // Atualiza animações baseado no estado atual
        _playerAnimationController?.UpdateAnimation(_playerStateMachine.currentState, sheet, _isMoving);
    }

    private void OnMovementChanged(bool moving)
    {
        _isMoving = moving;
    }

    private void OnGroundedChanged(bool grounded)
    {
        _playerAnimationController?.SetGrounded(grounded);
    }

    private void OnDestroy()
    {
        // Desinscreve eventos para evitar memory leaks
        if (_playerMotor != null)
        {
            _playerMotor.IsMoving -= OnMovementChanged;
            _playerMotor.IsGrounded -= OnGroundedChanged;
        }
    }


}