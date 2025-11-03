using UnityEngine;

/// <summary>
/// Módulo de UI responsável pela exibição de habilidades do player.
/// Atualiza ícones, cooldowns, charges, etc.
/// 
/// TODO: Implementar sistema de cooldown visual, charges, hotkeys
/// </summary>
public class PlayerAbilityUI : MonoBehaviour
{
    [Header("Configurações")]
    [Tooltip("Número de slots de habilidade para exibir")]
    [SerializeField] private int numberOfSlots = 8;

    // TODO: Adicionar referências para slots de UI de habilidades
    // [SerializeField] private AbilitySlotUI[] abilitySlots;

    /// <summary>
    /// Atualiza o cooldown de uma habilidade específica
    /// </summary>
    public void UpdateAbilityCooldown(int slotIndex, float currentCooldown, float maxCooldown)
    {
        // TODO: Implementar atualização visual do cooldown
        // if (slotIndex >= 0 && slotIndex < abilitySlots.Length)
        // {
        //     abilitySlots[slotIndex].UpdateCooldown(currentCooldown, maxCooldown);
        // }
    }

    /// <summary>
    /// Atualiza as charges de uma habilidade
    /// </summary>
    public void UpdateAbilityCharges(int slotIndex, int currentCharges, int maxCharges)
    {
        // TODO: Implementar atualização visual de charges
    }

    /// <summary>
    /// Mostra feedback de habilidade usada
    /// </summary>
    public void ShowAbilityUsedFeedback(int slotIndex)
    {
        // TODO: Implementar feedback visual (flash, animação, etc.)
    }
}
