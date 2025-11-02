using UnityEngine;

public abstract class Ability : ScriptableObject
{
    [Header("Informações De UI")]
    public string abilityName;

    [TextArea(5, 10)]
    public string abilityDescription;
    public Sprite abilityIcon;
    public AbilityType abilityType;

    [Header("Informações De Habilidade")]
    public float baseDamage;
    public float physicalMultiplier;
    public float magicalMultiplier;
    public DamageType damageType;
    public DamageNature damageNature;
    public float range;
    public float energyCost;

    [Header("Informações relacionadas ao tempo")]
    public float castTime;
    public float cooldownTime;

    [Header("Dados de instância")]
    public Transform instanceAnchorTransform;
    public AudioClip abilitySound;
    public ParticleSystem abilityVFX;
    public int animationIndex;

    [Header("Alvo e filtragem")]
    public LayerMask targetLayers = ~0;
    public TeamFilter targetTeam = TeamFilter.Enemies;

    [Header("Variaveis de controle")]
    public bool isCancelable;
    public bool isCastable;


    public virtual void Execute(AbilityContext context) {}
    public bool IsCanceledByMovement(Transform casterTransform, Vector3 castStartPosition, float moveCancelThreshold = 0.1f)
    {
        // Só aplica a lógica de cancelamento por movimento se a flag de controle estiver ativada
        if (!isCancelable) return false;
        if (casterTransform == null) return false;
        float sqrThreshold = moveCancelThreshold * moveCancelThreshold;
        return (casterTransform.position - castStartPosition).sqrMagnitude > sqrThreshold;
    }
    
    public float CalculateDamage(Character caster)
    {
        float damage = baseDamage;
        if(caster.Data != null)
        {
            switch (damageNature)
            {
                case DamageNature.Physical:
                    damage += (caster.Data.TotalPhysicalDamage * physicalMultiplier);
                    break;
                case DamageNature.Magical:
                    damage += (caster.Data.TotalMagicalDamage * magicalMultiplier);
                    break;
                case DamageNature.PhysicalTrue:
                    damage += caster.Data.TotalPhysicalDamage;
                    break;
                case DamageNature.MagicalTrue:
                    damage += caster.Data.TotalMagicalDamage;
                    break;
                case DamageNature.Mixed:
                    damage += (caster.Data.TotalPhysicalDamage * physicalMultiplier) + (caster.Data.TotalMagicalDamage * magicalMultiplier);
                    break;
            }
        }
        return damage;  
    }

    public float CalculateDamage(CharacterData casterData)
    {
        float damage = baseDamage;
        if(casterData != null)
        {
            switch (damageNature)
            {
                case DamageNature.Physical:
                    damage += (casterData.TotalPhysicalDamage * physicalMultiplier);
                    break;
                case DamageNature.Magical:
                    damage += (casterData.TotalMagicalDamage * magicalMultiplier);
                    break;
                case DamageNature.PhysicalTrue:
                    damage += casterData.TotalPhysicalDamage;
                    break;
                case DamageNature.MagicalTrue:
                    damage += casterData.TotalMagicalDamage;
                    break;
                case DamageNature.Mixed:
                    damage += (casterData.TotalPhysicalDamage * physicalMultiplier) + (casterData.TotalMagicalDamage * magicalMultiplier);
                    break;
            }
        }
        return damage;  
    }
}