using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Módulo de UI responsável pela exibição de energia do player.
/// Atualiza slider de energia, números, etc.
/// Compatível com UI: Canvas/GUI Container/Status Background/Slider (Energia)
/// </summary>
public class PlayerEnergyUI : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Slider de energia (UI Slider)")]
    [SerializeField] private Slider energySlider;
    
    [Tooltip("Texto mostrando energia atual/máxima")]
    [SerializeField] private TextMeshProUGUI energyText;

    [Header("Configurações")]
    [Tooltip("Animar a barra de energia suavemente?")]
    [SerializeField] private bool smoothTransition = true;
    
    [Tooltip("Velocidade da animação da barra")]
    [SerializeField] private float transitionSpeed = 5f;

    private float targetValue = 1f;
    private float currentValue = 1f;

    private void Update()
    {
        if (smoothTransition && energySlider != null)
        {
            // Anima suavemente para o valor alvo
            currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * transitionSpeed);
            energySlider.value = currentValue;
        }
    }

    /// <summary>
    /// Atualiza a barra de energia
    /// </summary>
    public void UpdateEnergy(int currentEnergy, int maxEnergy)
    {
        if (maxEnergy <= 0) return;

        targetValue = Mathf.Clamp01((float)currentEnergy / maxEnergy);

        if (energySlider != null)
        {
            if (!smoothTransition)
            {
                energySlider.value = targetValue;
                currentValue = targetValue;
            }
        }

        if (energyText != null)
        {
            energyText.text = $"{currentEnergy} / {maxEnergy}";
        }
    }
}
