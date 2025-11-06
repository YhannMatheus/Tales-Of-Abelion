using System;
using System.Collections.Generic;
using UnityEngine;

// Requisitos para usar/desbloquear uma skill
[System.Serializable]
public class SkillRequirements
{
    [Header("Requisitos de Personagem")]
    [Tooltip("Nível mínimo do personagem")]
    public int minCharacterLevel = 1;
    
    [Tooltip("Força mínima necessária")]
    public int minStrength = 0;
    
    [Tooltip("Destreza mínima necessária")]
    public int minDexterity = 0;
    
    [Tooltip("Inteligência mínima necessária")]
    public int minIntelligence = 0;

    [Header("Requisitos de Equipamento")]
    [Tooltip("Precisa estar equipado com uma dessas armas")]
    public List<string> requiredWeaponTypes = new List<string>();
    
    [Tooltip("Precisa ter escudo equipado")]
    public bool requiresShield = false;

    [Header("Requisitos de Skills (Synergies)")]
    [Tooltip("Skills que precisam estar desbloqueadas")]
    public List<SkillRequirement> requiredSkills = new List<SkillRequirement>();

    [Header("Outros Requisitos")]
    [Tooltip("IDs de quests que precisam estar completas")]
    public List<string> requiredQuestIDs = new List<string>();
    
    [Tooltip("Precisa ter derrotado algum boss específico")]
    public bool requiresBossDefeated = false;
    
    [Tooltip("Nome do boss (se requiresBossDefeated = true)")]
    public string bossName = "";

    // Verifica se todos os requisitos são atendidos
    public bool IsMet(Character character)
    {
        if (character == null)
        {
            Debug.LogWarning("[SkillRequirements] Character é null!");
            return false;
        }

        var data = character.Data;

        // Verifica nível
        if (data.level < minCharacterLevel)
        {
            Debug.Log($"[SkillRequirements] Nível insuficiente. Necessário: {minCharacterLevel}, Atual: {data.level}");
            return false;
        }

        // Verifica atributos
        if (data.strength < minStrength)
        {
            Debug.Log($"[SkillRequirements] Força insuficiente. Necessário: {minStrength}, Atual: {data.strength}");
            return false;
        }

        if (data.dexterity < minDexterity)
        {
            Debug.Log($"[SkillRequirements] Destreza insuficiente. Necessário: {minDexterity}, Atual: {data.dexterity}");
            return false;
        }

        if (data.intelligence < minIntelligence)
        {
            Debug.Log($"[SkillRequirements] Inteligência insuficiente. Necessário: {minIntelligence}, Atual: {data.intelligence}");
            return false;
        }

        // TODO: Verificar equipamento quando sistema de inventário existir
        // if (requiredWeaponTypes.Count > 0) { ... }

        // TODO: Verificar skills requeridas quando PlayerSkillManager existir
        // if (requiredSkills.Count > 0) { ... }

        // TODO: Verificar quests quando sistema de quests existir
        // if (requiredQuestIDs.Count > 0) { ... }

        return true;
    }

    // Retorna mensagem descrevendo qual requisito não foi atendido
    public string GetFailureReason(Character character)
    {
        if (character == null) return "Personagem inválido";

        var data = character.Data;

        if (data.level < minCharacterLevel)
            return $"Necessário nível {minCharacterLevel} (atual: {data.level})";

        if (data.strength < minStrength)
            return $"Necessário {minStrength} de Força (atual: {data.strength})";

        if (data.dexterity < minDexterity)
            return $"Necessário {minDexterity} de Destreza (atual: {data.dexterity})";

        if (data.intelligence < minIntelligence)
            return $"Necessário {minIntelligence} de Inteligência (atual: {data.intelligence})";

        return "Requisitos não atendidos";
    }
}

// Requisito de skill específica
[System.Serializable]
public class SkillRequirement
{
    [Tooltip("Skill que precisa estar desbloqueada")]
    public SkillData requiredSkill;
    
    [Tooltip("Nível mínimo dessa skill")]
    public int minLevel = 1;
}

// Custo de recurso para usar skill
[System.Serializable]
public class ResourceCost
{
    [Tooltip("Custo de energia/mana")]
    public float energy = 0f;
    
    [Tooltip("Custo de HP (skills de sacrifício)")]
    public float health = 0f;
    
    [Tooltip("Precisa consumir item específico")]
    public string requiredItemID = "";
    
    [Tooltip("Quantidade do item")]
    public int itemQuantity = 0;

    // Verifica se personagem tem recursos suficientes
    public bool CanAfford(Character character)
    {
        if (character == null) return false;

        var data = character.Data;

        // Verifica energia
        if (energy > 0f && data.currentEnergy < energy)
        {
            Debug.Log($"[ResourceCost] Energia insuficiente. Necessário: {energy}, Atual: {data.currentEnergy}");
            return false;
        }

        // Verifica HP
        if (health > 0f && data.currentHealth <= health)
        {
            Debug.Log($"[ResourceCost] HP insuficiente. Necessário: {health}, Atual: {data.currentHealth}");
            return false;
        }

        // TODO: Verificar items quando inventário existir
        // if (!string.IsNullOrEmpty(requiredItemID)) { ... }

        return true;
    }

    // Consome os recursos
    public void Consume(Character character)
    {
        if (character == null) return;

        // Consome energia
        if (energy > 0f)
        {
            int energyCost = Mathf.CeilToInt(energy);
            character.TrySpendEnergy(energyCost);
        }

        // Consome HP
        if (health > 0f)
        {
            character.TakeDamage(health);
        }

        // TODO: Consumir items quando inventário existir
    }

    // Retorna mensagem descrevendo qual recurso está faltando
    public string GetFailureReason(Character character)
    {
        if (character == null) return "Personagem inválido";

        var data = character.Data;

        if (energy > 0f && data.currentEnergy < energy)
            return $"Energia insuficiente ({data.currentEnergy:F0}/{energy:F0})";

        if (health > 0f && data.currentHealth <= health)
            return $"HP insuficiente ({data.currentHealth:F0}/{health:F0})";

        return "Recursos insuficientes";
    }
}

// Dados de cooldown
[System.Serializable]
public class CooldownData
{
    [Tooltip("Duração do cooldown em segundos")]
    public float duration = 5f;
    
    [Tooltip("Número de charges (0 = sem charges)")]
    public int maxCharges = 0;
    
    [Tooltip("Tempo para recarregar 1 charge")]
    public float chargeRechargeTime = 0f;
    
    [Tooltip("Começa em cooldown quando desbloqueada")]
    public bool startsOnCooldown = false;
}

// Dados de targeting
[System.Serializable]
public class TargetingData
{
    [Tooltip("Modo de targeting")]
    public TargetingMode mode = TargetingMode.Enemy;
    
    [Tooltip("Alcance máximo")]
    public float range = 10f;
    
    [Tooltip("Filtro de alvos válidos")]
    public TargetFilter filter = TargetFilter.Enemies;
    
    [Tooltip("Layer mask para detecção")]
    public LayerMask validLayers = ~0;
    
    [Tooltip("Requer linha de visão (raycast)")]
    public bool requiresLineOfSight = true;
    
    [Tooltip("Pode ser usado em si mesmo")]
    public bool canTargetSelf = false;
}
