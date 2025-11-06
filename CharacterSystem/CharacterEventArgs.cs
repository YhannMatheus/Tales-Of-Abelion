using System;

/// <summary>
/// EventArgs para mudanças de HP
/// Contém informação completa sobre a mudança de saúde
/// </summary>
public class HealthChangedEventArgs : EventArgs
{
    public int CurrentHealth { get; }
    public int MaxHealth { get; }
    public int PreviousHealth { get; }
    public int Delta { get; }
    public bool IsDamage => Delta < 0;
    public bool IsHealing => Delta > 0;
    public float HealthPercentage => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;

    public HealthChangedEventArgs(int currentHealth, int maxHealth, int previousHealth)
    {
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        PreviousHealth = previousHealth;
        Delta = currentHealth - previousHealth;
    }
}

/// <summary>
/// EventArgs para mudanças de Energia
/// Contém informação completa sobre a mudança de energia
/// </summary>
public class EnergyChangedEventArgs : EventArgs
{
    public int CurrentEnergy { get; }
    public int MaxEnergy { get; }
    public int PreviousEnergy { get; }
    public int Delta { get; }
    public bool IsSpent => Delta < 0;
    public bool IsRestored => Delta > 0;
    public float EnergyPercentage => MaxEnergy > 0 ? (float)CurrentEnergy / MaxEnergy : 0f;

    public EnergyChangedEventArgs(int currentEnergy, int maxEnergy, int previousEnergy)
    {
        CurrentEnergy = currentEnergy;
        MaxEnergy = maxEnergy;
        PreviousEnergy = previousEnergy;
        Delta = currentEnergy - previousEnergy;
    }
}

/// <summary>
/// EventArgs para Level Up
/// Contém informação completa sobre o aumento de nível
/// </summary>
public class LevelUpEventArgs : EventArgs
{
    public int NewLevel { get; }
    public int PreviousLevel { get; }
    public int ExperienceToNextLevel { get; }
    public int RemainingExperience { get; }

    public LevelUpEventArgs(int newLevel, int previousLevel, int xpToNextLevel, int remainingXp)
    {
        NewLevel = newLevel;
        PreviousLevel = previousLevel;
        ExperienceToNextLevel = xpToNextLevel;
        RemainingExperience = remainingXp;
    }
}

/// <summary>
/// EventArgs para ganho de experiência
/// Contém informação completa sobre o XP ganho
/// </summary>
public class ExperienceGainedEventArgs : EventArgs
{
    public int AmountGained { get; }
    public int CurrentExperience { get; }
    public int ExperienceToNextLevel { get; }
    public int CurrentLevel { get; }
    public float ProgressToNextLevel => ExperienceToNextLevel > 0 
        ? (float)CurrentExperience / ExperienceToNextLevel 
        : 1f;

    public ExperienceGainedEventArgs(int amountGained, int currentXp, int xpToNextLevel, int currentLevel)
    {
        AmountGained = amountGained;
        CurrentExperience = currentXp;
        ExperienceToNextLevel = xpToNextLevel;
        CurrentLevel = currentLevel;
    }
}

/// <summary>
/// EventArgs para dano recebido
/// Contém informação completa sobre o dano
/// </summary>
public class DamageTakenEventArgs : EventArgs
{
    public float RawDamage { get; }
    public float FinalDamage { get; }
    public float DamageReduction { get; }
    public bool IsMagical { get; }
    public int CurrentHealth { get; }
    public bool WillDie => CurrentHealth <= 0;

    public DamageTakenEventArgs(float rawDamage, float finalDamage, float reduction, bool isMagical, int currentHealth)
    {
        RawDamage = rawDamage;
        FinalDamage = finalDamage;
        DamageReduction = reduction;
        IsMagical = isMagical;
        CurrentHealth = currentHealth;
    }
}

/// <summary>
/// EventArgs para morte do personagem
/// Contém informação sobre o contexto da morte
/// </summary>
public class DeathEventArgs : EventArgs
{
    public CharacterType CharacterType { get; }
    public string CharacterName { get; }
    public int Level { get; }

    public DeathEventArgs(CharacterType type, string name, int level)
    {
        CharacterType = type;
        CharacterName = name;
        Level = level;
    }
}

/// <summary>
/// EventArgs para reviver personagem
/// Contém informação sobre o revival
/// </summary>
public class ReviveEventArgs : EventArgs
{
    public int RestoredHealth { get; }
    public int RestoredEnergy { get; }
    public bool FullRestore { get; }

    public ReviveEventArgs(int restoredHealth, int restoredEnergy, bool fullRestore)
    {
        RestoredHealth = restoredHealth;
        RestoredEnergy = restoredEnergy;
        FullRestore = fullRestore;
    }
}
