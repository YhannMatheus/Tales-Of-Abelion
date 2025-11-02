using System;
public enum CharacterType
{
    Player,
    Enemy,
    Ally,
    NPC
}
public enum EnergyType
{
    Mana,
    Stamina,
    Rage,
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
    magicalResistence,
    magicalDamage,
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