using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Módulo de UI responsável pela exibição de experiência do player.
/// Atualiza slider de XP, números, etc.
/// Compatível com UI: Canvas/GUI Container/Status Background/Slider (Experiência)
/// </summary>
public class PlayerExperienceUI : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Slider de experiência (UI Slider)")]
    [SerializeField] private Slider experienceSlider;
    
    [Tooltip("Texto mostrando experiência atual/necessária")]
    [SerializeField] private TextMeshProUGUI experienceText;

    [Header("Configurações")]
    [Tooltip("Animar a barra de XP suavemente?")]
    [SerializeField] private bool smoothTransition = true;
    
    [Tooltip("Velocidade da animação da barra")]
    [SerializeField] private float transitionSpeed = 3f;

    [Header("Level Up Effect")]
    [Tooltip("Mostrar efeito visual ao subir de nível?")]
    [SerializeField] private bool showLevelUpEffect = true;
    
    [Tooltip("Duração do efeito de level up em segundos")]
    [SerializeField] private float levelUpEffectDuration = 1f;

    private float targetValue = 0f;
    private float currentValue = 0f;
    private bool isPlayingLevelUpEffect = false;
    private float levelUpEffectTimer = 0f;

    private void Update()
    {
        // Animação suave do slider
        if (smoothTransition && experienceSlider != null && !isPlayingLevelUpEffect)
        {
            currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * transitionSpeed);
            experienceSlider.value = currentValue;
        }

        // Efeito de level up (barra pisca ou pulsa)
        if (isPlayingLevelUpEffect)
        {
            levelUpEffectTimer += Time.deltaTime;

            if (levelUpEffectTimer >= levelUpEffectDuration)
            {
                isPlayingLevelUpEffect = false;
                levelUpEffectTimer = 0f;
            }
        }
    }

    /// <summary>
    /// Atualiza a barra de experiência
    /// </summary>
    public void UpdateExperience(int currentXP, int xpToNextLevel)
    {
        if (xpToNextLevel <= 0) return;

        targetValue = Mathf.Clamp01((float)currentXP / xpToNextLevel);

        if (experienceSlider != null)
        {
            if (!smoothTransition)
            {
                experienceSlider.value = targetValue;
                currentValue = targetValue;
            }
        }

        if (experienceText != null)
        {
            experienceText.text = $"{currentXP} / {xpToNextLevel} XP";
        }
    }

    /// <summary>
    /// Mostra efeito visual de level up
    /// </summary>
    public void ShowLevelUpEffect()
    {
        if (!showLevelUpEffect) return;

        isPlayingLevelUpEffect = true;
        levelUpEffectTimer = 0f;

        // Reseta a barra para 0 (novo nível começou)
        currentValue = 0f;
        targetValue = 0f;

        if (experienceSlider != null)
        {
            experienceSlider.value = 0f;
        }

        Debug.Log("[PlayerExperienceUI] Efeito de Level Up!");
    }
}
