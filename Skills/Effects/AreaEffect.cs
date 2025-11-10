using System.Collections.Generic;
using UnityEngine;

// Efeito de área (aplica outros efeitos em área)
// TODOS OS VALORES são configurados no SkillData, não aqui!
[CreateAssetMenu(fileName = "New Area Effect", menuName = "Skills/Effects/Area Effect")]
public class SkillAreaEffect : SkillEffect
{
    [Header("Configuração")]
    [Tooltip("Layer mask de detecção")]
    public LayerMask detectionLayers = ~0;
    
    [Tooltip("Efeitos aplicados em cada alvo dentro da área")]
    public List<SkillEffect> effectsPerTarget = new List<SkillEffect>();
    
    [Header("Visual")]
    [Tooltip("Prefab de efeito visual (opcional)")]
    public GameObject vfxPrefab;
    
    [Tooltip("Duração do VFX")]
    public float vfxDuration = 1f;
    
    [Header("Info")]
    [Tooltip("Configure valores de área no SkillData:")]
    [TextArea(3, 5)]
    public string info = "Configure no SkillData:\n- areaRadius\n- areaShape\n- coneAngle\n- forwardOffset\n- targetFilter\n- maxTargets";

    public override SkillEffectResult Execute(SkillContext context)
    {
        var result = new SkillEffectResult();

        if (!CanExecute(context))
        {
            result.Success = false;
            result.ErrorMessage = "Validação falhou";
            return result;
        }

        if (context.SkillData == null)
        {
            LogError("SkillData não encontrado no contexto!");
            result.Success = false;
            result.ErrorMessage = "SkillData é null";
            return result;
        }

        // Determina centro da área
        Vector3 areaCenter = DetermineAreaCenter(context);

        // Encontra alvos na área
        List<CharacterManager> targets = FindTargetsInArea(areaCenter, context);

        if (targets.Count == 0)
        {
            Log("Nenhum alvo encontrado na área");
            result.Success = true; // Não é erro, só não acertou ninguém
            result.AffectedTargets = new CharacterManager[0];
            return result;
        }

        // Aplica efeitos em cada alvo
        List<CharacterManager> affectedTargets = new List<CharacterManager>();
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

    // Determina centro da área baseado em targeting (lê forwardOffset do SkillData)
    Vector3 DetermineAreaCenter(SkillContext context)
    {
        Vector3 center = context.OriginPosition;
        float forwardOffset = context.SkillData.forwardOffset;

        // Se tem posição alvo, usa ela
        if (context.TargetPosition != Vector3.zero)
        {
            center = context.TargetPosition;
        }
        // Se tem alvo CharacterManager, usa posição dele
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

    // Encontra todos os personagens na área (lê valores do SkillData)
    List<CharacterManager> FindTargetsInArea(Vector3 center, SkillContext context)
    {
        List<CharacterManager> foundTargets = new List<CharacterManager>();
        SkillData skillData = context.SkillData;

        // Lê valores DO SKILLDATA
        float radius = skillData.areaRadius;
        SkillAreaShape shape = skillData.areaShape;
        float coneAngle = skillData.coneAngle;
        int maxTargets = skillData.maxTargets;

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
            var CharacterManager = col.GetComponent<CharacterManager>();
            if (CharacterManager == null) continue;

            // Filtra por team
            if (!IsValidTarget(CharacterManager, context))
                continue;

            // Filtra por ângulo (se cone)
            if (shape == SkillAreaShape.Cone)
            {
                Vector3 dirToTarget = (CharacterManager.transform.position - context.Caster.transform.position).normalized;
                float angle = Vector3.Angle(context.Direction, dirToTarget);
                
                if (angle > coneAngle * 0.5f)
                    continue;
            }

            foundTargets.Add(CharacterManager);

            // Limita número de alvos
            if (maxTargets > 0 && foundTargets.Count >= maxTargets)
                break;
        }

        return foundTargets;
    }

    // Verifica se alvo é válido baseado no filtro (lê do SkillData)
    bool IsValidTarget(CharacterManager target, SkillContext context)
    {
        TargetFilter targetFilter = context.SkillData.targetFilter;

        switch (targetFilter)
        {
            case TargetFilter.Enemies:
                // Compara CharacterManager type (Player vs NPC)
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

    // Gizmos removidos - valores não são mais fixos neste ScriptableObject
    // Para visualizar área, use o SkillData no Inspector
}
