using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct InstanceContext // informações que a instancia poderá usar
{   
    public SkillData SkillData;          // dados da skill que gerou essa instancia
    public CharacterManager Caster;        // personagem que lançou a skill
    public GameObject TargetCharacter;     // alvo da skill (pode ser nulo)
    public List<EffectData> Effects;       // efeitos que esta instância carregará (OnHit/OverTime)
    public int SkillLevel;                 // nível da skill
    public Vector3 TargetPosition;         // posição alvo

    // Alvo afetado/resultado
    public DamageType DamageType;         // tipo de dano aplicado pela skill
    public float totalDamageAmount;      // dano total calculado para essa instancia
    public float totalHealAmount;        // cura total calculada para essa instancia

    public bool consumedOnHit;           // indica se a instancia deve ser consumida ao atingir um alvo
    public InstanceContext(CharacterManager caster, GameObject targetCharacter = null, SkillData skillData = null)
    {
        Caster = caster;
        TargetCharacter = targetCharacter;
        SkillData = skillData;

        Effects = new List<EffectData>();
        SkillLevel = 1;
        TargetPosition = Vector3.zero;

        DamageType = DamageType.None;
        totalDamageAmount = 0f;
        totalHealAmount = 0f;

        consumedOnHit = false;
    }
}   