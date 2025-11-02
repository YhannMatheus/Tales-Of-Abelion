using UnityEngine;
using System.Collections.Generic;

public enum TargetingType
{
    Targeted,
    Skillshot
}

public enum CollisionType
{
    FirstObject,
    AllObjects,
}

public enum AbilityType
{
    BasicAttack,
    Skill,
    Ultimate
}

public enum DamageType
{
    Percent,
    Flat,
    ForLifeSteal,
    ForMaxLife,
}

public enum DamageNature
{
    Physical,
    Magical,
    PhysicalTrue,
    MagicalTrue,
    Mixed
}

public enum TeamFilter
{
    All,
    Allies,
    Enemies
}

public enum AreaShape
{
    Sphere,
    Cone,
    Box
}

public enum ProjectileType
{
    Skillshot,
    TargetShot
}

public class AbilityContext
{
    public GameObject Caster { get; set; }
    public GameObject Target { get; set; }
    public Vector3 CastStartPosition { get; set; }
    public Vector3? TargetPosition { get; set; }
}
