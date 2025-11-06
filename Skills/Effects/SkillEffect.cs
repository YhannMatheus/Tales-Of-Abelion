using UnityEngine;

// Classe base abstrata para todos os efeitos de skills
public abstract class SkillEffect : ScriptableObject
{
    [Header("Informações do Efeito")]
    [Tooltip("Nome do efeito (para debug)")]
    public string effectName = "Novo Efeito";
    
    [Tooltip("Delay antes de executar (segundos)")]
    public float executionDelay = 0f;
    
    [Tooltip("Pode ser cancelado durante execução")]
    public bool isCancelable = true;

    // Executa o efeito (implementado por classes filhas)
    public abstract SkillEffectResult Execute(SkillContext context);

    // Clona o efeito (necessário para instanciar)
    public virtual SkillEffect Clone()
    {
        return Instantiate(this);
    }

    // Callback quando efeito é cancelado
    public virtual void OnCancelled(SkillContext context)
    {
        // Override em classes filhas se necessário
    }

    // Validação antes de executar
    public virtual bool CanExecute(SkillContext context)
    {
        if (context == null)
        {
            Debug.LogWarning($"[{effectName}] Contexto é null!");
            return false;
        }

        if (context.Caster == null)
        {
            Debug.LogWarning($"[{effectName}] Caster é null!");
            return false;
        }

        return true;
    }

    // Log helper
    protected void Log(string message)
    {
        Debug.Log($"[{effectName}] {message}");
    }

    protected void LogWarning(string message)
    {
        Debug.LogWarning($"[{effectName}] {message}");
    }

    protected void LogError(string message)
    {
        Debug.LogError($"[{effectName}] {message}");
    }
}
