using UnityEngine;

public abstract class EffectBase
{
    protected CharacterManager target;
    protected CharacterManager source;
    protected EffectData data;

    protected int stacks = 1;

    public virtual void Initialize(EffectData data, CharacterManager target, CharacterManager source, int stacks)
    {
        this.data = data;
        this.target = target;
        this.source = source;
        this.stacks += stacks;

        OnApply();
    }
    public virtual void OnApply() {}
    public virtual void Tick(float deltaTime) {}
    public virtual void OnRemove() {}

    protected float GetValue() => data.modifierValue * stacks * data.stackValueMultiplier;
}
