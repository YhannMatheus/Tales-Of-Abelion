using UnityEngine;

[CreateAssetMenu(fileName = "New Ranged Ability", menuName = "Abilities/Ranged Ability")]
public class RangedAbility : Ability
{
    public TargetingType targetingType;
    public CollisionType collisionType;
    public ProjectileType projectileType = ProjectileType.Skillshot;
    public float speed;
    public float maxRange;
    public float areaOfEffectRadius;
    public GameObject projectilePrefab;

    public override void Execute(AbilityContext context)
    {
        if (context.Caster == null || context.Target == null)
        {
            Debug.LogWarning("Caster or Target is null in AbilityContext.");
            return;
        }

        if (isCancelable && context.CastStartPosition != default && IsCanceledByMovement(context.Caster.transform, context.CastStartPosition))
        {
            Debug.Log("Ability cast canceled due to movement.");
            return;
        }

        var casterGO = context.Caster;
        var targetGO = context.Target;

        float damage = 0f;
        var casterChar = casterGO.GetComponent<Character>();
        if (casterChar != null)
            damage = CalculateDamage(casterChar);

        var ctx = new AbilityContext { Caster = casterGO, Target = targetGO, CastStartPosition = context.CastStartPosition };
        var instGO = new GameObject("RangedInstance");
        var inst = instGO.AddComponent<RangedInstance>();
        inst.Initialize(this, ctx);
    }


}