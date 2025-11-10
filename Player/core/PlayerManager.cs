using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(MouseManager))]
[RequireComponent(typeof(CharacterAnimatorController))]
[RequireComponent(typeof(PlayerSkillManager))]
public class PlayerManager : MonoBehaviour
{
    [Header("STATE MACHINE ")]
    [SerializeField] private bool useStateMachine;
    private PlayerStateMachine stateMachine;

    [Header("MOTOR")]
    [SerializeField] private float rotationSpeed;
    [SerializeField] private bool useSmoothRotation;
    [SerializeField] private float movementThreshold;
    [SerializeField] private float slowThreshold;
    [SerializeField] private float buffedThreshold;
    [SerializeField] private float destinationReachedThreshold;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance;
    [SerializeField] private LayerMask groundMask;

    [Header("MOUSE - DETECTION")]
    [SerializeField] private LayerMask clickableLayerMask = ~0;
    [SerializeField] private float maxRaycastDistance;
    
    [Header("ANIMATOR")]
    [SerializeField] private float movementSmoothTime;
    [SerializeField] private float animSlowThreshold;
    [SerializeField] private float animBuffedThreshold;
    [SerializeField] private bool debugAnimator;

    [Header("COMBAT")]
    [SerializeField] private float attackDuration;
    [SerializeField] private bool canRotateDuringAttack;

    [Header("INTERACTION")]
    [SerializeField] private bool autoRotateOnInteract;

    [Header("CASTING")]
    [SerializeField] private bool canRotateDuringCast;
    [SerializeField] private bool canCancelCastByMoving;

    [Header("STUN")]
    [SerializeField] private float minStunDuration;
    [SerializeField] private float maxStunDuration;

    [Header("DEBUG")]
    [SerializeField] private bool showDebugGUI;
    [SerializeField] private bool showStateLogs = false;

    [Header("Components")]
    private CharacterAnimatorController animator;
    private PlayerSkillManager skillManager;
    private Vector3 moveDirection;
    private CharacterManager character;
    private InputManager input;
    private MouseManager mouse;
    private PlayerMotor motor;

    // ========== Propriedades Públicas (para States acessarem) ==========
    public CharacterAnimatorController Animator => animator;
    public PlayerSkillManager SkillManager => skillManager;
    public CharacterManager Character => character;
    public InputManager Input => input;
    public MouseManager Mouse => mouse;
    public PlayerMotor Motor => motor;

    // ========== Configurações Públicas (para States acessarem) ==========
    public bool CanRotateDuringAttack => canRotateDuringAttack;
    public bool CanCancelCastByMoving => canCancelCastByMoving;
    public bool AutoRotateOnInteract => autoRotateOnInteract;
    public bool CanRotateDuringCast => canRotateDuringCast;
    public float MovementThreshold => movementThreshold;
    public float MinStunDuration => minStunDuration;
    public float MaxStunDuration => maxStunDuration;
    public float AttackDuration => attackDuration;
    
    // ========== Propriedades de Velocidade (para debug e UI) ==========
    public float SpeedMultiplier => motor != null ? motor.SpeedMultiplier : 1f;
    public bool IsMovingBuffed => motor != null && motor.GetSpeedType() == "BUFFED";
    public bool IsMovingSlow => motor != null && motor.GetSpeedType() == "SLOW";
    public string SpeedType => motor != null ? motor.GetSpeedType() : "NORMAL";

    private void Awake()
    {
        // Obtém componentes
        character = GetComponent<CharacterManager>();
        CharacterController controller = GetComponent<CharacterController>();
        Animator anim = GetComponentInChildren<Animator>();
        input = Object.FindFirstObjectByType<InputManager>();
        
        motor = GetComponent<PlayerMotor>();
        mouse = GetComponent<MouseManager>();
        animator = GetComponent<CharacterAnimatorController>();
        skillManager = GetComponent<PlayerSkillManager>();
        
        // Inicializa componentes (injeta dependências E configurações via PlayerManager)
        motor?.Initialize(controller, character);
        motor?.ConfigureSettings(rotationSpeed, useSmoothRotation, slowThreshold, buffedThreshold, 
                                 destinationReachedThreshold, groundCheck, groundDistance, groundMask);
        
        mouse?.Initialize();
        mouse?.ConfigureSettings(clickableLayerMask, maxRaycastDistance);
        
        animator?.Initialize(anim, character);
        animator?.ConfigureSettings(movementSmoothTime, animSlowThreshold, animBuffedThreshold, debugAnimator);
        
        // Inscreve animator em eventos do CharacterManager
        animator?.SubscribeToCharacterEvents();

        // Inicializa State Machine
        if (useStateMachine)
        {
            stateMachine = new PlayerStateMachine(this);
            stateMachine.Initialize(new PlayerIdleState(stateMachine, this));

            // Subscreve evento de mudança de estado (para debug)
            stateMachine.OnStateChanged += OnPlayerStateChanged;

            // Subscreve evento de morte do personagem
            if (character != null)
            {
                character.OnDeath += OnCharacterDeath;
                character.OnRevive += OnCharacterRevive;
            }

            if (showStateLogs)
            {
                Debug.Log("[PlayerManager] State Machine ativada!");
            }
        }
    }

