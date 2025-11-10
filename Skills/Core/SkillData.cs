using System.Collections.Generic;
using UnityEngine;

// ScriptableObject principal que define uma skill
[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("═══ Informações Básicas ═══")]
    [Tooltip("Nome da skill")]
    public string skillName = "Nova Skill";
    
    [Tooltip("Ícone da skill (UI)")]
    public Sprite icon;
    
    [Tooltip("Descrição da skill")]
    [TextArea(3, 5)]
    public string description = "Descrição da skill";
    
    [Tooltip("Tipo da skill")]
    public SkillType skillType = SkillType.Active;
    
    [Tooltip("Categoria(s) da skill")]
    public SkillCategory category = SkillCategory.Attack;

    [Header("═══ Requisitos ═══")]
    [Tooltip("Requisitos para desbloquear/usar")]
    public SkillRequirements requirements = new SkillRequirements();

    [Header("═══ Custo ═══")]
    [Tooltip("Custo de recursos")]
    public ResourceCost cost = new ResourceCost();

    [Header("═══ Cooldown ═══")]
    [Tooltip("Configuração de cooldown")]
    public CooldownData cooldown = new CooldownData();

    [Header("═══ Casting ═══")]
    [Tooltip("Tempo de cast em segundos (0 = instantâneo)")]
    public float castTime = 0f;
    
    [Tooltip("Pode ser cancelada por movimento")]
    public bool canceledByMovement = true;
    
    [Tooltip("Pode ser cancelada por dano")]
    public bool canceledByDamage = false;
    
    [Tooltip("Nome da animação a ser tocada")]
    public string animationName = "";
    
    [Tooltip("Usa trigger 'isUsingAbility' do animator")]
    public bool usesAbilityTrigger = true;
    
    [Tooltip("Índice da ability no animator (se usesAbilityTrigger = true)")]
    public int abilityIndex = 0;

    [Header("═══ Targeting ═══")]
    [Tooltip("Configuração de targeting")]
    public TargetingData targeting = new TargetingData();

    [Header("═══ Efeitos ═══")]
    [Tooltip("Lista de efeitos executados quando skill é usada (SEM valores, só lógica)")]
    public List<SkillEffect> effects = new List<SkillEffect>();

    [Header("═══ Damage Configuration ═══")]
    [Tooltip("Dano base da skill")]
    public float baseDamage = 0f;
    
    [Tooltip("Tipo de dano")]
    public SkillDamageType damageType = SkillDamageType.Physical;
    
    [Tooltip("Como o dano escala com stats")]
    public DamageScaling damageScaling = DamageScaling.Physical;
    
    [Tooltip("Multiplicador de Physical Damage do caster")]
    [Range(0f, 5f)]
    public float physicalDamageRatio = 1f;
    
    [Tooltip("Multiplicador de Magical Damage do caster")]
    [Range(0f, 5f)]
    public float magicalDamageRatio = 0f;
    
    [Tooltip("Multiplicador de HP máximo")]
    [Range(0f, 1f)]
    public float healthRatio = 0f;
    
    [Tooltip("Ignora defesa do alvo")]
    public bool ignoresArmor = false;
    
    [Tooltip("Pode dar critical hit")]
    public bool canCrit = true;
    
    [Tooltip("Multiplicador de crítico")]
    public float critMultiplier = 2f;

    [Header("═══ Area Configuration ═══")]
    [Tooltip("Raio da área (se usar AreaEffect)")]
    public float areaRadius = 3f;
    
    [Tooltip("Formato da área")]
    public SkillAreaShape areaShape = SkillAreaShape.Circle;
    
    [Tooltip("Ângulo do cone (se shape = Cone)")]
    [Range(0f, 360f)]
    public float coneAngle = 90f;
    
    [Tooltip("Distância forward do caster")]
    public float forwardOffset = 0f;
    
    [Tooltip("Filtro de alvos")]
    public TargetFilter targetFilter = TargetFilter.Enemies;
    
    [Tooltip("Número máximo de alvos (0 = ilimitado)")]
    public int maxTargets = 0;

    [Header("═══ Buff/Debuff Configuration ═══")]
    [Tooltip("Duração do buff/debuff aplicado pela skill (em segundos)")]
    public float buffDuration = 0f;
    
    [Tooltip("Modificadores de stats aplicados pela skill")]
    public List<Modifier> buffModifiers = new List<Modifier>();
    
    [Tooltip("Distribuir efeito ao longo do tempo (DoT/HoT)")]
    public bool distributeBuffOverTime = false;
    
    [Tooltip("Intervalo entre ticks do buff (se distribuído)")]
    public float buffTickInterval = 1f;

    [Header("═══ Visual/Audio ═══")]
    [Tooltip("Prefab de efeito visual (spawn no caster)")]
    public GameObject casterVFXPrefab;
    
    [Tooltip("Som ao usar skill")]
    public AudioClip castSound;
    
    [Tooltip("Volume do som (0-1)")]
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    [Header("═══ Upgrades (Opcional) ═══")]
    [Tooltip("Nível máximo da skill")]
    public int maxLevel = 1;
    
    [Tooltip("Skill pode ser evoluída")]
    public bool canUpgrade = false;

    // Valida se skill pode ser usada
    public bool CanUse(CharacterManager caster)
    {
        if (caster == null)
            return false;

        // Verifica requisitos
        if (!requirements.IsMet(caster))
        {
            Debug.Log($"[{skillName}] Requisitos não atendidos: {requirements.GetFailureReason(caster)}");
            return false;
        }

        // Verifica custo
        if (!cost.CanAfford(caster))
        {
            Debug.Log($"[{skillName}] Recursos insuficientes: {cost.GetFailureReason(caster)}");
            return false;
        }

        return true;
    }

    // Consome recursos da skill
    public void ConsumeResources(CharacterManager caster)
    {
        if (caster == null)
            return;

        cost.Consume(caster);
    }

    // Retorna descrição formatada (útil para tooltips)
    public string GetFormattedDescription()
    {
        string formatted = description;

        // Adiciona custo
        if (cost.energy > 0f)
            formatted += $"\n\n<color=cyan>Custo: {cost.energy} Energia</color>";

        // Adiciona cooldown
        if (cooldown.duration > 0f)
            formatted += $"\n<color=yellow>Cooldown: {cooldown.duration}s</color>";

        // Adiciona alcance
        if (targeting.range > 0f)
            formatted += $"\n<color=green>Alcance: {targeting.range}m</color>";

        return formatted;
    }

    // Validação no editor
    void OnValidate()
    {
        // Garante que maxLevel é pelo menos 1
        if (maxLevel < 1)
            maxLevel = 1;

        // Se pode upgrade, maxLevel deve ser > 1
        if (canUpgrade && maxLevel == 1)
            maxLevel = 5;

        // Se não tem efeitos, avisa
        if (effects.Count == 0)
        {
            Debug.LogWarning($"[{skillName}] Skill não tem nenhum efeito configurado!");
        }

        // Valida nome da animação
        if (!string.IsNullOrEmpty(animationName) && usesAbilityTrigger)
        {
            Debug.LogWarning($"[{skillName}] animationName e usesAbilityTrigger estão configurados. Use apenas um!");
        }
    }
}
