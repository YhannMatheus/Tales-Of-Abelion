using UnityEngine;

public abstract class Event : MonoBehaviour
{
    [Header("Visual Feedback")]
    private Renderer objectRenderer;
    private Material originalMaterial;
    public Material outlineMaterial;

    [Header("Interaction")] 
    public float minDistanceToTrigger = 3f;
    public bool showDistanceGizmo = true;

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
        }
    }

    private void OnMouseEnter()
    {
        if (objectRenderer != null && outlineMaterial != null)
        {
            objectRenderer.material = outlineMaterial;
        }
    }

    private void OnMouseExit()
    {
        if (objectRenderer != null && originalMaterial != null)
        {
            objectRenderer.material = originalMaterial;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (showDistanceGizmo)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minDistanceToTrigger);
        }
    }

    public abstract void OnClick();
}