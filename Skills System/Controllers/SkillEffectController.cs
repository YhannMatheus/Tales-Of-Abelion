using System.Collections.Generic;
using UnityEngine;

public class SkillEffectController
{
    private SkillData _data;
    private CharacterManager _caster;

    public SkillEffectController(SkillData data, CharacterManager caster)
    {
        _data = data;
        _caster = caster;
    }

    // Controlador de aplicação de efeitos.
    // Aplica efeitos filtrando por timing (OnCast, OnHit, OverTime, Passive, Aura)
    public void ApplySkillEffects(SkillContext context, EffectTiming when)
    {
        if (_data == null || _caster == null || _data.Effects == null || _data.Effects.Count == 0) return;

        void ApplyTo(CharacterManager target)
        {
            if (target == null) return;
            foreach (var effect in _data.Effects)
            {
                if (effect == null || effect.effectBehavior == null) continue;
                if (effect.effectTiming != when) continue;
                effect.effectBehavior.Initialize(effect, target, _caster, context.SkillLevel);
            }
        }

        switch (_data.targetingMode)
        {
            case TargetingMode.Self:
                ApplyTo(_caster);
                break;
            case TargetingMode.Area:
                if (_data.areaRadius <= 0f) break;
                var cols = Physics.OverlapSphere(context.TargetPosition, _data.areaRadius);
                foreach (var col in cols)
                {
                    if (col == null) continue;
                    if (col.TryGetComponent<CharacterManager>(out var cm))
                    {
                        if (!PassesTargetFilter(cm, _data.targetFilter)) continue;
                        ApplyTo(cm);
                    }
                }
                break;
            default:
                if (_data.areaRadius > 0f)
                {
                    var hits = Physics.OverlapSphere(context.TargetPosition, _data.areaRadius);
                    foreach (var c in hits)
                    {
                        if (c == null) continue;
                        if (c.TryGetComponent<CharacterManager>(out var cm2))
                        {
                            if (!PassesTargetFilter(cm2, _data.targetFilter)) continue;
                            ApplyTo(cm2);
                        }
                    }
                }
                else
                {
                    if (context.Target.GetComponent<CharacterManager>() != null && PassesTargetFilter(context.Target.GetComponent<CharacterManager>(), _data.targetFilter))
                        ApplyTo(context.Target.GetComponent<CharacterManager>());
                    else
                        ApplyTo(_caster);
                }
                break;
        }
    }

    // Executa efeitos passivos e auras (snapshot ao chamar)
    public void ExecutePassive()
    {
        if (_data == null || _data.Effects == null || _caster == null) return;

        foreach (var effectData in _data.Effects)
        {
            if (effectData == null || effectData.effectBehavior == null) continue;
            if (effectData.effectTiming == EffectTiming.Passive)
            {
                effectData.effectBehavior.Initialize(effectData, _caster, _caster, 1);
            }
        }
    }

    // Verifica se o alvo satisfaz o filtro de targeting configurado
    private bool PassesTargetFilter(CharacterManager target, TargetFilter filter)
    {
        if (target == null) return false;
        switch (filter)
        {
            case TargetFilter.All: return true;
            case TargetFilter.Self: return target == _caster;
            case TargetFilter.AllExceptSelf: return target != _caster;
            case TargetFilter.Allies: return IsAlly(target);
            case TargetFilter.Enemies: return !IsAlly(target) && target != _caster;
            default: return true;
        }
    }

    // Verifica se outro CharacterManager é aliado do caster
    private bool IsAlly(CharacterManager other)
    {
        if (other == null || _caster == null) return false;
        if (other == _caster) return true;
        if (_caster.characterType == other.characterType) return true;
        if ((_caster.characterType == CharacterType.Player && other.characterType == CharacterType.Ally) ||
            (_caster.characterType == CharacterType.Ally && other.characterType == CharacterType.Player)) return true;
        return false;
    }
}
