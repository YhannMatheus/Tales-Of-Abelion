using UnityEngine;

public class MouseManager : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private LayerMask clickableLayerMask = ~0; // Tudo por padrão
    [SerializeField] private float maxRaycastDistance = 1000f;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    // Retorna a posição 3D no mundo onde o mouse está apontando
    public Vector3 GetMousePosition()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxRaycastDistance, clickableLayerMask))
        {
            return hitInfo.point;
        }

        return Vector3.zero;
    }

    // Retorna o GameObject clicado pelo mouse
    public GameObject GetClickedObject()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxRaycastDistance, clickableLayerMask))
        {
            return hitInfo.collider.gameObject;
        }

        return null;
    }

    // Retorna RaycastHit completo do mouse
    public bool GetMouseHit(out RaycastHit hitInfo)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hitInfo, maxRaycastDistance, clickableLayerMask);
    }

    // Verifica se o mouse está sobre um inimigo (Character com tag Enemy)
    public bool IsMouseOverEnemy(out Character enemyCharacter)
    {
        enemyCharacter = null;
        GameObject clickedObject = GetClickedObject();

        if (clickedObject == null) return false;

        Character character = clickedObject.GetComponent<Character>();
        if (character != null && clickedObject.CompareTag("Enemy"))
        {
            enemyCharacter = character;
            return true;
        }

        return false;
    }

    // Verifica se o mouse está sobre um objeto interativo (Event)
    public bool IsMouseOverInteractable(out Event eventComponent)
    {
        eventComponent = null;
        GameObject clickedObject = GetClickedObject();

        if (clickedObject == null) return false;

        eventComponent = clickedObject.GetComponent<Event>();
        return eventComponent != null;
    }

    public bool CollisionDetected => GetClickedObject() != null;

    // Detecta input de botão direito do mouse
    public bool RightMouseButtonDown => Input.GetMouseButtonDown(1);

    // Detecta se botão direito está pressionado
    public bool RightMouseButtonHeld => Input.GetMouseButton(1);
}