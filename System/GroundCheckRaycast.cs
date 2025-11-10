using UnityEngine;
public class GroundCheckRaycast : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float rayDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    private bool isGrounded;

    private void Update()
    {
        // Lança um ray para baixo para detectar o chão
        isGrounded = Physics.Raycast(transform.position, Vector3.down, rayDistance, groundLayer);
    }

    // Desenha o ray na Scene view para debug
    private void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * rayDistance);
    }

    public bool IsGrounded => isGrounded;
}
