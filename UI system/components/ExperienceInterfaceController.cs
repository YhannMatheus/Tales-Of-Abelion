using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceInterfaceController: MonoBehaviour
{
    [HideInInspector] public Image experienceFillImage;
    [HideInInspector] public TextMeshProUGUI levelText;
    [HideInInspector] public TextMeshProUGUI currentExperienceText;
    [HideInInspector] public CharacterManager CharacterManager;
    [HideInInspector] public float smoothSpeed = 5f;
    private float targetFillAmount = 0f;
    
    private void Awake()
    {
        if (CharacterManager == null)
        {
            Debug.LogWarning("[ExperienceInterfaceController] CharacterManager não atribuído");
        }

        if (experienceFillImage == null)
        {
            Debug.LogError("[ExperienceInterfaceController] experienceFillImage não atribuído!");
        }
    }

    private void Start()
    {
        if (CharacterManager != null)
        {
            CharacterManager.OnExperienceGained += UpdateExperienceDisplay;
            CharacterManager.OnLevelUp += HandleLevelUp;
            
            var initialExpArgs = new ExperienceGainedEventArgs(
                0,
                CharacterManager.Data.experiencePoints,
                CharacterManager.Data.experienceToNextLevel,
                CharacterManager.Data.level
            );
            UpdateExperienceDisplay(this, initialExpArgs);
        }
    }

    private void OnDestroy()
    {
        if (CharacterManager != null)
        {
            CharacterManager.OnExperienceGained -= UpdateExperienceDisplay;
            CharacterManager.OnLevelUp -= HandleLevelUp;
        }
    }

    private void Update()
    {
        // Suaviza a transição do fillAmount
        if (experienceFillImage != null && Mathf.Abs(experienceFillImage.fillAmount - targetFillAmount) > 0.001f)
        {
            experienceFillImage.fillAmount = Mathf.Lerp(
                experienceFillImage.fillAmount, 
                targetFillAmount, 
                smoothSpeed * Time.deltaTime
            );
        }
    }

    private void UpdateExperienceDisplay(object sender, ExperienceGainedEventArgs e)
    {
        if (experienceFillImage == null) return;
        
        // Calcula porcentagem (0.0 a 1.0) usando progresso até próximo nível
        float expPercentage = e.ProgressToNextLevel;
        
        // Define valor alvo (será suavizado no Update)
        targetFillAmount = Mathf.Clamp01(expPercentage);
        
        // Se smoothSpeed for 0, aplica instantaneamente
        if (smoothSpeed <= 0f)
        {
            experienceFillImage.fillAmount = targetFillAmount;
        }

        // Atualiza texto de XP atual
        UpdateExperienceText(e.CurrentExperience, e.ExperienceToNextLevel);
    }

    private void HandleLevelUp(object sender, LevelUpEventArgs e)
    {
        // Reseta barra ao subir de nível (começa do zero)
        targetFillAmount = 0f;
        
        if (smoothSpeed <= 0f && experienceFillImage != null)
        {
            experienceFillImage.fillAmount = 0f;
        }

        // Atualiza texto de level
        UpdateLevelText(e.NewLevel);
    }

    // Atualiza o texto do nível
    private void UpdateLevelText(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Nível {level}";
        }
    }

    // Atualiza o texto de XP (formato: "atual/máximo")
    private void UpdateExperienceText(int currentExp, int maxExp)
    {
        if (currentExperienceText != null)
        {
            currentExperienceText.text = $"{currentExp}/{maxExp}";
        }
    }

    public int GetExperiencePercentage()
    {
        if (CharacterManager == null) return 0;
        
        float expPercentage = CharacterManager.Data.experienceToNextLevel > 0 
            ? (float)CharacterManager.Data.experiencePoints / CharacterManager.Data.experienceToNextLevel 
            : 0f;
        return Mathf.RoundToInt(expPercentage * 100);
    }
    
    public void SetExperienceImmediate(float fillAmount)
    {
        if (experienceFillImage == null) return;
        
        targetFillAmount = Mathf.Clamp01(fillAmount);
        experienceFillImage.fillAmount = targetFillAmount;
    }
    
    public void SetSmoothSpeed(float speed)
    {
        smoothSpeed = Mathf.Max(0f, speed);
    }
}