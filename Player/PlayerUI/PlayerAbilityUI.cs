using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Módulo de UI responsável pela exibição de habilidades do player.
/// Atualiza ícones, cooldowns, feedback visual, etc.
/// 
/// Estrutura esperada para cada slot:
/// - Background (Image)
/// - Overlay (Image) - feedback visual (sem energia, fora de alcance)
/// - Icon (Image) - ícone da habilidade
/// - Slider (UI Slider) - cooldown da habilidade
/// - Cooldown Text (TextMeshProUGUI) - tempo restante (opcional)
/// - Hotkey Text (TextMeshProUGUI) - tecla de atalho (opcional)
/// </summary>
public class PlayerAbilityUI : MonoBehaviour
{
    [System.Serializable]
    public class AbilitySlotUI
    {
        [Tooltip("Nome do slot (para debug)")]
        public string slotName = "Slot 1";
        
        [Tooltip("Image de fundo do slot")]
        public Image background;
        
        [Tooltip("Image de overlay (feedback visual)")]
        public Image overlay;
        
        [Tooltip("Image do ícone da habilidade")]
        public Image icon;
        
        [Tooltip("Slider para cooldown")]
        public Slider cooldownSlider;

        [Tooltip("Texto opcional para mostrar tempo restante")]
        public TextMeshProUGUI cooldownText;

        [Tooltip("Texto opcional para mostrar hotkey (ex: 'Q', 'E')")]
        public TextMeshProUGUI hotkeyText;
    }

    [Header("Ability Slots (8 total)")]
    [Tooltip("Array com os 8 slots de habilidades")]
    [SerializeField] private AbilitySlotUI[] abilitySlots = new AbilitySlotUI[8];

    [Header("Configurações")]
    [Tooltip("Cor do overlay quando sem energia")]
    [SerializeField] private Color noEnergyColor = new Color(1f, 0f, 0f, 0.5f);

    [Tooltip("Cor do overlay quando fora de alcance")]
    [SerializeField] private Color outOfRangeColor = new Color(1f, 0.5f, 0f, 0.5f);

    [Tooltip("Mostrar tempo de cooldown em texto?")]
    [SerializeField] private bool showCooldownText = true;

    private PlayerAbilityManager abilityManager;

    private void Awake()
    {
        // Valida se temos exatamente 8 slots
        if (abilitySlots.Length != 8)
        {
            Debug.LogWarning($"[PlayerAbilityUI] Esperado 8 slots, mas tem {abilitySlots.Length}");
        }
    }

    /// <summary>
    /// Inicializa com referência ao PlayerAbilityManager
    /// </summary>
    public void Initialize(PlayerAbilityManager manager)
    {
        abilityManager = manager;

        // Subscreve aos eventos de cada slot
        if (abilityManager != null && abilityManager.SkillSlots != null)
        {
            for (int i = 0; i < abilityManager.SkillSlots.Length && i < abilitySlots.Length; i++)
            {
                int slotIndex = i;
                var slot = abilityManager.SkillSlots[i];

                if (slot != null)
                {
                    slot.OnUsed += (s) => OnSlotUsed(slotIndex);
                    slot.OnCooldownEnded += (s) => OnSlotReady(slotIndex);
                }
            }
        }

        // Configura hotkeys padrão
        SetDefaultHotkeys();

        // Inicializa UI
        RefreshAllSlots();
    }

    private void Update()
    {
        // Atualiza cooldowns em tempo real
        if (abilityManager != null)
        {
            for (int i = 0; i < abilityManager.SkillSlots.Length && i < abilitySlots.Length; i++)
            {
                UpdateSlotCooldown(i);
            }
        }
    }

