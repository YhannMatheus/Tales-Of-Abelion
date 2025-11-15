using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct InstanceContext // informações que a instancia poderá usar
{
    public SkillData Skill;                // ScriptableObject da skill
    public int SkillLevel;                 // nível da skill usada
    public CharacterManager Caster;        // personagem que lançou a skill
    public CharacterManager TargetCharacter; // alvo (se aplicável)
    public GameObject TargetGameObject;    // alvo genérico (prefab/objeto)
    public Vector3 TargetPosition;         // posição alvo (skill de posição/direção)

    // Alvo afetado/resultado
    public DamageType DamageType;    // tipo de dano aplicado pela skill
    public float totalDamageAmount;      // dano total calculado para essa instancia
    public float totalHealAmount;        // cura total calculada para essa instancia

    public InstanceContext(SkillData skill, CharacterManager caster, Vector3 targetPos, int skillLevel = 1)
    {
        Skill = skill;
        SkillLevel = skillLevel;
        Caster = caster;
        TargetCharacter = null;
        TargetGameObject = null;
        TargetPosition = targetPos;

        DamageType = DamageType.None;
        
        totalDamageAmount = 0f;
        totalHealAmount = 0f;
    }
}   