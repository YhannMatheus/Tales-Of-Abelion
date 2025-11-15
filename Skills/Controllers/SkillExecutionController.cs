using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillExecutionController
{
    // ----- Config / References -----
    protected SkillData data = null;
    protected CharacterManager character;
    protected Transform castPoint;

    // ----- Runtime -----
    protected SkillAnimationController animationController;
    private float _cooldownTimer = 0f;

    // ----- Properties -----
    public bool IsOnCooldown => _cooldownTimer > 0f;
    public float CooldownRemaining => Mathf.Max(0f, _cooldownTimer);
    public float CooldownPercent => data != null && data.cooldownTime > 0f ? (Mathf.Clamp01(_cooldownTimer / data.cooldownTime)) : 0f;

    // ----- Events (manager integration) -----
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

    // Quando o executor determina que um projétil deve ser criado, ele dispara este evento.
    // O SkillManager (ou outro owner) deve escutar e criar o GameObject/projétil.
    public event Action<ProjectileSpawnRequest> OnRequestSpawnProjectile;

    // ----- Constructor -----
    public SkillExecutionController(SkillData skillData, CharacterManager characterManager, Transform castTransform)
    {
        data = skillData;
        character = characterManager;
        castPoint = castTransform;
    }

    public void SwitchSkill(SkillData newSkillData)
    {
        data = newSkillData;
    }

    // ----- Execution entry -----
    public void Executate(SkillContext context)
    {
        if (data == null) return;

        switch (data.skillType)
        {
            case SkillType.Active:
                ExecuteActive(context);
                break;
            case SkillType.Passive:
                ExecutePassive();
                break;
            case SkillType.Toggle:
                ExecuteToggle(context);
                break;
            case SkillType.AutoCast:
                ExecuteAutoCast(context);
                break;
            default:
                ExecuteActive(context);
                break;
        }
    }

    // ----- TryExecute (entrada que inicia a execução) -----
    public bool TryExecute(SkillContext context)
    {
        if (!CanUse()) return false;

        if (data.energyCost > 0)
        {
            if (!character.TrySpendEnergy(data.energyCost)) return false;
        }

        // Prepara InstanceContext com valores finais que serão aplicados pela instância/projétil
        var instanceCtx = new InstanceContext
        {
            Skill = data,
            SkillLevel = context.SkillLevel,
            Caster = character,
            TargetCharacter = context.TargetCharacter,
            TargetGameObject = context.TargetGameObject,
            TargetPosition = context.TargetPosition,
            DamageType = data.damageType,
            totalDamageAmount = CalculateDamage(context),
            totalHealAmount = context.TargetCharacter != null ? CalculateHeal(context, context.TargetCharacter) : 0f
        };

        // Se existe prefab de skill e velocidade de projétil configurada, solicita spawn ao manager.
        if (data.skillPrefab != null && data.projectileSpeed > 0f)
        {
            var req = new ProjectileSpawnRequest
            {
                Context = instanceCtx,
                Prefab = data.skillPrefab,
                Position = castPoint != null ? castPoint.position : (context.TargetPosition != Vector3.zero ? context.TargetPosition : character.transform.position),
                Rotation = castPoint != null ? castPoint.rotation : Quaternion.identity,
                Direction = (instanceCtx.TargetPosition != Vector3.zero) ? (instanceCtx.TargetPosition - (castPoint != null ? castPoint.position : character.transform.position)).normalized : (castPoint != null ? castPoint.forward : character.transform.forward),
                HomingTarget = instanceCtx.TargetCharacter,
                Speed = data.projectileSpeed,
                Lifetime = data.projectileLifetime,
                Behavior = data.projectileBehavior
            };

            if (OnRequestSpawnProjectile != null)
            {
                OnRequestSpawnProjectile.Invoke(req);
            }
            else
            {
                // fallback de compatibilidade: spawn local se ninguém estiver escutando
                SpawnProjectile(instanceCtx, context);
            }
        }
        else
        {
            // Fallback: aplica efeitos imediatamente (ex.: cura, melee instant)
            ExecuteActive(context);
        }

        // Inicia animação associada (se houver)
        StartAnimation(context, context.OnComplete);
        StartCooldown();
        return true;
    }

    // ----- Cooldown -----
    public void StartCooldown()
    {
        _cooldownTimer = data != null ? data.cooldownTime : 0f;
    }

    public void TickCooldown(float deltaTime)
    {
        if (_cooldownTimer <= 0f) return;
        _cooldownTimer = Mathf.Max(0f, _cooldownTimer - deltaTime);
    }

    // ----- Execution variants -----
    public void ExecuteActive(SkillContext context)
    {
        if (data == null) return;

        var targets = new List<CharacterManager>();

        if (data.skillCategory.HasFlag(SkillCategory.Healing))
        {
            if (data.targetingMode == TargetingMode.Self)
            {
                targets.Add(character);
            }
            else if (context.TargetCharacter != null)
            {
                targets.Add(context.TargetCharacter);
            }
        }

        if (targets.Count == 0) return;

        foreach (var t in targets)
        {
            if (t == null) continue;
            float healAmount = CalculateHeal(context, t);
            int healInt = Mathf.RoundToInt(Mathf.Max(0f, healAmount));
            t.Heal(healInt);
        }
    }

    public void ExecutePassive()
    {
        var ctx = new SkillContext(skill: data, caster: character, targetCharacter: character, targetPos: character.transform.position, skillLevel: 1);
        foreach(var effect in data.effects)
        {
            effect.effect.ApplyEffect(ctx);
        }
    }

    public void ExecuteToggle(SkillContext context)
    {
        ExecuteActive(context);
    }

    public void ExecuteAutoCast(SkillContext context)
    {
        if (CanUse()) TryExecute(context);
    }

    // ----- Animation integration -----
    public void StartAnimation(SkillContext context, Action onComplete = null, float explicitAttackSpeed = -1f)
    {
        if (data == null || character == null) return;

        Animator animator = character.GetComponent<Animator>();
        if (animator == null || data.animation == null)
        {
            onComplete?.Invoke();
            return;
        }

        float playbackSpeed = CalculatePlaybackSpeed(data, character, explicitAttackSpeed);

        animationController = new SkillAnimationController();
        animationController.Play(animator, data, playbackSpeed, onComplete);
    }

    public void TickAnimation(float deltaTime)
    {
        if (animationController == null) return;
        animationController.Tick(deltaTime);
        if (!animationController.IsPlaying) animationController = null;
    }

    public void StopAnimation()
    {
        if (animationController == null) return;
        animationController.Stop();
        animationController = null;
    }

    // ----- Projectile spawn -----
    private void SpawnProjectile(InstanceContext instanceCtx, SkillContext context)
    {
        if (data.skillPrefab == null || castPoint == null) return;

        Vector3 spawnPos = castPoint.position;
        Quaternion spawnRot = castPoint.rotation;

        GameObject go = GameObject.Instantiate(data.skillPrefab, spawnPos, spawnRot);

        Projectile proj = go.GetComponent<Projectile>();
        if (proj == null) proj = go.AddComponent<Projectile>();

        Vector3 dir = (instanceCtx.TargetPosition != Vector3.zero) ?
            (instanceCtx.TargetPosition - spawnPos).normalized : castPoint.forward;

        CharacterManager homing = instanceCtx.TargetCharacter;

        proj.Initialize(instanceCtx, data.projectileSpeed, data.projectileLifetime, data.projectileBehavior, dir, homing);
    }

    // ----- Calculations / Helpers -----
    public float CalculatePlaybackSpeed(SkillData data, CharacterManager character, float explicitAttackSpeed = -1f)
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

    public float CalculateHeal(SkillContext context, CharacterManager target)
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
                if (target != null) heal += target.Data.TotalMaxHealth * data.healModifierValue;
                break;
            case HealScaling.PercentMissingHP:
                if (target != null)
                {
                    float missing = Mathf.Max(0f, target.Data.TotalMaxHealth - target.Data.currentHealth);
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

    public float CalculateDamage(SkillContext context)
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
                damage += character.Data.TotalMaxHealth * data.characterLifePercentInfluence;
                break;
            case DamageScaling.MissingHealth:
                float missingHealth = Mathf.Max(0f, character.Data.TotalMaxHealth - character.Data.currentHealth);
                damage += missingHealth * data.characterLifePercentInfluence;
                break;
            case DamageScaling.TargetHealth:
                if (context.TargetCharacter != null) damage += context.TargetCharacter.Data.TotalMaxHealth * data.characterLifePercentInfluence;
                break;
            case DamageScaling.TargetMissingHealth:
                if (context.TargetCharacter != null)
                {
                    float targetMissing = Mathf.Max(0f, context.TargetCharacter.Data.TotalMaxHealth - context.TargetCharacter.Data.currentHealth);
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
                    damage *= (1f + data.criticalDamageValue / 100f);
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
        if (data == null) return false;
        if (IsOnCooldown) return false;
        if (character == null) return false;
        if (data.energyCost > 0 && (character.Energy == null || !character.Energy.HasEnoughEnergy(data.energyCost))) return false;
        return true;
    }
}
