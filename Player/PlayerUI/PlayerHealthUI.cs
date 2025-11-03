using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Módulo de UI responsável pela exibição de vida do player.
/// Atualiza barra de vida, números, etc.
/// </summary>
public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Barra de vida (Image com fillAmount)")]
    [SerializeField] private Image healthBar;
    
    [Tooltip("Texto mostrando vida atual/máxima")]
    [SerializeField] private Text healthText;

    [Header("Configurações")]
    [Tooltip("Animar a barra de vida suavemente?")]
    [SerializeField] private bool smoothTransition = true;
    
    [Tooltip("Velocidade da animação da barra")]
    [SerializeField] private float transitionSpeed = 5f;

    private float targetFillAmount = 1f;
    private float currentFillAmount = 1f;

    private void Update()
    {
        if (smoothTransition && healthBar != null)
        {
            // Anima suavemente para o valor alvo
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * transitionSpeed);
            healthBar.fillAmount = currentFillAmount;
        }
    }

    /// <summary>
    /// Atualiza a barra de vida
    /// </summary>
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (maxHealth <= 0) return;

        targetFillAmount = Mathf.Clamp01(currentHealth / maxHealth);

        if (!smoothTransition && healthBar != null)
        {
            healthBar.fillAmount = targetFillAmount;
            currentFillAmount = targetFillAmount;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }
}
