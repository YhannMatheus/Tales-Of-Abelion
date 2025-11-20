using System;
public enum SkillType
{
    Active,      // Skill ativa (precisa apertar botão)
    Passive,     // Skill passiva (sempre ativa)
    Toggle,      // Liga/desliga
    AutoCast     // Dispara automaticamente sob condições
}

// Como a skill é targetada
public enum TargetingMode
{
    Self,              // No próprio caster
    Enemy,             // Inimigo selecionado
    Ally,              // Aliado selecionado
    Position,          // Posição no mundo
    Direction,         // Direção (skillshot)
    Area,              // Área ao redor
    Auto               // Auto-target (mais próximo)
}

// Filtro de alvos
public enum TargetFilter
{
    Enemies,           // Apenas inimigos
    Allies,            // Apenas aliados
    Self,              // Apenas caster
    All,               // Todos (inimigos + aliados)
    AllExceptSelf      // Todos menos caster
}

// Tipo de dano (renomeado para evitar conflito com Ability System)
public enum DamageType
{
    None,              // Sem dano
    Physical,          // Dano físico puro
    Magical,           // Dano mágico puro
    Mixed,             // Físico + mágico
    True               // True damage (ignora defesa)
}

// Scaling de dano
public enum DamageScaling
{
    None,              // Sem scaling (dano fixo)
    Health,            // Escala com HP máximo
    MissingHealth,     // Escala com HP perdido
    TargetHealth,       // Escala com HP do alvo
    TargetMissingHealth  // Escala com HP perdido do alvo
}

// Scaling de cura
public enum HealScaling
{
    None,              // Sem scaling (cura fixa)
    FlatAmount,        // Valor fixo
    PercentMaxHP,      // % do HP máximo
    PercentMissingHP,  // % do HP perdido
    SpellPower         // Escala com poder mágico
}

// Comportamento de projétil
public enum ProjectileBehavior
{
    Linear,            // Reta
    Homing,            // Segue alvo
    Arcing,            // Arco (parábola)
    Boomerang,         // Volta ao caster
    Chaining,          // Salta entre alvos
}

// Tipo de operação de modificador
public enum ModifierOperation
{
    Add,               // Adiciona valor (100 + 50 = 150)
    Multiply,          // Multiplica (100 * 1.5 = 150)
    Override,         // Sobrescreve (100 -> 50)
}


// Categoria de skill (para UI/organização)
[Flags]
public enum SkillCategory
{
    None = 0,
    Attack = 1 << 0,       // 1
    Defense = 1 << 1,      // 2
    Mobility = 1 << 2,     // 4
    Utility = 1 << 3,      // 8
    Buff = 1 << 4,         // 16
    Debuff = 1 << 5,       // 32
    Healing = 1 << 6,      // 64
    Summon = 1 << 7,       // 128
    Ultimate = 1 << 8      // 256
}

public enum SkillRangeType
{
    Melee,              // Curto alcance
    Ranged,             // Longo alcance
    Global              // Alcance global
}

public enum CriticalDamageType
{
    None,               // Sem crítico
    FlatIncrease,      // Aumento fixo
    PercentageIncrease // Aumento percentual
}
public enum PlaybackMode
{
    FitToAttackSpeed,   // ajustar duração conforme velocidade de ataque
    FitToCastTime,      // ajustar duração conforme tempo de lançamento
    FixedDuration       // duração fixa (definida pelo usuário)
}
public enum EffectTiming
{
    OnCast,             // ao lançar a skill
    OnHit,              // ao atingir o alvo
    Passive,            // efeito passivo

}

public enum DamageOrigin
{
    None,
    Fire,
    Ice,
    Lightning,
    Earth,
    Wind,
    Water,
    Light,
    Dark,
}

[System.Serializable]
public enum EffectType
{
    Buff,
    Debuff,
    DamageOverTime,
    HealOverTime,
    Shield,
    Stun,
    Slow,
    Root,
    Silence,
    Knockback
}