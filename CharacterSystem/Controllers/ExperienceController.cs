using UnityEngine;
using System;

[System.Serializable]
public class ExperienceController : IExperienceController
{
    [NonSerialized] private CharacterManager _character;

    public void Initialize(CharacterManager characterManager)
    {
        _character = characterManager;
    }

    // Deve ser chamado após CharacterData.Initialization para garantir que
    // variáveis base (xpBaseValue, xpScalingExponent, level) estejam corretas.
    public void SyncAfterInitialization()
    {
        if (_character == null) return;
        var data = _character.Data;
        data.experienceToNextLevel = CalculateExperienceForNextLevel(data);
    }

    public void GainExperience(int amount)
    {
        if (_character == null) return;
        var data = _character.Data;

        if (amount < 0)
        {
            Debug.LogWarning($"[ExperienceController] GainExperience recebeu valor negativo ({amount}). Ignorando.");
            return;
        }

        data.experiencePoints += amount;

        // Checa se sobe(s) de nível
        while (data.experiencePoints >= data.experienceToNextLevel)
        {
            LevelUp(data);
        }
    }

    private void LevelUp(CharacterData data)
    {
        data.level++;
        data.experiencePoints -= data.experienceToNextLevel;
        data.experienceToNextLevel = CalculateExperienceForNextLevel(data);

        // Ajusta vida/energia ao subir de nível usando os controllers
        if (_character != null)
        {
            // Atualiza vida via HealthController
            _character.Health.SetCurrentHealth(data.TotalMaxHealth);

            // Atualiza energia via EnergyController
            _character.Energy.SetCurrentEnergy(data.TotalMaxEnergy);
        }
        else
        {
            // Fallback: atualiza os dados diretamente se não houver CharacterManager
            data.currentHealth = data.TotalMaxHealth;
            data.currentEnergy = data.TotalMaxEnergy;
        }
    }

    private int CalculateExperienceForNextLevel(CharacterData data)
    {
        return Mathf.RoundToInt(data.xpBaseValue * Mathf.Pow(Mathf.Max(1, data.level), data.xpScalingExponent));
    }
}
