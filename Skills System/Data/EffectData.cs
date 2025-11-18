using UnityEngine;

[CreateAssetMenu(fileName = "New Effect Data", menuName = "Skills/Effect Data")]
public class EffectData : ScriptableObject
{
    [Header("Effect Info")]
    public string effectID;                                           // identificador único do efeito
    public string effectName;                                         // nome do efeito
    [TextArea(5,10)] public string description;                       // descrição do efeito
    public EffectType effectType;                                     // tipo do efeito (Buff, Debuff, DoT, etc)
    public EffectBase effectBehavior;                                 // comportamento do efeito
    public ModifierVar targetModifier;                                // modificador alvo do efeito
    public EffectTiming effectTiming = EffectTiming.OnHit;            // momento em que o efeito é aplicado
    public DamageOrigin damageOrigin = DamageOrigin.None;             // origem/elemento do efeito
    [Header("Aura")]
    public CharacterType[] auraTargetTypes;                           // tipos de character que a aura afetará

    [Header("Effect Parameters")]
    [Header("Duration")]
    public float inDuration;                                          // duração do efeito até a aplicação completa - se 0 é instantâneo
    public float atDuration;                                          // duração em que o efeito está ativo - se 0 é permanente
    public float ofDuration;                                          // duração em segundos para a retirada do efeito - se 0 é instantâneo

    [Header("Stacking")]
    public bool isStackable;                                          // indica se o efeito pode ser empilhado
    public int maxStacks;                                             // número máximo de pilhas (se empilhável)
    public float stackValueMultiplier;                                // multiplicador por stack do modificador

    [Header("Modifiers")]
    public ModifierVar modifierType;                                  // tipo de modificador (Buff/Debuff)
    public ModifierType modifierCalculation;                          // como o modificador é calculado
    public float modifierValue;                                       // valor base do modificador

}

[SerializeField]
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