using System;
using UnityEngine;

// Slot de skill (gerencia cooldown, charges, equipamento)
[System.Serializable]
public class SkillSlot
{
    [Header("Skill Equipada")]
    [Tooltip("Skill atribuída a este slot")]
    [SerializeField] private SkillData assignedSkill;

    [Header("Estado Atual")]
    [Tooltip("Nível atual da skill")]
    [SerializeField] private int currentLevel = 1;
    
    [Tooltip("Cooldown restante em segundos")]
    [SerializeField] private float cooldownRemaining = 0f;
    
    [Tooltip("Charges atuais")]
    [SerializeField] private int currentCharges = 0;
    
    [Tooltip("Tempo para próxima charge")]
    [SerializeField] private float chargeRechargeTimer = 0f;
    
    [Tooltip("Slot está bloqueado (não pode ser usado)")]
    [SerializeField] private bool isLocked = false;

    // Eventos
    public event Action<SkillSlot> OnSkillUsed;
    public event Action<SkillSlot> OnCooldownStarted;
    public event Action<SkillSlot> OnCooldownEnded;
    public event Action<SkillSlot> OnChargeGained;
    public event Action<SkillSlot> OnLevelUp;

    // Propriedades públicas
    public SkillData AssignedSkill => assignedSkill;
    public int CurrentLevel => currentLevel;
    public float CooldownRemaining => cooldownRemaining;
    public int CurrentCharges => currentCharges;
    public bool IsLocked => isLocked;
    public bool IsEmpty => assignedSkill == null;
    public SkillState State => GetCurrentState();

    // Propriedades calculadas
    public bool IsReady => GetCurrentState() == SkillState.Ready;
    public bool IsOnCooldown => cooldownRemaining > 0f;
    public float CooldownProgress => assignedSkill != null && assignedSkill.cooldown.duration > 0f
        ? 1f - (cooldownRemaining / assignedSkill.cooldown.duration)
        : 1f;

    // Construtor
    public SkillSlot()
    {
        currentLevel = 1;
    }

    public SkillSlot(SkillData skill)
    {
        AssignSkill(skill);
    }

    // Atribui skill ao slot
    public void AssignSkill(SkillData skill)
    {
        assignedSkill = skill;
        currentLevel = 1;
        cooldownRemaining = 0f;

        // Inicializa charges
        if (skill != null && skill.cooldown.maxCharges > 0)
        {
            currentCharges = skill.cooldown.maxCharges;
        }
        else
        {
            currentCharges = 0;
        }

        // Começa em cooldown se configurado
        if (skill != null && skill.cooldown.startsOnCooldown)
        {
            StartCooldown();
        }
    }

    // Remove skill do slot
    public void ClearSlot()
    {
        assignedSkill = null;
        currentLevel = 1;
        cooldownRemaining = 0f;
        currentCharges = 0;
        isLocked = false;
    }

    // Verifica se pode usar a skill
    public bool CanUse(CharacterManager caster)
    {
        if (IsEmpty)
            return false;

        if (isLocked)
            return false;

        if (GetCurrentState() != SkillState.Ready)
            return false;

        // Verifica requisitos e recursos da skill
        return assignedSkill.CanUse(caster);
    }

    // Usa a skill (inicia cooldown e consome charge)
    public void Use()
    {
        if (IsEmpty)
            return;

        // Sistema de charges
        if (assignedSkill.cooldown.maxCharges > 0)
        {
            currentCharges--;
            
            // Inicia timer de recarga se não estava rodando
            if (chargeRechargeTimer <= 0f)
            {
                chargeRechargeTimer = assignedSkill.cooldown.chargeRechargeTime;
            }
        }
        else
        {
            // Cooldown normal
            StartCooldown();
        }

        OnSkillUsed?.Invoke(this);
    }

    // Inicia cooldown
    public void StartCooldown()
    {
        if (IsEmpty)
            return;

        cooldownRemaining = assignedSkill.cooldown.duration;
        OnCooldownStarted?.Invoke(this);
    }

    // Reseta cooldown (forçado)
    public void ResetCooldown()
    {
        cooldownRemaining = 0f;
        OnCooldownEnded?.Invoke(this);
    }

    // Atualiza cooldown (chamar todo frame)
    public void TickCooldown(float deltaTime)
    {
        if (IsEmpty)
            return;

        // Atualiza cooldown normal
        if (cooldownRemaining > 0f)
        {
            cooldownRemaining -= deltaTime;
            
            if (cooldownRemaining <= 0f)
            {
                cooldownRemaining = 0f;
                OnCooldownEnded?.Invoke(this);
            }
        }

        // Atualiza recarga de charges
        if (assignedSkill.cooldown.maxCharges > 0 && currentCharges < assignedSkill.cooldown.maxCharges)
        {
            chargeRechargeTimer -= deltaTime;
            
            if (chargeRechargeTimer <= 0f)
            {
                currentCharges++;
                OnChargeGained?.Invoke(this);
                
                // Se ainda não está cheio, continua contando
                if (currentCharges < assignedSkill.cooldown.maxCharges)
                {
                    chargeRechargeTimer = assignedSkill.cooldown.chargeRechargeTime;
                }
                else
                {
                    chargeRechargeTimer = 0f;
                }
            }
        }
    }

    // Aumenta nível da skill
    public bool LevelUp()
    {
        if (IsEmpty)
            return false;

        if (currentLevel >= assignedSkill.maxLevel)
            return false;

        currentLevel++;
        OnLevelUp?.Invoke(this);
        return true;
    }

    // Bloqueia/desbloqueia slot
    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }

    // Retorna estado atual
    SkillState GetCurrentState()
    {
        if (IsEmpty)
            return SkillState.Locked;

        if (isLocked)
            return SkillState.Locked;

        if (cooldownRemaining > 0f)
            return SkillState.Cooldown;

        // Sistema de charges
        if (assignedSkill.cooldown.maxCharges > 0)
        {
            if (currentCharges > 0)
                return SkillState.Ready;
            else
                return SkillState.Cooldown;
        }

        return SkillState.Ready;
    }

    // Retorna tempo restante de recarga de charge
    public float GetChargeRechargeProgress()
    {
        if (IsEmpty || assignedSkill.cooldown.maxCharges == 0)
            return 0f;

        if (assignedSkill.cooldown.chargeRechargeTime <= 0f)
            return 0f;

        return 1f - (chargeRechargeTimer / assignedSkill.cooldown.chargeRechargeTime);
    }
}
