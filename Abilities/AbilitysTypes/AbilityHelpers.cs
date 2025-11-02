using System.Collections.Generic;
using UnityEngine;

public static class AbilityHelpers
{
    public static List<Character> CollectCharactersInSphere(Vector3 center, float radius, LayerMask allowedLayers, TeamFilter teamFilter, GameObject caster)
    {
        List<Character> result = new List<Character>();
        Collider[] hits = Physics.OverlapSphere(center, Mathf.Max(0.01f, radius));
        var casterChar = caster != null ? caster.GetComponentInParent<Character>() : null;
        foreach (var c in hits)
        {
            if (c == null) continue;
            if (((1 << c.gameObject.layer) & allowedLayers.value) == 0) continue;
            var ch = c.GetComponentInParent<Character>();
            if (ch == null) continue;
            if (teamFilter == TeamFilter.Enemies && ch.characterType == casterChar?.characterType) continue;
            if (teamFilter == TeamFilter.Allies && ch.characterType != casterChar?.characterType) continue;
            if (!result.Contains(ch)) result.Add(ch);
        }
        return result;
    }

    public static List<GameObject> ApplyAreaDamageAndBuffs(Vector3 center, float radius, LayerMask allowedLayers, TeamFilter teamFilter, GameObject caster, float damage, bool damageIsMagical, BuffData buffToApply = null, bool applyBuffOnEnter = false)
    {
        var affectedGOs = new List<GameObject>();
        var chars = CollectCharactersInSphere(center, radius, allowedLayers, teamFilter, caster);
        foreach (var ch in chars)
        {
            if (buffToApply != null && applyBuffOnEnter)
            {
                buffToApply.ApplyTo(ch);
                if (!affectedGOs.Contains(ch.gameObject)) affectedGOs.Add(ch.gameObject);
            }

            if (damage != 0f)
            {
                ch.TakeDamage(damage, damageIsMagical);
                if (!affectedGOs.Contains(ch.gameObject)) affectedGOs.Add(ch.gameObject);
            }
        }

        return affectedGOs;
    }
}
