using UnityEngine;
using System;

public class CharacterManager : MonoBehaviour
{
    [Header("CharacterManager Sheet")] // Ficha do personagem
    [SerializeField] private CharacterData characterData = new CharacterData();

    [Header("Controllers")]
    [SerializeField] private EnergyController energyController = new EnergyController();
    [SerializeField] private HealthController healthController = new HealthController();
    [SerializeField] private ModifiersController modifiersController = new ModifiersController();
    [SerializeField] private ExperienceController experienceController = new ExperienceController();

    [Header("Settings")]
    [SerializeField] private bool initializeOnStart = true;
    [SerializeField] private bool enableHealthRegen = true;
    [SerializeField] private bool enableEnergyRegen = true;
    
    [Header("Regeneration Settings")]
    [Tooltip("Intervalo em segundos entre cada tick de regeneração de HP")]
    [SerializeField] private float healthRegenInterval = 1f;
    
    [Tooltip("Intervalo em segundos entre cada tick de regeneração de Energia")]
    [SerializeField] private float energyRegenInterval = 1f;

    // ✅ Eventos com EventArgs (dados completos)
    public event EventHandler<HealthChangedEventArgs> OnHealthChanged;
    public event EventHandler<EnergyChangedEventArgs> OnEnergyChanged;
    public event EventHandler<LevelUpEventArgs> OnLevelUp;
    public event EventHandler<ExperienceGainedEventArgs> OnExperienceGained;
    public event EventHandler<DamageTakenEventArgs> OnTakeDamage;
    public event EventHandler<DeathEventArgs> OnDeath;
    public event EventHandler<ReviveEventArgs> OnRevive;

    private float _healthRegenTimer = 0f;
    private float _energyRegenTimer = 0f;
    
    // Rastreamento de valores anteriores para EventArgs
    private int _previousHealth;
    private int _previousEnergy;
    private int _previousLevel;

    [Header("CharacterManager Type")]
    public CharacterType characterType = CharacterType.Player;

    public CharacterData Data => characterData;
    public EnergyController Energy => energyController;
    public HealthController Health => healthController;
    // Interfaces expostas para consumo externo (somente-leitura)
    public IHealthController HealthInterface => healthController;
    public IEnergyController EnergyInterface => energyController;
    public IExperienceController ExperienceInterface => experienceController;
    public IModifiersController ModifiersInterface => modifiersController;
    public int ExperiencePoints => characterData.experiencePoints;
    public int ExperienceToNextLevel => characterData.experienceToNextLevel;
    public int Level => characterData.level;
    public EnergyType EnergyType => characterData.energyType;

    private void Awake()
    {
        // Inicializar EnergyController
        energyController.Initialize(this);
        // Inicializar HealthController
        healthController.Initialize(this);
        // Inicializar ModifiersController
        modifiersController.Initialize(this);
        // Inicializar ExperienceController
        experienceController.Initialize(this);
        
        // Subscrever eventos dos controllers para que o Manager seja o re-emissor único
        // EnergyController emite eventos próprios; aqui transformamos em EnergyChangedEventArgs
        energyController.OnManaChanged += HandleEnergyControllerChanged;
        energyController.OnStaminaChanged += HandleEnergyControllerChanged;
        energyController.OnFuryChanged += HandleEnergyControllerChanged;

        // HealthController emitirá eventos locais que o Manager re-emite como HealthChangedEventArgs
        healthController.OnHealthChanged += HandleHealthControllerChanged;
    }

    private void Start()
    { 
        if (initializeOnStart && characterType == CharacterType.Player)
        {
            InitializeCharacter();
        }
    }

    private void Update()
    {
        if (characterData.IsAlive)
        {
            HandleRegeneration();
            
            // Atualizar EnergyController
            energyController.Tick(Time.deltaTime);
        }
    }

    // Inicializa o personagem baseado na raça e classe escolhidas
    public void InitializeCharacter(bool resetHealthAndEnergy = true)
    {
        if (characterData.characterClass == null || characterData.characterRace == null)
        {
            Debug.LogWarning($"[CharacterManager] {gameObject.name} não pode ser inicializado: classe ou raça não definida.");
            return;
        }

        characterData.Initialization(resetHealthAndEnergy);
        experienceController.SyncAfterInitialization();
        energyController.SyncAfterInitialization();

        // Emitir estado inicial de energia via evento do controller (ou emitir manualmente se necessário)
        _previousEnergy = 0;
        OnEnergyChanged?.Invoke(this, new EnergyChangedEventArgs(
            energyController.CurrentEnergyInt,
            energyController.MaxEnergyInt,
            _previousEnergy));
    }

