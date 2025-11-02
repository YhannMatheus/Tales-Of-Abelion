using System.Collections;
using UnityEngine;

public class MeleeInstance : AbilityInstanceBase
{
    MeleeAbility meleeData;
    GameObject caster;
    float damage;
    float waitTime;
    float radius;
    float offset;
    Coroutine running;

    public override void Initialize(Ability abilityData, AbilityContext context)
    {
        base.Initialize(abilityData, context);
        meleeData = abilityData as MeleeAbility;
        caster = Context?.Caster;
        var casterChar = caster != null ? caster.GetComponent<Character>() : null;
        damage = casterChar != null ? abilityData.CalculateDamage(casterChar) : abilityData.baseDamage;
        waitTime = abilityData != null ? abilityData.castTime : 0f;
        radius = meleeData != null ? meleeData.areaRadius : 1f;
        offset = meleeData != null ? meleeData.forwardOffset : 1f;
        running = StartCoroutine(Run());
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
        Finish();
    }

    public override void Finish()
    {
        if (caster == null) { base.Finish(); return; }

        var center = caster.transform.position + caster.transform.forward * offset;
        Collider[] candidates = Physics.OverlapSphere(center, radius);
        var casterChar = caster.GetComponent<Character>();
        bool isMagic = AbilityData.damageNature == DamageNature.Magical || AbilityData.damageNature == DamageNature.MagicalTrue;

        LayerMask mask = AbilityData != null ? AbilityData.targetLayers : ~0;
        TeamFilter teamFilter = AbilityData != null ? AbilityData.targetTeam : TeamFilter.Enemies;
        AreaShape shape = meleeData != null ? meleeData.areaShape : AreaShape.Sphere;
        float coneHalf = meleeData != null ? meleeData.coneAngleDegrees * 0.5f : 30f;
        Vector3 boxHalf = meleeData != null ? meleeData.boxSize * 0.5f : Vector3.one * 0.5f;

        foreach (var col in candidates)
        {
            if (col == null) continue;
            if (((1 << col.gameObject.layer) & mask.value) == 0) continue;

            var other = col.GetComponentInParent<Character>();
            if (other == null) continue;
            if (casterChar != null && other == casterChar) continue;

            // team filtering
            if (teamFilter == TeamFilter.Enemies && other.characterType == casterChar?.characterType) continue;
            if (teamFilter == TeamFilter.Allies && other.characterType != casterChar?.characterType) continue;

            // shape test
            Vector3 dir = (other.transform.position - center);
            bool inside = false;
            switch (shape)
            {
                case AreaShape.Sphere:
                    inside = dir.sqrMagnitude <= radius * radius;
                    break;
                case AreaShape.Cone:
                    float angle = Vector3.Angle(caster.transform.forward, dir);
                    inside = dir.sqrMagnitude <= radius * radius && angle <= coneHalf;
                    break;
                case AreaShape.Box:
                    var local = Quaternion.Inverse(caster.transform.rotation) * (other.transform.position - (center));
                    inside = Mathf.Abs(local.x) <= boxHalf.x && Mathf.Abs(local.y) <= boxHalf.y && Mathf.Abs(local.z) <= boxHalf.z;
                    break;
            }

            if (!inside) continue;

            other.TakeDamage(damage, isMagic);
        }
        base.Finish();
        base.Finish();
    }

    public override void Cancel()
    {
        base.Cancel();
    }
}
