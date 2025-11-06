using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gerencia skills de personagens controlados por IA
/// Adaptado do sistema de Skill (não mais Ability)
/// </summary>
[DisallowMultipleComponent]
public class IASkillManager : MonoBehaviour
{
    [Header("Skill Slots")]
    public SkillSlot BasicAttackSlot;
    public List<SkillSlot> SkillSlots = new List<SkillSlot>();

    [Header("AI Behavior")]
    [Range(0f, 1f)]
    [Tooltip("Chance da IA usar skill ao invés de ataque básico")]
    public float skillUsageChance = 0.7f;
    
    [Tooltip("Cooldown mínimo entre uso de skills")]
    public float minSkillCooldown = 1f;

    private IAManager _iaManager;
    private float _lastSkillUseTime;

    public event Action<int> OnSkillUsed;
    public event Action<int> OnInsufficientEnergy;

    private void Awake()
    {
        _iaManager = GetComponent<IAManager>();
    }

    private void Update()
    {
        TickCooldowns(Time.deltaTime);
    }

    /// <summary>
    /// Inicializa skills baseado na classe do personagem
    /// </summary>
    public void InitializeSkills()
    {
        if (_iaManager == null || _iaManager.Data.characterClass == null) return;

        ClassData classData = _iaManager.Data.characterClass;

        SkillSlots.Clear();

        foreach (SkillData skill in classData.classSkillTree)
        {
            if (skill == null) continue;

            SkillSlot slot = new SkillSlot(skill);
            SkillSlots.Add(slot);
        }

        Debug.Log($"[IASkillManager] {_iaManager.Data.characterName} inicializado com {SkillSlots.Count} skills");
    }

    public bool CanUseAnySkill()
    {
        if (!_iaManager.IsAlive) return false;
        if (Time.time < _lastSkillUseTime + minSkillCooldown) return false;

        foreach (SkillSlot slot in SkillSlots)
        {
            if (CanUseSkill(slot)) return true;
        }

        return false;
    }

    public bool CanUseSkill(SkillSlot slot)
    {
        if (slot == null || slot.AssignedSkill == null) return false;
        if (!slot.CanUse(_iaManager.character)) return false;

        return true;
    }

