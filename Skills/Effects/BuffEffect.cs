using UnityEngine;
using System.Collections.Generic;

// Efeito de buff/debuff
// DURAÇÃO E VALORES são configurados no SkillData, não aqui!
[CreateAssetMenu(fileName = "New Buff Effect", menuName = "Skills/Effects/Buff Effect")]
public class BuffEffect : SkillEffect
{
    [Header("Configuração do Buff")]
    [Tooltip("Dados base do buff (visual, nome, tipo)")]
    public BuffData buffData;
    
    [Header("Targeting")]
    [Tooltip("Aplica no caster ao invés do alvo")]
    public bool applyToSelf = false;
    
    [Header("Info")]
    [Tooltip("Duração e modificadores são configurados no SkillData")]
    [TextArea(2, 3)]
    public string info = "Configure:\n- buffDuration\n- buffModifiers\n- distributeBuffOverTime\nno SkillData!";

    public override SkillEffectResult Execute(SkillContext context)
    {
        var result = new SkillEffectResult();

        if (!CanExecute(context))
        {
            result.Success = false;
            result.ErrorMessage = "Validação falhou";
            return result;
        }

        if (buffData == null)
        {
            LogError("BuffData não configurado!");
            result.Success = false;
            result.ErrorMessage = "BuffData é null";
            return result;
        }

        if (context.SkillData == null)
        {
            LogError("SkillData não encontrado no contexto!");
            result.Success = false;
            result.ErrorMessage = "SkillData é null";
            return result;
        }

        // Determina alvo do buff
        CharacterManager target = applyToSelf ? context.Caster : context.Target;

        if (target == null)
        {
            LogWarning("Alvo para buff é null");
            result.Success = false;
            result.ErrorMessage = "Sem alvo";
            return result;
        }

        // Aplica buff
        var buffManager = target.GetComponent<BuffManager>();
        
        if (buffManager == null)
        {
            LogError($"Alvo {target.Data.characterName} não tem BuffManager!");
            result.Success = false;
            result.ErrorMessage = "BuffManager não encontrado";
            return result;
        }

        // Pega valores configurados NO SKILLDATA
        SkillData skillData = context.SkillData;
        string buffName = buffData.buffName;
        bool isDebuff = buffData.isDebuff;
        float duration = skillData.buffDuration;
        List<Modifier> modifiers = skillData.buffModifiers;
        bool distributeOverTime = skillData.distributeBuffOverTime;
        float tickInterval = skillData.buffTickInterval;

        // Valida se skill tem configuração de buff
        if (duration <= 0f)
        {
            LogWarning($"Skill '{skillData.skillName}' não tem buffDuration configurado!");
            result.Success = false;
            result.ErrorMessage = "buffDuration não configurado no SkillData";
            return result;
        }

        if (modifiers == null || modifiers.Count == 0)
        {
            LogWarning($"Skill '{skillData.skillName}' não tem buffModifiers configurados!");
            result.Success = false;
            result.ErrorMessage = "buffModifiers não configurado no SkillData";
            return result;
        }

        // Aplica buff com valores configurados NO SKILLDATA
        buffManager.ApplyBuff(
            buffName, 
            duration, 
            modifiers, 
            isDebuff, 
            distributeOverTime, 
            tickInterval
        );

        // Preenche resultado
        result.Success = true;
        result.AffectedTargets = new CharacterManager[] { target };

        Log($"Aplicou buff '{buffName}' em {target.Data.characterName} por {duration}s com {modifiers.Count} modificadores");

        return result;
    }

    public override bool CanExecute(SkillContext context)
    {
        if (!base.CanExecute(context))
            return false;

        // Se aplica no alvo, precisa ter alvo
        if (!applyToSelf && context.Target == null)
        {
            LogWarning("BuffEffect precisa de alvo (ou marcar applyToSelf = true)");
            return false;
        }

        return true;
    }
}