    /// Define a classe e raça do personagem (usado por IAManager para NPCs)
    public void SetCharacterData(ClassData classData, RaceData raceData)
    {
        characterData.characterClass = classData;
        characterData.characterRace = raceData;
    }

    /// Define configurações de regeneração (usado por IAManager)
    public void SetRegenerationSettings(bool healthRegen, bool energyRegen)
    {
        enableHealthRegen = healthRegen;
        enableEnergyRegen = energyRegen;
    }

    // Sistema de regeneração de vida e energia
    private void HandleRegeneration()
    {
        // Regeneração de Vida
        if (enableHealthRegen && !characterData.IsFullHealth)
        {
            _healthRegenTimer += Time.deltaTime;

            // Regenera no intervalo configurado (padrão: 1 segundo)
            if (_healthRegenTimer >= healthRegenInterval)
            {
                _previousHealth = healthController.CurrentHealth;
                int regenAmount = Mathf.RoundToInt(characterData.TotalHealthRegen);
                healthController.Heal(regenAmount);
                // HealthController emits event; Manager will re-emit via handler
                
                _healthRegenTimer = 0f;
            }
        }

        // Regeneração de Energia
        if (enableEnergyRegen && !characterData.IsFullEnergy)
        {
            _energyRegenTimer += Time.deltaTime;

            // Regenera no intervalo configurado (padrão: 1 segundo)
            if (_energyRegenTimer >= energyRegenInterval)
            {
                    _previousEnergy = energyController.CurrentEnergyInt;
                    energyController.RestoreEnergy(characterData.energyRegen);
                    // EnergyController emite seu evento internamente; Manager irá re-emitir via handler
                
                _energyRegenTimer = 0f;
            }
        }
    }

    // Aplica dano ao personagem
    public void TakeDamage(float damage, bool isMagical = false, CharacterManager source = null)
    {
        if (!characterData.IsAlive) return;

        _previousHealth = healthController.CurrentHealth;
        // Resistencia do alvo
        float resistance = isMagical ? characterData.TotalMagicalResistance : characterData.TotalPhysicalResistance;

        // Penetração do atacante (percentual 0-100)
        float penetrationPercent = 0f;
        if (source != null)
        {
            penetrationPercent = isMagical ? source.Data.TotalMagicPenetration : source.Data.TotalArmorPenetration;
        }
        penetrationPercent = Mathf.Clamp(penetrationPercent, 0f, 100f);

        // Resistência efetiva após penetração (reduzida proporcionalmente)
        float effectiveResistance = resistance * (1f - (penetrationPercent / 100f));

        float damageReduction = effectiveResistance / (effectiveResistance + 100f);
        float finalDamage = damage * (1f - damageReduction);

        // Aplica dano final via HealthController
        healthController.TakeDamage(finalDamage);

        // Manager emite detalhes do dano (raw/final/reduction) - HealthChanged será re-emitido pelo handler
        OnTakeDamage?.Invoke(this, new DamageTakenEventArgs(
            damage,
            finalDamage,
            damageReduction,
            isMagical,
            healthController.CurrentHealth));

        // Verificar morte
        if (!characterData.IsAlive)
        {
            Die();
        }
    }

    // Cura o personagem
    public void Heal(int amount)
    {
        if (!characterData.IsAlive) return;

        _previousHealth = healthController.CurrentHealth;
        healthController.Heal(amount);
        // HealthController emite evento local; Manager re-emite via handler
    }
    
    public void RestoreEnergy(int amount)
    {
        if (!characterData.IsAlive) return;
        _previousEnergy = energyController.CurrentEnergyInt;
        energyController.RestoreEnergy(amount);
        // EnergyController emite evento; Manager re-emite via handler
    }

