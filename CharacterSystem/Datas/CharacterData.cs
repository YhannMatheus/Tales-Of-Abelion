using System.Collections.Generic;
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

    [Header("Level Progression Settings")]
    public float xpBaseValue = 100f;
    public float xpScalingExponent = 1.5f;

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

    [Header("Skills")]
    public int skillPoints;
    public List<SkillData> Skills = new List<SkillData>();

    [Header("Modifiers")]
    public float physicalDamage;
    public float physicalResistence;
    public float armorPenetration;
    public float magicalDamage;
    public float magicalResistence;
    public float magicPenetration;
    public float criticalChance;
    public float criticalDamage;
    public float attackSpeed;
    public float luck;
    public float speed;

    [Header("Visual Information")]
    public RuntimeAnimatorController animatorController;
    public GameObject characterModel;
    public Material characterMaterial;
    public Sprite characterIcon;

    [Header("Equipament Modifiers")]
    public float equipamentPhysicalDamageBonus;
    public float equipamentPhysicalResistenceBonus;
    public float equipamentArmorPenetrationBonus;
    public float equipamentMagicalDamageBonus;
    public float equipamentMagicalResistenceBonus;
    public float equipamentMagicPenetrationBonus;
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
    public float externalArmorPenetrationBonus;
    public float externalMagicalDamageBonus;
    public float externalMagicalResistenceBonus;
    public float externalMagicPenetrationBonus;
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
    public float TotalArmorPenetration => armorPenetration + equipamentArmorPenetrationBonus + externalArmorPenetrationBonus;
    public float TotalMagicalDamage => magicalDamage + equipamentMagicalDamageBonus + externalMagicalDamageBonus;
    public float TotalMagicalResistance => magicalResistence + equipamentMagicalResistenceBonus + externalMagicalResistenceBonus;
    public float TotalMagicPenetration => magicPenetration + equipamentMagicPenetrationBonus + externalMagicPenetrationBonus;
    public float TotalCriticalChance => criticalChance + equipamentCriticalChanceBonus + externalCriticalChanceBonus;
    public float TotalCriticalDamage => criticalDamage + equipamentCriticalDamageBonus + externalCriticalDamageBonus;
    public float TotalAttackSpeed => attackSpeed + equipamentAttackSpeedBonus + externalAttackSpeedBonus;
    public float TotalLuck => luck + equipamentLuckBonus + externalLuckBonus;
    public float TotalSpeed => speed + equipamentSpeedBonus + externalSpeedBonus;


    // Inicializa todos os valores base do personagem a partir da raça e classe
    public void Initialization(bool resetHealthAndEnergy = true)
    {
        strength = characterRace.Strength + characterClass.StrengthBonus;
        dexterity = characterRace.Dexterity + characterClass.DexterityBonus;
        intelligence = characterRace.Intelligence + characterClass.IntelligenceBonus;
        charisma = characterRace.Charisma + characterClass.CharismaBonus;
        endurance = characterRace.Endurance + characterClass.EnduranceBonus;

        maxHealth = characterClass.baseMaxHealth + (endurance * characterClass.healthMultiplierPerEndurance) + (level * characterClass.healthMultiplierPerLevel);
        
        // Apenas reseta HP/Energia se for a primeira vez (protege contra exploit de re-init)
        if (resetHealthAndEnergy)
        {
            currentHealth = TotalMaxHealth;
            currentEnergy = TotalMaxEnergy;
        }
        else
        {
            // Ajusta HP/Energia proporcionalmente se o máximo mudou
            currentHealth = Mathf.Min(currentHealth, TotalMaxHealth);
            currentEnergy = Mathf.Min(currentEnergy, TotalMaxEnergy);
        }

        energyType = characterClass.energyType;
        maxEnergy = characterClass.baseMaxEnergy;

        Skills.Clear();
        Skills.AddRange(characterRace.raceSkills);
        Skills.AddRange(characterClass.classSkillTree);

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

        // experienceToNextLevel será sincronizado pelo ExperienceController logo após a inicialização
    }
    

    // Tenta gastar energia. Retorna true se conseguiu, false se não tinha energia suficiente
    public bool SpendEnergy(int amount)
    {
        // Validação: gasto não pode ser negativo (recuperar deve usar RestoreEnergy)
        if (amount < 0)
        {
            Debug.LogWarning($"[CharacterData] SpendEnergy recebeu valor negativo ({amount}). Use RestoreEnergy() para recuperar energia.");
            return false;
        }

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
        if (amount < 0)
        {
            Debug.LogWarning($"[CharacterData] RestoreEnergy recebeu valor negativo ({amount}). Use SpendEnergy() para gastar energia.");
            return;
        }

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