    private void OnDestroy()
    {
        // Desinscreve eventos para evitar memory leaks
        if (stateMachine != null)
        {
            stateMachine.OnStateChanged -= OnPlayerStateChanged;
        }

        if (character != null)
        {
            character.OnDeath -= OnCharacterDeath;
            character.OnRevive -= OnCharacterRevive;
        }
        
        // Desinscreve animator de eventos do CharacterManager
        animator?.UnsubscribeFromCharacterEvents();
    }

    private void OnPlayerStateChanged(string previousState, string newState)
    {
        if (showStateLogs)
        {
            Debug.Log($"[PlayerManager] Estado mudou: {previousState} → {newState}");
        }
    }

    private void OnCharacterDeath(object sender, DeathEventArgs e)
    {
        if (useStateMachine && stateMachine != null)
        {
            // Força transição para DeadState quando morrer
            stateMachine.SwitchState(new PlayerDeadState(stateMachine, this));
        }
    }

    private void OnCharacterRevive(object sender, ReviveEventArgs e)
    {
        if (useStateMachine && stateMachine != null)
        {
            // Volta para IdleState quando reviver
            stateMachine.SwitchState(new PlayerIdleState(stateMachine, this));
        }
    }

    //TODO: ========== Métodos Públicos (para controle externo de estados) ==========

    public void ApplyStun(float duration)
    {
        if (!useStateMachine || stateMachine == null) return;
        if (!character.Data.IsAlive) return; // Não pode stunnar se estiver morto

        stateMachine.SwitchState(new PlayerStunnedState(stateMachine, this, duration));
    }
    
    public void RecalculateMovementSpeed()
    {
        if (motor != null)
        {
            motor.RecalculateBaseSpeed();
        }
        
        if (animator != null)
        {
            animator.RecalculateBaseSpeed();
        }
    }

    private void Update()
    {
        // ========== MODO STATE MACHINE ==========
        if (useStateMachine)
        {
            motor.ApplyGravity();
            
            if (character.Data.IsAlive)
            {
                CheckGroundedState();

                stateMachine?.Update();
            }

            return; // Não executa código legado
        }

        motor.ApplyGravity();
    }
    
    private void CheckGroundedState()
    {
        if (stateMachine == null) return;

        // Não verifica se estiver morto ou stunado (esses estados têm prioridade)
        var currentState = stateMachine.CurrentState;
        if (currentState is PlayerDeadState || currentState is PlayerStunnedState)
            return;

        // Se não está no chão e não está em FallingState, transita
        if (!motor.IsGrounded && !(currentState is PlayerFallingState))
        {
            stateMachine.SwitchState(new PlayerFallingState(stateMachine, this));
        }
    }

    private void HandleMovement()
    {
        moveDirection = new Vector3(input.horizontalInput, 0, input.verticalInput);

        if (moveDirection.magnitude > 0.1f)
        {
            motor.Move(moveDirection, character.Data.TotalSpeed);
            motor.Rotate(moveDirection);
        }
        else
        {
            motor.Stop();
        }
    }


    private void HandleEventClick()
    {
        GameObject clickedObject = mouse.GetClickedObject();

        if (clickedObject != null)
        {
            Event eventComponent = clickedObject.GetComponent<Event>();

            if (!motor.IsMoving)
            {
                motor.RotateToPosition(mouse.GetMousePosition());
            }

            if (eventComponent != null)
            {
                float distanceToObject = Vector3.Distance(transform.position, clickedObject.transform.position);

                if (distanceToObject <= eventComponent.minDistanceToTrigger)
                {
                    eventComponent.OnClick();
                }
            }
        }
    }

    private void HandleAttack()
    {
        Vector3 mouseWorldPos = mouse.GetMousePosition();

        Vector3 attackDirection = (mouseWorldPos - transform.position);
        attackDirection.y = 0;

        if (attackDirection.magnitude > 0.1f)
        {
            motor.Rotate(attackDirection);
            animator.TriggerAbility(0);
            
            GameObject target = mouse.GetClickedObject();
            CharacterManager targetChar = target != null ? target.GetComponent<CharacterManager>() : null;
            skillManager.UseSkill(0, mouse.GetMousePosition(), targetChar);
        }
        
    }

    // ========== Debug Visual ==========
    private void OnGUI()
    {
        if (!showDebugGUI || !useStateMachine || stateMachine == null) return;

        // Mostra estado atual no canto superior esquerdo
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.yellow;
        style.fontStyle = FontStyle.Bold;

        string stateInfo = $"Estado: {stateMachine.GetCurrentStateName()}";
        GUI.Label(new Rect(10, 10, 300, 30), stateInfo, style);

        // Mostra permissões do estado atual
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        
        string permissions = $"Move: {stateMachine.CanMove()} | Attack: {stateMachine.CanAttack()} | Interact: {stateMachine.CanInteract()}";
        GUI.Label(new Rect(10, 40, 500, 25), permissions, style);
    }
}