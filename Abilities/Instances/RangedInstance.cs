using System.Collections;
using UnityEngine;

public class RangedInstance : AbilityInstanceBase
{
    RangedAbility rangedData;
    GameObject caster;
    float damage;
    float waitTime;

    public override void Initialize(Ability abilityData, AbilityContext context)
    {
        base.Initialize(abilityData, context);
        rangedData = abilityData as RangedAbility;
        caster = Context?.Caster;
        var casterChar = caster != null ? caster.GetComponent<Character>() : null;
        damage = casterChar != null ? abilityData.CalculateDamage(casterChar) : abilityData.baseDamage;
        waitTime = abilityData != null ? abilityData.castTime : 0f;

        if (waitTime <= 0f)
        {
            LaunchProjectile();
            Finish();
        }
        else
        {
            StartCoroutine(Run());
        }
    }

    IEnumerator Run()
    {
        float elapsed = 0f;
        while (elapsed < waitTime)
        {
            if (AbilityData != null && AbilityData.isCancelable && caster != null && AbilityData.IsCanceledByMovement(caster.transform, Context.CastStartPosition))
            {
                Cancel();
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        LaunchProjectile();
        Finish();
    }

    void LaunchProjectile()
    {
        if (rangedData == null || rangedData.projectilePrefab == null || caster == null) return;
        var spawnPos = caster.transform.position + caster.transform.forward * 1f;
        var projObj = GameObject.Instantiate(rangedData.projectilePrefab, spawnPos, Quaternion.identity);
        var proj = projObj.GetComponent<Projectile>();
        if (proj != null)
        {
            Vector3 aimPos = Context != null && Context.TargetPosition.HasValue ? Context.TargetPosition.Value : Vector3.zero;
            proj.Initialize(damage, caster, Context != null ? Context.Target : null, rangedData.speed, rangedData.maxRange, spawnPos, rangedData.targetLayers, rangedData.targetTeam);
            proj.ConfigureProjectile(rangedData.projectileType, aimPos);
        }
    }

    public override void Cancel()
    {
        base.Cancel();
    }
}
