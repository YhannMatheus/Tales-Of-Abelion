using UnityEngine;

[CreateAssetMenu(fileName = "New Area Ability", menuName = "Abilities/AreaAbility")]
public class AreaAbility : Ability
{
    [Header("Area Ability Settings")]
    public float areaRadius;
    public float durationInSeconds;
    public bool applyBuffOnEnter = true;
    public GameObject instancePrefab;
    public BuffData buffToApply;
    public bool removeBuffOnExit = false;
    public float tickInterval = 1f;
    public float tickDamage = 0f;
    public bool tickIsMagical = false;

    public override void Execute(AbilityContext context)
    {
        Vector3 spawnPos = Vector3.zero;

        if (context != null && context.CastStartPosition != default)
        {
            spawnPos = context.CastStartPosition;
        }
        else if (context != null && context.Caster != null)
        {
            spawnPos = context.Caster.transform.position;
        }

        var ctx = new AbilityContext { Caster = context?.Caster, Target = context?.Target, CastStartPosition = spawnPos };
        var instGO = new GameObject("AreaInstance");
        var inst = instGO.AddComponent<AreaInstance>();
        inst.Initialize(this, ctx);
    }
}