using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaEffect : MonoBehaviour
{
    float radius;
    float lifetime;
    BuffData buff;
    bool applyOnEnter;
    bool removeOnExit;
    float tickInterval;
    float tickDamage;
    bool tickIsMagical;

    List<GameObject> affected = new List<GameObject>();

    LayerMask allowedLayers = ~0;
    TeamFilter teamFilter = TeamFilter.Enemies;

    public void Initialize(float radius, float lifetime, BuffData buffToApply, bool applyOnEnter, bool removeOnExit, float tickInterval, float tickDamage, bool tickIsMagical, LayerMask allowedLayers, TeamFilter teamFilter)
    {
        this.radius = Mathf.Max(0.01f, radius);
        this.lifetime = lifetime;
        this.buff = buffToApply;
        this.applyOnEnter = applyOnEnter;
        this.removeOnExit = removeOnExit;
        this.tickInterval = Mathf.Max(0.01f, tickInterval);
        this.tickDamage = tickDamage;
        this.tickIsMagical = tickIsMagical;
        this.allowedLayers = allowedLayers;
        this.teamFilter = teamFilter;

        if (applyOnEnter && buff != null)
        {
            ApplyBuffToAll();
        }

        if (tickDamage != 0f)
            StartCoroutine(TickDamage());

        if (lifetime > 0f)
            StartCoroutine(AutoReleaseAfter(lifetime));
    }

    void ApplyBuffToAll()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var c in hits)
        {
            if (((1 << c.gameObject.layer) & allowedLayers.value) == 0) continue;
            var ch = c.GetComponentInParent<Character>();
            if (ch == null) continue;
            var caster = GetComponentInParent<Character>();
            if (teamFilter == TeamFilter.Enemies && ch.characterType == caster?.characterType) continue;
            if (teamFilter == TeamFilter.Allies && ch.characterType != caster?.characterType) continue;
            buff.ApplyTo(ch);
            if (!affected.Contains(ch.gameObject)) affected.Add(ch.gameObject);
        }
    }

    IEnumerator TickDamage()
    {
        while (true)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (var c in hits)
            {
                if (((1 << c.gameObject.layer) & allowedLayers.value) == 0) continue;
                var ch = c.GetComponentInParent<Character>();
                if (ch == null) continue;
                var caster = GetComponentInParent<Character>();
                if (teamFilter == TeamFilter.Enemies && ch.characterType == caster?.characterType) continue;
                if (teamFilter == TeamFilter.Allies && ch.characterType != caster?.characterType) continue;
                ch.TakeDamage(tickDamage, tickIsMagical);
            }
            yield return new WaitForSeconds(tickInterval);
        }
    }

    private void OnDestroy()
    {
        if (removeOnExit && buff != null)
        {
            foreach (var go in affected)
            {
                if (go == null) continue;
                var mgr = go.GetComponent<BuffManager>();
                if (mgr != null)
                {
                    mgr.RemoveBuffByName(buff.buffName);
                }
            }
        }
    }

    IEnumerator AutoReleaseAfter(float time)
    {
        yield return new WaitForSeconds(time);
        if (buff != null && removeOnExit)
        {
            foreach (var go in affected)
            {
                if (go == null) continue;
                var mgr = go.GetComponent<BuffManager>();
                if (mgr != null)
                {
                    mgr.RemoveBuffByName(buff.buffName);
                }
            }
        }

        AbilityPool.Release(gameObject, null);
    }
}