    // Tenta usar energia (para habilidades)
    public bool TrySpendEnergy(int amount)
    {
        if (!characterData.IsAlive) return false;

        _previousEnergy = energyController.CurrentEnergyInt;
        bool success = energyController.TrySpendEnergy(amount);

        if (success)
        {
                // EnergyController emitted its own event; Manager will re-emit via handler
        }

        return success;
    }

    // Adiciona experiência ao personagem
    public void GainExperience(int amount)
    {
        _previousLevel = characterData.level;
        int xpBefore = characterData.experiencePoints;

        experienceController.GainExperience(amount);
        
        OnExperienceGained?.Invoke(this, new ExperienceGainedEventArgs(
            amount,
            characterData.experiencePoints,
            characterData.experienceToNextLevel,
            characterData.level));

        // Verificar se subiu de nível
        if (characterData.level > _previousLevel)
        {
            int levelsGained = characterData.level - _previousLevel;

            OnLevelUp?.Invoke(this, new LevelUpEventArgs(
                characterData.level,
                _previousLevel,
                characterData.experienceToNextLevel,
                characterData.experiencePoints));
            
            // Após LevelUp, controllers já ajustaram seus valores; events serão re-emitidos pelos handlers
        }
    }

    // Adiciona bônus de equipamento usando enum (recomendado - type-safe)
    public void ApplyEquipmentBonus(ModifierVar variable, float value)
    {
        modifiersController.ApplyEquipmentModifier(variable, value);
        
        if (variable == ModifierVar.maxHealth)
        {
            // Recalcula / clamp da vida atual para refletir o novo máximo
            healthController.SetCurrentHealth(healthController.CurrentHealth);
            // HealthController emitirá evento e Manager re-emite via handler
        }
        else if (variable == ModifierVar.maxEnergy)
        {
            // Atualiza valores do EnergyController a partir do CharacterData e forçar clamp
            energyController.SyncAfterInitialization();
            energyController.SetCurrentEnergy(energyController.CurrentEnergyInt);
            // EnergyController emitirá evento e Manager re-emite via handler
        }
    }

    public void RemoveEquipmentBonus(ModifierVar variable, float value)
    {
        ApplyEquipmentBonus(variable, -value);
    }
    
    protected void Die()
    {
        healthController.SetCurrentHealth(0);
        // HealthController emits change; Manager re-emits via handler
        OnDeath?.Invoke(this, new DeathEventArgs(
            characterType,
            characterData.characterName,
            characterData.level));
    }

    public void Revive()
    {
        int restoredHealth = characterData.TotalMaxHealth;
        int restoredEnergy = characterData.TotalMaxEnergy;
        
        healthController.Revive(restoredHealth);
        
        _previousEnergy = energyController.CurrentEnergyInt;
        energyController.SetCurrentEnergy(restoredEnergy);

        // Controllers emit events; Manager will re-emit via handlers
        
        OnRevive?.Invoke(this, new ReviveEventArgs(
            restoredHealth,
            restoredEnergy,
            fullRestore: true));

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe handlers and cleanup controllers
        if (energyController != null)
        {
            energyController.OnManaChanged -= HandleEnergyControllerChanged;
            energyController.OnStaminaChanged -= HandleEnergyControllerChanged;
            energyController.OnFuryChanged -= HandleEnergyControllerChanged;
            energyController.Cleanup();
        }

        if (healthController != null)
        {
            healthController.OnHealthChanged -= HandleHealthControllerChanged;
        }
    }

    private void OnValidate()
    {
        energyController.ValidateValues();
    }

    // Handlers para eventos vindos dos controllers - Manager re-emite eventos centralizados
    private void HandleEnergyControllerChanged(object sender, EnergyTypeChangedEventArgs e)
    {
        int current = Mathf.RoundToInt(e.CurrentValue);
        int max = Mathf.RoundToInt(e.MaxValue);
        int previous = Mathf.RoundToInt(e.PreviousValue);

        OnEnergyChanged?.Invoke(this, new EnergyChangedEventArgs(current, max, previous));
    }

    private void HandleHealthControllerChanged(object sender, HealthChangedEventArgs e)
    {
        // Re-emitir com o Manager como sender
        OnHealthChanged?.Invoke(this, new HealthChangedEventArgs(e.CurrentHealth, e.MaxHealth, e.PreviousHealth));
    }
}