    public bool CanUseSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SkillSlots.Count) return false;
        return CanUseSkill(SkillSlots[slotIndex]);
    }

    /// <summary>
    /// Retorna skill utilizável aleatória dentro do alcance
    /// </summary>
    public SkillSlot GetBestSkillForRange(float distanceToTarget)
    {
        List<SkillSlot> usableSkills = new List<SkillSlot>();

        foreach (SkillSlot slot in SkillSlots)
        {
            if (!CanUseSkill(slot)) continue;
            if (slot.AssignedSkill.targeting.range < distanceToTarget) continue;

            usableSkills.Add(slot);
        }

        if (usableSkills.Count == 0) return null;

        // Chance de não usar skill (usar ataque básico)
        if (UnityEngine.Random.value > skillUsageChance) return null;

        int randomIndex = UnityEngine.Random.Range(0, usableSkills.Count);
        return usableSkills[randomIndex];
    }

    public bool TryUseSkill(SkillSlot slot, Character target)
    {
        if (!CanUseSkill(slot)) return false;

        SkillContext context = new SkillContext
        {
            Caster = _iaManager.character,
            Target = target,
            OriginPosition = _iaManager.transform.position,
            TargetPosition = target != null ? target.transform.position : _iaManager.transform.position
        };

        // Consome recursos
        slot.AssignedSkill.ConsumeResources(_iaManager.character);

        // Usa slot (inicia cooldown)
        slot.Use();

        // Adiciona dados da skill ao contexto
        context.SkillData = slot.AssignedSkill;
        context.SkillLevel = slot.CurrentLevel;

        // Cria executor
        GameObject executorObj = new GameObject($"IASkillExecutor_{slot.AssignedSkill.skillName}");
        executorObj.transform.SetParent(_iaManager.transform);
        
        var executor = executorObj.AddComponent<SkillExecutor>();
        executor.Initialize(slot.AssignedSkill, context);
        
        // Executa
        executor.Execute();

        _lastSkillUseTime = Time.time;

        int index = SkillSlots.IndexOf(slot);
        OnSkillUsed?.Invoke(index);

        Debug.Log($"[IASkillManager] {_iaManager.Data.characterName} usou {slot.AssignedSkill.skillName}");

        return true;
    }

    public bool TryUseSkill(int slotIndex, Character target)
    {
        if (slotIndex < 0 || slotIndex >= SkillSlots.Count) return false;
        return TryUseSkill(SkillSlots[slotIndex], target);
    }

    public bool TryUseBasicAttack(Character target)
    {
        if (BasicAttackSlot == null || BasicAttackSlot.AssignedSkill == null) return false;
        if (!_iaManager.CanAttack()) return false;

        SkillContext context = new SkillContext
        {
            Caster = _iaManager.character,
            Target = target,
            OriginPosition = _iaManager.transform.position,
            TargetPosition = target != null ? target.transform.position : _iaManager.transform.position
        };

        // Consome recursos
        BasicAttackSlot.AssignedSkill.ConsumeResources(_iaManager.character);

        // Usa slot
        BasicAttackSlot.Use();

        // Adiciona dados da skill ao contexto
        context.SkillData = BasicAttackSlot.AssignedSkill;
        context.SkillLevel = BasicAttackSlot.CurrentLevel;

        // Cria executor
        GameObject executorObj = new GameObject($"IABasicAttack_{BasicAttackSlot.AssignedSkill.skillName}");
        executorObj.transform.SetParent(_iaManager.transform);
        
        var executor = executorObj.AddComponent<SkillExecutor>();
        executor.Initialize(BasicAttackSlot.AssignedSkill, context);
        
        // Executa
        executor.Execute();

        _iaManager.lastAttackTime = Time.time;
        Debug.Log($"[IASkillManager] {_iaManager.Data.characterName} usou ataque básico");
        
        return true;
    }

    public float GetSkillCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SkillSlots.Count) return 0f;
        return SkillSlots[slotIndex].CooldownRemaining;
    }

    public SkillData GetSkillInSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SkillSlots.Count) return null;
        return SkillSlots[slotIndex].AssignedSkill;
    }

    public int GetSkillCount()
    {
        return SkillSlots.Count;
    }

    public List<SkillSlot> GetAllUsableSkills()
    {
        List<SkillSlot> usable = new List<SkillSlot>();

        foreach (SkillSlot slot in SkillSlots)
        {
            if (CanUseSkill(slot))
            {
                usable.Add(slot);
            }
        }

        return usable;
    }

    /// <summary>
    /// Retorna skill com maior dano potencial dentro do alcance
    /// </summary>
    public SkillSlot GetHighestDamageSkill(float distanceToTarget, bool includeBasicAttack = true)
    {
        SkillSlot bestSkill = null;
        float highestDamage = 0f;

        // Verifica ataque básico
        if (includeBasicAttack && BasicAttackSlot != null && BasicAttackSlot.AssignedSkill != null)
        {
            if (BasicAttackSlot.AssignedSkill.targeting.range >= distanceToTarget && _iaManager.CanAttack())
            {
                float damage = CalculateSkillDamage(BasicAttackSlot.AssignedSkill);
                if (damage > highestDamage)
                {
                    highestDamage = damage;
                    bestSkill = BasicAttackSlot;
                }
            }
        }

        // Verifica skills
        foreach (SkillSlot slot in SkillSlots)
        {
            if (!CanUseSkill(slot)) continue;
            if (slot.AssignedSkill.targeting.range < distanceToTarget) continue;

            float damage = CalculateSkillDamage(slot.AssignedSkill);
            if (damage > highestDamage)
            {
                highestDamage = damage;
                bestSkill = slot;
            }
        }

        return bestSkill;
    }

    public List<SkillSlot> GetActiveSkillsInRange(float distanceToTarget)
    {
        List<SkillSlot> skills = new List<SkillSlot>();

        foreach (SkillSlot slot in SkillSlots)
        {
            if (!CanUseSkill(slot)) continue;
            if (slot.AssignedSkill.targeting.range < distanceToTarget) continue;

            skills.Add(slot);
        }

        return skills;
    }

    /// <summary>
    /// Calcula dano estimado de uma skill (simplificado)
    /// </summary>
    private float CalculateSkillDamage(SkillData skill)
    {
        if (skill.effects == null || skill.effects.Count == 0) return 0f;

        float totalDamage = 0f;

        foreach (SkillEffect effect in skill.effects)
        {
            if (effect is DamageEffect damageEffect)
            {
                // Dano base
                totalDamage += damageEffect.baseDamage;

                // Escalamento
                switch (damageEffect.scaling)
                {
                    case DamageScaling.Physical:
                        totalDamage += _iaManager.Data.TotalPhysicalDamage * damageEffect.physicalRatio;
                        break;

                    case DamageScaling.Magical:
                        totalDamage += _iaManager.Data.TotalMagicalDamage * damageEffect.magicalRatio;
                        break;

                    case DamageScaling.Both:
                        totalDamage += _iaManager.Data.TotalPhysicalDamage * damageEffect.physicalRatio;
                        totalDamage += _iaManager.Data.TotalMagicalDamage * damageEffect.magicalRatio;
                        break;

                    case DamageScaling.Health:
                        totalDamage += _iaManager.Data.maxHealth * damageEffect.healthRatio;
                        break;
                }
            }
        }

        return totalDamage;
    }

    private void TickCooldowns(float deltaTime)
    {
        if (SkillSlots == null) return;

        foreach (SkillSlot slot in SkillSlots)
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
        string info = $"Skills: {SkillSlots.Count}\n";

        foreach (SkillSlot slot in SkillSlots)
        {
            if (slot == null || slot.AssignedSkill == null) continue;
            
            string readyStatus = slot.IsReady ? "✓" : $"{slot.CooldownRemaining:F1}s";
            info += $"{slot.AssignedSkill.skillName} [{readyStatus}]\n";
        }

        UnityEditor.Handles.Label(labelPosition, info);
        #endif
    }

    #endregion
}
