using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(MouseManager))]
[RequireComponent(typeof(CharacterAnimatorController))]
[RequireComponent(typeof(PlayerAbilityManager))]
public class PlayerManager : MonoBehaviour
{
    [Header("State Machine")]
    [SerializeField] private bool useStateMachine = true; // Toggle para ativar/desativar state machine
    private PlayerStateMachine stateMachine;

    [Header("Combat Settings")]
    [Tooltip("Duração da animação de ataque básico em segundos")]
    [SerializeField] private float attackDuration = 0.6f;
    
    [Tooltip("Permite rotacionar durante ataque?")]
    [SerializeField] private bool canRotateDuringAttack = true;

    [Header("Movement Settings")]
    [Tooltip("Magnitude mínima de input para considerar que está se movendo")]
    [SerializeField] private float movementThreshold = 0.1f;

    [Header("Interaction Settings")]
    [Tooltip("Rotaciona automaticamente para o objeto ao interagir?")]
    [SerializeField] private bool autoRotateOnInteract = true;

    [Header("Casting Settings")]
    [Tooltip("Permite rotacionar durante cast de habilidade?")]
    [SerializeField] private bool canRotateDuringCast = true;
    
    [Tooltip("Cancelar cast ao tentar se mover?")]
    [SerializeField] private bool canCancelCastByMoving = true;

    [Header("Stun Settings")]
    [Tooltip("Duração mínima de atordoamento (segundos)")]
    [SerializeField] private float minStunDuration = 0.3f;
    
    [Tooltip("Duração máxima de atordoamento (segundos)")]
    [SerializeField] private float maxStunDuration = 5.0f;

    [Header("Debug")]
    [Tooltip("Mostra informações de estado na tela?")]
    [SerializeField] private bool showDebugGUI = true;
    
    [Tooltip("Mostra logs de transição de estado no Console?")]
    [SerializeField] private bool showStateLogs = true;

    [Header("Components")]
    private PlayerMotor motor;
    private InputManager input;
    private CharacterAnimatorController animator;
    private MouseManager mouse;
    private Character character;
    private PlayerAbilityManager ability;
    private Vector3 moveDirection;

    // ========== Propriedades Públicas (para States acessarem) ==========
    public PlayerMotor Motor => motor;
    public InputManager Input => input;
    public CharacterAnimatorController Animator => animator;
    public MouseManager Mouse => mouse;
    public Character Character => character;
    public PlayerAbilityManager Ability => ability;

    // ========== Configurações Públicas (para States acessarem) ==========
    public float AttackDuration => attackDuration;
    public bool CanRotateDuringAttack => canRotateDuringAttack;
    public float MovementThreshold => movementThreshold;
    public bool AutoRotateOnInteract => autoRotateOnInteract;
    public bool CanRotateDuringCast => canRotateDuringCast;
    public bool CanCancelCastByMoving => canCancelCastByMoving;
    public float MinStunDuration => minStunDuration;
    public float MaxStunDuration => maxStunDuration;

    private void Awake()
    {
        motor = GetComponent<PlayerMotor>();
        character = GetComponent<Character>();
        mouse = GetComponent<MouseManager>();
        ability = GetComponent<PlayerAbilityManager>();
        input = Object.FindFirstObjectByType<InputManager>();
        animator = GetComponent<CharacterAnimatorController>();

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
    }

    private void OnPlayerStateChanged(string previousState, string newState)
    {
        if (showStateLogs)
        {
            Debug.Log($"[PlayerManager] Estado mudou: {previousState} → {newState}");
        }
    }

    private void OnCharacterDeath()
    {
        if (useStateMachine && stateMachine != null)
        {
            // Força transição para DeadState quando morrer
            stateMachine.SwitchState(new PlayerDeadState(stateMachine, this));
        }
    }

    private void OnCharacterRevive()
    {
        if (useStateMachine && stateMachine != null)
        {
            // Volta para IdleState quando reviver
            stateMachine.SwitchState(new PlayerIdleState(stateMachine, this));
        }
    }

    // ========== Métodos Públicos (para controle externo de estados) ==========

    /// <summary>
    /// Aplica stun ao player por uma duração
    /// </summary>
    public void ApplyStun(float duration)
    {
        if (!useStateMachine || stateMachine == null) return;
        if (!character.Data.IsAlive) return; // Não pode stunnar se estiver morto

        stateMachine.SwitchState(new PlayerStunnedState(stateMachine, this, duration));
    }

    /// <summary>
    /// Inicia cast de habilidade com tempo de conjuração
    /// </summary>
    public void StartCasting(int abilitySlotIndex, AbilityContext context, float castTime)
    {
        if (!useStateMachine || stateMachine == null) return;
        if (!character.Data.IsAlive) return;

        stateMachine.SwitchState(new PlayerCastingState(stateMachine, this, abilitySlotIndex, context, castTime));
    }

    private void Update()
    {
        // ========== MODO STATE MACHINE ==========
        if (useStateMachine)
        {
            // Verifica se está vivo antes de atualizar state machine
            if (character.Data.IsAlive)
            {
                stateMachine?.Update();
            }

            // Gravidade é aplicada sempre (mesmo quando morto)
            motor.ApplyGravity();

            return; // Não executa código legado
        }

        // ========== MODO LEGADO (sem State Machine) ==========
        if (character.Data.IsAlive)
        {
            HandleMovement();
        }
        
        motor.ApplyGravity();
        
        animator?.UpdateMovementSpeed(motor.CurrentSpeedNormalized, motor.IsGrounded);

        if (input.interactButton)
        {
            HandleEventClick();
        }

        if (input.attackInput)
        {
            HandleAttack();
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
            ability.UseAbilityInSlot(0, mouse.GetMousePosition(), mouse.GetClickedObject());
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