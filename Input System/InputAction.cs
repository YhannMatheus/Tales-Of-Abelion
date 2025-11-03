using UnityEngine;

/// <summary>
/// Representa uma ação de input configurável com múltiplas teclas
/// </summary>
[System.Serializable]
public class InputAction
{
    [Header("Identificação")]
    [Tooltip("Nome da ação (para debug e UI)")]
    public string actionName;
    
    [Tooltip("Descrição da ação")]
    public string description;

    [Header("Keybindings")]
    [Tooltip("Tecla primária")]
    public KeyCode primaryKey;
    
    [Tooltip("Tecla alternativa (opcional)")]
    public KeyCode alternativeKey = KeyCode.None;
    
    [Tooltip("Botão do mouse (opcional, -1 = nenhum)")]
    [Range(-1, 2)]
    public int mouseButton = -1;

    [Header("Configurações")]
    [Tooltip("Tipo de input")]
    public InputType inputType = InputType.Button;
    
    [Tooltip("Requer tecla modificadora? (Shift, Ctrl, Alt)")]
    public bool requiresModifier;
    
    [Tooltip("Tecla modificadora necessária")]
    public ModifierKey modifierKey = ModifierKey.None;

    // Estado interno
    private bool _wasPressed;

    /// <summary>
    /// Verifica se a ação foi pressionada neste frame (GetKeyDown)
    /// </summary>
    public bool IsPressed()
    {
        if (requiresModifier && !CheckModifier())
            return false;

        bool primaryPressed = primaryKey != KeyCode.None && Input.GetKeyDown(primaryKey);
        bool alternativePressed = alternativeKey != KeyCode.None && Input.GetKeyDown(alternativeKey);
        bool mousePressed = mouseButton >= 0 && Input.GetMouseButtonDown(mouseButton);

        return primaryPressed || alternativePressed || mousePressed;
    }

    /// <summary>
    /// Verifica se a ação está sendo segurada (GetKey)
    /// </summary>
    public bool IsHeld()
    {
        if (requiresModifier && !CheckModifier())
            return false;

        bool primaryHeld = primaryKey != KeyCode.None && Input.GetKey(primaryKey);
        bool alternativeHeld = alternativeKey != KeyCode.None && Input.GetKey(alternativeKey);
        bool mouseHeld = mouseButton >= 0 && Input.GetMouseButton(mouseButton);

        return primaryHeld || alternativeHeld || mouseHeld;
    }

    /// <summary>
    /// Verifica se a ação foi solta neste frame (GetKeyUp)
    /// </summary>
    public bool IsReleased()
    {
        bool primaryReleased = primaryKey != KeyCode.None && Input.GetKeyUp(primaryKey);
        bool alternativeReleased = alternativeKey != KeyCode.None && Input.GetKeyUp(alternativeKey);
        bool mouseReleased = mouseButton >= 0 && Input.GetMouseButtonUp(mouseButton);

        return primaryReleased || alternativeReleased || mouseReleased;
    }

    /// <summary>
    /// Retorna o valor do input baseado no tipo
    /// </summary>
    public bool GetValue()
    {
        switch (inputType)
        {
            case InputType.Button:
                return IsPressed();
            case InputType.ButtonHold:
                return IsHeld();
            default:
                return IsPressed();
        }
    }

    /// <summary>
    /// Verifica se a tecla modificadora está pressionada
    /// </summary>
    private bool CheckModifier()
    {
        switch (modifierKey)
        {
            case ModifierKey.Shift:
                return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            case ModifierKey.Ctrl:
                return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            case ModifierKey.Alt:
                return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            default:
                return true;
        }
    }

    /// <summary>
    /// Retorna string com as teclas configuradas
    /// </summary>
    public string GetBindingString()
    {
        string result = "";

        if (requiresModifier && modifierKey != ModifierKey.None)
        {
            result += modifierKey.ToString() + " + ";
        }

        if (primaryKey != KeyCode.None)
        {
            result += primaryKey.ToString();
        }

        if (alternativeKey != KeyCode.None)
        {
            result += (result.Length > 0 ? " / " : "") + alternativeKey.ToString();
        }

        if (mouseButton >= 0)
        {
            result += (result.Length > 0 ? " / " : "") + "Mouse" + mouseButton;
        }

        return result;
    }

    /// <summary>
    /// Troca a tecla primária
    /// </summary>
    public void SetPrimaryKey(KeyCode newKey)
    {
        primaryKey = newKey;
    }

    /// <summary>
    /// Troca a tecla alternativa
    /// </summary>
    public void SetAlternativeKey(KeyCode newKey)
    {
        alternativeKey = newKey;
    }
}

/// <summary>
/// Tipo de input
/// </summary>
public enum InputType
{
    Button,      // Pressionar (GetKeyDown)
    ButtonHold   // Segurar (GetKey)
}

/// <summary>
/// Teclas modificadoras
/// </summary>
public enum ModifierKey
{
    None,
    Shift,
    Ctrl,
    Alt
}
