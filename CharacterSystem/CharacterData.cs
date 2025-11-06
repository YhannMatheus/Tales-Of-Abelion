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
    
    [Header("Level Progression Settings")]
    [Tooltip("Valor base de XP necessário para o nível 1")]
    public float xpBaseValue = 100f;
    [Tooltip("Expoente de escalamento (1.5 = crescimento moderado, 2.0 = crescimento rápido)")]
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
    public List<SkillData> Skills = new List<SkillData>();

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
    // ⚠️ NOTA: Estes campos são gerenciados pelo BuffSystem
    // Não modifique diretamente - use BuffManager.ApplyBuff() em vez disso
    // BuffManager aplica/remove automaticamente estes modificadores durante a duração dos buffs
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


    // Aplica modificador de equipamento usando enum (type-safe)
    public void ApplyEquipmentModifier(ModifierVar variable, float value)
    {
        switch (variable)
        {
            case ModifierVar.physicalDamage:
                equipamentPhysicalDamageBonus += value;
                break;
            case ModifierVar.physicalResistence:
                equipamentPhysicalResistenceBonus += value;
                break;
            case ModifierVar.magicalDamage:
                equipamentMagicalDamageBonus += value;
                break;
            case ModifierVar.magicalResistence:
                equipamentMagicalResistenceBonus += value;
                break;
            case ModifierVar.maxHealth:
                equipamentMaxHealthBonus += Mathf.RoundToInt(value);
                break;
            case ModifierVar.maxEnergy:
                equipamentMaxEnergyBonus += Mathf.RoundToInt(value);
                break;
            case ModifierVar.speed:
                equipamentSpeedBonus += value;
                break;
            case ModifierVar.criticalChance:
                equipamentCriticalChanceBonus += value;
                break;
            case ModifierVar.criticalDamage:
                equipamentCriticalDamageBonus += value;
                break;
            case ModifierVar.attackSpeed:
                equipamentAttackSpeedBonus += value;
                break;
            case ModifierVar.luck:
                equipamentLuckBonus += value;
                break;
            case ModifierVar.healthRegen:
                equipamentHealthRegenBonus += Mathf.RoundToInt(value);
                break;
            default:
                Debug.LogWarning($"[CharacterData] Modificador de equipamento não suportado: {variable}");
                break;
        }
    }

    // Remove modificador de equipamento usando enum (type-safe)
    public void RemoveEquipmentModifier(ModifierVar variable, float value)
    {
        ApplyEquipmentModifier(variable, -value);
    }

    // Obtém o valor total de um modificador específico (útil para BuffSystem)
    public float GetTotalModifier(ModifierVar variable)
    {
        switch (variable)
        {
            case ModifierVar.physicalDamage:
                return TotalPhysicalDamage;
            case ModifierVar.physicalResistence:
                return TotalPhysicalResistance;
            case ModifierVar.magicalDamage:
                return TotalMagicalDamage;
            case ModifierVar.magicalResistence:
                return TotalMagicalResistance;
            case ModifierVar.maxHealth:
                return TotalMaxHealth;
            case ModifierVar.maxEnergy:
                return TotalMaxEnergy;
            case ModifierVar.speed:
                return TotalSpeed;
            case ModifierVar.criticalChance:
                return TotalCriticalChance;
            case ModifierVar.criticalDamage:
                return TotalCriticalDamage;
            case ModifierVar.attackSpeed:
                return TotalAttackSpeed;
            case ModifierVar.luck:
                return TotalLuck;
            case ModifierVar.healthRegen:
                return TotalHealthRegen;
            default:
                Debug.LogWarning($"[CharacterData] GetTotalModifier não suporta: {variable}");
                return 0f;
        }
    }

    // Obtém o valor base de um modificador (sem bônus)
    public float GetBaseModifier(ModifierVar variable)
    {
        switch (variable)
        {
            case ModifierVar.physicalDamage:
                return physicalDamage;
            case ModifierVar.physicalResistence:
                return physicalResistence;
            case ModifierVar.magicalDamage:
                return magicalDamage;
            case ModifierVar.magicalResistence:
                return magicalResistence;
            case ModifierVar.maxHealth:
                return maxHealth;
            case ModifierVar.maxEnergy:
                return maxEnergy;
            case ModifierVar.speed:
                return speed;
            case ModifierVar.criticalChance:
                return criticalChance;
            case ModifierVar.criticalDamage:
                return criticalDamage;
            case ModifierVar.attackSpeed:
                return attackSpeed;
            case ModifierVar.luck:
                return luck;
            case ModifierVar.healthRegen:
                return healthRegen;
            default:
                Debug.LogWarning($"[CharacterData] GetBaseModifier não suporta: {variable}");
                return 0f;
        }
    }


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

        experienceToNextLevel = CalculateExperienceForNextLevel();
    }

    // Calcula quantos pontos de experiência são necessários para o próximo nível
    // Agora usa valores configuráveis no Inspector!
    private int CalculateExperienceForNextLevel()
    {
        // Fórmula: XP = baseValue * (level ^ exponent)
        // Exemplo com padrões (100, 1.5):
        // Level 1: 100 XP
        // Level 2: 282 XP
        // Level 5: 1118 XP
        // Level 10: 3162 XP
        return Mathf.RoundToInt(xpBaseValue * Mathf.Pow(level, xpScalingExponent));
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
        // Validação: XP não pode ser negativo
        if (amount < 0)
        {
            Debug.LogWarning($"[CharacterData] GainExperience recebeu valor negativo ({amount}). XP não pode ser reduzido dessa forma.");
            return;
        }

        experiencePoints += amount;

        while (experiencePoints >= experienceToNextLevel)
        {
            LevelUp();
        }
    }

    // Aplica dano ao personagem considerando resistência
    public void TakeDamage(float damage, bool isMagical = false)
    {
        // Validação: dano não pode ser negativo (cura deve usar Heal)
        if (damage < 0)
        {
            Debug.LogWarning($"[CharacterData] TakeDamage recebeu valor negativo ({damage}). Use Heal() para curar.");
            return;
        }

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
        // Validação: cura não pode ser negativa (dano deve usar TakeDamage)
        if (amount < 0)
        {
            Debug.LogWarning($"[CharacterData] Heal recebeu valor negativo ({amount}). Use TakeDamage() para causar dano.");
            return;
        }

        currentHealth = Mathf.Min(TotalMaxHealth, currentHealth + amount);
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
        // Validação: recuperação não pode ser negativa (gastar deve usar SpendEnergy)
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
