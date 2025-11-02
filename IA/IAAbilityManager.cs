using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class IAAbilityManager : MonoBehaviour
{
    [Header("Ability Slots")]
    public AbilitySlot BasicAttackSlot;
    public List<AbilitySlot> SkillSlots = new List<AbilitySlot>();

    [Header("AI Behavior")]
    [Range(0f, 1f)]
    public float abilityUsageChance = 0.7f;
    public float minAbilityCooldown = 1f;

    private IAManager _iaManager;
    private float _lastAbilityUseTime;

    public event Action<int> OnAbilityUsed;
    public event Action<int> OnInsufficientEnergy;

    private void Awake()
    {
        _iaManager = GetComponent<IAManager>();
    }

    private void Update()
    {
        TickCooldowns(Time.deltaTime);
    }

    public void InitializeAbilities()
    {
        if (_iaManager == null || _iaManager.Data.characterClass == null) return;

        ClassData classData = _iaManager.Data.characterClass;

        SkillSlots.Clear();

        foreach (Ability ability in classData.classSkillTree)
        {
            if (ability == null) continue;

            AbilitySlot slot = new AbilitySlot
            {
                AssignedAbility = ability,
                CooldownDuration = ability.cooldownTime,
                CooldownRemaining = 0f,
                IsEquipped = true,
                IsPassive = false
            };

            SkillSlots.Add(slot);
        }

        Debug.Log($"[IAAbilityManager] {_iaManager.Data.characterName} inicializado com {SkillSlots.Count} habilidades");
    }

    public bool CanUseAnyAbility()
    {
        if (!_iaManager.IsAlive) return false;
        if (Time.time < _lastAbilityUseTime + minAbilityCooldown) return false;

        foreach (AbilitySlot slot in SkillSlots)
        {
            if (CanUseAbility(slot)) return true;
        }

        return false;
    }

    public bool CanUseAbility(AbilitySlot slot)
    {
        if (slot == null || slot.AssignedAbility == null) return false;
        if (!slot.CanUse()) return false;

        int energyCost = Mathf.CeilToInt(slot.AssignedAbility.energyCost);
        if (_iaManager.Data.currentEnergy < energyCost) return false;

        return true;
    }

    public bool CanUseAbility(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SkillSlots.Count) return false;
        return CanUseAbility(SkillSlots[slotIndex]);
    }

    public AbilitySlot GetBestAbilityForRange(float distanceToTarget)
    {
        List<AbilitySlot> usableAbilities = new List<AbilitySlot>();

        foreach (AbilitySlot slot in SkillSlots)
        {
            if (!CanUseAbility(slot)) continue;
            if (slot.AssignedAbility.range < distanceToTarget) continue;

            usableAbilities.Add(slot);
        }

        if (usableAbilities.Count == 0) return null;

        if (UnityEngine.Random.value > abilityUsageChance) return null;

        int randomIndex = UnityEngine.Random.Range(0, usableAbilities.Count);
        return usableAbilities[randomIndex];
    }

    public bool TryUseAbility(AbilitySlot slot, Transform target)
    {
        if (!CanUseAbility(slot)) return false;

        Vector3 targetPosition = target != null ? target.position : _iaManager.transform.position;

        AbilityContext context = new AbilityContext
        {
            Caster = _iaManager.gameObject,
            Target = target != null ? target.gameObject : null,
            TargetPosition = targetPosition,
            CastStartPosition = _iaManager.transform.position
        };

        int energyCost = Mathf.CeilToInt(slot.AssignedAbility.energyCost);
        if (!_iaManager.TrySpendEnergy(energyCost))
        {
            int slotIndex = SkillSlots.IndexOf(slot);
            OnInsufficientEnergy?.Invoke(slotIndex);
            return false;
        }

        slot.Use(context);
        _lastAbilityUseTime = Time.time;

        int index = SkillSlots.IndexOf(slot);
        OnAbilityUsed?.Invoke(index);

        Debug.Log($"[IAAbilityManager] {_iaManager.Data.characterName} usou {slot.AssignedAbility.abilityName}");

        return true;
    }

    public bool TryUseAbility(int slotIndex, Transform target)
    {
        if (slotIndex < 0 || slotIndex >= SkillSlots.Count) return false;
        return TryUseAbility(SkillSlots[slotIndex], target);
    }

    public bool TryUseBasicAttack(Transform target)
    {
        if (BasicAttackSlot == null || BasicAttackSlot.AssignedAbility == null) return false;
        if (!_iaManager.CanAttack()) return false;

        Vector3 targetPosition = target != null ? target.position : _iaManager.transform.position;

        AbilityContext context = new AbilityContext
        {
            Caster = _iaManager.gameObject,
            Target = target != null ? target.gameObject : null,
            TargetPosition = targetPosition,
            CastStartPosition = _iaManager.transform.position
        };

        BasicAttackSlot.Use(context);
        _iaManager.lastAttackTime = Time.time;

        Debug.Log($"[IAAbilityManager] {_iaManager.Data.characterName} usou ataque básico");

        return true;
    }

    public float GetAbilityCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SkillSlots.Count) return 0f;
        return SkillSlots[slotIndex].CooldownRemaining;
    }

    public Ability GetAbilityInSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SkillSlots.Count) return null;
        return SkillSlots[slotIndex].AssignedAbility;
    }

    public int GetAbilityCount()
    {
        return SkillSlots.Count;
    }

    public List<AbilitySlot> GetAllUsableAbilities()
    {
        List<AbilitySlot> usable = new List<AbilitySlot>();

        foreach (AbilitySlot slot in SkillSlots)
        {
            if (CanUseAbility(slot))
            {
                usable.Add(slot);
            }
        }

        return usable;
    }

    public AbilitySlot GetHighestDamageAbility(float distanceToTarget, bool includeBasicAttack = true)
    {
        AbilitySlot bestAbility = null;
        float highestDamage = 0f;

        if (includeBasicAttack && BasicAttackSlot != null && BasicAttackSlot.AssignedAbility != null)
        {
            if (BasicAttackSlot.AssignedAbility.range >= distanceToTarget && _iaManager.CanAttack())
            {
                float damage = BasicAttackSlot.AssignedAbility.CalculateDamage(_iaManager.Data);
                if (damage > highestDamage)
                {
                    highestDamage = damage;
                    bestAbility = BasicAttackSlot;
                }
            }
        }

        foreach (AbilitySlot slot in SkillSlots)
        {
            if (slot.IsPassive) continue;
            if (!CanUseAbility(slot)) continue;
            if (slot.AssignedAbility.range < distanceToTarget) continue;

            float damage = slot.AssignedAbility.CalculateDamage(_iaManager.Data);
            if (damage > highestDamage)
            {
                highestDamage = damage;
                bestAbility = slot;
            }
        }

        return bestAbility;
    }

    public List<AbilitySlot> GetActiveAbilitiesInRange(float distanceToTarget)
    {
        List<AbilitySlot> abilities = new List<AbilitySlot>();

        foreach (AbilitySlot slot in SkillSlots)
        {
            if (slot.IsPassive) continue;
            if (!CanUseAbility(slot)) continue;
            if (slot.AssignedAbility.range < distanceToTarget) continue;

            abilities.Add(slot);
        }

        return abilities;
    }

    private void TickCooldowns(float deltaTime)
    {
        if (SkillSlots == null) return;

        foreach (AbilitySlot slot in SkillSlots)
        {
            if (slot == null) continue;
            slot.TickCooldown(deltaTime);
        }

        if (BasicAttackSlot != null)
        {
            BasicAttackSlot.TickCooldown(deltaTime);
        }
    }

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        #if UNITY_EDITOR
        if (SkillSlots == null || SkillSlots.Count == 0) return;

        Vector3 labelPosition = transform.position + Vector3.up * 3f;
        string info = $"Abilities: {SkillSlots.Count}\n";

        foreach (AbilitySlot slot in SkillSlots)
        {
            if (slot == null || slot.AssignedAbility == null) continue;
            
            string readyStatus = slot.IsReady ? "✓" : $"{slot.CooldownRemaining:F1}s";
            info += $"{slot.AssignedAbility.abilityName} [{readyStatus}]\n";
        }

        UnityEditor.Handles.Label(labelPosition, info);
        #endif
    }

    #endregion
}
