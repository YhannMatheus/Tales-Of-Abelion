using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    [Header("Identity")]
    public string characterName;
    public ClassData characterClass;
    public RaceData characterRace;


    [Header("Leveling Information")]
    public int level;
    public int experiencePoints;
    public int experienceToNextLevel;

    [Header("Life Information")]
    public int currentHealth;
    public int maxHealth;
    public float healthRegen;

    [Header("Energy Information")]
    public EnergyType energyType;
    public int currentEnergy;
    public int maxEnergy;
    public int energyRegen;

    [Header("Attributes")]
    public int strength;
    public int dexterity;
    public int intelligence;
    public int charisma;
    public int endurance;
    public int freePoints;

    [Header("Ability")]
    public List<Ability> Ability = new List<Ability>();

    [Header("Modifiers")]
    public float physicalDamage;
    public float physicalResistence;
    public float magicalDamage;
    public float magicalResistence;
    public float criticalChance;
    public float criticalDamage;
    public float attackSpeed;
    public float luck;
    public float speed;

    [Header("Visual Information")]
    public AnimatorController animatorOverrideController;
    public GameObject characterModel;
    public Material characterMaterial;
    public Sprite characterIcon;
    public AnimatorController animatorController;

    [Header("Equipament Modifiers")]
    public float equipamentPhysicalDamageBonus;
    public float equipamentPhysicalResistenceBonus;
    public float equipamentMagicalDamageBonus;
    public float equipamentMagicalResistenceBonus;
    public float equipamentCriticalChanceBonus;
    public float equipamentCriticalDamageBonus;
    public float equipamentAttackSpeedBonus;
    public float equipamentLuckBonus;
    public float equipamentSpeedBonus;
    public int equipamentMaxHealthBonus;
    public int equipamentMaxEnergyBonus;
    public int equipamentHealthRegenBonus;

    [Header("External Modifiers")]
    public float externalPhysicalDamageBonus;
    public float externalPhysicalResistenceBonus;
    public float externalMagicalDamageBonus;
    public float externalMagicalResistenceBonus;
    public float externalCriticalChanceBonus;
    public float externalCriticalDamageBonus;
    public float externalAttackSpeedBonus;
    public float externalLuckBonus;
    public float externalSpeedBonus;
    public int externalMaxHealthBonus;
    public int externalMaxEnergyBonus;
    public int externalHealthRegenBonus;


    // Propriedades calculadas (base + equipamentos + externos)
    public int TotalMaxHealth => maxHealth + equipamentMaxHealthBonus + externalMaxHealthBonus;
    public int TotalMaxEnergy => maxEnergy + equipamentMaxEnergyBonus + externalMaxEnergyBonus;
    public float TotalHealthRegen => healthRegen + equipamentHealthRegenBonus + externalHealthRegenBonus;
    public float TotalPhysicalDamage => physicalDamage + equipamentPhysicalDamageBonus + externalPhysicalDamageBonus;
    public float TotalPhysicalResistance => physicalResistence + equipamentPhysicalResistenceBonus + externalPhysicalResistenceBonus;
    public float TotalMagicalDamage => magicalDamage + equipamentMagicalDamageBonus + externalMagicalDamageBonus;
    public float TotalMagicalResistance => magicalResistence + equipamentMagicalResistenceBonus + externalMagicalResistenceBonus;
    public float TotalCriticalChance => criticalChance + equipamentCriticalChanceBonus + externalCriticalChanceBonus;
    public float TotalCriticalDamage => criticalDamage + equipamentCriticalDamageBonus + externalCriticalDamageBonus;
    public float TotalAttackSpeed => attackSpeed + equipamentAttackSpeedBonus + externalAttackSpeedBonus;
    public float TotalLuck => luck + equipamentLuckBonus + externalLuckBonus;
    public float TotalSpeed => speed + equipamentSpeedBonus + externalSpeedBonus;


    // Inicializa todos os valores base do personagem a partir da raça e classe
    public void Initialization()
    {
        strength = characterRace.Strength + characterClass.StrengthBonus;
        dexterity = characterRace.Dexterity + characterClass.DexterityBonus;
        intelligence = characterRace.Intelligence + characterClass.IntelligenceBonus;
        charisma = characterRace.Charisma + characterClass.CharismaBonus;
        endurance = characterRace.Endurance + characterClass.EnduranceBonus;

        maxHealth = characterClass.baseMaxHealth + (endurance * characterClass.healthMultiplierPerEndurance) + (level * characterClass.healthMultiplierPerLevel);
        currentHealth = TotalMaxHealth;

        energyType = characterClass.energyType;
        maxEnergy = characterClass.baseMaxEnergy;
        currentEnergy = TotalMaxEnergy;

        Ability.Clear();
        Ability.AddRange(characterRace.raceAbility);
        Ability.AddRange(characterClass.classSkillTree);

        physicalDamage = characterClass.physicalDamage;
        physicalResistence = characterClass.physicalResistence;
        magicalDamage = characterClass.magicalDamage;
        magicalResistence = characterClass.magicalResistence;
        criticalChance = characterClass.criticalChance;
        criticalDamage = characterClass.criticalDamage;
        attackSpeed = characterClass.attackSpeed;
        luck = characterClass.luck;
        speed = characterRace.moveSpeed;

        animatorController = characterClass.animatorController;

        experienceToNextLevel = CalculateExperienceForNextLevel();
    }

    // Calcula quantos pontos de experiência são necessários para o próximo nível
    private int CalculateExperienceForNextLevel()
    {
        return Mathf.RoundToInt(100 * Mathf.Pow(level, 1.5f));
    }

    // Aumenta o nível do personagem quando tem experiência suficiente
    public void LevelUp()
    {
        level++;
        experiencePoints -= experienceToNextLevel;
        experienceToNextLevel = CalculateExperienceForNextLevel();

        currentHealth = TotalMaxHealth;
        currentEnergy = TotalMaxEnergy;
    }

    public void GainExperience(int amount)
    {
        experiencePoints += amount;

        while (experiencePoints >= experienceToNextLevel)
        {
            LevelUp();
        }
    }

    // Aplica dano ao personagem considerando resistência
    public void TakeDamage(float damage, bool isMagical = false)
    {
        float resistance = isMagical ? TotalMagicalResistance : TotalPhysicalResistance;
        
        // Fórmula de redução: Redução% = Resistência / (Resistência + 100)
        // Exemplo: 50 resistência = 33% redução, 100 resistência = 50% redução
        float damageReduction = resistance / (resistance + 100f);
        float finalDamage = damage * (1f - damageReduction);

        currentHealth = Mathf.Max(0, currentHealth - Mathf.RoundToInt(finalDamage));
    }

    // Cura o personagem (não ultrapassa a vida máxima)
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(TotalMaxHealth, currentHealth + amount);
    }

    // Tenta gastar energia. Retorna true se conseguiu, false se não tinha energia suficiente
    public bool SpendEnergy(int amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            return true;
        }
        return false;
    }

    // Recupera energia (não ultrapassa a energia máxima)
    public void RestoreEnergy(int amount)
    {
        currentEnergy = Mathf.Min(TotalMaxEnergy, currentEnergy + amount);
    }

    // Verifica se o personagem está vivo
    public bool IsAlive => currentHealth > 0;

    // Verifica se o personagem está com vida cheia
    public bool IsFullHealth => currentHealth >= TotalMaxHealth;

    // Verifica se o personagem está com energia cheia
    public bool IsFullEnergy => currentEnergy >= TotalMaxEnergy;

    // Retorna a porcentagem de vida atual (0 a 1)
    public float HealthPercentage => TotalMaxHealth > 0 ? (float)currentHealth / TotalMaxHealth : 0f;

    // Retorna a porcentagem de energia atual (0 a 1)
    public float EnergyPercentage => TotalMaxEnergy > 0 ? (float)currentEnergy / TotalMaxEnergy : 0f;
    
}
