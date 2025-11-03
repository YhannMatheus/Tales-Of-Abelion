using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Módulo de UI responsável pela exibição de energia do player.
/// Atualiza barra de energia, números, etc.
/// </summary>
public class PlayerEnergyUI : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Barra de energia (Image com fillAmount)")]
    [SerializeField] private Image energyBar;
    
    [Tooltip("Texto mostrando energia atual/máxima")]
    [SerializeField] private Text energyText;

    [Header("Configurações")]
    [Tooltip("Animar a barra de energia suavemente?")]
    [SerializeField] private bool smoothTransition = true;
    
    [Tooltip("Velocidade da animação da barra")]
    [SerializeField] private float transitionSpeed = 5f;

    private float targetFillAmount = 1f;
    private float currentFillAmount = 1f;

    private void Update()
    {
        if (smoothTransition && energyBar != null)
        {
            // Anima suavemente para o valor alvo
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * transitionSpeed);
            energyBar.fillAmount = currentFillAmount;
        }
    }

    /// <summary>
    /// Atualiza a barra de energia
    /// </summary>
    public void UpdateEnergy(float currentEnergy, float maxEnergy)
    {
        if (maxEnergy <= 0) return;

        targetFillAmount = Mathf.Clamp01(currentEnergy / maxEnergy);

        if (!smoothTransition && energyBar != null)
        {
            energyBar.fillAmount = targetFillAmount;
            currentFillAmount = targetFillAmount;
        }

        if (energyText != null)
        {
            energyText.text = $"{Mathf.CeilToInt(currentEnergy)} / {Mathf.CeilToInt(maxEnergy)}";
        }
    }
}
