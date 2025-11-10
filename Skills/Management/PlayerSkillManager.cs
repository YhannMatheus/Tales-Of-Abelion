using System;
using System.Collections.Generic;
using UnityEngine;

// Gerenciador de skills do player - gerenciado pelo PlayerManager
// Usa RequireComponent pois precisa de CharacterManager para funcionar
[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterManager))]
public class PlayerSkillManager : MonoBehaviour
{
    [Header(" Slots de Skills ")]
    public SkillSlot BasicAttackSlot = new SkillSlot();
    public SkillSlot[] SkillSlots = new SkillSlot[6];

    // Eventos
    public event Action<int> OnSlotReady;
    public event Action<int, float> OnSlotCooldownTick;
    public event Action<int> OnSkillUsed;
    public event Action<int> OnInsufficientResources;
    public event Action<int> OnRequirementsNotMet;

    // Componentes
    CharacterManager CharacterManager;
    
    // Skills ativas sendo executadas
    readonly List<SkillExecutor> activeExecutors = new List<SkillExecutor>();

    void Awake()
    {
        CharacterManager = GetComponent<CharacterManager>();
        
        // Inicializa slots vazios
        for (int i = 0; i < SkillSlots.Length; i++)
        {
            if (SkillSlots[i] == null)
            {
                SkillSlots[i] = new SkillSlot();
            }
        }

        // Inscreve em eventos dos slots
        SubscribeToSlotEvents();
    }

    void Update()
    {
        // Atualiza cooldowns
        TickCooldowns(Time.deltaTime);
    }

    // Usa skill em slot específico
    public bool UseSkill(int slotIndex, Vector3 targetPosition, CharacterManager targetCharacter = null)
    {
        // Cria contexto
        SkillContext context = CreateContext(targetPosition, targetCharacter);

        return TryUseSkillInSlot(slotIndex, context);
    }

    // Usa basic attack
    public bool UseBasicAttack(CharacterManager target)
    {
        SkillContext context = new SkillContext(CharacterManager, target);
        return TryUseBasicAttack(context);
    }

    // Tenta usar skill (com validações)
    public bool TryUseSkillInSlot(int slotIndex, SkillContext context)
    {
        if (slotIndex < 0 || slotIndex >= SkillSlots.Length)
        {
            Debug.LogWarning($"[PlayerSkillManager] Índice inválido: {slotIndex}");
            return false;
        }

        var slot = SkillSlots[slotIndex];
        
        if (slot.IsEmpty)
        {
            Debug.Log($"[PlayerSkillManager] Slot {slotIndex} está vazio");
            return false;
        }

        return ExecuteSkillFromSlot(slot, slotIndex, context);
    }

    // Tenta usar basic attack
    public bool TryUseBasicAttack(SkillContext context)
    {
        if (BasicAttackSlot.IsEmpty)
        {
            Debug.LogWarning("[PlayerSkillManager] Basic Attack não está configurado!");
            return false;
        }

        return ExecuteSkillFromSlot(BasicAttackSlot, -1, context);
    }

    // Executa skill de um slot
    bool ExecuteSkillFromSlot(SkillSlot slot, int slotIndex, SkillContext context)
    {
        // Valida slot
        if (!slot.CanUse(CharacterManager))
        {
            Debug.Log($"[PlayerSkillManager] Skill '{slot.AssignedSkill.skillName}' não pode ser usada");
            
            // Dispara evento apropriado
            if (!slot.AssignedSkill.requirements.IsMet(CharacterManager))
            {
                OnRequirementsNotMet?.Invoke(slotIndex);
            }
            else if (!slot.AssignedSkill.cost.CanAfford(CharacterManager))
            {
                OnInsufficientResources?.Invoke(slotIndex);
            }
            
            return false;
        }

        var skill = slot.AssignedSkill;
        
        // Consome recursos
        skill.ConsumeResources(CharacterManager);

        // Usa slot (inicia cooldown)
        slot.Use();

        // Adiciona dados da skill ao contexto
        context.SkillData = skill;
        context.SkillLevel = slot.CurrentLevel;

        // Cria executor
        GameObject executorObj = new GameObject($"SkillExecutor_{skill.skillName}");
        executorObj.transform.SetParent(transform);
        
        var executor = executorObj.AddComponent<SkillExecutor>();
        executor.Initialize(skill, context);
        
        // Executa
        executor.Execute();
        
        // Rastreia executor
        activeExecutors.Add(executor);

        // Dispara evento
        OnSkillUsed?.Invoke(slotIndex);

        Debug.Log($"[PlayerSkillManager] Executou skill '{skill.skillName}' (Slot {slotIndex})");

        return true;
    }

