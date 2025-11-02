using System;
using UnityEngine;

[Serializable]
public class AbilitySlot
{
    public Ability AssignedAbility;
    public float CooldownDuration;
    public int Charges = 0;
    public bool IsEquipped = false;
    public bool IsPassive = false; 
    public event Action<AbilitySlot> OnUsed;
    public event Action<AbilitySlot> OnCooldownStarted;
    public event Action<AbilitySlot> OnCooldownEnded;

    [NonSerialized] public float CooldownRemaining;
    
    public bool IsReady => AssignedAbility != null && CooldownRemaining <= 0f;

    public float RemainingTime => Mathf.Max(0f, CooldownRemaining);

    public bool CanUse()
    {
        if (AssignedAbility == null) return false;
        if (CooldownRemaining > 0f) return false;
        if (Charges == 0) return true;
        return Charges > 0;
    }

    public void Use(AbilityContext context)
    {
        if (!CanUse()) return;

        AssignedAbility.Execute(context);

        if (Charges > 0) Charges--;

        CooldownRemaining = CooldownDuration > 0f ? CooldownDuration : (AssignedAbility != null ? AssignedAbility.cooldownTime : 0f);

        OnUsed?.Invoke(this);
        OnCooldownStarted?.Invoke(this);
    }

    public void TickCooldown(float delta)
    {
        if (CooldownRemaining <= 0f) return;
        CooldownRemaining -= delta;
        if (CooldownRemaining <= 0f)
        {
            CooldownRemaining = 0f;
            OnCooldownEnded?.Invoke(this);
        }
    }

    
    public void UpdateAssignedAbility(Ability newAbility, bool preserveCooldown = true, bool preserveCharges = true)
    {
        if (AssignedAbility == newAbility) return;

        if (!preserveCooldown)
        {
            CooldownDuration = newAbility != null ? newAbility.cooldownTime : 0f;
            CooldownRemaining = 0f;
        }

       AssignedAbility = newAbility;

        if (!preserveCharges)
        {
            Charges = 0;
        }
    }
}
