using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillExecutionController
{
    // ----- Config / Referências -----
    // runtime Skill wrapper (contém SkillruntimeSkill.Data + estado mutável)
    public Skill runtimeSkill = null;
    protected CharacterManager character;
    protected Transform castPoint;

    // ----- Tempo de execução -----
    private float _cooldownRemainingSeconds = 0f;
    private string _lastFailureReasonMessage = "";
    
    
    // Controllers auxiliares (efeitos, tempos e animação) encapsulados
    private SkillEffectController _skillEffectController;
    private SkillTimesController _skillTimesController;
    private SkillAnimationController _skillAnimationController;

    // ----- Propriedades -----
    public bool IsOnCooldown => _skillTimesController != null ? _skillTimesController.IsOnCooldown : _cooldownRemainingSeconds > 0f;
    public float CooldownRemaining => _skillTimesController != null ? _skillTimesController.CooldownRemaining : Mathf.Max(0f, _cooldownRemainingSeconds);
    public float CooldownPercent => _skillTimesController != null ? _skillTimesController.CooldownPercent : (runtimeSkill.Data != null && runtimeSkill.Data.cooldownTime > 0f ? (Mathf.Clamp01(_cooldownRemainingSeconds / runtimeSkill.Data.cooldownTime)) : 0f);
    
    #region Execuções E metodos Publicos
    // TryExecute é o ponto de entrada principal para execução de skills ativas
    public bool TryExecute(SkillContext context)
    {
        if (!CanUse()) return false;

        var d = runtimeSkill.Data;
        if (d == null)
        {
            _lastFailureReasonMessage = "Skill não configurada";
            return false;
        }

        if (d.energyCost > 0)
        {
            if (!character.TrySpendEnergy(d.energyCost))
            {
                _lastFailureReasonMessage = "Falha ao gastar energia";
                return false;
            }
        }

        // Dispara animação da skill via SkillAnimationController
        if (d.animation != null && character != null)
        {
            var animator = character.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    float attackSpeed = character.Data.TotalAttackSpeed;
                    _skillAnimationController?.Play(animator, d, attackSpeed, null);
                }
        }

        // Prepara InstanceContext com valores finais que serão aplicados pela instância/projétil
            var instanceCtx = new InstanceContext
            {
                SkillData = d,
                Caster = character,
                TargetCharacter = context.Target,
                DamageType = d.damageType,
                totalDamageAmount = CalculateDamage(context),
                totalHealAmount = context.Target != null && context.Target.GetComponent<CharacterManager>() != null ? CalculateHeal(context, context.Target.GetComponent<CharacterManager>()) : 0f,
                consumedOnHit = d.consumeOnHit
            };

        // Preencher lista de efeitos que a instância carregará (somente OnHit/OverTime)
        instanceCtx.Effects = new List<EffectData>();
        if (d.Effects != null)
        {
            foreach (var ef in d.Effects)
            {
                if (ef == null) continue;
                if (ef.effectTiming == EffectTiming.OnHit)
                    instanceCtx.Effects.Add(ef);
            }
        }
        instanceCtx.SkillLevel = context.SkillLevel;
        instanceCtx.TargetPosition = context.TargetPosition;

        // Se existe prefab de skill e velocidade de projétil configurada, solicita spawn ao manager.
        if (d.skillPrefab != null && d.projectileSpeed > 0f)
        {
            
        }
        else
        {
            // Aplica efeitos imediatamente (cura, buff, etc.) ou aguarda eventos de hit se houver tempos definidos
            if (d.hitEventNormalizedTimes != null && d.hitEventNormalizedTimes.Count > 0)
            {
                // efeitos do tipo OnHit serão aplicados pela sincronização de animação quando os tempos forem alcançados
            }
            else
            {
                // usar o controller de efeitos diretamente (sem wrapper)
                _skillEffectController?.ApplySkillEffects(context, EffectTiming.OnCast);
            }
        }

        // Inicia animação associada (se houver)
        StartAnimation(context, context.OnComplete);
        StartCooldown();
        return true;
    }

    public void TryExecutePassive(SkillContext context)
    {
        // Aplica efeitos passivos imediatamente
        _skillEffectController?.ApplySkillEffects(context, EffectTiming.Passive);
    }
    #endregion
    
    
    // ----- Cooldown / Tempo de recarga -----
    private void StartCooldown()
    {
        if (_skillTimesController != null) _skillTimesController.StartCooldown();
        else _cooldownRemainingSeconds = runtimeSkill.Data != null ? runtimeSkill.Data.cooldownTime : 0f;
    }

    public void TickCooldown(float deltaTime)
    {
        if (_skillTimesController != null) { _skillTimesController.TickCooldown(deltaTime); return; }
        if (_cooldownRemainingSeconds <= 0f) return;
        _cooldownRemainingSeconds = Mathf.Max(0f, _cooldownRemainingSeconds - deltaTime);
    }


    private void StartAnimation(SkillContext context, Action onComplete = null, float explicitAttackSpeed = -1f)
    {
        // delegar para SkillTimesController; callback onHit aplica efeitos via SkillEffectController
        _skillTimesController?.StartAnimation(context, (ctx) => _skillEffectController?.ApplySkillEffects(ctx, EffectTiming.OnHit), onComplete, explicitAttackSpeed);
    }

    public void TickAnimation(float deltaTime)
    {
        _skillTimesController?.TickAnimation(deltaTime);
        _skillAnimationController?.Tick(deltaTime);
    }

    #region  Calculo de valores de dano/curas
    private float CalculateHeal(SkillContext context, CharacterManager target)
    {
        var d = runtimeSkill.Data;
        if (d == null) return 0f;

        float heal = d.baseHeal + (d.incrementalHealPerLevel * (context.SkillLevel - 1));
        float casterMag = character.Data.TotalMagicalDamage;

        switch (d.healModifierOperation)
        {
            case ModifierOperation.Add:
                heal += casterMag;
                break;
            case ModifierOperation.Multiply:
                heal += casterMag * d.healModifierValue;
                break;
            case ModifierOperation.Override:
                heal = casterMag > 0 ? casterMag : heal;
                break;
        }

        switch (d.healScaling)
        {
            case HealScaling.FlatAmount:
                break;
            case HealScaling.PercentMaxHP:
                if (target != null) heal += target.Health.MaxHealth * d.healModifierValue;
                break;
            case HealScaling.PercentMissingHP:
                if (target != null)
                {
                    float missing = Mathf.Max(0f, target.Health.MaxHealth - target.Health.CurrentHealth);
                    heal += missing * d.healModifierValue;
                }
                break;
            case HealScaling.SpellPower:
                heal += casterMag * d.healModifierValue;
                break;
            case HealScaling.None:
            default:
                break;
        }

        return heal;
    }

    private float CalculateDamage(SkillContext context)
    {
        var d = runtimeSkill.Data;
        if (d == null) return 0f;

        float damage = d.baseDamage + (d.incrementalDamagePerLevel * (context.SkillLevel - 1));
        float casterPhys = character.Data.TotalPhysicalDamage;
        float casterMag = character.Data.TotalMagicalDamage;

        switch (d.damageType)
        {
            case DamageType.Physical:
                ApplyStatComponent(ref damage, casterPhys, d.damageModifierValue, d.healModifierOperation);
                break;
            case DamageType.Magical:
                ApplyStatComponent(ref damage, casterMag, d.damageModifierValue, d.healModifierOperation);
                break;
            case DamageType.Mixed:
                float physComp = 0f;
                float magComp = 0f;
                ApplyStatComponent(ref physComp, casterPhys, d.damageModifierValue, d.healModifierOperation);
                ApplyStatComponent(ref magComp, casterMag, d.damageModifierValue, d.healModifierOperation);
                damage += physComp + magComp;
                break;
            case DamageType.True:
                return Mathf.Max(0f, damage);
        }

        switch (d.damageScaling)
        {
            case DamageScaling.Health:
                damage += character.Health.MaxHealth * d.characterLifePercentInfluence;
                break;
            case DamageScaling.MissingHealth:
                float missingHealth = Mathf.Max(0f, character.Health.MaxHealth - character.Health.CurrentHealth);
                damage += missingHealth * d.characterLifePercentInfluence;
                break;
            case DamageScaling.TargetHealth:
                if (context.Target != null) damage += context.Target.GetComponent<CharacterManager>().Health.MaxHealth * d.characterLifePercentInfluence;
                break;
            case DamageScaling.TargetMissingHealth:
                if (context.Target != null)
                {
                    float targetMissing = Mathf.Max(0f, context.Target.GetComponent<CharacterManager>().Health.MaxHealth - context.Target.GetComponent<CharacterManager>().Health.CurrentHealth);
                    damage += targetMissing * d.characterLifePercentInfluence;
                }
                break;
            case DamageScaling.None:
            default:
                break;
        }

        if (IsCriticalHit())
        {
            switch (d.criticalDamageType)
            {
                case CriticalDamageType.FlatIncrease:
                    damage += d.criticalDamageValue;
                    break;
                case CriticalDamageType.PercentageIncrease:
                    damage *= 1f + d.criticalDamageValue / 100f;
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
    #endregion
    
    public bool CanUse()
    {
        _lastFailureReasonMessage = "";
        
        if (runtimeSkill.Data == null)
        {
            _lastFailureReasonMessage = "Skill não configurada";
            return false;
        }
        if (IsOnCooldown)
        {
            _lastFailureReasonMessage = $"Em cooldown ({CooldownRemaining:F1}s restantes)";
            return false;
        }
        if (character == null)
        {
            _lastFailureReasonMessage = "Personagem não atribuído";
            return false;
        }
        var d = runtimeSkill.Data;
        if (d == null)
        {
            _lastFailureReasonMessage = "Skill não configurada";
            return false;
        }
        if (d.energyCost > 0 && (character.Energy == null || !character.Energy.HasEnoughEnergy(d.energyCost)))
        {
            _lastFailureReasonMessage = "Energia insuficiente";
            return false;
        }
        return true;
    }

    public string GetLastFailureReason()
    {
        return string.IsNullOrEmpty(_lastFailureReasonMessage) ? "Não pode usar agora" : _lastFailureReasonMessage;
    }
}
