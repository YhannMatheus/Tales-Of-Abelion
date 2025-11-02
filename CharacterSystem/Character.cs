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

    public event Action<int, int> OnHealthChanged;  // (currentHealth, maxHealth)
    public event Action<int, int> OnEnergyChanged;  // (currentEnergy, maxEnergy)
    public event Action<int> OnLevelUp;             // (newLevel)
    public event Action<int> OnExperienceGained;    // (experienceGained)
    public event Action OnDeath;
    public event Action OnRevive;
    public event Action OnTakeDamage;               // Evento quando leva dano

    private float _healthRegenTimer = 0f;
    private float _energyRegenTimer = 0f;

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
    public void InitializeCharacter()
    {
        if (characterData.characterClass == null || characterData.characterRace == null)
        {
            Debug.LogWarning($"[Character] {gameObject.name} não pode ser inicializado: classe ou raça não definida.");
            return;
        }

        characterData.Initialization();

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

            // Regenera a cada 1 segundo
            if (_healthRegenTimer >= 1f)
            {
                int regenAmount = Mathf.RoundToInt(characterData.TotalHealthRegen);
                characterData.Heal(regenAmount);
                OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);
                _healthRegenTimer = 0f;
            }
        }

        // Regeneração de Energia
        if (enableEnergyRegen && !characterData.IsFullEnergy)
        {
            _energyRegenTimer += Time.deltaTime;

            // Regenera a cada 1 segundo
            if (_energyRegenTimer >= 1f)
            {
                characterData.RestoreEnergy(characterData.energyRegen);
                OnEnergyChanged?.Invoke(characterData.currentEnergy, characterData.TotalMaxEnergy);
                _energyRegenTimer = 0f;
            }
        }
    }

    // Aplica dano ao personagem
    public void TakeDamage(float damage, bool isMagical = false)
    {
        if (!characterData.IsAlive) return;

        int healthBefore = characterData.currentHealth;
        characterData.TakeDamage(damage, isMagical);

        OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);
        OnTakeDamage?.Invoke(); // Disparar evento de dano

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

        characterData.Heal(amount);
        OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);
    }
    public void RestoreEnergy(int amount)
    {
        if (!characterData.IsAlive) return;

        characterData.RestoreEnergy(amount);
        OnEnergyChanged?.Invoke(characterData.currentEnergy, characterData.TotalMaxEnergy);
    }

    // Tenta usar energia (para habilidades)
    public bool TrySpendEnergy(int amount)
    {
        if (!characterData.IsAlive) return false;

        bool success = characterData.SpendEnergy(amount);

        if (success)
        {
            OnEnergyChanged?.Invoke(characterData.currentEnergy, characterData.TotalMaxEnergy);
        }

        return success;
    }

    // Adiciona experiência ao personagem
    public void GainExperience(int amount)
    {
        int levelBefore = characterData.level;

        characterData.GainExperience(amount);
        OnExperienceGained?.Invoke(amount);

        // Verificar se subiu de nível
        if (characterData.level > levelBefore)
        {
            int levelsGained = characterData.level - levelBefore;

            OnLevelUp?.Invoke(characterData.level);
            OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);
            OnEnergyChanged?.Invoke(characterData.currentEnergy, characterData.TotalMaxEnergy);
        }
    }

    // Adiciona bônus de equipamento
    public void ApplyEquipmentBonus(string stat, float value)
    {
        switch (stat.ToLower())
        {
            case "physicaldamage":
                characterData.equipamentPhysicalDamageBonus += value;
                break;
            case "physicalresistance":
                characterData.equipamentPhysicalResistenceBonus += value;
                break;
            case "magicaldamage":
                characterData.equipamentMagicalDamageBonus += value;
                break;
            case "magicalresistance":
                characterData.equipamentMagicalResistenceBonus += value;
                break;
            case "maxhealth":
                characterData.equipamentMaxHealthBonus += Mathf.RoundToInt(value);
                OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);
                break;
            case "maxenergy":
                characterData.equipamentMaxEnergyBonus += Mathf.RoundToInt(value);
                OnEnergyChanged?.Invoke(characterData.currentEnergy, characterData.TotalMaxEnergy);
                break;
            case "speed":
                characterData.equipamentSpeedBonus += value;
                break;
            case "criticalchance":
                characterData.equipamentCriticalChanceBonus += value;
                break;
            case "criticaldamage":
                characterData.equipamentCriticalDamageBonus += value;
                break;
            case "attackspeed":
                characterData.equipamentAttackSpeedBonus += value;
                break;
            case "luck":
                characterData.equipamentLuckBonus += value;
                break;
            case "healthregen":
                characterData.equipamentHealthRegenBonus += Mathf.RoundToInt(value);
                break;
        }
    }

    // Remove bônus de equipamento
    public void RemoveEquipmentBonus(string stat, float value)
    {
        ApplyEquipmentBonus(stat, -value); // Simplesmente aplica o valor negativo
    }

    protected void Die()
    {
        characterData.currentHealth = 0;
        OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);
        OnDeath?.Invoke();

        // Lógica específica de morte por tipo é tratada em IAManager ou PlayerDeathManager
        if (characterType == CharacterType.Player)
        {
            PlayerDeathManager.Instance?.ShowDeathOverlay(this);
        }
    }

    public void Revive()
    {
        characterData.currentHealth = characterData.TotalMaxHealth;
        characterData.currentEnergy = characterData.TotalMaxEnergy;

        OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);
        OnEnergyChanged?.Invoke(characterData.currentEnergy, characterData.TotalMaxEnergy);
        OnRevive?.Invoke();

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }
}