    // Atualiza cooldowns
    void TickCooldowns(float deltaTime)
    {
        // Basic attack
        BasicAttackSlot.TickCooldown(deltaTime);

        // Skills
        for (int i = 0; i < SkillSlots.Length; i++)
        {
            SkillSlots[i].TickCooldown(deltaTime);
            
            if (SkillSlots[i].IsOnCooldown)
            {
                OnSlotCooldownTick?.Invoke(i, SkillSlots[i].CooldownRemaining);
            }
        }

        // Remove executors finalizados
        activeExecutors.RemoveAll(e => e == null || !e.IsExecuting);
    }

    // Atribui skill a um slot
    public void AssignSkillToSlot(int slotIndex, SkillData skill)
    {
        if (slotIndex < 0 || slotIndex >= SkillSlots.Length)
        {
            Debug.LogWarning($"[PlayerSkillManager] Índice inválido: {slotIndex}");
            return;
        }

        SkillSlots[slotIndex].AssignSkill(skill);
        Debug.Log($"[PlayerSkillManager] Skill '{skill.skillName}' atribuída ao slot {slotIndex}");
    }

    // Atribui basic attack
    public void AssignBasicAttack(SkillData skill)
    {
        BasicAttackSlot.AssignSkill(skill);
        Debug.Log($"[PlayerSkillManager] Basic Attack '{skill.skillName}' atribuído");
    }

    // Remove skill de um slot
    public void ClearSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SkillSlots.Length)
            return;

        SkillSlots[slotIndex].ClearSlot();
    }

    // Retorna skill em slot
    public SkillData GetSkill(int slotIndex)
    {
        if (slotIndex == -1)
            return BasicAttackSlot.AssignedSkill;

        if (slotIndex < 0 || slotIndex >= SkillSlots.Length)
            return null;

        return SkillSlots[slotIndex].AssignedSkill;
    }

    // Retorna slot
    public SkillSlot GetSlot(int slotIndex)
    {
        if (slotIndex == -1)
            return BasicAttackSlot;

        if (slotIndex < 0 || slotIndex >= SkillSlots.Length)
            return null;

        return SkillSlots[slotIndex];
    }

    // Cancela todas as skills ativas
    public void CancelAllActiveSkills()
    {
        foreach (var executor in activeExecutors)
        {
            if (executor != null && executor.IsExecuting)
            {
                executor.Cancel();
            }
        }

        activeExecutors.Clear();
    }

    // Verifica se alguma skill está sendo executada
    public bool IsExecutingSkill()
    {
        return activeExecutors.Exists(e => e != null && e.IsExecuting);
    }

    // Verifica se está casting
    public bool IsCasting()
    {
        return activeExecutors.Exists(e => e != null && e.IsCasting);
    }

    // Cria contexto de skill
    SkillContext CreateContext(Vector3 targetPosition, CharacterManager targetCharacter)
    {
        SkillContext context = new SkillContext(CharacterManager);
        context.TargetPosition = targetPosition;
        context.Target = targetCharacter;

        if (targetCharacter != null)
        {
            context.Direction = (targetCharacter.transform.position - CharacterManager.transform.position).normalized;
        }
        else if (targetPosition != Vector3.zero)
        {
            context.Direction = (targetPosition - CharacterManager.transform.position).normalized;
        }

        return context;
    }

    // Inscreve em eventos dos slots
    void SubscribeToSlotEvents()
    {
        // Basic attack
        BasicAttackSlot.OnCooldownEnded += (slot) => OnSlotReady?.Invoke(-1);

        // Skills
        for (int i = 0; i < SkillSlots.Length; i++)
        {
            int index = i; // Capture para lambda
            SkillSlots[i].OnCooldownEnded += (slot) => OnSlotReady?.Invoke(index);
        }
    }

    // Debug: Força cooldown de um slot
    public void DebugResetCooldown(int slotIndex)
    {
        if (slotIndex == -1)
        {
            BasicAttackSlot.ResetCooldown();
            return;
        }

        if (slotIndex >= 0 && slotIndex < SkillSlots.Length)
        {
            SkillSlots[slotIndex].ResetCooldown();
        }
    }
}
