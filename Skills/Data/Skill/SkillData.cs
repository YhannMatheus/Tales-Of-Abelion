using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Skill")]
public class SkillData : ScriptableObject
{
    [Header("Basic Info")]
    public string skillID;                                          // identificador único da skill
    public string skillName;                                        // nome exibido da skill
    [TextArea (5,10)]public string description;                     // descrição da skill
    public Sprite icon;                                             // ícone exibido na UI
    public SkillCategory skillCategory = SkillCategory.None;        // categorias (flags)
    public SkillType skillType;                                     // tipo de skill (ativa/passiva/etc)
    public int energyCost;                                          // custo principal (ex: energia) para usar a skill

    [Header("Conditionais")]
    public bool isCastable;                                         // indica se a habilidade pode ser interrompida ou não

    [Header("Animation")]
    public float cooldownTime;                                      // tempo de recarga em segundos
    public float castTime;                                          // tempo de conjuração/casting em segundos
    public float minSpeedVelocity = 0.3f;                          // velocidade mínima usada no cálculo de playback
    public AnimationClip animation;                                // AnimationClip associado à skill
    
    [Header("Audio")]
    public AudioClip castSound;                                    // som tocado ao iniciar o cast
    public AudioClip impactSound;                                  // som tocado ao impactar o alvo

    [Header("Animation Settings")]
    public bool useAttackSpeedForPlayback = true;                  // ajustar playback pela velocidade de ataque do personagem
    public bool matchCastTime = false;                             // forçar duração para igualar castTime
    public float playbackSpeedMultiplier = 1f;                     // multiplicador extra aplicado ao playbackSpeed
    public float minPlaybackSpeed = 0.01f;                         // limite mínimo para playbackSpeed
    public float maxPlaybackSpeed = 10f;                           // limite máximo para playbackSpeed
    public float targetDurationOverride = 0f;                      // override de duração desejada em segundos (se >0)
    public bool forcePlaybackToFitAttacksPerSecond = true;         // forçar ajuste para 1/attackSpeed

    [Header("Execution")]
    // Como ajustar a duração/velocidade da animação: definido em SkillEnum.PlaybackMode
    public PlaybackMode playbackMode = PlaybackMode.FitToAttackSpeed; // modo de cálculo do playback (enum)

    [Header("Animation - Playback & IK")]
    public bool applyRootMotion = false;                           // permite root motion durante a reprodução
    public bool applyFootIK = false;                               // aplicar Foot IK no AnimationClipPlayable
    public bool applyPlayableIK = false;                           // aplicar Playable IK no AnimationClipPlayable
    public bool loopAnimation = false;                             // repetir a animação enquanto a skill estiver ativa
    public float blendInTime = 0f;                                 // tempo de blend de entrada (se usar mixers)
    public float blendOutTime = 0f;                                // tempo de blend de saída (se usar mixers)
    public int animatorLayer = 0;                                  // camada do Animator onde aplicar a animação
    public string animatorStateName = "";                          // nome do estado do Animator para fallback
    public bool useAnimatorStateTrigger = false;                   // usar um estado do Animator em vez de tocar o clip

    [Header("Animation - Events")]
    public bool syncWithAnimationEvents = false;                   // sincronizar efeitos via Animation Events
    public List<float> hitEventNormalizedTimes = new List<float>(); // tempos normalizados (0..1) para disparar hits/efeitos

    [Header("Buffs")]
    public float buffDuration = 0f;                                 // duração padrão do buff aplicado pela skill
    public List<SkillModifier> effects = new List<SkillModifier>(); // modificadores (struct) aplicados
    public bool distributeBuffOverTime = false;                      // distribuir o buff ao longo do tempo (DoT/HoT)
    public float buffTickInterval = 1f;                             // intervalo entre ticks quando distribuído
    public bool isDebuff = false;                                   // marca o buff como debuff se true

    [Header("Targeting")]
    public TargetingMode targetingMode;                             // modo de targeting da skill (Self/Enemy/Area/etc)
    public TargetFilter targetFilter;                               // filtro de alvos (Enemies/Allies/All/etc)

    [Header("Skill Prefab")]
    public GameObject skillPrefab;                                  // prefab usado para efeitos visuais/execução da skill
    [Header("Damage")]
    public float baseDamage;                                        // dano base da skill
    public float damageModifierValue;                               // valor do modificador de dano (aplicado conforme operação)
    public float characterLifePercentInfluence = 0.0f;              // influência do HP do personagem no dano (percentual)
    public DamageType damageType = DamageType.None;                 // tipo de dano
    public DamageScaling damageScaling = DamageScaling.None;        // tipo de scaling do dano
    public ModifierOperation damageModifierOperation = ModifierOperation.Add; // operação do modificador de dano

    [Header("Critical Damage")]
    public CriticalDamageType criticalDamageType = CriticalDamageType.None; // tipo de efeito crítico
    public float criticalDamageValue;                               // valor do crítico (multiplicador ou flat conforme tipo)

    [Header("Healing")]
    public float baseHeal;                                           // cura base da skill
    public HealScaling healScaling = HealScaling.None;               // scaling de cura
    public ModifierOperation healModifierOperation = ModifierOperation.Add; // operação aplicada ao modificador de cura
    public float healModifierValue;                                 // valor do modificador de cura

    [Header("Ranges & Area")]
    public float minRange;                                          // alcance mínimo da skill
    public float maxRange;                                          // alcance máximo da skill
    public float areaRadius;                                        // raio para skills em área
    public float lineWidth;                                         // largura de linha (skill line)

    [Header("Projectile")]
    public ProjectileBehavior projectileBehavior = ProjectileBehavior.Linear; // comportamento do projétil
    public float projectileSpeed;                                   // velocidade do projétil
    public float projectileLifetime;                               // tempo de vida do projétil
    public bool consumeOnHit = true;                                // consumir (destruir) a instância ao atingir um alvo

    [Header("Skill Levels")]
    public int maxSkillLevel = 1;                                   // nível máximo da skill
    public float incrementalDamagePerLevel;                         // dano adicional por nível
    public float incrementalHealPerLevel;                           // cura adicional por nível
}