using System;
using JetBrains.Annotations;
using UnityEngine;

public class PlayerSkillManager : SkillManager
{
    public PlayerManager playerManager;
    public event Action<SkillData, int?> OnSkillRequested;

    public bool[] inputsSkill  = new bool[8];
    
    void Awake()
    {
        // Garantir que temos referência ao PlayerManager antes de usar
        playerManager = GetComponent<PlayerManager>();

        // Inicializa array de inputs (usaremos Input nativo do Unity)
        inputsSkill = new bool[8];
    }

    public override void InstructionForUse()
    {
        // Verifica se o estado atual permite casting
        if (playerManager != null && playerManager.playerStateMachine != null)
        {
            if (playerManager.playerStateMachine.currentState != null)
            {
                if (!playerManager.playerStateMachine.currentState.CanCasting)
                    return; // Estado atual não permite usar skills
            }
        }

        // Skills por teclas (Q,W,E,R,A,S,D,F)
        KeyCode[] keys = new KeyCode[] {
            KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R,
            KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F
        };

        for (int i = 0; i < keys.Length; i++)
        {
            if (Input.GetKeyDown(keys[i]))
            {
                // tenta usar slot correspondente
                if (skillSlots != null && i < skillSlots.Length && skillSlots[i] != null)
                {
                    RequestSkillSlot(i);
                }
            }
        }
    }

    public void RequestSkillSlot(int slotIndex)
    {
        // Validações de segurança
        if (slotIndex < 0 || slotIndex >= skillSlots.Length)
        {
            Debug.LogWarning($"[PlayerSkillManager] RequestSkillSlot: slotIndex {slotIndex} fora do range [0-{skillSlots.Length - 1}]");
            return;
        }

        var skillController = skillSlots[slotIndex];
        if (skillController == null)
        {
            Debug.LogWarning($"[PlayerSkillManager] RequestSkillSlot: skillSlots[{slotIndex}] é null");
            return;
        }

        var skillData = skillController.runtimeSkill.Data;
        if (skillData == null)
        {
            Debug.LogWarning($"[PlayerSkillManager] RequestSkillSlot: SkillData em slot {slotIndex} é null");
            return;
        }

        // Verifica se o controller permite execução (cooldown/energia)
        if (!skillController.CanUse())
        {
            Debug.LogWarning($"[PlayerSkillManager] RequestSkillSlot: controller em slot {slotIndex} não pode usar a skill agora: {skillController.GetLastFailureReason()}");
            return;
        }

        // Cria contexto de skill
        SkillContext context = CreateContext(skillSlots[slotIndex]);

        // Tenta executar a skill (a implementação herdada fará TryExecute e disparará eventos de sucesso/falha)
        UseSkill(slotIndex, context);

        // Dispara evento de request após tentativa de execução
        OnSkillRequested?.Invoke(skillData, slotIndex);
    }


    public override SkillContext CreateContext(SkillExecutionController slot)
    {
        SkillContext context = new SkillContext
        {
            Skill = slot.runtimeSkill.Data,
            Caster = playerManager.characterManager,
            SkillLevel = slot.runtimeSkill.GetLevel(),
            Target = playerManager.playerMouseController != null ? playerManager.playerMouseController.GetTargetObject() : null,
            TargetPosition = playerManager.playerMouseController != null ? playerManager.playerMouseController.GetTargetPosition() : Vector3.zero
        };

        return context;   
    }

}