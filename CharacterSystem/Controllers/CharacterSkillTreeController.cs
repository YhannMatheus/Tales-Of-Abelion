using System;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class CharacterSkillTreeController
{
    public List<ClassSkill> classSkills = new List<ClassSkill>();
    public List<SkillData> raceSkills = new List<SkillData>();
    
    public CharacterManager characterManager;

    public int GetSkillLevel(SkillData skillData)
    {
        if (skillData == null) return 0;

        var classSkill = classSkills.Find(s => s.skillData == skillData);
        if (classSkill != null && classSkill.IsUnlocked)
        {
            return classSkill.currentLevel;
        }

        return 0;
    }

    public bool TryUnlockSkill(SkillData skillData)
    {
        if (skillData == null) return false;

        var data = characterManager?.Data;
        if (data == null)
        {
            Debug.LogWarning("Dados do personagem são nulos. Não é possível desbloquear a habilidade.");
            return false;
        }

        // Tenta por referência primeiro, depois por nome (case-insensitive) como fallback
        ClassSkill classSkill = classSkills.Find(s => s.skillData == skillData);
        if (classSkill == null && !string.IsNullOrEmpty(skillData.name))
        {
            classSkill = classSkills.Find(s => s.skillData != null &&
                string.Equals(s.skillData.name, skillData.name, System.StringComparison.OrdinalIgnoreCase));
        }

        if (classSkill == null)
        {
            Debug.LogWarning($"Habilidade '{skillData?.name}' não encontrada nas habilidades de classe.");
            return false;
        }

        if (classSkill.IsUnlocked) return false;

        if (data.level < skillData.requiredLevel)
        {
            Debug.Log($"Não é possível desbloquear '{skillData.name}' - requer nível {skillData.requiredLevel}.");
            return false;
        }

        if (data.skillPoints < skillData.requiredSkillPoints)
        {
            Debug.Log($"Pontos de habilidade insuficientes para desbloquear '{skillData.name}'.");
            return false;
        }

        var prereqs = skillData.requiredSkills ?? new List<string>();
        foreach (var reqName in prereqs)
        {
            bool satisfied = classSkills.Exists(cs =>
                cs.skillData != null &&
                string.Equals(cs.skillData.name, reqName, System.StringComparison.OrdinalIgnoreCase) &&
                cs.IsUnlocked);

            if (!satisfied)
            {
                Debug.Log($"Não é possível desbloquear '{skillData.name}' - requisito '{reqName}' não satisfeito.");
                return false;
            }
        }

        // Consome pontos e desbloqueia
        data.skillPoints -= skillData.requiredSkillPoints;
        classSkill.IsUnlocked = true;
        Debug.Log($"Habilidade desbloqueada: {skillData.name}");
        OnSkillUnlocked?.Invoke(classSkill);
        return true;

    }

    public void LoadSkillsFromData(CharacterData data)
    {
        classSkills.Clear();

        foreach (var skillData in data.characterClass.classSkillTree)
        {
            ClassSkill classSkill = new ClassSkill
            {
                skillData = skillData,
                IsUnlocked = true
            };
            classSkills.Add(classSkill);
        }
        OnSkillTreeUpdated?.Invoke(classSkills.ConvertAll(cs => cs.skillData));
    }

    public void LoadRaceSkills(CharacterData data)
    {
        raceSkills.Clear();

        foreach (var skillData in data.characterRace.raceSkills)
        {
            raceSkills.Add(skillData);
        }
    }
    
    // ____ Eventos para UI ____
    public Action <List<SkillData>> OnSkillTreeUpdated;
    public Action<ClassSkill> OnSkillUnlocked;
}


[System.Serializable]
public class ClassSkill
{
    public SkillData skillData;
    public int currentLevel;
    public bool IsUnlocked;

}