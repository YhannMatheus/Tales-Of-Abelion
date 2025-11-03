using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema centralizado de inputs configurável via Inspector
/// Focado em PC com suporte a rebinding
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Movement Axes")]
    [SerializeField] private InputAxis horizontalAxis = new InputAxis
    {
        axisName = "Horizontal",
        unityAxisName = "Horizontal",
        useUnityAxis = true,
        positiveKey = KeyCode.D,
        positiveKeyAlt = KeyCode.RightArrow,
        negativeKey = KeyCode.A,
        negativeKeyAlt = KeyCode.LeftArrow,
        smoothing = 10f,
        deadZone = 0.1f
    };

    [SerializeField] private InputAxis verticalAxis = new InputAxis
    {
        axisName = "Vertical",
        unityAxisName = "Vertical",
        useUnityAxis = true,
        positiveKey = KeyCode.W,
        positiveKeyAlt = KeyCode.UpArrow,
        negativeKey = KeyCode.S,
        negativeKeyAlt = KeyCode.DownArrow,
        smoothing = 10f,
        deadZone = 0.1f
    };

    [Header("Game Actions")]
    [SerializeField] private InputAction interactAction = new InputAction
    {
        actionName = "Interact",
        description = "Interagir com objetos",
        primaryKey = KeyCode.E,
        mouseButton = 1,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction interactHoldAction = new InputAction
    {
        actionName = "Interact Hold",
        description = "Segurar para interagir (revive, etc)",
        primaryKey = KeyCode.E,
        mouseButton = 1,
        inputType = InputType.ButtonHold
    };

    [SerializeField] private InputAction attackAction = new InputAction
    {
        actionName = "Attack",
        description = "Ataque básico",
        primaryKey = KeyCode.None,
        mouseButton = 0,
        inputType = InputType.ButtonHold
    };

    [SerializeField] private InputAction dodgeAction = new InputAction
    {
        actionName = "Dodge",
        description = "Esquiva/Dash",
        primaryKey = KeyCode.Space,
        inputType = InputType.Button
    };

    [Header("Interface Actions")]
    [SerializeField] private InputAction pauseAction = new InputAction
    {
        actionName = "Pause",
        description = "Pausar jogo",
        primaryKey = KeyCode.Escape,
        alternativeKey = KeyCode.P,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction inventoryAction = new InputAction
    {
        actionName = "Inventory",
        description = "Abrir inventário",
        primaryKey = KeyCode.I,
        alternativeKey = KeyCode.Tab,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction worldMapAction = new InputAction
    {
        actionName = "World Map",
        description = "Abrir mapa",
        primaryKey = KeyCode.M,
        alternativeKey = KeyCode.U,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction characterMenuAction = new InputAction
    {
        actionName = "Character Menu",
        description = "Abrir menu de personagem",
        primaryKey = KeyCode.C,
        alternativeKey = KeyCode.O,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction skillTreeAction = new InputAction
    {
        actionName = "Skill Tree",
        description = "Abrir árvore de habilidades",
        primaryKey = KeyCode.K,
        alternativeKey = KeyCode.J,
        inputType = InputType.Button
    };

    [Header("Ability Actions")]
    [SerializeField] private InputAction basicAttackAction = new InputAction
    {
        actionName = "Basic Attack",
        description = "Ataque básico (slot 0)",
        primaryKey = KeyCode.None,
        mouseButton = 0,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction ability1Action = new InputAction
    {
        actionName = "Ability 1",
        description = "Habilidade 1",
        primaryKey = KeyCode.Q,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction ability2Action = new InputAction
    {
        actionName = "Ability 2",
        description = "Habilidade 2",
        primaryKey = KeyCode.E,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction ability3Action = new InputAction
    {
        actionName = "Ability 3",
        description = "Habilidade 3",
        primaryKey = KeyCode.R,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction ability4Action = new InputAction
    {
        actionName = "Ability 4",
        description = "Habilidade 4",
        primaryKey = KeyCode.F,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction ability5Action = new InputAction
    {
        actionName = "Ability 5",
        description = "Habilidade 5",
        primaryKey = KeyCode.Alpha1,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction ability6Action = new InputAction
    {
        actionName = "Ability 6",
        description = "Habilidade 6",
        primaryKey = KeyCode.Alpha2,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction ability7Action = new InputAction
    {
        actionName = "Ability 7",
        description = "Habilidade 7",
        primaryKey = KeyCode.Alpha3,
        inputType = InputType.Button
    };

    [SerializeField] private InputAction ability8Action = new InputAction
    {
        actionName = "Ability 8",
        description = "Habilidade 8",
        primaryKey = KeyCode.Alpha4,
        inputType = InputType.Button
    };

    [Header("Settings")]
    [SerializeField] private bool enableInputLogging = false;

    // Propriedades públicas (compatibilidade com código existente)
    public float horizontalInput => horizontalAxis.GetValue();
    public float verticalInput => verticalAxis.GetValue();
    public bool interactButton => interactAction.IsPressed();
    public bool interactButtonHold => interactHoldAction.IsHeld();
    public bool attackInput => attackAction.IsHeld();
    public bool pauseInput => pauseAction.IsPressed();
    public bool inventoryInput => inventoryAction.IsPressed();
    public bool worldMapInput => worldMapAction.IsPressed();
    public bool characterMenuInput => characterMenuAction.IsPressed();
    public bool skillTreeInput => skillTreeAction.IsPressed();
    public bool basicAttackInput => basicAttackAction.IsPressed();
    public bool ability1Input => ability1Action.IsPressed();
    public bool ability2Input => ability2Action.IsPressed();
    public bool ability3Input => ability3Action.IsPressed();
    public bool ability4Input => ability4Action.IsPressed();
    public bool ability5Input => ability5Action.IsPressed();
    public bool ability6Input => ability6Action.IsPressed();
    public bool ability7Input => ability7Action.IsPressed();
    public bool ability8Input => ability8Action.IsPressed();
    public bool dodgeInput => dodgeAction.IsPressed();

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
            { "Dodge", dodgeAction },
            { "Pause", pauseAction },
            { "Inventory", inventoryAction },
            { "WorldMap", worldMapAction },
            { "CharacterMenu", characterMenuAction },
            { "SkillTree", skillTreeAction },
            { "BasicAttack", basicAttackAction },
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

    /// <summary>
    /// Retorna uma ação por nome
    /// </summary>
    public InputAction GetAction(string actionName)
    {
        if (_actionMap != null && _actionMap.TryGetValue(actionName, out InputAction action))
        {
            return action;
        }
        Debug.LogWarning($"[InputManager] Ação '{actionName}' não encontrada!");
        return null;
    }

    /// <summary>
    /// Retorna um eixo por nome
    /// </summary>
    public InputAxis GetAxis(string axisName)
    {
        if (_axisMap != null && _axisMap.TryGetValue(axisName, out InputAxis axis))
        {
            return axis;
        }
        Debug.LogWarning($"[InputManager] Eixo '{axisName}' não encontrado!");
        return null;
    }

    /// <summary>
    /// Rebind de tecla primária para uma ação
    /// </summary>
    public void RebindPrimaryKey(string actionName, KeyCode newKey)
    {
        InputAction action = GetAction(actionName);
        if (action != null)
        {
            action.SetPrimaryKey(newKey);
            Debug.Log($"[InputManager] Ação '{actionName}' revinculada para {newKey}");
        }
    }

    /// <summary>
    /// Rebind de tecla alternativa para uma ação
    /// </summary>
    public void RebindAlternativeKey(string actionName, KeyCode newKey)
    {
        InputAction action = GetAction(actionName);
        if (action != null)
        {
            action.SetAlternativeKey(newKey);
            Debug.Log($"[InputManager] Tecla alternativa de '{actionName}' revinculada para {newKey}");
        }
    }

    /// <summary>
    /// Retorna todas as ações configuradas
    /// </summary>
    public Dictionary<string, InputAction> GetAllActions()
    {
        return new Dictionary<string, InputAction>(_actionMap);
    }

    /// <summary>
    /// Desabilita todos os inputs (útil para cutscenes, diálogos, etc)
    /// </summary>
    public void DisableAllInputs()
    {
        enabled = false;
    }

    /// <summary>
    /// Reabilita todos os inputs
    /// </summary>
    public void EnableAllInputs()
    {
        enabled = true;
    }

    // ========== Debug ==========

    private void OnGUI()
    {
        if (!enableInputLogging) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 600));
        GUILayout.Label("=== INPUT DEBUG ===");
        GUILayout.Label($"Horizontal: {horizontalInput:F2}");
        GUILayout.Label($"Vertical: {verticalInput:F2}");
        GUILayout.Label($"Interact: {interactButton}");
        GUILayout.Label($"Interact Hold: {interactButtonHold}");
        GUILayout.Label($"Attack: {attackInput}");
        GUILayout.Label($"Dodge: {dodgeInput}");
        GUILayout.EndArea();
    }
}
