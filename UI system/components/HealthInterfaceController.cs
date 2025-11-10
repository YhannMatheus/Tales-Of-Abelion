using UnityEngine;
using UnityEngine.UI;

public class HealthInterfaceController: MonoBehaviour
{
    [HideInInspector] public Image healthFillImage;
    [HideInInspector] public CharacterManager CharacterManager;
    [HideInInspector] public float smoothSpeed = 5f;

    // Valor alvo para suavização
    private float targetFillAmount = 1f;

    private void Awake()
    {
        if (CharacterManager == null)
        {
            Debug.LogWarning("[HealthInterfaceController] CharacterManager não atribuído");
        }

        if (healthFillImage == null)
        {
            Debug.LogError("[HealthInterfaceController] healthFillImage não atribuído!");
        }
    }

    private void Start()
    {
        if (CharacterManager != null)
        {
            CharacterManager.OnHealthChanged += UpdateHealthDisplay;
            CharacterManager.OnDeath += HandleDeath;
            CharacterManager.OnRevive += HandleRevive;
            

            var initialHealthArgs = new HealthChangedEventArgs(
                CharacterManager.Data.currentHealth,
                CharacterManager.Data.TotalMaxHealth,
                CharacterManager.Data.currentHealth 
            );
            UpdateHealthDisplay(this, initialHealthArgs);
        }
    }

    private void OnDestroy()
    {
        if (CharacterManager != null)
        {
            CharacterManager.OnHealthChanged -= UpdateHealthDisplay;
            CharacterManager.OnDeath -= HandleDeath;
            CharacterManager.OnRevive -= HandleRevive;
        }
    }

    private void Update()
    {
        // Suaviza a transição do fillAmount
        if (healthFillImage != null && Mathf.Abs(healthFillImage.fillAmount - targetFillAmount) > 0.001f)
        {
            healthFillImage.fillAmount = Mathf.Lerp(
                healthFillImage.fillAmount, 
                targetFillAmount, 
                smoothSpeed * Time.deltaTime
            );
        }
    }

    private void UpdateHealthDisplay(object sender, HealthChangedEventArgs e)
    {
        if (healthFillImage == null) return;
        
        // Calcula porcentagem (0.0 a 1.0) usando os dados do evento
        float healthPercentage = e.MaxHealth > 0 ? (float)e.CurrentHealth / e.MaxHealth : 0f;
        
        // Define valor alvo (será suavizado no Update)
        targetFillAmount = Mathf.Clamp01(healthPercentage);
        
        // Se smoothSpeed for 0, aplica instantaneamente
        if (smoothSpeed <= 0f)
        {
            healthFillImage.fillAmount = targetFillAmount;
        }
    }

    private void HandleDeath(object sender, DeathEventArgs e)
    {
        targetFillAmount = 0f;
        
        if (smoothSpeed <= 0f && healthFillImage != null)
        {
            healthFillImage.fillAmount = 0f;
        }
    }

    private void HandleRevive(object sender, ReviveEventArgs e)
    {
        if (CharacterManager != null)
        {
            // Cria EventArgs manualmente para atualizar display
            var healthArgs = new HealthChangedEventArgs(
                CharacterManager.Data.currentHealth,
                CharacterManager.Data.TotalMaxHealth,
                0 // previousHealth não importa aqui
            );
            UpdateHealthDisplay(this, healthArgs);
        }
    }

    public int GetHealthPercentage()
    {
        if (CharacterManager == null) return 0;
        
        float healthPercentage = (float)CharacterManager.Data.currentHealth / CharacterManager.Data.TotalMaxHealth;
        return Mathf.RoundToInt(healthPercentage * 100);
    }
    
    public void SetHealthImmediate(float fillAmount)
    {
        if (healthFillImage == null) return;
        
        targetFillAmount = Mathf.Clamp01(fillAmount);
        healthFillImage.fillAmount = targetFillAmount;
        
    }
    
    public void SetSmoothSpeed(float speed)
    {
        smoothSpeed = Mathf.Max(0f, speed);
    }
}