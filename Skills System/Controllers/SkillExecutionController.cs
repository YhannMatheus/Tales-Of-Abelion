using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillExecutionController
{
    // ----- Config / Referências -----
    public SkillData data = null;
    protected CharacterManager character;
    protected Transform castPoint;

    // ----- Tempo de execução -----
    private float _cooldownTimer = 0f;
    private string _lastFailureReason = "";
    // Runtime for animation-hit sync
    private List<float> _pendingHitTimes;
    private SkillContext _currentContext;
    // Passivos aplicados uma vez
    private bool _passivesApplied = false;
    
    // Auxiliares responsáveis por efeitos e tempos/cooldown
    private SkillEffectController _effectController;
    private SkillTimesController _timesController;
    private SkillAnimationController _animationController;

    // ----- Propriedades -----
    public bool IsOnCooldown => _timesController != null ? _timesController.IsOnCooldown : _cooldownTimer > 0f;
    public float CooldownRemaining => _timesController != null ? _timesController.CooldownRemaining : Mathf.Max(0f, _cooldownTimer);
    public float CooldownPercent => _timesController != null ? _timesController.CooldownPercent : (data != null && data.cooldownTime > 0f ? (Mathf.Clamp01(_cooldownTimer / data.cooldownTime)) : 0f);
    // Expor SkillData para uso externo (ex.: SkillManager)
    public SkillData Data
    {
        get => data;
        set
        {
            data = value;
            _passivesApplied = false; // reset ao trocar skill
        }
    }

    // ----- Eventos (integração com o Manager) -----
    public struct ProjectileSpawnRequest
    {
        public InstanceContext Context;
        public GameObject Prefab;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Direction;
        public CharacterManager HomingTarget;
        public float Speed;
        public float Lifetime;
        public ProjectileBehavior Behavior;
    }

    public event Action<ProjectileSpawnRequest> OnRequestSpawnProjectile;

    // ----- Construtor -----
    public SkillExecutionController(SkillData skillData, CharacterManager characterManager, Transform castTransform)
    {
        data = skillData;
        character = characterManager;
        castPoint = castTransform;
        _effectController = new SkillEffectController(data, character);
        _timesController = new SkillTimesController(data, character);
        _animationController = new SkillAnimationController();
    }

    // Liga referências de runtime (usado pelo SkillManager para injetar Character e pontos de cast)
    public void Bind(CharacterManager characterManager, Transform castTransform)
    {
        character = characterManager;
        castPoint = castTransform;
        // recreate or update helpers
        _effectController = new SkillEffectController(data, character);
        _timesController = new SkillTimesController(data, character);
        if (_animationController == null)
            _animationController = new SkillAnimationController();
    }

    // TryExecute é o ponto de entrada principal para execução de skills ativas

    // ----- TryExecute (entrada que inicia a execução) -----
    public bool TryExecute(SkillContext context)
    {
        if (!CanUse()) return false;

        if (data.energyCost > 0)
        {
            if (!character.TrySpendEnergy(data.energyCost))
            {
                _lastFailureReason = "Falha ao gastar energia";
                return false;
            }
        }

        // Dispara animação da skill via SkillAnimationController
        if (data.animation != null && character != null)
        {
            var animator = character.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                float attackSpeed = character.Data.TotalAttackSpeed;
                _animationController?.Play(animator, data, attackSpeed, null);
            }
        }

        // Prepara InstanceContext com valores finais que serão aplicados pela instância/projétil
        var instanceCtx = new InstanceContext
        {
            SkillData = data,
            Caster = character,
            TargetCharacter = context.TargetCharacter != null ? context.TargetCharacter.gameObject : null,
            DamageType = data.damageType,
            totalDamageAmount = CalculateDamage(context),
            totalHealAmount = context.TargetCharacter != null ? CalculateHeal(context, context.TargetCharacter) : 0f,
            consumedOnHit = data.consumeOnHit
        };
        // Preencher lista de efeitos que a instância carregará (somente OnHit/OverTime)
        instanceCtx.Effects = new List<EffectData>();
        if (data.Effects != null)
        {
            foreach (var ef in data.Effects)
            {
                if (ef == null) continue;
                if (ef.effectTiming == EffectTiming.OnHit || ef.effectTiming == EffectTiming.OverTime)
                    instanceCtx.Effects.Add(ef);
            }
        }
        instanceCtx.SkillLevel = context.SkillLevel;
        instanceCtx.TargetPosition = context.TargetPosition;

        // Se existe prefab de skill e velocidade de projétil configurada, solicita spawn ao manager.
        if (data.skillPrefab != null && data.projectileSpeed > 0f)
        {
            var req = new ProjectileSpawnRequest
            {
                Context = instanceCtx,
                Prefab = data.skillPrefab,
                Position = castPoint != null ? castPoint.position : (context.TargetPosition != Vector3.zero ? context.TargetPosition : character.transform.position),
                Rotation = castPoint != null ? castPoint.rotation : Quaternion.identity,
                Direction = (context.TargetPosition != Vector3.zero)
                    ? (context.TargetPosition - (castPoint != null ? castPoint.position : character.transform.position)).normalized
                    : (castPoint != null ? castPoint.forward : character.transform.forward),
                HomingTarget = context.TargetCharacter,
                Speed = data.projectileSpeed,
                Lifetime = data.projectileLifetime,
                Behavior = data.projectileBehavior
            };

            if (OnRequestSpawnProjectile != null)
            {
                OnRequestSpawnProjectile.Invoke(req);
            }
        }
        else
        {
            // Aplica efeitos imediatamente (cura, buff, etc.) ou aguarda eventos de hit se houver tempos definidos
            if (data.hitEventNormalizedTimes != null && data.hitEventNormalizedTimes.Count > 0)
            {
                // efeitos do tipo OnHit serão aplicados pela sincronização de animação quando os tempos forem alcançados
            }
            else
            {
                ApplySkillEffects(context, EffectTiming.OnCast);
            }
        }

        // Inicia animação associada (se houver)
        StartAnimation(context, context.OnComplete);
        StartCooldown();
        return true;
    }

    // ----- Cooldown / Tempo de recarga -----
    private void StartCooldown()
    {
        if (_timesController != null) _timesController.StartCooldown();
        else _cooldownTimer = data != null ? data.cooldownTime : 0f;
    }

    public void TickCooldown(float deltaTime)
    {
        if (_timesController != null) { _timesController.TickCooldown(deltaTime); return; }
        if (_cooldownTimer <= 0f) return;
        _cooldownTimer = Mathf.Max(0f, _cooldownTimer - deltaTime);
    }

    // ----- Aplicar efeitos (cura, buffs/debuffs) -----
    // 'when' indica o timing de aplicação (OnCast, OnHit, OverTime, Passive, Aura)
    private void ApplySkillEffects(SkillContext context, EffectTiming when = EffectTiming.OnHit)
    {
        if (_effectController != null)
        {
            _effectController.ApplySkillEffects(context, when);
        }
    }

    // Verifica filtro de alvos com base em CharacterType
    private bool PassesTargetFilter(CharacterManager target, TargetFilter filter)
    {
        if (target == null) return false;
        switch (filter)
        {
            case TargetFilter.All:
                return true;
            case TargetFilter.Self:
                return target == character;
            case TargetFilter.AllExceptSelf:
                return target != character;
            case TargetFilter.Allies:
                return IsAlly(target);
            case TargetFilter.Enemies:
                return !IsAlly(target) && target != character;
            default:
                return true;
        }
    }

    private bool IsAlly(CharacterManager other)
    {
        if (other == null) return false;
        if (character == null) return false;
        if (other == character) return true;
        // Consider Player <-> Ally as same team
        if (character.characterType == other.characterType) return true;
        if ((character.characterType == CharacterType.Player && other.characterType == CharacterType.Ally) ||
            (character.characterType == CharacterType.Ally && other.characterType == CharacterType.Player)) return true;
        return false;
    }

    public void ExecutePassive()
    {
        if (_passivesApplied) return;
        _effectController?.ExecutePassive();
        _passivesApplied = true;
    }

    // ExecuteToggle/AutoCast removidos - use TryExecute diretamente

    // ----- Integração com animação -----
    private void StartAnimation(SkillContext context, Action onComplete = null, float explicitAttackSpeed = -1f)
    {
        // delegar para SkillTimesController; callback onHit aplica efeitos via SkillEffectController
        _timesController?.StartAnimation(context, (ctx) => ApplySkillEffects(ctx, EffectTiming.OnHit), onComplete, explicitAttackSpeed);
    }

    public void TickAnimation(float deltaTime)
    {
        // avança o controlador de tempos/animação
        _timesController?.TickAnimation(deltaTime);
        // atualiza PlayableGraph da animação de skill
        _animationController?.Tick(deltaTime);
    }

    public void StopAnimation()
    {
        // interrompe animação e limpa estado de timing
        _timesController?.StopAnimation();
    }

    /// <summary>
    /// Tenta interromper a animação/execução atual (por exemplo quando o player se move).
    /// Retorna true se a animação foi cancelada. A interrupção respeita o castTime
    /// definido no SkillData (não cancela se já passou do tempo mínimo).
    /// </summary>
    public bool TryInterruptAnimation()
    {
        if (_timesController == null) return false;
        return _timesController.TryInterruptAnimation();
    }

    // Animation progress handled by SkillTimesController

    // ----- Cálculos / Auxiliares -----
    
    /// <summary>
    /// Calcula velocidade de playback da animação (público para eventos)
    /// </summary>
    public float CalculatePlaybackSpeed()
    {
        return CalculatePlaybackSpeed(data, character, -1f);
    }

    private float CalculatePlaybackSpeed(SkillData data, CharacterManager character, float explicitAttackSpeed = -1f)
    {
        if (data == null || data.animation == null) return 1f;

        float clipLength = data.animation.length;

        float attackSpeed = explicitAttackSpeed > 0f ? explicitAttackSpeed : (character != null ? character.Data.TotalAttackSpeed : 1f);

        float desiredDuration = 0f;
        if (data.targetDurationOverride > 0f)
        {
            desiredDuration = data.targetDurationOverride;
        }
        else if (data.playbackMode == PlaybackMode.FitToCastTime && data.castTime > 0f)
        {
            desiredDuration = data.castTime;
        }
        else if (data.playbackMode == PlaybackMode.FitToAttackSpeed && data.useAttackSpeedForPlayback)
        {
            desiredDuration = 1f / Mathf.Max(0.0001f, attackSpeed);
        }
        else
        {
            desiredDuration = data.castTime > 0f ? data.castTime : clipLength;
        }

        float playbackSpeed = clipLength / Mathf.Max(0.0001f, desiredDuration);
        playbackSpeed *= Mathf.Max(0.0001f, data.playbackSpeedMultiplier);
        playbackSpeed = Mathf.Clamp(playbackSpeed, data.minPlaybackSpeed, data.maxPlaybackSpeed);

        return playbackSpeed;
    }

    private float CalculateHeal(SkillContext context, CharacterManager target)
    {
        float heal = data.baseHeal + (data.incrementalHealPerLevel * (context.SkillLevel - 1));
        float casterMag = character.Data.TotalMagicalDamage;

        switch (data.healModifierOperation)
        {
            case ModifierOperation.Add:
                heal += casterMag;
                break;
            case ModifierOperation.Multiply:
                heal += casterMag * data.healModifierValue;
                break;
            case ModifierOperation.Override:
                heal = casterMag > 0 ? casterMag : heal;
                break;
        }

        switch (data.healScaling)
        {
            case HealScaling.FlatAmount:
                break;
            case HealScaling.PercentMaxHP:
                if (target != null) heal += target.Health.MaxHealth * data.healModifierValue;
                break;
            case HealScaling.PercentMissingHP:
                if (target != null)
                {
                    float missing = Mathf.Max(0f, target.Health.MaxHealth - target.Health.CurrentHealth);
                    heal += missing * data.healModifierValue;
                }
                break;
            case HealScaling.SpellPower:
                heal += casterMag * data.healModifierValue;
                break;
            case HealScaling.None:
            default:
                break;
        }

        return heal;
    }

    private float CalculateDamage(SkillContext context)
    {
        float damage = data.baseDamage + (data.incrementalDamagePerLevel * (context.SkillLevel - 1));
        float casterPhys = character.Data.TotalPhysicalDamage;
        float casterMag = character.Data.TotalMagicalDamage;

        switch (data.damageType)
        {
            case DamageType.Physical:
                ApplyStatComponent(ref damage, casterPhys, data.damageModifierValue, data.healModifierOperation);
                break;
            case DamageType.Magical:
                ApplyStatComponent(ref damage, casterMag, data.damageModifierValue, data.healModifierOperation);
                break;
            case DamageType.Mixed:
                float physComp = 0f;
                float magComp = 0f;
                ApplyStatComponent(ref physComp, casterPhys, data.damageModifierValue, data.healModifierOperation);
                ApplyStatComponent(ref magComp, casterMag, data.damageModifierValue, data.healModifierOperation);
                damage += physComp + magComp;
                break;
            case DamageType.True:
                return Mathf.Max(0f, damage);
        }

        switch (data.damageScaling)
        {
            case DamageScaling.Health:
                damage += character.Health.MaxHealth * data.characterLifePercentInfluence;
                break;
            case DamageScaling.MissingHealth:
                float missingHealth = Mathf.Max(0f, character.Health.MaxHealth - character.Health.CurrentHealth);
                damage += missingHealth * data.characterLifePercentInfluence;
                break;
            case DamageScaling.TargetHealth:
                if (context.TargetCharacter != null) damage += context.TargetCharacter.Health.MaxHealth * data.characterLifePercentInfluence;
                break;
            case DamageScaling.TargetMissingHealth:
                if (context.TargetCharacter != null)
                {
                    float targetMissing = Mathf.Max(0f, context.TargetCharacter.Health.MaxHealth - context.TargetCharacter.Health.CurrentHealth);
                    damage += targetMissing * data.characterLifePercentInfluence;
                }
                break;
            case DamageScaling.None:
            default:
                break;
        }

        if (IsCriticalHit())
        {
            switch (data.criticalDamageType)
            {
                case CriticalDamageType.FlatIncrease:
                    damage += data.criticalDamageValue;
                    break;
                case CriticalDamageType.PercentageIncrease:
                    damage *= 1f + data.criticalDamageValue / 100f;
                    break;
            }
        }

        return Mathf.Max(0f, damage);
    }

    private void ApplyStatComponent(ref float target, float statValue, float modifierValue, ModifierOperation op)
    {
        switch (op)
        {
            case ModifierOperation.Add:
                target += statValue;
                break;
            case ModifierOperation.Multiply:
                target += statValue * modifierValue;
                break;
            case ModifierOperation.Override:
                target = statValue;
                break;
        }
    }

    private bool IsCriticalHit()
    {
        float critChancePercent = character.Data.TotalCriticalChance;
        return UnityEngine.Random.value < (critChancePercent / 100f);
    }

    public bool CanUse()
    {
        _lastFailureReason = "";
        
        if (data == null)
        {
            _lastFailureReason = "Skill não configurada";
            return false;
        }
        if (IsOnCooldown)
        {
            _lastFailureReason = $"Em cooldown ({CooldownRemaining:F1}s restantes)";
            return false;
        }
        if (character == null)
        {
            _lastFailureReason = "Personagem não atribuído";
            return false;
        }
        if (data.energyCost > 0 && (character.Energy == null || !character.Energy.HasEnoughEnergy(data.energyCost)))
        {
            _lastFailureReason = "Energia insuficiente";
            return false;
        }
        return true;
    }

    public string GetLastFailureReason()
    {
        return string.IsNullOrEmpty(_lastFailureReason) ? "Não pode usar agora" : _lastFailureReason;
    }
}
