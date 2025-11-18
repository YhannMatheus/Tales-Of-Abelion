using System;

public interface IHealthController
{
    void Initialize(CharacterManager characterManager);
    void TakeDamage(float damage);
    void Heal(int amount);
    void Revive(int restoredHealth);
    void SetCurrentHealth(int value);
    int CurrentHealth { get; }
    int MaxHealth { get; }
    bool IsAlive { get; }
    event EventHandler<HealthChangedEventArgs> OnHealthChanged;
}

public interface IEnergyController
{
    void Initialize(CharacterManager characterManager);
    void SyncAfterInitialization();
    void Tick(float deltaTime);
    void RestoreEnergy(int amount);
    bool TrySpendEnergy(int amount);
    int CurrentEnergyInt { get; }
    int MaxEnergyInt { get; }
    void SetCurrentEnergy(int value);
    void Cleanup();
    void ValidateValues();
    event EventHandler<EnergyTypeChangedEventArgs> OnManaChanged;
    event EventHandler<EnergyTypeChangedEventArgs> OnStaminaChanged;
    event EventHandler<EnergyTypeChangedEventArgs> OnFuryChanged;
}

public interface IExperienceController
{
    void Initialize(CharacterManager characterManager);
    void SyncAfterInitialization();
    void GainExperience(int amount);
}

public interface IModifiersController
{
    void Initialize(CharacterManager characterManager);
    void ApplyEquipmentModifier(ModifierVar variable, float value);
    void ApplyEffectModifier(ModifierVar variable, float value);
    void RemoveEquipmentModifier(ModifierVar variable, float value);
    void RemoveEffectModifier(ModifierVar variable, float value);
    float GetTotalModifier(ModifierVar variable);
    float GetBaseModifier(ModifierVar variable);
}
