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

    // Runtime / valores resolvidos
    public float ResolvedAttackSpeed;      // attackSpeed resolvido do caster (ou override)
    public float PlaybackSpeed;            // multiplicador a ser passado para AnimationClipPlayable.SetSpeed
    public float DesiredDuration;          // duração alvo (1/attackSpeed ou castTime ou override)
    public float ClipLength;               // comprimento do clip (quando houver)
    public float CastTime;                 // castTime (copiado do SkillData)
    public float Cooldown;                 // cooldown (copiado do SkillData)
    public int SlotIndex;                  // índice do slot que gerou essa execução

    // Eventos / sincronização
    public bool UseAnimationEvents;        // usar Animation Events do clip
    public List<float> HitEventNormalizedTimes; // tempos normalizados 0..1 para disparar hits
    public int NextHitEventIndex;          // índice do próximo hit a disparar (runtime)

    // Estado e callbacks
    public Action OnComplete;              // callback chamado quando a execução terminar
    public bool IsCritical;                // resultado de roll crítico (se já calculado)
    public bool WasCancelled;              // flag setada se a execução foi cancelada


    // Dados livres (extensibilidade)
    public Dictionary<string, object> Metadata;

    // Construtor helper (opcional)
    public SkillContext(SkillData skill, CharacterManager caster, CharacterManager targetCharacter, Vector3 targetPos, int skillLevel = 1, int slot = -1)
    {
        Skill = skill;
        SkillLevel = skillLevel;
        Caster = caster;
        TargetCharacter = targetCharacter != null ? targetCharacter : null;
        TargetPosition = targetPos;

        ResolvedAttackSpeed = caster != null ? caster.Data.TotalAttackSpeed : 1f;
        ClipLength = skill?.animation != null ? skill.animation.length : 0f;
        CastTime = skill != null ? skill.castTime : 0f;
        Cooldown = skill != null ? skill.cooldownTime : 0f;
        SlotIndex = slot;

        // preencher listas
        UseAnimationEvents = skill != null ? skill.syncWithAnimationEvents : false;
        HitEventNormalizedTimes = skill != null ? new List<float>(skill.hitEventNormalizedTimes) : new List<float>();
        NextHitEventIndex = 0;

        // playback/duração devem ser preenchidos pelo caller (CalculatePlaybackSpeed)
        PlaybackSpeed = 1f;
        DesiredDuration = 0f;

        OnComplete = null;
        IsCritical = false;
        WasCancelled = false;
        Metadata = null;
    }
}