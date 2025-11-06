using UnityEngine;

[RequireComponent(typeof(PlayerManager))]
public class PlayerStateDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [Tooltip("Ativar debug de estados via teclado?")]
    [SerializeField] private bool enableDebugKeys = true;
    
    [Tooltip("Mostrar UI de debug na tela?")]
    [SerializeField] private bool showDebugUI = true;
    
    [Tooltip("Posição da UI de debug")]
    [SerializeField] private Vector2 debugUIPosition = new Vector2(10, 100);
    
    [Header("UI Scale Settings")]
    [Tooltip("Escala da UI de debug (1.0 = tamanho padrão)")]
    [SerializeField] [Range(0.5f, 3.0f)] private float uiScale = 1.0f;
    
    [Tooltip("Largura base da UI")]
    [SerializeField] private float baseUIWidth = 400f;
    
    [Tooltip("Altura base da UI")]
    [SerializeField] private float baseUIHeight = 550f;

    private PlayerManager playerManager;
    private PlayerStateMachine stateMachine;

    // Sistema de travamento de estados
    private bool isStateLocked = false;
    private System.Type lockedStateType = null;
    private float lockCheckInterval = 0.1f; // Verifica a cada 100ms
    private float lockCheckTimer = 0f;

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
    }

    private void Start()
    {
        // Acessa a state machine via reflection (já que é privada)
        var field = typeof(PlayerManager).GetField("stateMachine", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            stateMachine = field.GetValue(playerManager) as PlayerStateMachine;
        }

        if (stateMachine == null)
        {
            Debug.LogWarning("[PlayerStateDebugger] Não foi possível acessar a StateMachine do PlayerManager");
            enabled = false;
        }
    }

    private void Update()
    {
        if (!enableDebugKeys || stateMachine == null) return;

        HandleStateToggleInput();
        EnforceLockedState();
        HandleUtilityInput();
        HandleUIScaleInput();
    }

    private void HandleStateToggleInput()
    {
        // Numpad 0 - Destrava qualquer estado
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            UnlockState();
        }

        // Numpad 1 - Trava/destrava estado IDLE
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            ToggleStateLock(typeof(PlayerIdleState), "IDLE");
        }

        // Numpad 2 - Trava/destrava estado MOVING
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            ToggleStateLock(typeof(PlayerMovingState), "MOVING");
        }

        // Numpad 3 - Trava/destrava estado ATTACKING
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            ToggleStateLock(typeof(PlayerAttackingState), "ATTACKING");
        }

        // Numpad 4 - Trava/destrava estado CASTING
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            ToggleStateLock(typeof(PlayerCastingState), "CASTING");
        }

        // Numpad 5 - Trava/destrava estado STUNNED (2s de duração)
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            ToggleStateLock(typeof(PlayerStunnedState), "STUNNED");
        }

        // Numpad 6 - Trava/destrava estado DEAD
        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            ToggleStateLock(typeof(PlayerDeadState), "DEAD");
        }
    }

    private void ToggleStateLock(System.Type stateType, string stateName)
    {
        // Se já está travado neste estado, destrava
        if (isStateLocked && lockedStateType == stateType)
        {
            UnlockState();
            return;
        }

        // Se estava travado em outro estado, destrava primeiro
        if (isStateLocked && lockedStateType != stateType)
        {
            UnlockState();
        }

        // Trava no novo estado
        isStateLocked = true;
        lockedStateType = stateType;
        Debug.Log($"<color=yellow>[DEBUG] Estado {stateName} TRAVADO - Pressione Numpad 0 ou {GetKeyForState(stateType)} novamente para destravar</color>");

        // Força transição imediata para o estado travado
        ForceStateTransition(stateType);
    }

    private void UnlockState()
    {
        if (!isStateLocked)
        {
            Debug.Log("<color=gray>[DEBUG] Nenhum estado estava travado</color>");
            return;
        }

        string stateTypeName = lockedStateType?.Name ?? "Unknown";
        isStateLocked = false;
        lockedStateType = null;
        Debug.Log($"<color=green>[DEBUG] Estado {stateTypeName} DESTRAVADO - Controle normal retomado</color>");
    }

    private void EnforceLockedState()
    {
        if (!isStateLocked || lockedStateType == null) return;

        lockCheckTimer += Time.deltaTime;
        if (lockCheckTimer < lockCheckInterval) return;

        lockCheckTimer = 0f;

        var currentState = stateMachine.CurrentState;
        if (currentState == null) return;

        // Se o estado atual não é o travado, força retorno
        if (currentState.GetType() != lockedStateType)
        {
            ForceStateTransition(lockedStateType);
        }
    }

    private void ForceStateTransition(System.Type stateType)
    {
        if (stateType == typeof(PlayerIdleState))
        {
            stateMachine.SwitchState(new PlayerIdleState(stateMachine, playerManager));
        }
        else if (stateType == typeof(PlayerMovingState))
        {
            stateMachine.SwitchState(new PlayerMovingState(stateMachine, playerManager));
        }
        else if (stateType == typeof(PlayerAttackingState))
        {
            stateMachine.SwitchState(new PlayerAttackingState(stateMachine, playerManager));
        }
        else if (stateType == typeof(PlayerCastingState))
        {
            // Cria um contexto dummy para casting
            var dummyContext = new SkillContext
            {
                Caster = playerManager.Character,
                Target = null,
                TargetPosition = playerManager.transform.position + playerManager.transform.forward * 3f,
                OriginPosition = playerManager.transform.position
            };
            stateMachine.SwitchState(new PlayerCastingState(stateMachine, playerManager, 1, dummyContext, 2f));
        }
        else if (stateType == typeof(PlayerStunnedState))
        {
            stateMachine.SwitchState(new PlayerStunnedState(stateMachine, playerManager, 2f));
        }
        else if (stateType == typeof(PlayerDeadState))
        {
            stateMachine.SwitchState(new PlayerDeadState(stateMachine, playerManager));
        }
    }

    private string GetKeyForState(System.Type stateType)
    {
        if (stateType == typeof(PlayerIdleState)) return "Numpad 1";
        if (stateType == typeof(PlayerMovingState)) return "Numpad 2";
        if (stateType == typeof(PlayerAttackingState)) return "Numpad 3";
        if (stateType == typeof(PlayerCastingState)) return "Numpad 4";
        if (stateType == typeof(PlayerStunnedState)) return "Numpad 5";
        if (stateType == typeof(PlayerDeadState)) return "Numpad 6";
        return "?";
    }

    private void HandleUtilityInput()
    {
        // F7 - Aplica dano de teste (20% da vida)
        if (Input.GetKeyDown(KeyCode.F7))
        {
            float damage = playerManager.Character.Data.TotalMaxHealth * 0.2f;
            Debug.Log($"[DEBUG] Aplicando {damage} de dano de teste");
            playerManager.Character.TakeDamage(damage);
        }

        // F8 - Cura total
        if (Input.GetKeyDown(KeyCode.F8))
        {
            Debug.Log("[DEBUG] Curando 100% da vida");
            int healAmount = (int)playerManager.Character.Data.TotalMaxHealth;
            playerManager.Character.Heal(healAmount);
        }

        // F9 - Toggle movimento (para testar animação parada)
        if (Input.GetKeyDown(KeyCode.F9))
        {
            var motor = playerManager.Motor;
            if (motor != null)
            {
                motor.Stop();
                Debug.Log("[DEBUG] Motor parado - teste animação parada");
            }
        }

        // F10 - Reseta Animator completamente
        if (Input.GetKeyDown(KeyCode.F10))
        {
            Debug.Log("[DEBUG] Resetando Animator completamente");
            playerManager.Animator?.ResetAnimatorState();
        }

        // F11 - Info do estado atual
        if (Input.GetKeyDown(KeyCode.F11))
        {
            var currentState = stateMachine.CurrentState;
            string stateName = currentState?.GetType().Name ?? "NULL";
            Debug.Log($"[DEBUG] Estado atual: {stateName}");
            Debug.Log($"[DEBUG] Estado travado? {isStateLocked} ({lockedStateType?.Name ?? "None"})");
            Debug.Log($"[DEBUG] Pode mover? {currentState?.CanMove()}");
            Debug.Log($"[DEBUG] Pode atacar? {currentState?.CanAttack()}");
            Debug.Log($"[DEBUG] Pode usar habilidade? {currentState?.CanUseAbility()}");
        }

        // F12 - Toggle Debug UI
        if (Input.GetKeyDown(KeyCode.F12))
        {
            showDebugUI = !showDebugUI;
            Debug.Log($"[DEBUG] Debug UI: {(showDebugUI ? "ATIVADA" : "DESATIVADA")}");
        }
    }

    /// <summary>
    /// Gerencia inputs para ajustar escala da UI
    /// </summary>
    private void HandleUIScaleInput()
    {
        // PageUp - Aumenta escala da UI
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            uiScale = Mathf.Clamp(uiScale + 0.1f, 0.5f, 3.0f);
            Debug.Log($"[DEBUG] UI Scale aumentada: {uiScale:F1}x");
        }

        // PageDown - Diminui escala da UI
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            uiScale = Mathf.Clamp(uiScale - 0.1f, 0.5f, 3.0f);
            Debug.Log($"[DEBUG] UI Scale reduzida: {uiScale:F1}x");
        }

        // Home - Reseta escala para 1.0
        if (Input.GetKeyDown(KeyCode.Home))
        {
            uiScale = 1.0f;
            Debug.Log("[DEBUG] UI Scale resetada para 1.0x");
        }
    }

    private void OnGUI()
    {
        if (!showDebugUI || stateMachine == null) return;

        // Calcula tamanhos escalados
        float scaledWidth = baseUIWidth * uiScale;
        float scaledHeight = baseUIHeight * uiScale;
        int scaledHeaderSize = Mathf.RoundToInt(16 * uiScale);
        int scaledLabelSize = Mathf.RoundToInt(12 * uiScale);
        int scaledKeySize = Mathf.RoundToInt(11 * uiScale);

        // Estilo da UI com tamanhos escalados
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = scaledHeaderSize,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.yellow }
        };

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = scaledLabelSize,
            normal = { textColor = Color.white }
        };

        GUIStyle keyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = scaledKeySize,
            normal = { textColor = Color.cyan }
        };

        // Área de debug com tamanho escalado
        GUILayout.BeginArea(new Rect(debugUIPosition.x, debugUIPosition.y, scaledWidth, scaledHeight));
        
        // Fundo semi-transparente
        GUI.Box(new Rect(0, 0, scaledWidth, scaledHeight), "");

        GUILayout.Space(10 * uiScale);
        GUILayout.Label("=== PLAYER STATE DEBUGGER ===", headerStyle);
        GUILayout.Space(5);

        // Estado atual
        var currentState = stateMachine.CurrentState;
        string stateName = currentState?.GetType().Name ?? "NULL";
        
        // Indica se estado está travado
        Color stateColor = isStateLocked ? Color.yellow : Color.white;
        GUIStyle stateStyle = new GUIStyle(labelStyle) { normal = { textColor = stateColor } };
        
        string lockIndicator = isStateLocked ? " [TRAVADO]" : "";
        GUILayout.Label($"Estado Atual: {stateName}{lockIndicator}", stateStyle);
        
        // Permissões
        GUILayout.Label($"Pode Mover: {currentState?.CanMove()}", labelStyle);
        GUILayout.Label($"Pode Atacar: {currentState?.CanAttack()}", labelStyle);
        GUILayout.Label($"Pode Habilidade: {currentState?.CanUseAbility()}", labelStyle);

        GUILayout.Space(10 * uiScale);
        GUILayout.Label("--- TECLADO NUMÉRICO (TOGGLE) ---", headerStyle);
        GUILayout.Space(5 * uiScale);

        GUILayout.Label("Numpad 0 - DESTRAVAR estado", keyStyle);
        GUILayout.Label("Numpad 1 - TRAVAR/DESTRAVAR IDLE", keyStyle);
        GUILayout.Label("Numpad 2 - TRAVAR/DESTRAVAR MOVING", keyStyle);
        GUILayout.Label("Numpad 3 - TRAVAR/DESTRAVAR ATTACKING", keyStyle);
        GUILayout.Label("Numpad 4 - TRAVAR/DESTRAVAR CASTING", keyStyle);
        GUILayout.Label("Numpad 5 - TRAVAR/DESTRAVAR STUNNED", keyStyle);
        GUILayout.Label("Numpad 6 - TRAVAR/DESTRAVAR DEAD", keyStyle);
        
        GUILayout.Space(10 * uiScale);
        GUILayout.Label("--- UTILIDADES ---", headerStyle);
        GUILayout.Space(5 * uiScale);
        
        GUILayout.Label("F7  - Aplica 20% dano", keyStyle);
        GUILayout.Label("F8  - Cura 100%", keyStyle);
        GUILayout.Label("F9  - Para motor", keyStyle);
        GUILayout.Label("F10 - Reseta Animator", keyStyle);
        GUILayout.Label("F11 - Info estado (Console)", keyStyle);
        GUILayout.Label("F12 - Toggle esta UI", keyStyle);

        GUILayout.Space(10 * uiScale);
        GUILayout.Label("--- ESCALA DA UI ---", headerStyle);
        GUILayout.Space(5 * uiScale);
        
        GUILayout.Label($"Escala Atual: {uiScale:F1}x", labelStyle);
        GUILayout.Label("PageUp   - Aumentar escala", keyStyle);
        GUILayout.Label("PageDown - Diminuir escala", keyStyle);
        GUILayout.Label("Home     - Resetar escala (1.0x)", keyStyle);

        GUILayout.Space(10 * uiScale);
        GUILayout.Label("--- STATUS ---", headerStyle);
        GUILayout.Space(5 * uiScale);

        float healthPercent = (playerManager.Character.Data.currentHealth / playerManager.Character.Data.TotalMaxHealth) * 100f;
        GUILayout.Label($"Vida: {playerManager.Character.Data.currentHealth:F0}/{playerManager.Character.Data.TotalMaxHealth:F0} ({healthPercent:F0}%)", labelStyle);
        GUILayout.Label($"Energia: {playerManager.Character.Data.currentEnergy:F0}/{playerManager.Character.Data.TotalMaxEnergy:F0}", labelStyle);
        GUILayout.Label($"Velocidade: {playerManager.Character.Data.TotalSpeed:F1}", labelStyle);
        GUILayout.Label($"Motor Speed: {playerManager.Motor.CurrentSpeedNormalized:F2}", labelStyle);

        GUILayout.EndArea();
    }
}
