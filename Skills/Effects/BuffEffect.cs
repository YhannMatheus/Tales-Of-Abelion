using UnityEngine;

// Efeito de buff/debuff
[CreateAssetMenu(fileName = "New Buff Effect", menuName = "Skills/Effects/Buff Effect")]
public class BuffEffect : SkillEffect
{
    [Header("Configuração do Buff")]
    [Tooltip("Dados do buff a ser aplicado")]
    public BuffData buffData;
    
    [Tooltip("Duração do buff em segundos")]
    public float duration = 5f;
    
    [Tooltip("Pode empilhar (stack)")]
    public bool canStack = false;
    
    [Tooltip("Máximo de stacks")]
    public int maxStacks = 1;
    
    [Header("Targeting")]
    [Tooltip("Aplica no caster ao invés do alvo")]
    public bool applyToSelf = false;

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

        // Determina alvo do buff
        Character target = applyToSelf ? context.Caster : context.Target;

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

        // Aplica usando sistema apropriado
        if (canStack)
        {
            // TODO: Implementar ApplyBuffToSlot quando BuffManager tiver o método
            // Por enquanto, usa ApplyBuff padrão
            buffManager.ApplyBuff(buffData);
            Debug.LogWarning($"[BuffEffect] Stacking não suportado ainda, aplicou buff normal");
        }
        else
        {
            buffManager.ApplyBuff(buffData);
        }

        // Preenche resultado
        result.Success = true;
        result.AffectedTargets = new Character[] { target };

        Log($"Aplicou buff '{buffData.buffName}' em {target.Data.characterName} por {duration}s");

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
