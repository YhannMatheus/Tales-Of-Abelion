using UnityEngine;

// Efeito de dano direto
[CreateAssetMenu(fileName = "New Damage Effect", menuName = "Skills/Effects/Damage Effect")]
public class DamageEffect : SkillEffect
{
    [Header("Configuração de Dano")]
    [Tooltip("Dano base")]
    public float baseDamage = 50f;
    
    [Tooltip("Tipo de dano")]
    public SkillDamageType damageType = SkillDamageType.Physical;
    
    [Tooltip("Como o dano escala com stats")]
    public DamageScaling scaling = DamageScaling.Physical;
    
    [Header("Scaling Ratios")]
    [Tooltip("Multiplicador de Physical Damage do caster")]
    [Range(0f, 5f)]
    public float physicalRatio = 1f;
    
    [Tooltip("Multiplicador de Magical Damage do caster")]
    [Range(0f, 5f)]
    public float magicalRatio = 0f;
    
    [Tooltip("Multiplicador de HP máximo do caster")]
    [Range(0f, 1f)]
    public float healthRatio = 0f;
    
    [Header("Opções Avançadas")]
    [Tooltip("Ignora defesa do alvo")]
    public bool ignoresArmor = false;
    
    [Tooltip("Pode dar critical hit")]
    public bool canCrit = true;
    
    [Tooltip("Multiplicador de crítico (se canCrit = true)")]
    public float critMultiplier = 2f;

    public override SkillEffectResult Execute(SkillContext context)
    {
        var result = new SkillEffectResult();

        if (!CanExecute(context))
        {
            result.Success = false;
            result.ErrorMessage = "Validação falhou";
            return result;
        }

        if (context.Target == null)
        {
            LogWarning("Alvo é null, efeito não pode ser aplicado");
            result.Success = false;
            result.ErrorMessage = "Sem alvo";
            return result;
        }

        // Calcula dano final
        float finalDamage = CalculateDamage(context);

        // Aplica dano no alvo
        context.Target.TakeDamage(finalDamage);

        // Preenche resultado
        result.Success = true;
        result.DamageDealt = finalDamage;
        result.AffectedTargets = new Character[] { context.Target };

        Log($"Causou {finalDamage:F1} de dano em {context.Target.Data.characterName}");

        return result;
    }

    // Calcula dano baseado em scaling
    float CalculateDamage(SkillContext context)
    {
        float damage = baseDamage;
        var casterData = context.Caster.Data;

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

        // TODO: Aplicar critical hit (quando sistema de stats tiver crit chance)
        // if (canCrit && Random.value < casterData.critChance)
        // {
        //     damage *= critMultiplier;
        // }

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
