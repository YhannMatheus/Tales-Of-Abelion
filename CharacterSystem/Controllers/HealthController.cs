using UnityEngine;
using System;

[System.Serializable]
public class HealthController
{
    [System.NonSerialized] private CharacterManager _character;

    // Inicializa o controller - chamado por CharacterManager.Awake()
    public void Initialize(CharacterManager characterManager)
    {
        _character = characterManager;
    }

    // Aplica dano já calculado ao personagem (valor final após resistências/penetrações)
    public void TakeDamage(float damage)
    {
        if (_character == null) return;

        if (damage < 0f)
        {
            Debug.LogWarning($"[HealthController] TakeDamage recebeu valor negativo ({damage}). Use Heal() para curar.");
            return;
        }

        _character.Data.currentHealth = Mathf.Max(0, _character.Data.currentHealth - Mathf.RoundToInt(damage));
    }

    // Cura o personagem (não ultrapassa a vida máxima)
    public void Heal(int amount)
    {
        if (_character == null) return;

        if (amount < 0)
        {
            Debug.LogWarning($"[HealthController] Heal recebeu valor negativo ({amount}). Use TakeDamage() para causar dano.");
            return;
        }

        _character.Data.currentHealth = Mathf.Min(_character.Data.TotalMaxHealth, _character.Data.currentHealth + amount);
    }

    // Revive (aplica valor de vida fornecido)
    public void Revive(int restoredHealth)
    {
        if (_character == null) return;
        _character.Data.currentHealth = Mathf.Clamp(restoredHealth, 0, _character.Data.TotalMaxHealth);
    }

    // Ajusta diretamente o valor de vida (uso administrativo)
    public void SetCurrentHealth(int value)
    {
        if (_character == null) return;
        _character.Data.currentHealth = Mathf.Clamp(value, 0, _character.Data.TotalMaxHealth);
    }

    // Leitura rápida
    public int CurrentHealth => _character != null ? _character.Data.currentHealth : 0;
    public int MaxHealth => _character != null ? _character.Data.TotalMaxHealth : 0;
    public bool IsAlive => CurrentHealth > 0;
}
