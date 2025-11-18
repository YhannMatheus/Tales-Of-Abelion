using UnityEngine;
using System;

[System.Serializable]
public class HealthController : IHealthController
{
    [System.NonSerialized] private CharacterManager _character;
    private int _previousHealth;

    // Evento interno que o CharacterManager pode assinar para re-emitir
    public event EventHandler<HealthChangedEventArgs> OnHealthChanged;

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

        _previousHealth = _character.Data.currentHealth;
        _character.Data.currentHealth = Mathf.Max(0, _character.Data.currentHealth - Mathf.RoundToInt(damage));

        // Notificar assinantes locais (Manager irá re-emitir se necessário)
        OnHealthChanged?.Invoke(_character, new HealthChangedEventArgs(
            _character.Data.currentHealth,
            _character.Data.TotalMaxHealth,
            _previousHealth));
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

        _previousHealth = _character.Data.currentHealth;
        _character.Data.currentHealth = Mathf.Min(_character.Data.TotalMaxHealth, _character.Data.currentHealth + amount);

        OnHealthChanged?.Invoke(_character, new HealthChangedEventArgs(
            _character.Data.currentHealth,
            _character.Data.TotalMaxHealth,
            _previousHealth));
    }

    // Revive (aplica valor de vida fornecido)
    public void Revive(int restoredHealth)
    {
        if (_character == null) return;
        _previousHealth = _character.Data.currentHealth;
        _character.Data.currentHealth = Mathf.Clamp(restoredHealth, 0, _character.Data.TotalMaxHealth);

        OnHealthChanged?.Invoke(_character, new HealthChangedEventArgs(
            _character.Data.currentHealth,
            _character.Data.TotalMaxHealth,
            _previousHealth));
    }

    // Ajusta diretamente o valor de vida (uso administrativo)
    public void SetCurrentHealth(int value)
    {
        if (_character == null) return;
        _previousHealth = _character.Data.currentHealth;
        _character.Data.currentHealth = Mathf.Clamp(value, 0, _character.Data.TotalMaxHealth);

        OnHealthChanged?.Invoke(_character, new HealthChangedEventArgs(
            _character.Data.currentHealth,
            _character.Data.TotalMaxHealth,
            _previousHealth));
    }

    // Leitura rápida
    public int CurrentHealth => _character != null ? _character.Data.currentHealth : 0;
    public int MaxHealth => _character != null ? _character.Data.TotalMaxHealth : 0;
    public bool IsAlive => CurrentHealth > 0;
}
