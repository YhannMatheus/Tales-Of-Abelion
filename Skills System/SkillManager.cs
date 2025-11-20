using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class SkillManager : MonoBehaviour
{
    public SkillExecutionController[] skillSlots = new SkillExecutionController[8]; // skils listada como usaveis pelo jogador
    public SkillExecutionController basicAttackSkill;
    public SkillExecutionController classPassiveSkill;
    public CharacterManager character;
    public List<Skill> Skills = new List<Skill>();

    // Emitido quando uma skill é executada com sucesso
    public event Action<int, SkillContext> OnSkillExecuted;
    // Emitido quando tentativa de usar skill falha (cooldown, energia, etc)
    public event Action<int, string> OnSkillExecuteFailed;
    // Emitido quando cooldown de um slot muda (útil para atualizar UI)
    public event Action<int, float, float> OnCooldownChanged; // slotIndex, remaining, percent
    // Emitido quando uma skill é atribuída/trocada em um slot
    public event Action<int, SkillData> OnSkillAssigned;

    private void Awake()
    {
    }
    
    private void Update()
    {
        UpdateCooldowns(Time.deltaTime);        // Atualiza cooldowns de todas as skills
        UpdateSkillAnimations(Time.deltaTime);  // Atualiza animações de skills em progresso
        InstructionForUse();                    // Verifica inputs para uso de skills
    }

    public abstract void InstructionForUse();
    public abstract SkillContext CreateContext(SkillExecutionController skillController);

    private void UpdateCooldowns(float deltaTime)
    {   
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] == null) continue;

            var sc = skillSlots[i];
            if (sc != null)
            {
                float previousRemaining = sc.CooldownRemaining;
                sc.TickCooldown(deltaTime);
                
                // Emitir evento se cooldown mudou significativamente (evitar spam)
                if (Mathf.Abs(previousRemaining - sc.CooldownRemaining) > 0.01f)
                {
                    OnCooldownChanged?.Invoke(i, sc.CooldownRemaining, sc.CooldownPercent);
                }
            }
        }
    }

    private void UpdateSkillAnimations(float deltaTime)
    {
        // Atualiza SkillAnimationController de cada slot
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] != null)
            {
                skillSlots[i].TickAnimation(deltaTime);
            }
        }

        // Atualiza basic attack também
        if (basicAttackSkill != null)
        {
            basicAttackSkill.TickAnimation(deltaTime);
        }
    }

    public void SwitchSkill(int slotIndex, SkillData newData)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Length)
        {
            Debug.LogError($"[SkillManager] Índice de slot inválido: {slotIndex}");
            return;
        }

        if (newData == null)
        {
            Debug.LogError("[SkillManager] newData não pode ser nulo ao trocar skill.");
            return;
        }

        
        skillSlots[slotIndex].runtimeSkill.Data = newData;

        OnSkillAssigned?.Invoke(slotIndex, newData);
    }

    protected virtual void ClearSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Length)
        {
            Debug.LogError($"[SkillManager] Índice de slot inválido: {slotIndex}");
            return;
        }
        
        skillSlots[slotIndex] = null;
    }    

    // Método principal para executar skill de um slot (usado por input/AI)
    public virtual void UseSkill(int slotIndex, SkillContext context)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Length)
        {
            Debug.LogWarning($"[SkillManager] Tentativa de executar skill em slot inválido: {slotIndex}");
            return;
        }

        var controller = skillSlots[slotIndex];
        if (controller == null)
        {
            OnSkillExecuteFailed?.Invoke(slotIndex, "Slot vazio");
            return;
        }

        // Controller faz todas as validações e execução
        bool success = controller.TryExecute(context);
        if (success)
        {
            OnSkillExecuted?.Invoke(slotIndex, context);
        }
        else
        {
            OnSkillExecuteFailed?.Invoke(slotIndex, controller.GetLastFailureReason());
        }
    }

    public virtual void UseBasicAttack(SkillContext context)
    {
        if(basicAttackSkill == null)
        {
            Debug.LogWarning("[SkillManager] BasicAttackSkill não está atribuída.");
            return;
        }

        basicAttackSkill.TryExecute(context);
    }


    #region Skill Context Helpers
    public float GetCooldownRemaining(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Length || skillSlots[slotIndex] == null)
            return 0f;
        return skillSlots[slotIndex].CooldownRemaining;
    }

    public float GetCooldownPercent(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Length || skillSlots[slotIndex] == null)
            return 0f;
        return skillSlots[slotIndex].CooldownPercent;
    }

    public float GetCooldownProgress(int slotIndex)
    {
        return 1f - GetCooldownPercent(slotIndex);
    }

    public bool IsSlotOnCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Length || skillSlots[slotIndex] == null)
            return false;
        return skillSlots[slotIndex].IsOnCooldown;
    }

    public SkillData GetSkillInSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Length || skillSlots[slotIndex] == null)
            return null;
        return skillSlots[slotIndex].runtimeSkill.Data;
    }

    public bool CanUseSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Length || skillSlots[slotIndex] == null)
            return false;
        return skillSlots[slotIndex].CanUse();
    }
    #endregion
}