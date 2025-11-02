using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Ability", menuName = "Abilities/Melee Ability")]
public class MeleeAbility : Ability
{
	[Header("Melee Settings")]
	public float areaRadius = 1f;
	public float forwardOffset = 1f;

	public AreaShape areaShape = AreaShape.Sphere;
	[Tooltip("Cone half-angle in degrees (used when AreaShape == Cone)")]
	public float coneAngleDegrees = 60f;
	[Tooltip("Box size (used when AreaShape == Box)")]
	public Vector3 boxSize = new Vector3(1f, 1f, 1f);

	public override void Execute(AbilityContext context)
	{
		if (context == null || context.Caster == null)
		{
			Debug.LogWarning("MeleeAbility: context ou caster nulo.");
			return;
		}

		var casterGO = context.Caster;
		var casterChar = casterGO.GetComponent<Character>();

		float damage = 0f;
		if (casterChar != null)
			damage = CalculateDamage(casterChar);

		Vector3 castStartPos = context.CastStartPosition != default ? context.CastStartPosition : casterGO.transform.position;

		var ctx = new AbilityContext { Caster = casterGO, Target = context.Target, CastStartPosition = castStartPos };

		var instGO = new GameObject("MeleeInstance");
		var inst = instGO.AddComponent<MeleeInstance>();
		inst.Initialize(this, ctx);
	}

	public void ApplyAreaDamage(GameObject casterGO, float damage)
	{
		Vector3 center = casterGO.transform.position + casterGO.transform.forward * forwardOffset;
		bool isMagical = damageNature == DamageNature.Magical || damageNature == DamageNature.MagicalTrue;
		AbilityHelpers.ApplyAreaDamageAndBuffs(center, areaRadius, targetLayers, targetTeam, casterGO, damage, isMagical, null, false);
	}


}