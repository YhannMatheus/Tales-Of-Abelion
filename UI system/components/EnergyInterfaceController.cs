using UnityEngine;
using UnityEngine.UI;

public class EnergyInterfaceController: MonoBehaviour
{
    [HideInInspector] public Image energyFillImage;
    [HideInInspector] public CharacterManager CharacterManager;
    [HideInInspector] public float smoothSpeed = 5f;

    // Valor alvo para suavização
    private float targetFillAmount = 1f;

    private void Awake()
    {
        if (CharacterManager == null)
        {
            Debug.LogWarning("[EnergyInterfaceController] CharacterManager não atribuído");
        }

        if (energyFillImage == null)
        {
            Debug.LogError("[EnergyInterfaceController] energyFillImage não atribuído!");
        }
    }

    private void Start()
    {
        if (CharacterManager != null)
        {
            CharacterManager.OnEnergyChanged += UpdateEnergyDisplay;
            CharacterManager.OnDeath += HandleDeath;
            CharacterManager.OnRevive += HandleRevive;
            // Inicializa usando o EnergyController (fonte de verdade sincronizada)
            var initialEnergyArgs = new EnergyChangedEventArgs(
                CharacterManager.Energy.CurrentEnergyInt,
                CharacterManager.Energy.MaxEnergyInt,
                CharacterManager.Energy.CurrentEnergyInt
            );
            UpdateEnergyDisplay(this, initialEnergyArgs);
        }
    }

    private void OnDestroy()
    {
        if (CharacterManager != null)
        {
            CharacterManager.OnEnergyChanged -= UpdateEnergyDisplay;
            CharacterManager.OnDeath -= HandleDeath;
            CharacterManager.OnRevive -= HandleRevive;
        }
    }

    private void Update()
    {
        // Suaviza a transição do fillAmount
        if (energyFillImage != null && Mathf.Abs(energyFillImage.fillAmount - targetFillAmount) > 0.001f)
        {
            energyFillImage.fillAmount = Mathf.Lerp(
                energyFillImage.fillAmount, 
                targetFillAmount, 
                smoothSpeed * Time.deltaTime
            );
        }
    }

    private void UpdateEnergyDisplay(object sender, EnergyChangedEventArgs e)
    {
        if (energyFillImage == null) return;
        
        // Calcula porcentagem (0.0 a 1.0) usando os dados do evento
        float energyPercentage = e.MaxEnergy > 0 ? (float)e.CurrentEnergy / e.MaxEnergy : 0f;
        
        // Define valor alvo (será suavizado no Update)
        targetFillAmount = Mathf.Clamp01(energyPercentage);
        
        // Se smoothSpeed for 0, aplica instantaneamente
        if (smoothSpeed <= 0f)
        {
            energyFillImage.fillAmount = targetFillAmount;
        }
    }

    private void HandleDeath(object sender, DeathEventArgs e)
    {
        targetFillAmount = 0f;
        
        if (smoothSpeed <= 0f && energyFillImage != null)
        {
            energyFillImage.fillAmount = 0f;
        }
    }

    private void HandleRevive(object sender, ReviveEventArgs e)
    {
        if (CharacterManager != null)
        {
            var energyArgs = new EnergyChangedEventArgs(
                CharacterManager.Energy.CurrentEnergyInt,
                CharacterManager.Energy.MaxEnergyInt,
                0
            );
            UpdateEnergyDisplay(this, energyArgs);
        }
    }

    public int GetEnergyPercentage()
    {
        if (CharacterManager == null) return 0;
        
        float energyPercentage = CharacterManager.Energy.MaxEnergyInt > 0 ? (float)CharacterManager.Energy.CurrentEnergyInt / CharacterManager.Energy.MaxEnergyInt : 0f;
        return Mathf.RoundToInt(energyPercentage * 100);
    }
    
    public void SetEnergyImmediate(float fillAmount)
    {
        if (energyFillImage == null) return;
        
        targetFillAmount = Mathf.Clamp01(fillAmount);
        energyFillImage.fillAmount = targetFillAmount;
    }
    
    public void SetSmoothSpeed(float speed)
    {
        smoothSpeed = Mathf.Max(0f, speed);
    }
}