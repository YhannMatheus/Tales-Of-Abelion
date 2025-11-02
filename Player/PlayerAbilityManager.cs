using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerAbilityManager : MonoBehaviour
{
    public AbilitySlot BasicAttackSlot;
    public AbilitySlot[] SkillSlots = new AbilitySlot[6];
    public GameObject CasterGameObject;

    public struct AbilitySlotInfo
    {
        public int SlotIndex;
        public Ability AssignedAbility;
        public bool IsReady;
        public float CooldownRemaining;
        public float CooldownDuration;
        public int Charges;
    }

    public event Action<int> OnSlotReady;
    public event Action<int, float> OnSlotCooldownTick;
    public event Action<int> OnAbilityUsed;
    public event Action<int> OnInsufficientResources;

    private readonly List<AbilityInstanceBase> activeInstances = new List<AbilityInstanceBase>();

    private void Awake()
    {
    }

    private void Update()
    {
        TickCooldowns(Time.deltaTime);
    }

    public void UseAbilityInSlot(int slotIndex, Vector3 targetPosition, GameObject targetObject)
    {
        if (slotIndex < 0 || slotIndex >= SkillSlots.Length) return;

        var context = new AbilityContext
        {
            Caster = CasterGameObject,
            Target = targetObject,
            TargetPosition = targetPosition,
            CastStartPosition = CasterGameObject != null ? CasterGameObject.transform.position : Vector3.zero
        };

        TryUseAbilityInSlot(slotIndex, context);
    }

    public bool TryUseAbilityInSlot(int slotIndex, AbilityContext context)
    {
        if (slotIndex < 0 || slotIndex >= SkillSlots.Length) return false;
        var slot = SkillSlots[slotIndex];
        if (slot == null || slot.AssignedAbility == null) return false;

        if (!slot.CanUse()) return false;

        var ability = slot.AssignedAbility;

        if (ability != null && ability.energyCost > 0f)
        {
            var casterChar = CasterGameObject != null ? CasterGameObject.GetComponent<Character>() : null;
            if (casterChar != null)
            {
                int cost = Mathf.CeilToInt(ability.energyCost);
                if (!casterChar.TrySpendEnergy(cost))
                {
                    OnInsufficientResources?.Invoke(slotIndex);
                    return false;
                }
            }
        }

        slot.Use(context);
        OnAbilityUsed?.Invoke(slotIndex);
        return true;
    }

    public bool ForceUseAbilityInSlot(int slotIndex, AbilityContext context)
    {
        return TryUseAbilityInSlot(slotIndex, context);
    }

    public void AssignAbilityToSlot(int slotIndex, Ability ability, bool preserveCooldown = true, bool preserveCharges = true)
    {
        if (slotIndex < 0 || slotIndex >= SkillSlots.Length) return;
        var slot = SkillSlots[slotIndex];
        if (slot == null) return;
        slot.UpdateAssignedAbility(ability, preserveCooldown, preserveCharges);
    }

    public AbilitySlotInfo GetSlotInfo(int slotIndex)
    {
        var info = new AbilitySlotInfo();
        info.SlotIndex = slotIndex;
        if (slotIndex < 0 || slotIndex >= SkillSlots.Length) return info;
        var slot = SkillSlots[slotIndex];
        info.AssignedAbility = slot?.AssignedAbility;
        info.IsReady = slot != null && slot.IsReady;
        info.CooldownRemaining = slot != null ? slot.RemainingTime : 0f;
        info.CooldownDuration = slot != null ? slot.CooldownDuration : 0f;
        info.Charges = slot != null ? slot.Charges : 0;
        return info;
    }

    public AbilitySlotInfo[] GetAllSlotsInfo()
    {
        var arr = new AbilitySlotInfo[SkillSlots.Length];
        for (int i = 0; i < SkillSlots.Length; i++) arr[i] = GetSlotInfo(i);
        return arr;
    }

    public float GetCooldownRemaining(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SkillSlots.Length) return 0f;
        var slot = SkillSlots[slotIndex];
        return slot != null ? slot.RemainingTime : 0f;
    }

    public void RegisterInstance(AbilityInstanceBase instance)
    {
        if (instance == null) return;
        if (!activeInstances.Contains(instance)) activeInstances.Add(instance);
    }

    public void CancelAllCasts()
    {
        for (int i = activeInstances.Count - 1; i >= 0; i--)
        {
            var inst = activeInstances[i];
            if (inst != null)
            {
                inst.Cancel();
            }
        }
        activeInstances.Clear();
    }

    public void CancelInstance(AbilityInstanceBase instance)
    {
        if (instance == null) return;
        instance.Cancel();
        activeInstances.Remove(instance);
    }

    public void PreloadAbilityPrefabs(params GameObject[] prefabs)
    {
        foreach (var prefab in prefabs)
        {
            AbilityPool.Get(prefab);
        }
    }

    public void SetCasterGameObject(GameObject caster)
    {
        CasterGameObject = caster;
    }

    public bool UseBasicAttack(Vector3 targetPosition, GameObject targetObject)
    {
        if (BasicAttackSlot == null || BasicAttackSlot.AssignedAbility == null) return false;
        BasicAttackSlot.AssignedAbility.Execute(new AbilityContext
        {
            Caster = CasterGameObject,
            Target = targetObject,
            TargetPosition = targetPosition,
            CastStartPosition = CasterGameObject != null ? CasterGameObject.transform.position : Vector3.zero
        });
        return true;
    }

    private void TickCooldowns(float dt)
    {
        if (SkillSlots == null) return;

        for (int i = 0; i < SkillSlots.Length; i++)
        {
            var slot = SkillSlots[i];
            if (slot == null) continue;

            float prev = slot.CooldownRemaining;
            slot.TickCooldown(dt);
            float now = slot.CooldownRemaining;

            if (now > 0f)
            {
                OnSlotCooldownTick?.Invoke(i, now);
            }

            if (prev > 0f && now <= 0f)
            {
                OnSlotReady?.Invoke(i);
            }
        }

        if (BasicAttackSlot != null)
        {
            float prevB = BasicAttackSlot.CooldownRemaining;
            BasicAttackSlot.TickCooldown(dt);
            float nowB = BasicAttackSlot.CooldownRemaining;
            if (prevB > 0f && nowB <= 0f)
            {
                OnSlotReady?.Invoke(-1);
            }
        }
    }
}
