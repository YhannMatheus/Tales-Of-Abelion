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
                _previousHealth = characterData.currentHealth;
                int regenAmount = Mathf.RoundToInt(characterData.TotalHealthRegen);
                healthController.Heal(regenAmount);
                
                OnHealthChanged?.Invoke(this, new HealthChangedEventArgs(
                    characterData.currentHealth, 
                    characterData.TotalMaxHealth, 
                    _previousHealth));
                
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
                    
                    OnEnergyChanged?.Invoke(this, new EnergyChangedEventArgs(
                        energyController.CurrentEnergyInt,
                        energyController.MaxEnergyInt,
                        _previousEnergy));
                
                _energyRegenTimer = 0f;
            }
        }
    }

    // Aplica dano ao personagem
    public void TakeDamage(float damage, bool isMagical = false, CharacterManager source = null)
    {
        if (!characterData.IsAlive) return;

        _previousHealth = characterData.currentHealth;
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

        OnHealthChanged?.Invoke(this, new HealthChangedEventArgs(
            characterData.currentHealth,
            characterData.TotalMaxHealth,
            _previousHealth));
        
        OnTakeDamage?.Invoke(this, new DamageTakenEventArgs(
            damage,
            finalDamage,
            damageReduction,
            isMagical,
            characterData.currentHealth));

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

        _previousHealth = characterData.currentHealth;
        healthController.Heal(amount);
        
        OnHealthChanged?.Invoke(this, new HealthChangedEventArgs(
            characterData.currentHealth,
            characterData.TotalMaxHealth,
            _previousHealth));
    }
    
    public void RestoreEnergy(int amount)
    {
        if (!characterData.IsAlive) return;
        _previousEnergy = energyController.CurrentEnergyInt;
        energyController.RestoreEnergy(amount);

        OnEnergyChanged?.Invoke(this, new EnergyChangedEventArgs(
            energyController.CurrentEnergyInt,
            energyController.MaxEnergyInt,
            _previousEnergy));
    }

    // Tenta usar energia (para habilidades)
    public bool TrySpendEnergy(int amount)
    {
        if (!characterData.IsAlive) return false;

        _previousEnergy = energyController.CurrentEnergyInt;
        bool success = energyController.TrySpendEnergy(amount);

        if (success)
        {
                OnEnergyChanged?.Invoke(this, new EnergyChangedEventArgs(
                    energyController.CurrentEnergyInt,
                    energyController.MaxEnergyInt,
                    _previousEnergy));
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
            
            OnHealthChanged?.Invoke(this, new HealthChangedEventArgs(
                characterData.currentHealth,
                characterData.TotalMaxHealth,
                _previousHealth));
            
            OnEnergyChanged?.Invoke(this, new EnergyChangedEventArgs(
                energyController.CurrentEnergyInt,
                energyController.MaxEnergyInt,
                _previousEnergy));
        }
    }

    // Adiciona bônus de equipamento usando enum (recomendado - type-safe)
    public void ApplyEquipmentBonus(ModifierVar variable, float value)
    {
        modifiersController.ApplyEquipmentModifier(variable, value);
        
        if (variable == ModifierVar.maxHealth)
        {
            OnHealthChanged?.Invoke(this, new HealthChangedEventArgs(
                characterData.currentHealth,
                characterData.TotalMaxHealth,
                _previousHealth));
        }
        else if (variable == ModifierVar.maxEnergy)
        {
            OnEnergyChanged?.Invoke(this, new EnergyChangedEventArgs(
                energyController.CurrentEnergyInt,
                energyController.MaxEnergyInt,
                _previousEnergy));
        }
    }

    public void RemoveEquipmentBonus(ModifierVar variable, float value)
    {
        ApplyEquipmentBonus(variable, -value);
    }
    
    protected void Die()
    {
        healthController.SetCurrentHealth(0);
        
        OnHealthChanged?.Invoke(this, new HealthChangedEventArgs(
            characterData.currentHealth,
            characterData.TotalMaxHealth,
            _previousHealth));
        
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

        OnHealthChanged?.Invoke(this, new HealthChangedEventArgs(
            characterData.currentHealth,
            characterData.TotalMaxHealth,
            _previousHealth));
        
        OnEnergyChanged?.Invoke(this, new EnergyChangedEventArgs(
            energyController.CurrentEnergyInt,
            energyController.MaxEnergyInt,
            _previousEnergy));
        
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
        energyController.Cleanup();
    }

    private void OnValidate()
    {
        energyController.ValidateValues();
    }
}
