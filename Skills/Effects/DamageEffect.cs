using UnityEngine;

// Efeito de dano direto
// TODOS OS VALORES são configurados no SkillData, não aqui!
[CreateAssetMenu(fileName = "New Damage Effect", menuName = "Skills/Effects/Damage Effect")]
public class DamageEffect : SkillEffect
{
    [Header("Info")]
    [Tooltip("Este efeito aplica dano. Configure valores no SkillData:")]
    [TextArea(3, 5)]
    public string info = "Configure no SkillData:\n- baseDamage\n- damageType\n- damageScaling\n- physicalDamageRatio\n- magicalDamageRatio\n- canCrit, critMultiplier";

    public override SkillEffectResult Execute(SkillContext context)
    {
        var result = new SkillEffectResult();

        if (!CanExecute(context))
        {
            result.Success = false;
            result.ErrorMessage = "Validação falhou";
            return result;
        }

        if (context.SkillData == null)
        {
            LogError("SkillData não encontrado no contexto!");
            result.Success = false;
            result.ErrorMessage = "SkillData é null";
            return result;
        }

        if (context.Target == null)
        {
            LogWarning("Alvo é null, efeito não pode ser aplicado");
            result.Success = false;
            result.ErrorMessage = "Sem alvo";
            return result;
        }

        // Calcula dano final usando valores do SkillData
        float finalDamage = CalculateDamage(context);

        // Aplica dano no alvo
        context.Target.TakeDamage(finalDamage);

        // Preenche resultado
        result.Success = true;
        result.DamageDealt = finalDamage;
        result.AffectedTargets = new CharacterManager[] { context.Target };

        Log($"Causou {finalDamage:F1} de dano em {context.Target.Data.characterName}");

        return result;
    }

    // Calcula dano baseado em valores do SkillData
    float CalculateDamage(SkillContext context)
    {
        SkillData skillData = context.SkillData;
        var casterData = context.Caster.Data;

        // Lê valores DO SKILLDATA
        float damage = skillData.baseDamage;
        DamageScaling scaling = skillData.damageScaling;
        float physicalRatio = skillData.physicalDamageRatio;
        float magicalRatio = skillData.magicalDamageRatio;
        float healthRatio = skillData.healthRatio;

        // Aplica scaling
        switch (scaling)
        {
            case DamageScaling.Physical:
                damage += casterData.TotalPhysicalDamage * physicalRatio;
                break;

            case DamageScaling.Magical:
                damage += casterData.TotalMagicalDamage * magicalRatio;
                break;

            case DamageScaling.Both:
                damage += casterData.TotalPhysicalDamage * physicalRatio;
                damage += casterData.TotalMagicalDamage * magicalRatio;
                break;

            case DamageScaling.Health:
                damage += casterData.maxHealth * healthRatio;
                break;

            case DamageScaling.MissingHealth:
                float missingHP = casterData.maxHealth - casterData.currentHealth;
                damage += missingHP * healthRatio;
                break;

            case DamageScaling.TargetHealth:
                if (context.Target != null)
                {
                    damage += context.Target.Data.maxHealth * healthRatio;
                }
                break;
        }

        // Aplica modificadores de nível da skill
        if (context.SkillLevel > 1)
        {
            float levelBonus = (context.SkillLevel - 1) * 0.15f; // +15% por nível
            damage *= (1f + levelBonus);
        }

        // Aplica critical hit se configurado
        if (skillData.canCrit && Random.value < casterData.TotalCriticalChance)
        {
            damage *= skillData.critMultiplier;
            Log("CRITICAL HIT!");
        }

        return damage;
    }

    public override bool CanExecute(SkillContext context)
    {
        if (!base.CanExecute(context))
            return false;

        if (context.Target == null)
        {
            LogWarning("DamageEffect precisa de um alvo!");
            return false;
        }

        return true;
    }
}
