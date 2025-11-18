using UnityEngine;
using System;

public abstract class SkillManager : MonoBehaviour
{
    public SkillExecutionController[] skillSlots = new SkillExecutionController[8]; // skils listada como usaveis pelo jogador
    public SkillExecutionController basicAttackSkill;
    public CharacterManager character;

    // ===== Eventos para UI e integração externa =====
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
        if (character == null)
        {
            character = GetComponent<CharacterManager>();
        }
        // Bind existing controllers (serializados) com referências de runtime
        if (skillSlots != null)
        {
            for (int i = 0; i < skillSlots.Length; i++)
            {
                if (skillSlots[i] != null)
                {
                    skillSlots[i].Bind(character, character != null ? character.transform : null);
                }
            }
        }
        if (basicAttackSkill != null)
        {
            basicAttackSkill.Bind(character, character != null ? character.transform : null);
        }
    }
    
    private void Update()
    {
        UpdateCooldowns(Time.deltaTime);
        ApplyPassiveSkills();
    }

    private void OnEnable()
    {
        if (skillSlots == null) return;
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] != null)
                skillSlots[i].OnRequestSpawnProjectile += HandleSpawnRequest;
        }
    }

    private void OnDisable()
    {
        if (skillSlots == null) return;
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] != null)
                skillSlots[i].OnRequestSpawnProjectile -= HandleSpawnRequest;
        }
    }

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

        // Se não existir controller no slot, crie um novo encapsulando o skill
        if (skillSlots[slotIndex] == null)
        {
            var controller = new SkillExecutionController(newData, character, character != null ? character.transform : null);
            skillSlots[slotIndex] = controller;
            // garantir que SkillManager esteja inscrito para requests de spawn
            controller.OnRequestSpawnProjectile += HandleSpawnRequest;
        }
        else
        {
            skillSlots[slotIndex].Data = newData;
        }

        OnSkillAssigned?.Invoke(slotIndex, newData);
    }

    protected virtual void ClearSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Length)
        {
            Debug.LogError($"[SkillManager] Índice de slot inválido: {slotIndex}");
            return;
        }

        if (skillSlots[slotIndex] != null)
        {
            // dessubscrever event handlers
            skillSlots[slotIndex].OnRequestSpawnProjectile -= HandleSpawnRequest;
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

    protected virtual void ExecuteBasicAttack(SkillContext context)
    {
        if(basicAttackSkill == null)
        {
            Debug.LogWarning("[SkillManager] BasicAttackSkill não está atribuída.");
            return;
        }

        basicAttackSkill.TryExecute(context);
    }

    // Método público para executar basic attack (para input/AI)
    public void UseBasicAttack(SkillContext context)
    {
        ExecuteBasicAttack(context);
    }

    // Handle spawn requests from controllers: instantiate prefab and initialize instance
    protected virtual void HandleSpawnRequest(SkillExecutionController.ProjectileSpawnRequest req)
    {
        if (req.Prefab == null) return;

        GameObject go = Instantiate(req.Prefab, req.Position, req.Rotation);
        if (go == null) return;

        var instance = go.GetComponent<InstanceBase>();
        if (instance != null)
        {
            instance.Initialize(req.Context, req.Direction, req.HomingTarget, req.Speed, req.Lifetime, req.Behavior);
        }

        // Ignore collisions between projectile and caster if possible (use instance helper)
        if (req.Context.Caster != null)
        {
            var casterCol = req.Context.Caster.GetComponent<Collider>();
            var projInstance = go.GetComponent<InstanceBase>();
            if (casterCol != null && projInstance != null)
            {
                projInstance.RegisterIgnoredCollisionWithCaster(casterCol);
            }
        }
    }

    protected virtual void ApplyPassiveSkills()
    {
        foreach(var skill in skillSlots)
        {
            if(skill == null) continue;

            if(skill.Data != null && skill.Data.skillType == SkillType.Passive)
            {
                skill.ExecutePassive();
            }
        }
    }

    // ===== Helpers públicos para UI =====
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
        return skillSlots[slotIndex].Data;
    }

    public bool CanUseSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Length || skillSlots[slotIndex] == null)
            return false;
        return skillSlots[slotIndex].CanUse();
    }
}