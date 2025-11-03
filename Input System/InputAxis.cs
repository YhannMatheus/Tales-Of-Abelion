using UnityEngine;

/// <summary>
/// Representa um eixo de movimento (Horizontal/Vertical)
/// </summary>
[System.Serializable]
public class InputAxis
{
    [Header("Identificação")]
    public string axisName;
    
    [Header("Teclas Positivas (Cima/Direita)")]
    public KeyCode positiveKey;
    public KeyCode positiveKeyAlt = KeyCode.None;
    
    [Header("Teclas Negativas (Baixo/Esquerda)")]
    public KeyCode negativeKey;
    public KeyCode negativeKeyAlt = KeyCode.None;

    [Header("Configurações")]
    [Tooltip("Usar Input.GetAxis() do Unity (suavização automática)?")]
    public bool useUnityAxis = true;
    
    [Tooltip("Nome do eixo no Input Manager do Unity")]
    public string unityAxisName = "Horizontal";
    
    [Tooltip("Velocidade de suavização (0 = sem suavização)")]
    [Range(0f, 20f)]
    public float smoothing = 10f;
    
    [Tooltip("Zona morta (dead zone)")]
    [Range(0f, 0.5f)]
    public float deadZone = 0.1f;

    private float _currentValue;
    private float _targetValue;

    /// <summary>
    /// Retorna o valor do eixo (-1 a 1)
    /// </summary>
    public float GetValue()
    {
        if (useUnityAxis)
        {
            return Input.GetAxis(unityAxisName);
        }

        // Calcula valor bruto baseado nas teclas
        float rawValue = GetRawValue();

        // Aplica suavização
        if (smoothing > 0)
        {
            _targetValue = rawValue;
            _currentValue = Mathf.MoveTowards(_currentValue, _targetValue, smoothing * Time.deltaTime);
            
            // Aplica dead zone
            if (Mathf.Abs(_currentValue) < deadZone)
            {
                _currentValue = 0f;
            }
            
            return _currentValue;
        }

        // Aplica dead zone no valor bruto
        if (Mathf.Abs(rawValue) < deadZone)
        {
            return 0f;
        }

        return rawValue;
    }

    /// <summary>
    /// Retorna o valor bruto sem suavização (-1, 0, ou 1)
    /// </summary>
    public float GetRawValue()
    {
        if (useUnityAxis)
        {
            return Input.GetAxisRaw(unityAxisName);
        }

        float value = 0f;

        // Verifica teclas positivas
        if (Input.GetKey(positiveKey) || (positiveKeyAlt != KeyCode.None && Input.GetKey(positiveKeyAlt)))
        {
            value += 1f;
        }

        // Verifica teclas negativas
        if (Input.GetKey(negativeKey) || (negativeKeyAlt != KeyCode.None && Input.GetKey(negativeKeyAlt)))
        {
            value -= 1f;
        }

        return Mathf.Clamp(value, -1f, 1f);
    }

    /// <summary>
    /// Reseta o estado interno
    /// </summary>
    public void Reset()
    {
        _currentValue = 0f;
        _targetValue = 0f;
    }
}
