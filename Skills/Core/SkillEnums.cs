using System;
using UnityEngine;

// Tipo da skill
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
public enum SkillDamageType
{
    Physical,          // Dano físico puro
    Magical,           // Dano mágico puro
    Mixed,             // Físico + mágico
    True               // True damage (ignora defesa)
}

// Scaling de dano
public enum DamageScaling
{
    None,              // Sem scaling (dano fixo)
    Physical,          // Escala com Physical Damage
    Magical,           // Escala com Magical Damage
    Both,              // Escala com ambos
    Health,            // Escala com HP máximo
    MissingHealth,     // Escala com HP perdido
    TargetHealth       // Escala com HP do alvo
}

// Scaling de cura
public enum HealScaling
{
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
    Chaining           // Salta entre alvos
}

// Formato de área (renomeado para evitar conflito)
public enum SkillAreaShape
{
    Circle,            // Círculo
    Cone,              // Cone
    Box,               // Retângulo
    Line,              // Linha reta
    Ring               // Anel (círculo com buraco)
}

// Comportamento de summon
public enum SummonBehavior
{
    Follow,            // Segue caster
    Stay,              // Fica parado
    AttackNearest,     // Ataca mais próximo
    Patrol,            // Patrulha área
    Defend             // Defende caster
}

// Tipo de operação de modificador
public enum ModifierOperation
{
    Add,               // Adiciona valor (100 + 50 = 150)
    Multiply,          // Multiplica (100 * 1.5 = 150)
    Override           // Sobrescreve (100 -> 50)
}

// Stat que pode ser modificada
public enum SkillStatType
{
    Damage,            // Dano base
    Cooldown,          // Tempo de cooldown
    Range,             // Alcance
    Cost,              // Custo de energia
    CastTime,          // Tempo de cast
    Radius,            // Raio de área
    Duration,          // Duração de efeito
    Charges            // Número de charges
}

// Estado de skill
public enum SkillState
{
    Ready,             // Pronta para usar
    Casting,           // Sendo conjurada
    Cooldown,          // Em cooldown
    Locked,            // Bloqueada (não desbloqueada)
    Disabled           // Desabilitada temporariamente
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
