using UnityEngine;

// Contexto de execução de uma skill (passa informações entre efeitos)
public class SkillContext
{
    // Quem usou a skill
    public CharacterManager Caster { get; set; }
    
    // Alvo principal (pode ser null)
    public CharacterManager Target { get; set; }
    
    // Posição de origem
    public Vector3 OriginPosition { get; set; }
    
    // Posição alvo
    public Vector3 TargetPosition { get; set; }
    
    // Direção da skill
    public Vector3 Direction { get; set; }
    
    // Dados da skill sendo executada
    public SkillData SkillData { get; set; }
    
    // Nível atual da skill
    public int SkillLevel { get; set; }
    
    // Informações adicionais (pode ser usado por efeitos custom)
    public object CustomData { get; set; }
    
    // Resultado do último efeito (útil para chains)
    public SkillEffectResult LastResult { get; set; }

    // Construtor básico
    public SkillContext()
    {
        SkillLevel = 1;
    }

    // Construtor com caster
    public SkillContext(CharacterManager caster)
    {
        Caster = caster;
        OriginPosition = caster.transform.position;
        Direction = caster.transform.forward;
        SkillLevel = 1;
    }

    // Construtor com caster e alvo
    public SkillContext(CharacterManager caster, CharacterManager target)
    {
        Caster = caster;
        Target = target;
        OriginPosition = caster.transform.position;
        TargetPosition = target.transform.position;
        Direction = (target.transform.position - caster.transform.position).normalized;
        SkillLevel = 1;
    }

    // Construtor com caster e posição
    public SkillContext(CharacterManager caster, Vector3 targetPosition)
    {
        Caster = caster;
        OriginPosition = caster.transform.position;
        TargetPosition = targetPosition;
        Direction = (targetPosition - caster.transform.position).normalized;
        SkillLevel = 1;
    }

    // Clone do contexto (útil para efeitos nested)
    public SkillContext Clone()
    {
        return new SkillContext
        {
            Caster = this.Caster,
            Target = this.Target,
            OriginPosition = this.OriginPosition,
            TargetPosition = this.TargetPosition,
            Direction = this.Direction,
            SkillData = this.SkillData,
            SkillLevel = this.SkillLevel,
            CustomData = this.CustomData,
            LastResult = this.LastResult
        };
    }
}

// Resultado da execução de um efeito
public class SkillEffectResult
{
    // Sucesso ou falha
    public bool Success { get; set; }
    
    // Dano causado (se aplicável)
    public float DamageDealt { get; set; }
    
    // Cura realizada (se aplicável)
    public float HealingDone { get; set; }
    
    // Alvos afetados
    public CharacterManager[] AffectedTargets { get; set; }
    
    // Mensagem de erro (se falhou)
    public string ErrorMessage { get; set; }
    
    // Dados adicionais
    public object AdditionalData { get; set; }

    public SkillEffectResult()
    {
        Success = true;
        AffectedTargets = new CharacterManager[0];
    }
}
