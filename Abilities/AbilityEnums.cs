using UnityEngine;
using System.Collections.Generic;

namespace LegacyDuplicates
{
    [System.Obsolete("Duplicate enums and AbilityContext kept for reference. Use canonical versions under Assets/Scripts/Abilities/Data instead.")]
    public enum TargetingType
    {
        Targeted,
        Skillshot
    }

    [System.Obsolete]
    public enum CollisionType
    {
        FirstObject,
        AllObjects,
    }

    [System.Obsolete]
    public enum AbilityType
    {
        BasicAttack,
        Skill,
        Ultimate
    }

    [System.Obsolete]
    public enum DamageType
    {
        Percent,
        Flat,
        ForLifeSteal,
        ForMaxLife,
    }

    [System.Obsolete]
    public enum DamageNature
    {
        Physical,
        Magical,
        PhysicalTrue,
        MagicalTrue,
        Mixed
    }

    [System.Obsolete]
    public enum TeamFilter
    {
        All,
        Allies,
        Enemies
    }

    [System.Obsolete]
    public enum AreaShape
    {
        Sphere,
        Cone,
        Box
    }

    [System.Obsolete]
    public enum ProjectileType
    {
        Skillshot,
        TargetShot
    }

    [System.Obsolete]
    public class AbilityContext
    {
        public GameObject Caster { get; set; }
        public GameObject Target { get; set; }
        public Vector3 CastStartPosition { get; set; }
        public Vector3? TargetPosition { get; set; }
    }
}