using System;
using UnityEngine;

public class IADetectSystem : MonoBehaviour
{
    public Transform currentTarget { get; private set; }
    private bool wasTargetVisible;

    public event Action<Transform> OnDetectTarget;
    public event Action OnLoseTarget;

    public bool CanSeeTarget(IAManager ia)
    {
        if (ia == null) return false;

        Vector3 eyePosition = ia.transform.position + ia.eyePositionOffset;

        int targetMask = LayerMaskToInt(ia.targetLayerMask);
        Collider[] targetsInRadius = Physics.OverlapSphere(eyePosition, ia.visionArea, targetMask);

        bool canSeeNow = false;
        Transform detectedTarget = null;

        foreach (Collider col in targetsInRadius)
        {
            Transform t = col.transform;
            Vector3 directionToTarget = (t.position - eyePosition).normalized;
            float angleToTarget = Vector3.Angle(ia.transform.forward, directionToTarget);

            if (angleToTarget <= ia.detectionAngle * 0.5f)
            {
                float distanceToTarget = Vector3.Distance(eyePosition, t.position);
                int obstructionMask = LayerMaskToInt(ia.obstructionLayerMask);

                if (!Physics.Raycast(eyePosition, directionToTarget, distanceToTarget, obstructionMask))
                {
                    canSeeNow = true;
                    detectedTarget = t;
                    ia.lastKnownTargetPosition = t;
                    break;
                }
            }
        }

        HandleDetectionEvents(ia, canSeeNow, detectedTarget);
        return canSeeNow;
    }

    private void HandleDetectionEvents(IAManager ia, bool canSeeNow, Transform detectedTarget)
    {
        if (canSeeNow && !wasTargetVisible)
        {
            currentTarget = detectedTarget;
            wasTargetVisible = true;
            OnDetectTarget?.Invoke(detectedTarget);
            Debug.Log($"[IADetect] detectou: {detectedTarget.name}");
        }
        else if (!canSeeNow && wasTargetVisible)
        {
            wasTargetVisible = false;
            currentTarget = null;
            OnLoseTarget?.Invoke();
            Debug.Log("[IADetect] perdeu alvo");
        }
    }

    private int LayerMaskToInt(LayerMask[] masks)
    {
        if (masks == null || masks.Length == 0) return 0;
        int result = 0;
        foreach (var m in masks) result |= m.value;
        return result;
    }

    private void OnDrawGizmosSelected()
    {
        IAManager ia = GetComponent<IAManager>();
        if (ia == null) return;

        Vector3 eye = transform.position + ia.eyePositionOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eye, ia.visionArea);
    }
}