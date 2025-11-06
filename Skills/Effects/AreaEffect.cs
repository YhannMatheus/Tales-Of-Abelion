using System.Collections.Generic;
using UnityEngine;

// Efeito de área (aplica outros efeitos em área) - renomeado para SkillAreaEffect
[CreateAssetMenu(fileName = "New Area Effect", menuName = "Skills/Effects/Area Effect")]
public class SkillAreaEffect : SkillEffect
{
    [Header("Configuração de Área")]
    [Tooltip("Raio da área")]
    public float radius = 3f;
    
    [Tooltip("Formato da área")]
    public SkillAreaShape shape = SkillAreaShape.Circle;
    
    [Tooltip("Ângulo do cone (se shape = Cone)")]
    [Range(0f, 360f)]
    public float coneAngle = 90f;
    
    [Tooltip("Distância forward do caster")]
    public float forwardOffset = 0f;
    
    [Header("Targeting")]
    [Tooltip("Filtro de alvos")]
    public TargetFilter targetFilter = TargetFilter.Enemies;
    
    [Tooltip("Layer mask de detecção")]
    public LayerMask detectionLayers = ~0;
    
    [Tooltip("Número máximo de alvos (0 = ilimitado)")]
    public int maxTargets = 0;
    
    [Header("Efeitos Aplicados")]
    [Tooltip("Efeitos aplicados em cada alvo dentro da área")]
    public List<SkillEffect> effectsPerTarget = new List<SkillEffect>();
    
    [Header("Visual")]
    [Tooltip("Prefab de efeito visual (opcional)")]
    public GameObject vfxPrefab;
    
    [Tooltip("Duração do VFX")]
    public float vfxDuration = 1f;

    public override SkillEffectResult Execute(SkillContext context)
    {
        var result = new SkillEffectResult();

        if (!CanExecute(context))
        {
            result.Success = false;
            result.ErrorMessage = "Validação falhou";
            return result;
        }

        // Determina centro da área
        Vector3 areaCenter = DetermineAreaCenter(context);

        // Encontra alvos na área
        List<Character> targets = FindTargetsInArea(areaCenter, context);

        if (targets.Count == 0)
        {
            Log("Nenhum alvo encontrado na área");
            result.Success = true; // Não é erro, só não acertou ninguém
            result.AffectedTargets = new Character[0];
            return result;
        }

        // Aplica efeitos em cada alvo
        List<Character> affectedTargets = new List<Character>();
        float totalDamage = 0f;
        float totalHealing = 0f;

        foreach (var target in targets)
        {
            // Cria novo contexto para este alvo
            var targetContext = context.Clone();
            targetContext.Target = target;
            targetContext.TargetPosition = target.transform.position;

            // Aplica cada efeito
            foreach (var effect in effectsPerTarget)
            {
                if (effect == null) continue;

                var effectResult = effect.Execute(targetContext);
                
                if (effectResult.Success)
                {
                    totalDamage += effectResult.DamageDealt;
                    totalHealing += effectResult.HealingDone;
                }
            }

            affectedTargets.Add(target);
        }

        // Spawna VFX se configurado
        if (vfxPrefab != null)
        {
            SpawnVFX(areaCenter);
        }

        // Preenche resultado
        result.Success = true;
        result.DamageDealt = totalDamage;
        result.HealingDone = totalHealing;
        result.AffectedTargets = affectedTargets.ToArray();

        Log($"Afetou {affectedTargets.Count} alvos com {effectsPerTarget.Count} efeitos");

        return result;
    }

    // Determina centro da área baseado em targeting
    Vector3 DetermineAreaCenter(SkillContext context)
    {
        Vector3 center = context.OriginPosition;

        // Se tem posição alvo, usa ela
        if (context.TargetPosition != Vector3.zero)
        {
            center = context.TargetPosition;
        }
        // Se tem alvo character, usa posição dele
        else if (context.Target != null)
        {
            center = context.Target.transform.position;
        }
        // Senão, usa posição do caster + offset
        else
        {
            center = context.Caster.transform.position;
            if (forwardOffset > 0f)
            {
                center += context.Caster.transform.forward * forwardOffset;
            }
        }

        return center;
    }

    // Encontra todos os personagens na área
    List<Character> FindTargetsInArea(Vector3 center, SkillContext context)
    {
        List<Character> foundTargets = new List<Character>();

        // Overlap baseado na forma
        Collider[] colliders = null;

        switch (shape)
        {
            case SkillAreaShape.Circle:
                colliders = Physics.OverlapSphere(center, radius, detectionLayers);
                break;

            case SkillAreaShape.Box:
                Vector3 halfExtents = new Vector3(radius, radius * 0.5f, radius);
                colliders = Physics.OverlapBox(center, halfExtents, Quaternion.identity, detectionLayers);
                break;

            case SkillAreaShape.Cone:
                // Usa sphere e filtra por ângulo depois
                colliders = Physics.OverlapSphere(center, radius, detectionLayers);
                break;

            default:
                colliders = Physics.OverlapSphere(center, radius, detectionLayers);
                break;
        }

        if (colliders == null || colliders.Length == 0)
            return foundTargets;

        // Processa cada collider
        foreach (var col in colliders)
        {
            var character = col.GetComponent<Character>();
            if (character == null) continue;

            // Filtra por team
            if (!IsValidTarget(character, context))
                continue;

            // Filtra por ângulo (se cone)
            if (shape == SkillAreaShape.Cone)
            {
                Vector3 dirToTarget = (character.transform.position - context.Caster.transform.position).normalized;
                float angle = Vector3.Angle(context.Direction, dirToTarget);
                
                if (angle > coneAngle * 0.5f)
                    continue;
            }

            foundTargets.Add(character);

            // Limita número de alvos
            if (maxTargets > 0 && foundTargets.Count >= maxTargets)
                break;
        }

        return foundTargets;
    }

    // Verifica se alvo é válido baseado no filtro
    bool IsValidTarget(Character target, SkillContext context)
    {
        switch (targetFilter)
        {
            case TargetFilter.Enemies:
                // Compara character type (Player vs NPC)
                return target.characterType != context.Caster.characterType;

            case TargetFilter.Allies:
                // Mesmo tipo mas não é o próprio caster
                return target.characterType == context.Caster.characterType && target != context.Caster;

            case TargetFilter.Self:
                return target == context.Caster;

            case TargetFilter.All:
                return true;

            case TargetFilter.AllExceptSelf:
                return target != context.Caster;

            default:
                return false;
        }
    }

    // Spawna VFX
    void SpawnVFX(Vector3 position)
    {
        GameObject vfx = Instantiate(vfxPrefab, position, Quaternion.identity);
        
        if (vfxDuration > 0f)
        {
            Object.Destroy(vfx, vfxDuration);
        }
    }

    // Gizmos para visualização no editor
    void OnDrawGizmosSelected()
    {
        // Desenha área no editor
        Gizmos.color = Color.yellow;
        
        switch (shape)
        {
            case SkillAreaShape.Circle:
                Gizmos.DrawWireSphere(Vector3.zero, radius);
                break;

            case SkillAreaShape.Box:
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(radius * 2f, radius, radius * 2f));
                break;
        }
    }
}
