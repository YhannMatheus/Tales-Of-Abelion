using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SkillContext
{
    // Identidade / design
    public SkillData Skill;                // ScriptableObject da skill
    public int SkillLevel;                 // nível da skill usada

    // Quem e onde00
    public CharacterManager Caster;        // personagem que lançou a skill
    public CharacterManager TargetCharacter; // alvo (se aplicável)
    public Vector3 TargetPosition;         // posição alvo (skill de posição/direção)
    // Estado e callbacks
    public Action OnComplete;              // callback chamado quando a execução terminar

    public Dictionary<string, object> Metadata;

    // Construtor helper (opcional)
    public SkillContext(SkillData skill, CharacterManager caster, CharacterManager targetCharacter, Vector3 targetPos, int skillLevel = 1)
    {
        Skill = skill;
        SkillLevel = skillLevel;
        Caster = caster;
        TargetCharacter = targetCharacter != null ? targetCharacter : null;
        TargetPosition = targetPos;

        OnComplete = null;
        Metadata = null;
    }
}