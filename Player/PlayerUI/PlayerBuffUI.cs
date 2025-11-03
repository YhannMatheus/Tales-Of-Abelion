using UnityEngine;

/// <summary>
/// Módulo de UI responsável pela exibição de buffs/debuffs do player.
/// Atualiza ícones de buffs ativos, durações, stacks, etc.
/// 
/// TODO: Implementar sistema de ícones de buff, tooltips, timers
/// </summary>
public class PlayerBuffUI : MonoBehaviour
{
    [Header("Configurações")]
    [Tooltip("Número máximo de buffs para exibir simultaneamente")]
    [SerializeField] private int maxBuffsToShow = 10;

    // TODO: Adicionar referências para slots de UI de buffs
    // [SerializeField] private BuffIconUI[] buffIcons;

    /// <summary>
    /// Adiciona um buff à exibição
    /// </summary>
    public void AddBuff(BuffData buff, float duration)
    {
        // TODO: Implementar adição visual de buff
        Debug.Log($"[PlayerBuffUI] Buff adicionado: {buff.buffName} (duração: {duration}s)");
    }

    /// <summary>
    /// Remove um buff da exibição
    /// </summary>
    public void RemoveBuff(BuffData buff)
    {
        // TODO: Implementar remoção visual de buff
        Debug.Log($"[PlayerBuffUI] Buff removido: {buff.buffName}");
    }

    /// <summary>
    /// Atualiza a duração restante de um buff
    /// </summary>
    public void UpdateBuffDuration(BuffData buff, float remainingTime)
    {
        // TODO: Implementar atualização de timer visual
    }

    /// <summary>
    /// Atualiza os stacks de um buff stackável
    /// </summary>
    public void UpdateBuffStacks(BuffData buff, int currentStacks)
    {
        // TODO: Implementar atualização de contador de stacks
    }
}
