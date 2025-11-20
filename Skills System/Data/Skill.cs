using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class Skill // obeto para processar skills em tempo de execução
{   
    public int skillLevel;
    public bool IsUnlocked;
    public SkillData Data;
    public List<Skill> SkillList;

    // Retorna o nível atual da skill
    public int GetLevel() => skillLevel;

    // Verifica se pode ser destravada. Recebe a lista completa de skills para checar prerequisitos
    public bool CanUnlock(CharacterManager player, IEnumerable<Skill> allSkills, out string reason)
    {
        reason = null;
        if (Data == null) { reason = "SkillData não atribuída"; return false; }
        if (IsUnlocked) { reason = "Já destravada"; return false; }

        // Verifica nível mínimo do jogador
        if (player != null && player.Data != null)
        {
            if (player.Data.level < Data.requiredLevel)
            {
                reason = $"Nível {Data.requiredLevel} requerido";
                return false;
            }
        }

        // Verifica prerequisitos por ID
        if (Data.requiredSkills != null && Data.requiredSkills.Count > 0)
        {
            foreach (var reqId in Data.requiredSkills)
            {
                bool found = false;
                foreach (var s in allSkills)
                {
                    if (s == null || s.Data == null) continue;
                    if (s.Data.skillID == reqId)
                    {
                        found = true;
                        if (!s.IsUnlocked)
                        {
                            reason = $"Requer '{s.Data.skillName}' desbloqueada";
                            return false;
                        }
                        break;
                    }
                }
                if (!found)
                {
                    reason = $"Prerequisito '{reqId}' não encontrado";
                    return false;
                }
            }
        }

        return true;
    }

    // Destrava a skill se possível (usa CanUnlock para validações)
    public bool Unlock(CharacterManager player, IEnumerable<Skill> allSkills, out string reason)
    {
        if (!CanUnlock(player, allSkills, out reason)) return false;
        IsUnlocked = true;
        return true;
    }

    // Verifica se pode subir de nível baseado em skill points do jogador
    public bool CanLevelUp(int playerSkillPoints, out string reason)
    {
        reason = null;
        if (Data == null) { reason = "SkillData não atribuída"; return false; }
        if (!IsUnlocked) { reason = "Skill não está desbloqueada"; return false; }
        if (skillLevel >= Data.maxSkillLevel) { reason = "Nível máximo atingido"; return false; }
        if (playerSkillPoints < Data.requiredSkillPoints) { reason = "Skill points insuficientes"; return false; }
        return true;
    }

    // Sobe o nível da skill consumindo skill points, retorna true se sucesso
    public bool LevelUp(ref int playerSkillPoints, out string reason)
    {
        if (!CanLevelUp(playerSkillPoints, out reason)) return false;
        playerSkillPoints -= Data.requiredSkillPoints;
        skillLevel = Mathf.Clamp(skillLevel + 1, 1, Data.maxSkillLevel);
        return true;
    }

}