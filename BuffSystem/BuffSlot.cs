using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuffSlot
{
    public string buffName;
    public BuffData buffData;
    public List<Modifier> baseModifiers = new List<Modifier>();
    public int stackCount = 1;
    public int maxStacks = 1;
    public float duration = 0f;
    public float remainingTime = 0f;
    public bool isDebuff = false;
    public GameObject source;
    private int lastAppliedStacks = 0;

    // Auxiliares para UI
    public Sprite icon => buffData != null ? buffData.icon : null;
    public string displayName => buffData != null ? buffData.buffName : buffName;

    // Eventos
    public event Action<BuffSlot> OnExpired;
    public event Action<BuffSlot> OnStackChanged;

    public BuffSlot() { }

    public void InitializeFromBuffData(BuffData data, GameObject source = null)
    {
        this.buffData = data;
        this.buffName = data != null ? data.buffName : this.buffName;
        // BuffData não tem modifiers nem duration - esses valores vêm do SkillData
        this.isDebuff = data != null ? data.isDebuff : this.isDebuff;
        this.source = source;
        this.stackCount = 1;
        this.maxStacks = 1;
    }

    public void SetStackConfig(int maxStacks, int startStacks = 1)
    {
        this.maxStacks = Mathf.Max(1, maxStacks);
        this.stackCount = Mathf.Clamp(startStacks, 1, this.maxStacks);
    }

    public void AddStack(int count = 1, bool refreshDuration = true)
    {
        int prev = stackCount;
        stackCount = Mathf.Clamp(stackCount + count, 1, maxStacks);
        if (refreshDuration)
            remainingTime = duration;
        if (prev != stackCount)
            OnStackChanged?.Invoke(this);
    }

    public void RemoveStack(int count = 1)
    {
        int prev = stackCount;
        stackCount = Mathf.Clamp(stackCount - count, 0, maxStacks);
        if (prev != stackCount)
            OnStackChanged?.Invoke(this);
        if (stackCount <= 0)
            Expire();
    }

    public void Refresh()
    {
        remainingTime = duration;
    }

    public void Tick(float dt)
    {
        if (duration <= 0f) return;
        remainingTime -= dt;
        if (remainingTime <= 0f) Expire();
    }

    void Expire()
    {
        OnExpired?.Invoke(this);
    }

    // Retorna os modificadores efetivos levando em conta os stacks (empilhamento multiplicativo simples)
    public List<Modifier> GetEffectiveModifiers()
    {
        if (baseModifiers == null) return new List<Modifier>();
        var outMods = new List<Modifier>(baseModifiers.Count);
        for (int i = 0; i < baseModifiers.Count; i++)
        {
            var m = baseModifiers[i];
            Modifier n = new Modifier();
            n.type = m.type;
            n.variable = m.variable;
            n.value = m.value * stackCount;
            n.baseValue = m.baseValue * stackCount;
            outMods.Add(n);
        }
        return outMods;
    }

    // Retorna os modificadores que representam o delta entre o stackCount atual e lastAppliedStacks,
    // e atualiza lastAppliedStacks para o stackCount atual.
    public List<Modifier> GetStackDeltaModifiersAndConsume()
    {
        int deltaStacks = stackCount - lastAppliedStacks;
        var deltaMods = new List<Modifier>();
        if (baseModifiers == null || deltaStacks == 0) { lastAppliedStacks = stackCount; return deltaMods; }

        foreach (var m in baseModifiers)
        {
            Modifier n = new Modifier();
            n.type = m.type;
            n.variable = m.variable;
            n.value = m.value * deltaStacks;
            n.baseValue = m.baseValue * deltaStacks;
            deltaMods.Add(n);
        }

        lastAppliedStacks = stackCount;
        return deltaMods;
    }

    // Retorna os modificadores correspondentes à quantidade atualmente aplicada (para remoção ao expirar)
    public List<Modifier> GetCurrentlyAppliedModifiers()
    {
        var applied = new List<Modifier>();
        if (baseModifiers == null || lastAppliedStacks == 0) return applied;
        foreach (var m in baseModifiers)
        {
            Modifier n = new Modifier();
            n.type = m.type;
            n.variable = m.variable;
            n.value = m.value * lastAppliedStacks;
            n.baseValue = m.baseValue * lastAppliedStacks;
            applied.Add(n);
        }
        return applied;
    }
}
