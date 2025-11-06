using UnityEngine;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;

public class Character : MonoBehaviour
{
    [Header("Character Sheet")] // Ficha do personagem
    [SerializeField] private CharacterData characterData = new CharacterData();

    [Header("Settings")] // Configurações
    [SerializeField] private bool initializeOnStart = true;
    [SerializeField] private bool enableHealthRegen = true;
    [SerializeField] private bool enableEnergyRegen = true;
    
    [Header("Regeneration Settings")]
    [Tooltip("Intervalo em segundos entre cada tick de regeneração de HP")]
    [SerializeField] private float healthRegenInterval = 1f;
    [Tooltip("Intervalo em segundos entre cada tick de regeneração de Energia")]
    [SerializeField] private float energyRegenInterval = 1f;

    // ⚠️ DEPRECATED: Use os novos eventos com EventArgs para informações completas
    // Mantidos para backward compatibility
    public event Action<int, int> OnHealthChanged;  // (currentHealth, maxHealth)
    public event Action<int, int> OnEnergyChanged;  // (currentEnergy, maxEnergy)
    public event Action<int> OnLevelUp;             // (newLevel)
    public event Action<int> OnExperienceGained;    // (experienceGained)
    public event Action OnDeath;
    public event Action OnRevive;
    public event Action OnTakeDamage;               // Evento quando leva dano
    
    // ✅ NOVOS EVENTOS - Com EventArgs (dados completos)
    public event EventHandler<HealthChangedEventArgs> OnHealthChangedEx;
    public event EventHandler<EnergyChangedEventArgs> OnEnergyChangedEx;
    public event EventHandler<LevelUpEventArgs> OnLevelUpEx;
    public event EventHandler<ExperienceGainedEventArgs> OnExperienceGainedEx;
    public event EventHandler<DamageTakenEventArgs> OnDamageTakenEx;
    public event EventHandler<DeathEventArgs> OnDeathEx;
    public event EventHandler<ReviveEventArgs> OnReviveEx;

    private float _healthRegenTimer = 0f;
    private float _energyRegenTimer = 0f;
    
    // Rastreamento de valores anteriores para EventArgs
    private int _previousHealth;
    private int _previousEnergy;
    private int _previousLevel;

    [Header("Character Type")]
    public CharacterType characterType = CharacterType.Player;

    public CharacterData Data => characterData;

