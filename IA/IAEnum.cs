using System;

[System.Serializable]
[Flags]
public enum IAState
{
    None = 0,
    Idle = 1 << 0,
    Patrol = 1 << 1,
    Chase = 1 << 2,
    Attack = 1 << 3,
    Flee = 1 << 4,
    Dead = 1 << 5
}

[System.Serializable]
public enum IATargetType
{
    Player,
    Ally,
    Enemy,
    Neutral
}

[System.Serializable]
public enum IAAlertLevel
{
    Low,
    Medium,
    High
}

[System.Serializable]
public enum IaType
{
    Enemy,
    Neutral,
    Ally,
    NPC
}

public enum StatesPriority
{
    low =0,
    medium=1,
    high= 2,
    Critical = 3
}