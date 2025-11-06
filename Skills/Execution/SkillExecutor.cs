using System.Collections;
using UnityEngine;

// Executor de skill (runtime, MonoBehaviour)
public class SkillExecutor : MonoBehaviour
{
    // Dados da skill sendo executada
    public SkillData SkillData { get; private set; }
    
    // Contexto de execução
    public SkillContext Context { get; private set; }
    
    // Estado
    public bool IsExecuting { get; private set; }
    public bool IsCasting { get; private set; }
    public bool WasCancelled { get; private set; }
    
    // Referência ao caster
    Character caster;
    
    // Coroutine de execução
    Coroutine executionCoroutine;

    // Inicializa executor
    public void Initialize(SkillData skillData, SkillContext context)
    {
        SkillData = skillData;
        Context = context;
        caster = context.Caster;
        IsExecuting = false;
        IsCasting = false;
        WasCancelled = false;
    }

    // Executa a skill
    public void Execute()
    {
        if (IsExecuting)
        {
            Debug.LogWarning("[SkillExecutor] Skill já está sendo executada!");
            return;
        }

        executionCoroutine = StartCoroutine(ExecutionCoroutine());
    }

    // Cancela execução
    public void Cancel()
    {
        if (!IsExecuting)
            return;

        WasCancelled = true;
        
        if (executionCoroutine != null)
        {
            StopCoroutine(executionCoroutine);
        }

        // Notifica efeitos
        foreach (var effect in SkillData.effects)
        {
            if (effect != null)
            {
                effect.OnCancelled(Context);
            }
        }

        Cleanup();
    }

    // Coroutine de execução
    IEnumerator ExecutionCoroutine()
    {
        IsExecuting = true;

        // Toca animação se configurada
        PlayAnimation();

        // Spawna VFX do caster se configurado
        if (SkillData.casterVFXPrefab != null)
        {
            Instantiate(SkillData.casterVFXPrefab, caster.transform.position, Quaternion.identity);
        }

        // Toca som se configurado
        if (SkillData.castSound != null)
        {
            AudioSource.PlayClipAtPoint(SkillData.castSound, caster.transform.position, SkillData.soundVolume);
        }

        // Cast time
        if (SkillData.castTime > 0f)
        {
            IsCasting = true;
            float castTimer = 0f;

            while (castTimer < SkillData.castTime)
            {
                castTimer += Time.deltaTime;
                yield return null;

                // Verifica cancelamento por movimento
                // TODO: Implementar quando Character tiver acesso ao Motor
                /*
                if (SkillData.canceledByMovement && caster.Motor != null && caster.Motor.IsMoving)
                {
                    Debug.Log("[SkillExecutor] Skill cancelada por movimento");
                    Cancel();
                    yield break;
                }
                */
            }

            IsCasting = false;
        }

        // Executa todos os efeitos
        foreach (var effect in SkillData.effects)
        {
            if (effect == null)
                continue;

            // Aplica delay do efeito
            if (effect.executionDelay > 0f)
            {
                yield return new WaitForSeconds(effect.executionDelay);
            }

            // Executa efeito
            var result = effect.Execute(Context);

            // Armazena resultado no contexto
            Context.LastResult = result;

            // Se falhou e é crítico, cancela skill
            if (!result.Success)
            {
                Debug.LogWarning($"[SkillExecutor] Efeito '{effect.effectName}' falhou: {result.ErrorMessage}");
            }
        }

        // Finaliza
        Cleanup();
    }

    // Toca animação
    void PlayAnimation()
    {
        var animatorController = caster.GetComponent<IAAnimatorController>();
        
        if (animatorController == null)
            return;

        // Usa trigger de ability
        if (SkillData.usesAbilityTrigger)
        {
            // TODO: Implementar quando IAAnimatorController tiver TriggerAbility()
            // Por enquanto, define como usando habilidade
            Debug.Log($"[SkillExecutor] Tocaria animação de habilidade {SkillData.abilityIndex}");
        }
        // Ou toca animação específica
        else if (!string.IsNullOrEmpty(SkillData.animationName))
        {
            // TODO: Implementar quando AnimatorController suportar triggers nomeados
            Debug.LogWarning($"[SkillExecutor] animationName '{SkillData.animationName}' não implementado ainda");
        }
    }

    // Limpeza
    void Cleanup()
    {
        IsExecuting = false;
        IsCasting = false;
        Destroy(gameObject);
    }
}