    /// <summary>
    /// Atualiza o cooldown visual de um slot específico
    /// </summary>
    private void UpdateSlotCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= abilitySlots.Length) return;
        if (abilityManager == null || abilityManager.SkillSlots == null) return;
        if (slotIndex >= abilityManager.SkillSlots.Length) return;

        var uiSlot = abilitySlots[slotIndex];
        var abilitySlot = abilityManager.SkillSlots[slotIndex];

        if (abilitySlot == null || uiSlot.cooldownSlider == null) return;

        // Calcula progresso do cooldown (0 = em cooldown, 1 = pronto)
        float cooldownProgress = 0f;

        if (abilitySlot.CooldownRemaining > 0f && abilitySlot.AssignedAbility != null)
        {
            cooldownProgress = 1f - (abilitySlot.CooldownRemaining / abilitySlot.AssignedAbility.cooldownTime);
        }
        else
        {
            cooldownProgress = 1f; // Pronto
        }

        // Slider value: 0 = vazio (em cooldown), 1 = cheio (pronto)
        uiSlot.cooldownSlider.value = cooldownProgress;

        // Atualiza texto de cooldown
        if (showCooldownText && uiSlot.cooldownText != null)
        {
            if (abilitySlot.CooldownRemaining > 0f)
            {
                uiSlot.cooldownText.text = $"{abilitySlot.CooldownRemaining:F1}s";
            }
            else
            {
                uiSlot.cooldownText.text = "";
            }
        }
    }

    /// <summary>
    /// Callback quando um slot é usado
    /// </summary>
    private void OnSlotUsed(int slotIndex)
    {
        // Pode adicionar efeito visual aqui (flash, pulse, etc.)
        Debug.Log($"[PlayerAbilityUI] Slot {slotIndex} usado!");
    }

    /// <summary>
    /// Callback quando um slot fica pronto (cooldown acabou)
    /// </summary>
    private void OnSlotReady(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= abilitySlots.Length) return;

        var uiSlot = abilitySlots[slotIndex];

        // Reseta slider para cheio
        if (uiSlot.cooldownSlider != null)
        {
            uiSlot.cooldownSlider.value = 1f;
        }

        // Limpa texto
        if (uiSlot.cooldownText != null)
        {
            uiSlot.cooldownText.text = "";
        }

        // Pode adicionar efeito visual aqui (glow, pulse, etc.)
        Debug.Log($"[PlayerAbilityUI] Slot {slotIndex} pronto!");
    }

    /// <summary>
    /// Atualiza o ícone de uma habilidade
    /// </summary>
    public void UpdateSlotIcon(int slotIndex, Sprite abilityIcon)
    {
        if (slotIndex < 0 || slotIndex >= abilitySlots.Length) return;

        var uiSlot = abilitySlots[slotIndex];

        if (uiSlot.icon != null)
        {
            uiSlot.icon.sprite = abilityIcon;
            uiSlot.icon.enabled = abilityIcon != null;
        }
    }

    /// <summary>
    /// Mostra feedback de energia insuficiente
    /// </summary>
    public void ShowNoEnergyFeedback(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= abilitySlots.Length) return;

        var uiSlot = abilitySlots[slotIndex];

        if (uiSlot.overlay != null)
        {
            uiSlot.overlay.color = noEnergyColor;
            uiSlot.overlay.enabled = true;

            // Desabilita overlay depois de um tempo
            StartCoroutine(HideOverlayAfterDelay(slotIndex, 0.5f));
        }
    }

    /// <summary>
    /// Mostra feedback de fora de alcance
    /// </summary>
    public void ShowOutOfRangeFeedback(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= abilitySlots.Length) return;

        var uiSlot = abilitySlots[slotIndex];

        if (uiSlot.overlay != null)
        {
            uiSlot.overlay.color = outOfRangeColor;
            uiSlot.overlay.enabled = true;

            // Desabilita overlay depois de um tempo
            StartCoroutine(HideOverlayAfterDelay(slotIndex, 0.5f));
        }
    }

    /// <summary>
    /// Esconde overlay após delay
    /// </summary>
    private System.Collections.IEnumerator HideOverlayAfterDelay(int slotIndex, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (slotIndex >= 0 && slotIndex < abilitySlots.Length)
        {
            var uiSlot = abilitySlots[slotIndex];
            if (uiSlot.overlay != null)
            {
                uiSlot.overlay.enabled = false;
            }
        }
    }

    /// <summary>
    /// Atualiza todos os slots de uma vez
    /// </summary>
    public void RefreshAllSlots()
    {
        if (abilityManager == null) return;

        for (int i = 0; i < abilityManager.SkillSlots.Length && i < abilitySlots.Length; i++)
        {
            var abilitySlot = abilityManager.SkillSlots[i];
            var uiSlot = abilitySlots[i];

            if (abilitySlot != null && abilitySlot.AssignedAbility != null)
            {
                // Atualiza ícone (se a habilidade tiver sprite)
                if (abilitySlot.AssignedAbility.abilityIcon != null)
                {
                    UpdateSlotIcon(i, abilitySlot.AssignedAbility.abilityIcon);
                }
            }

            // Atualiza cooldown
            UpdateSlotCooldown(i);
        }
    }

    /// <summary>
    /// Define a hotkey de um slot
    /// </summary>
    public void SetSlotHotkey(int slotIndex, string hotkey)
    {
        if (slotIndex < 0 || slotIndex >= abilitySlots.Length) return;

        var uiSlot = abilitySlots[slotIndex];

        if (uiSlot.hotkeyText != null)
        {
            uiSlot.hotkeyText.text = hotkey;
        }
    }

    /// <summary>
    /// Configura as hotkeys padrão (Q, E, R, T, 1, 2, 3, 4)
    /// </summary>
    public void SetDefaultHotkeys()
    {
        string[] defaultHotkeys = { "Q", "E", "R", "T", "1", "2", "3", "4" };

        for (int i = 0; i < defaultHotkeys.Length && i < abilitySlots.Length; i++)
        {
            SetSlotHotkey(i, defaultHotkeys[i]);
        }
    }
}