    private void Start()
    { 
        // Apenas Players inicializam automaticamente
        // NPCs são inicializados pelo IAManager
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
        }
    }

    // Inicializa o personagem baseado na raça e classe escolhidas
    public void InitializeCharacter(bool resetHealthAndEnergy = true)
    {
        if (characterData.characterClass == null || characterData.characterRace == null)
        {
            Debug.LogWarning($"[Character] {gameObject.name} não pode ser inicializado: classe ou raça não definida.");
            return;
        }

        characterData.Initialization(resetHealthAndEnergy);

        // Notificar UI
        OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);
        OnEnergyChanged?.Invoke(characterData.currentEnergy, characterData.TotalMaxEnergy);
    }

    /// <summary>
    /// Define a classe e raça do personagem (usado por IAManager para NPCs)
    /// </summary>
    public void SetCharacterData(ClassData classData, RaceData raceData)
    {
        characterData.characterClass = classData;
        characterData.characterRace = raceData;
    }

    /// <summary>
    /// Define configurações de regeneração (usado por IAManager)
    /// </summary>
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
                characterData.Heal(regenAmount);
                
                // Dispara ambos os eventos (compatibilidade + novo)
                OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);
                OnHealthChangedEx?.Invoke(this, new HealthChangedEventArgs(
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
                _previousEnergy = characterData.currentEnergy;
                characterData.RestoreEnergy(characterData.energyRegen);
                
                // Dispara ambos os eventos (compatibilidade + novo)
                OnEnergyChanged?.Invoke(characterData.currentEnergy, characterData.TotalMaxEnergy);
                OnEnergyChangedEx?.Invoke(this, new EnergyChangedEventArgs(
                    characterData.currentEnergy,
                    characterData.TotalMaxEnergy,
                    _previousEnergy));
                
                _energyRegenTimer = 0f;
            }
        }
    }

    // Aplica dano ao personagem
    public void TakeDamage(float damage, bool isMagical = false)
    {
        if (!characterData.IsAlive) return;

        _previousHealth = characterData.currentHealth;
        float resistance = isMagical ? characterData.TotalMagicalResistance : characterData.TotalPhysicalResistance;
        float damageReduction = resistance / (resistance + 100f);
        float finalDamage = damage * (1f - damageReduction);
        
        characterData.TakeDamage(damage, isMagical);

        // Dispara ambos os eventos (compatibilidade + novo)
        OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);
        OnHealthChangedEx?.Invoke(this, new HealthChangedEventArgs(
            characterData.currentHealth,
            characterData.TotalMaxHealth,
            _previousHealth));
        
        OnTakeDamage?.Invoke();
        OnDamageTakenEx?.Invoke(this, new DamageTakenEventArgs(
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
        characterData.Heal(amount);
        
        // Dispara ambos os eventos (compatibilidade + novo)
        OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);
        OnHealthChangedEx?.Invoke(this, new HealthChangedEventArgs(
            characterData.currentHealth,
            characterData.TotalMaxHealth,
            _previousHealth));
    }
    
    public void RestoreEnergy(int amount)
    {
        if (!characterData.IsAlive) return;

        _previousEnergy = characterData.currentEnergy;
        characterData.RestoreEnergy(amount);
        
        // Dispara ambos os eventos (compatibilidade + novo)
        OnEnergyChanged?.Invoke(characterData.currentEnergy, characterData.TotalMaxEnergy);
        OnEnergyChangedEx?.Invoke(this, new EnergyChangedEventArgs(
            characterData.currentEnergy,
            characterData.TotalMaxEnergy,
            _previousEnergy));
    }

    // Tenta usar energia (para habilidades)
    public bool TrySpendEnergy(int amount)
    {
        if (!characterData.IsAlive) return false;

        _previousEnergy = characterData.currentEnergy;
        bool success = characterData.SpendEnergy(amount);

        if (success)
        {
            // Dispara ambos os eventos (compatibilidade + novo)
            OnEnergyChanged?.Invoke(characterData.currentEnergy, characterData.TotalMaxEnergy);
            OnEnergyChangedEx?.Invoke(this, new EnergyChangedEventArgs(
                characterData.currentEnergy,
                characterData.TotalMaxEnergy,
                _previousEnergy));
        }

        return success;
    }

    // Adiciona experiência ao personagem
    public void GainExperience(int amount)
    {
        _previousLevel = characterData.level;
        int xpBefore = characterData.experiencePoints;

        characterData.GainExperience(amount);
        
        // Dispara ambos os eventos (compatibilidade + novo)
        OnExperienceGained?.Invoke(amount);
        OnExperienceGainedEx?.Invoke(this, new ExperienceGainedEventArgs(
            amount,
            characterData.experiencePoints,
            characterData.experienceToNextLevel,
            characterData.level));

        // Verificar se subiu de nível
        if (characterData.level > _previousLevel)
        {
            int levelsGained = characterData.level - _previousLevel;

            // Dispara ambos os eventos (compatibilidade + novo)
            OnLevelUp?.Invoke(characterData.level);
            OnLevelUpEx?.Invoke(this, new LevelUpEventArgs(
                characterData.level,
                _previousLevel,
                characterData.experienceToNextLevel,
                characterData.experiencePoints));
            
            OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);
            OnHealthChangedEx?.Invoke(this, new HealthChangedEventArgs(
                characterData.currentHealth,
                characterData.TotalMaxHealth,
                _previousHealth));
            
            OnEnergyChanged?.Invoke(characterData.currentEnergy, characterData.TotalMaxEnergy);
            OnEnergyChangedEx?.Invoke(this, new EnergyChangedEventArgs(
                characterData.currentEnergy,
                characterData.TotalMaxEnergy,
                _previousEnergy));
        }
    }

    // Adiciona bônus de equipamento usando enum (recomendado - type-safe)
    public void ApplyEquipmentBonus(ModifierVar variable, float value)
    {
        characterData.ApplyEquipmentModifier(variable, value);
        
        // Atualizar UI se for maxHealth ou maxEnergy
        if (variable == ModifierVar.maxHealth)
        {
            OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);
        }
        else if (variable == ModifierVar.maxEnergy)
        {
            OnEnergyChanged?.Invoke(characterData.currentEnergy, characterData.TotalMaxEnergy);
        }
    }

    // Remove bônus de equipamento usando enum (recomendado - type-safe)
    public void RemoveEquipmentBonus(ModifierVar variable, float value)
    {
        ApplyEquipmentBonus(variable, -value);
    }

    // ⚠️ DEPRECATED: Use ApplyEquipmentBonus(ModifierVar, float) em vez de string
    // Mantido para backward compatibility
    public void ApplyEquipmentBonus(string stat, float value)
    {
        // Converte string para enum
        ModifierVar? variable = ParseModifierVar(stat);
        
        if (variable.HasValue)
        {
            ApplyEquipmentBonus(variable.Value, value);
        }
        else
        {
            Debug.LogWarning($"[Character] Stat desconhecido em ApplyEquipmentBonus: {stat}");
        }
    }

    // ⚠️ DEPRECATED: Use RemoveEquipmentBonus(ModifierVar, float) em vez de string
    // Mantido para backward compatibility
    public void RemoveEquipmentBonus(string stat, float value)
    {
        ApplyEquipmentBonus(stat, -value);
    }

    // Converte string para ModifierVar enum
    private ModifierVar? ParseModifierVar(string stat)
    {
        switch (stat.ToLower())
        {
            case "physicaldamage": return ModifierVar.physicalDamage;
            case "physicalresistance": return ModifierVar.physicalResistence;
            case "magicaldamage": return ModifierVar.magicalDamage;
            case "magicalresistance": return ModifierVar.magicalResistence;
            case "maxhealth": return ModifierVar.maxHealth;
            case "maxenergy": return ModifierVar.maxEnergy;
            case "speed": return ModifierVar.speed;
            case "criticalchance": return ModifierVar.criticalChance;
            case "criticaldamage": return ModifierVar.criticalDamage;
            case "attackspeed": return ModifierVar.attackSpeed;
            case "luck": return ModifierVar.luck;
            case "healthregen": return ModifierVar.healthRegen;
            default: return null;
        }
    }

    protected void Die()
    {
        characterData.currentHealth = 0;
        
        // Dispara ambos os eventos (compatibilidade + novo)
        OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);
        OnDeath?.Invoke();
        OnDeathEx?.Invoke(this, new DeathEventArgs(
            characterType,
            characterData.characterName,
            characterData.level));

        // Lógica específica de morte por tipo é tratada em IAManager ou PlayerManager (via events)
        // UI de morte responderá ao evento OnDeath via script de UI separado
    }

    public void Revive()
    {
        int restoredHealth = characterData.TotalMaxHealth;
        int restoredEnergy = characterData.TotalMaxEnergy;
        
        characterData.currentHealth = restoredHealth;
        characterData.currentEnergy = restoredEnergy;

        // Dispara ambos os eventos (compatibilidade + novo)
        OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);
        OnEnergyChanged?.Invoke(characterData.currentEnergy, characterData.TotalMaxEnergy);
        OnRevive?.Invoke();
        OnReviveEx?.Invoke(this, new ReviveEventArgs(
            restoredHealth,
            restoredEnergy,
            fullRestore: true));

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }
}
