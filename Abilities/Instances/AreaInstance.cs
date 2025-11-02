using System.Collections;
using UnityEngine;

public class AreaInstance : AbilityInstanceBase
{
    AreaAbility areaData;
    GameObject caster;
    float waitTime;

    public override void Initialize(Ability abilityData, AbilityContext context)
    {
        base.Initialize(abilityData, context);
        areaData = abilityData as AreaAbility;
        caster = Context?.Caster;
        waitTime = abilityData != null ? abilityData.castTime : 0f;

        if (waitTime <= 0f)
        {
            SpawnArea();
            Finish();
        }
        else
        {
            StartCoroutine(Run());
        }
    }

    System.Collections.IEnumerator Run()
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
        SpawnArea();
        Finish();
    }

    void SpawnArea()
    {
        if (areaData == null || areaData.instancePrefab == null) return;

        Vector3 spawnPos = Context != null && Context.CastStartPosition != default ? Context.CastStartPosition : caster != null ? caster.transform.position : Vector3.zero;
        var obj = GameObject.Instantiate(areaData.instancePrefab, spawnPos, Quaternion.identity);
        var area = obj.GetComponent<AreaEffect>();
        if (area != null)
        {
            float radiusToUse = areaData.areaRadius > 0f ? areaData.areaRadius : 1f;
            float lifetimeToUse = areaData.durationInSeconds > 0f ? areaData.durationInSeconds : 0f;
                area.Initialize(radiusToUse, lifetimeToUse, areaData.buffToApply, areaData.applyBuffOnEnter, areaData.removeBuffOnExit, areaData.tickInterval, areaData.tickDamage, areaData.tickIsMagical, areaData.targetLayers, areaData.targetTeam);
        }
    }

    public override void Cancel()
    {
        base.Cancel();
    }
}
