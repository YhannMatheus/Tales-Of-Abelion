using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Movement Axes (WASD apenas)")]
    [SerializeField] private InputAxis horizontalAxis = new InputAxis
    {
        axisName = "Horizontal",
        unityAxisName = "Horizontal",
        useUnityAxis = true,
        positiveKey = KeyCode.D,
        negativeKey = KeyCode.A,
        smoothing = 10f,
        deadZone = 0.1f
    };

    [SerializeField] private InputAxis verticalAxis = new InputAxis
    {
        axisName = "Vertical",
        unityAxisName = "Vertical",
        useUnityAxis = true,
        positiveKey = KeyCode.W,
        negativeKey = KeyCode.S,
        smoothing = 10f,
        deadZone = 0.1f
    };

    [Header("Game Actions")]
    [SerializeField] private InputAction interactAction = new InputAction
    {
        actionName = "Interact",
        description = "Interagir com eventos/objetos",
        primaryKey = KeyCode.None,
        mouseButton = 0, // Botão esquerdo do mouse
        inputType = InputType.Button
    };

    [SerializeField] private InputAction interactHoldAction = new InputAction
    {
        actionName = "Interact Hold",
        description = "Segurar para interagir (revive, etc)",
        primaryKey = KeyCode.None,
        mouseButton = 0, // Botão esquerdo do mouse (hold)
        inputType = InputType.ButtonHold
    };

    // ⚠️ DESATIVADO - Botão direito agora é usado para movimento (estilo MOBA/ARPG)
    [SerializeField] private InputAction attackAction = new InputAction
    {
        actionName = "Attack",
        description = "Ataque básico (DESATIVADO - use clique direito para mover/atacar)",
        primaryKey = KeyCode.None,
        mouseButton = -1, // Desativado (antes era 1 - botão direito)
        inputType = InputType.ButtonHold
    };

    [SerializeField] private InputAction pingAction = new InputAction
    {
        actionName = "Ping",
        description = "Ping/Marcação",
        primaryKey = KeyCode.None,
        mouseButton = 2, // Botão central do mouse
        inputType = InputType.Button
    };

    [Header("Interface Actions")]
    [SerializeField] private InputAction pauseAction = new InputAction
    {
        actionName = "Pause",
        description = "Pausar jogo",
        primaryKey = KeyCode.Escape,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction inventoryAction = new InputAction
    {
        actionName = "Inventory",
        description = "Abrir inventário",
        primaryKey = KeyCode.I,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction equipmentAction = new InputAction
    {
        actionName = "Equipment",
        description = "Abrir equipamento",
        primaryKey = KeyCode.O,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction worldMapAction = new InputAction
    {
        actionName = "World Map",
        description = "Abrir mapa mundi",
        primaryKey = KeyCode.P,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction skillTreeAction = new InputAction
    {
        actionName = "Skill Tree",
        description = "Abrir árvore de habilidades",
        primaryKey = KeyCode.J,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction skillSelectionAction = new InputAction
    {
        actionName = "Skill Selection",
        description = "Menu de seleção de skill",
        primaryKey = KeyCode.K,
        inputType = InputType.Button
    };

    [Header("Ability Actions (6 Slots)")]
    [SerializeField] private InputAction ability1Action = new InputAction
    {
        actionName = "Ability 1",
        description = "Habilidade Q (Slot 1)",
        primaryKey = KeyCode.Q,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction ability2Action = new InputAction
    {
        actionName = "Ability 2",
        description = "Habilidade W (Slot 2)",
        primaryKey = KeyCode.W,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction ability3Action = new InputAction
    {
        actionName = "Ability 3",
        description = "Habilidade E (Slot 3)",
        primaryKey = KeyCode.E,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction ability4Action = new InputAction
    {
        actionName = "Ability 4",
        description = "Habilidade A (Slot 4)",
        primaryKey = KeyCode.A,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction ability5Action = new InputAction
    {
        actionName = "Ability 5",
        description = "Habilidade S (Slot 5)",
        primaryKey = KeyCode.S,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction ability6Action = new InputAction
    {
        actionName = "Ability 6",
        description = "Habilidade D (Slot 6)",
        primaryKey = KeyCode.D,
        inputType = InputType.Button
    };

    [Header("Extra Slots (Itens/Consumíveis)")]
    [SerializeField] private InputAction ability7Action = new InputAction
    {
        actionName = "Extra Slot 1",
        description = "Slot Extra Z (Itens/Consumíveis)",
        primaryKey = KeyCode.Z,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction ability8Action = new InputAction
    {
        actionName = "Extra Slot 2",
        description = "Slot Extra X (Itens/Consumíveis)",
        primaryKey = KeyCode.X,
        inputType = InputType.Button
    };

    [Header("Settings")]
    [SerializeField] private bool enableInputLogging = false;

    public float horizontalInput => horizontalAxis.GetValue();
    public float verticalInput => verticalAxis.GetValue();
    public bool interactButton => interactAction.IsPressed();
    public bool interactButtonHold => interactHoldAction.IsHeld();
    public bool attackInput => attackAction.IsHeld();
    public bool pingInput => pingAction.IsPressed();
    public bool pauseInput => pauseAction.IsPressed();
    public bool inventoryInput => inventoryAction.IsPressed();
    public bool equipmentInput => equipmentAction.IsPressed();
    public bool worldMapInput => worldMapAction.IsPressed();
    public bool skillTreeInput => skillTreeAction.IsPressed();
    public bool skillSelectionInput => skillSelectionAction.IsPressed();
    // basicAttackInput removido - ataque agora é via clique direito em inimigo
    public bool ability1Input => ability1Action.IsPressed();
    public bool ability2Input => ability2Action.IsPressed();
    public bool ability3Input => ability3Action.IsPressed();
    public bool ability4Input => ability4Action.IsPressed();
    public bool ability5Input => ability5Action.IsPressed();
    public bool ability6Input => ability6Action.IsPressed();
    public bool ability7Input => ability7Action.IsPressed();
    public bool ability8Input => ability8Action.IsPressed();

    // Dicionário para acesso por nome (útil para rebinding)
    private Dictionary<string, InputAction> _actionMap;
    private Dictionary<string, InputAxis> _axisMap;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInputMaps();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeInputMaps()
    {
        _actionMap = new Dictionary<string, InputAction>
        {
            { "Interact", interactAction },
            { "InteractHold", interactHoldAction },
            { "Attack", attackAction },
            { "Ping", pingAction },
            { "Pause", pauseAction },
            { "Inventory", inventoryAction },
            { "Equipment", equipmentAction },
            { "WorldMap", worldMapAction },
            { "SkillTree", skillTreeAction },
            { "SkillSelection", skillSelectionAction },
            // BasicAttack removido - ataque via clique direito
            { "Ability1", ability1Action },
            { "Ability2", ability2Action },
            { "Ability3", ability3Action },
            { "Ability4", ability4Action },
            { "Ability5", ability5Action },
            { "Ability6", ability6Action },
            { "Ability7", ability7Action },
            { "Ability8", ability8Action }
        };

        _axisMap = new Dictionary<string, InputAxis>
        {
            { "Horizontal", horizontalAxis },
            { "Vertical", verticalAxis }
        };
    }

    // ========== Métodos de Acesso por Nome (para rebinding e UI) ==========
    public InputAction GetAction(string actionName)
    {
        if (_actionMap != null && _actionMap.TryGetValue(actionName, out InputAction action))
        {
            return action;
        }
        Debug.LogWarning($"[InputManager] Ação '{actionName}' não encontrada!");
        return null;
    }

    public InputAxis GetAxis(string axisName)
    {
        if (_axisMap != null && _axisMap.TryGetValue(axisName, out InputAxis axis))
        {
            return axis;
        }
        Debug.LogWarning($"[InputManager] Eixo '{axisName}' não encontrado!");
        return null;
    }

    public void RebindPrimaryKey(string actionName, KeyCode newKey)
    {
        InputAction action = GetAction(actionName);
        if (action != null)
        {
            action.SetPrimaryKey(newKey);
            Debug.Log($"[InputManager] Ação '{actionName}' revinculada para {newKey}");
        }
    }

    public void RebindAlternativeKey(string actionName, KeyCode newKey)
    {
        InputAction action = GetAction(actionName);
        if (action != null)
        {
            action.SetAlternativeKey(newKey);
            Debug.Log($"[InputManager] Tecla alternativa de '{actionName}' revinculada para {newKey}");
        }
    }

    public Dictionary<string, InputAction> GetAllActions()
    {
        return new Dictionary<string, InputAction>(_actionMap);
    }

    public void DisableAllInputs()
    {
        enabled = false;
    }

    public void EnableAllInputs()
    {
        enabled = true;
    }

    // ========== Debug ==========

    private void OnGUI()
    {
        if (!enableInputLogging) return;

        GUILayout.BeginArea(new Rect(10, 10, 400, 700));
        GUILayout.Label("=== INPUT DEBUG ===");
        GUILayout.Label("--- Movimento (DESATIVADO - usa Mouse) ---");
        GUILayout.Label($"Horizontal: {horizontalInput:F2} (não usado)");
        GUILayout.Label($"Vertical: {verticalInput:F2} (não usado)");
        GUILayout.Label("--- Actions ---");
        GUILayout.Label($"Interact (LMB): {interactButton}");
        GUILayout.Label($"Interact Hold (LMB): {interactButtonHold}");
        GUILayout.Label($"Attack (RMB): {attackInput} (não usado - clique direito = movimento)");
        GUILayout.Label($"Ping (MMB): {pingInput}");
        GUILayout.Label("--- Interface ---");
        GUILayout.Label($"Pause (Esc): {pauseInput}");
        GUILayout.Label($"Inventory (I): {inventoryInput}");
        GUILayout.Label($"Equipment (O): {equipmentInput}");
        GUILayout.Label($"World Map (P): {worldMapInput}");
        GUILayout.Label($"Skill Tree (J): {skillTreeInput}");
        GUILayout.Label($"Skill Selection (K): {skillSelectionInput}");
        GUILayout.Label("--- Habilidades (6 Slots) ---");
        GUILayout.Label($"Q: {ability1Input} | W: {ability2Input} | E: {ability3Input}");
        GUILayout.Label($"A: {ability4Input} | S: {ability5Input} | D: {ability6Input}");
        GUILayout.Label("--- Slots Extras (Itens/Consumíveis) ---");
        GUILayout.Label($"Z: {ability7Input} | X: {ability8Input}");
        GUILayout.EndArea();
    }
}
