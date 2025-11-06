using UnityEngine;

/// <summary>
/// Gerenciador central de toda a UI relacionada ao player.
/// Escuta eventos do Character/PlayerManager e coordena módulos de UI específicos.
/// 
/// Arquitetura Modular:
/// - PlayerUIManager: Coordenador central, escuta eventos
/// - Módulos específicos: DeathUI, HealthUI, EnergyUI, AbilityUI, BuffUI, etc.
/// 
/// Responsabilidades:
/// - Subscrever aos eventos do Character (OnDeath, OnRevive, OnHealthChanged, etc.)
/// - Delegar atualizações para módulos de UI específicos
/// - Centralizar referências de UI para facilitar expansão
/// </summary>
[RequireComponent(typeof(Character))]
public class PlayerUIManager : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Character character;
    [SerializeField] private PlayerManager playerManager;

    [Header("Módulos de UI")]
    [SerializeField] private PlayerDeathUI deathUI;
    [SerializeField] private PlayerHealthUI healthUI;
    [SerializeField] private PlayerEnergyUI energyUI;
    [SerializeField] private PlayerExperienceUI experienceUI;
    [SerializeField] private PlayerAbilityUI abilityUI;
    [SerializeField] private PlayerBuffUI buffUI;

    private void Awake()
    {
        // Auto-referência se não foi setado no Inspector
        if (character == null)
        {
            character = GetComponent<Character>();
        }

        if (playerManager == null)
        {
            playerManager = GetComponent<PlayerManager>();
        }
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    /// <summary>
    /// Inscreve em todos os eventos relevantes do Character
    /// </summary>
    private void SubscribeToEvents()
    {
        if (character != null)
        {
            character.OnDeath += HandleDeath;
            character.OnRevive += HandleRevive;
            character.OnHealthChanged += HandleHealthChanged;
            character.OnEnergyChanged += HandleEnergyChanged;
            character.OnLevelUp += HandleLevelUp;
            character.OnExperienceGained += HandleExperienceGained;
        }

        // Inicializa PlayerSkillUI se disponível
        if (abilityUI != null && playerManager != null)
        {
            var skillManager = playerManager.GetComponent<PlayerSkillManager>();
            if (skillManager != null)
            {
                abilityUI.Initialize(skillManager);
            }
        }
    }

    /// <summary>
    /// Desinscreve de todos os eventos
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (character != null)
        {
            character.OnDeath -= HandleDeath;
            character.OnRevive -= HandleRevive;
            character.OnHealthChanged -= HandleHealthChanged;
            character.OnEnergyChanged -= HandleEnergyChanged;
            character.OnLevelUp -= HandleLevelUp;
            character.OnExperienceGained -= HandleExperienceGained;
        }
    }

    // ========== Event Handlers ==========

    private void HandleDeath()
    {
        Debug.Log("[PlayerUIManager] Player morreu - notificando módulo DeathUI");
        
        if (deathUI != null)
        {
            deathUI.ShowDeathOverlay(character);
        }
    }

    private void HandleRevive()
    {
        Debug.Log("[PlayerUIManager] Player reviveu - notificando módulo DeathUI");
        
        if (deathUI != null)
        {
            deathUI.HideDeathOverlay();
        }
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        if (healthUI != null)
        {
            healthUI.UpdateHealth(currentHealth, maxHealth);
        }
    }

    private void HandleEnergyChanged(int currentEnergy, int maxEnergy)
    {
        if (energyUI != null)
        {
            energyUI.UpdateEnergy(currentEnergy, maxEnergy);
        }
    }

    private void HandleLevelUp(int newLevel)
    {
        Debug.Log($"[PlayerUIManager] Player subiu para level {newLevel}");
        
        // Efeito visual de level up na barra de XP
        if (experienceUI != null)
        {
            experienceUI.ShowLevelUpEffect();
        }
        
        // Pode mostrar notificação de level up, efeitos, etc.
        // if (levelUpUI != null)
        // {
        //     levelUpUI.ShowLevelUpNotification(newLevel);
        // }
    }

    private void HandleExperienceGained(int experienceGained)
    {
        Debug.Log($"[PlayerUIManager] Player ganhou {experienceGained} XP");
        
        // Atualiza a barra de experiência
        if (experienceUI != null && character != null)
        {
            // TODO: Verificar se Character tem propriedades de XP
            // experienceUI.UpdateExperience(character.Data.currentExperience, character.Data.xpToNextLevel);
        }
    }

    // ========== Métodos Públicos para Acesso Externo ==========

    /// <summary>
    /// Força atualização de todos os módulos de UI
    /// </summary>
    public void RefreshAllUI()
    {
        if (character == null) return;

        HandleHealthChanged(character.Data.currentHealth, character.Data.TotalMaxHealth);
        HandleEnergyChanged(character.Data.currentEnergy, character.Data.TotalMaxEnergy);
    }
}
