using System;
using JetBrains.Annotations;
using UnityEngine;

public class PlayerSkillManager : SkillManager
{
    public PlayerManager playerManager;
    public CharacterSkillTreeController characterSkillTreeController;
    public event Action<SkillData, int?> OnSkillRequested;

    public bool[] inputsSkill  = new bool[8];
    
    void Awake()
    {
        // Garantir que temos referência ao PlayerManager antes de usar
        playerManager = GetComponent<PlayerManager>();

        // Se controller de skill tree não foi fornecido via inspector, instancia um novo
        if (characterSkillTreeController == null)
            characterSkillTreeController = new CharacterSkillTreeController();

        // Atribui o CharacterManager da player sheet (pode ser nulo se playerManager não existir)
        characterSkillTreeController.characterManager = playerManager != null ? playerManager.sheet : null;

        inputsSkill = new bool[] { 
            InputManager.Instance.ability1Input,
            InputManager.Instance.ability2Input,
            InputManager.Instance.ability3Input,
            InputManager.Instance.ability4Input,
            InputManager.Instance.ability5Input,
            InputManager.Instance.ability6Input,
            InputManager.Instance.ability7Input,
            InputManager.Instance.ability8Input
        };
    }

    public override void InstructionForUse()
    {
        // Verifica se o estado atual permite casting
        if (playerManager != null && playerManager._playerStateMachine != null)
        {
            if (playerManager._playerStateMachine.currentState != null)
            {
                if (!playerManager._playerStateMachine.currentState.CanCasting)
                    return; // Estado atual não permite usar skills
            }
        }

        if (InputManager.Instance.attackInput)
        {
            // Só executa basic attack se existir controller e puder usar
            if (basicAttackSkill != null && basicAttackSkill.Data != null)
            {
                if (basicAttackSkill.CanUse())
                {
                    ExecuteBasicAttack(CreateContext(basicAttackSkill.Data));
                    OnSkillRequested?.Invoke(basicAttackSkill.Data, null);
                }
                else
                {
                    Debug.LogWarning($"[PlayerSkillManager] InstructionForUse: basic attack não pode ser usado agora: {basicAttackSkill.GetLastFailureReason()}");
                }
            }
        }

        for (int i = 0; i < inputsSkill.Length; i++)
        {
            if (inputsSkill[i] && skillSlots.Length > i && skillSlots[i] != null)
            {
                RequestSkillSlot(i);
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

        var skillData = skillController.Data;
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
        SkillContext context = CreateContext(skillData);

        // Tenta executar a skill (a implementação herdada fará TryExecute e disparará eventos de sucesso/falha)
        UseSkill(slotIndex, context);

        // Dispara evento de request após tentativa de execução
        OnSkillRequested?.Invoke(skillData, slotIndex);
    }

    /// Cria um contexto de skill com base em SkillData fornecido diretamente
    public SkillContext CreateContext(SkillData skillData)
    {
        if (skillData == null)
        {
            Debug.LogWarning("[PlayerSkillManager] CreateContext: SkillData é null");
            return default(SkillContext);
        }

        // Obter alvo do mouse (com fallback seguro)
        Vector3 targetPos = Vector3.zero;
        CharacterManager targetChar = null;
        
        if (playerManager?._playerMouseController != null)
        {
            targetPos = playerManager._playerMouseController.GetTargetPosition();
            var targetObj = playerManager._playerMouseController.GetTargetObject();
            if (targetObj != null)
            {
                targetChar = targetObj.GetComponent<CharacterManager>();
            }
        }

        // Obter nível da skill (com fallback para 1)
        int skillLevel = 1;
        if (characterSkillTreeController != null)
        {
            skillLevel = characterSkillTreeController.GetSkillLevel(skillData);
        }

        SkillContext context = new SkillContext(
            skill: skillData,
            caster: character,
            targetPos: targetPos,
            targetCharacter: targetChar,
            skillLevel: skillLevel
        );

        return context;
    }

    public SkillContext CreateContext(int skillSlot)
    {
        // Validações de segurança
        if (skillSlots == null)
        {
            Debug.LogWarning("[PlayerSkillManager] CreateContext: skillSlots é null");
            return default(SkillContext);
        }

        if (skillSlot < 0 || skillSlot >= skillSlots.Length)
        {
            Debug.LogWarning($"[PlayerSkillManager] CreateContext: skillSlot {skillSlot} fora do range [0-{skillSlots.Length - 1}]");
            return default(SkillContext);
        }

        var skillController = skillSlots[skillSlot];
    
        if (skillController == null)
        {
            Debug.LogWarning($"[PlayerSkillManager] CreateContext: skillSlots[{skillSlot}] é null");
            return default(SkillContext);
        }

        var skillData = skillController.Data;
        if (skillData == null)
        {
            Debug.LogWarning($"[PlayerSkillManager] CreateContext: SkillData em slot {skillSlot} é null");
            return default(SkillContext);
        }

        // Reutiliza a lógica da função principal
        return CreateContext(skillData);
    }

}