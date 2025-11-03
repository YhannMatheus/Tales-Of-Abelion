using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Módulo de UI responsável pela exibição de vida do player.
/// Atualiza slider de vida, números, etc.
/// Compatível com UI: Canvas/GUI Container/Status Background/Slider (Vida)
/// </summary>
public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Slider de vida (UI Slider)")]
    [SerializeField] private Slider healthSlider;
    
    [Tooltip("Texto mostrando vida atual/máxima")]
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Configurações")]
    [Tooltip("Animar a barra de vida suavemente?")]
    [SerializeField] private bool smoothTransition = true;
    
    [Tooltip("Velocidade da animação da barra")]
    [SerializeField] private float transitionSpeed = 5f;

    private float targetValue = 1f;
    private float currentValue = 1f;

    private void Update()
    {
        if (smoothTransition && healthSlider != null)
        {
            // Anima suavemente para o valor alvo
            currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * transitionSpeed);
            healthSlider.value = currentValue;
        }
    }

    /// <summary>
    /// Atualiza a barra de vida
    /// </summary>
    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (maxHealth <= 0) return;

        targetValue = Mathf.Clamp01((float)currentHealth / maxHealth);

        if (healthSlider != null)
        {
            if (!smoothTransition)
            {
                healthSlider.value = targetValue;
                currentValue = targetValue;
            }
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }
    }
}
