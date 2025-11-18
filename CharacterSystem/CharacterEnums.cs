using System;
public enum CharacterType
{
    Player,
    Enemy,
    Neutral,
    Ally
}
public enum EnergyType
{
    Mana,       // Regenera lentamente, cresce em valores flat
    Stamina,    // Funciona por porcentagem (0-100), consome e regenera por %
    Fury        // Começa zerada, ganha ao atacar/receber dano, dispara evento ao máximo
}

// Define o modo de consumo/regeneração de energia
public enum EnergyModificationMode
{
    Flat,       // Valor absoluto (ex: +50 mana)
    Percentage  // Valor percentual (ex: +25% do máximo)
}

/// Fonte de ganho de Fúria
public enum FuryGainSource
{
    DealDamage,     // Ao causar dano
    TakeDamage,     // Ao receber dano
    Skill,          // Por habilidade específica
    Other           // Outras fontes
}

public enum ModifierType
{
    Flat,
    PercentAdd,
    PercentMult,
}

public enum ModifierVar
{
    physicalResistence,
    physicalDamage,
    armorPenetration,
    magicalResistence,
    magicalDamage,
    magicPenetration,
    maxHealth,
    maxEnergy,
    strength,
    dexterity,
    intelligence,
    charisma,
    endurance,
    speed,
    luck,
    criticalChance,
    criticalDamage,
    healthRegen,
    energyRegen,
    attackSpeed,
}

[System.Serializable]
public class Modifier
{
    public ModifierType type;
    public ModifierVar variable;
    public float value;
    public float baseValue = 0f;
}