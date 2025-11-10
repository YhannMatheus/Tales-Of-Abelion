using UnityEngine;

// Gerenciador de input do mouse - gerenciado pelo PlayerManager
// NÃO usar independentemente - requer inicialização via Initialize()
public class MouseManager : MonoBehaviour
{
    // Configurações injetadas pelo PlayerManager
    private LayerMask clickableLayerMask = ~0;
    private float maxRaycastDistance = 1000f;

    private Camera cam;

    // Inicializado pelo PlayerManager
    public void Initialize()
    {
        cam = Camera.main;
        
        if (cam == null)
        {
            Debug.LogError("[MouseManager] Camera.main não encontrada! Certifique-se de que há uma câmera com tag MainCamera.");
        }
    }
    
    // Configurações injetadas pelo PlayerManager
    public void ConfigureSettings(LayerMask layerMask, float maxDistance)
    {
        clickableLayerMask = layerMask;
        maxRaycastDistance = maxDistance;
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

    // Verifica se o mouse está sobre um inimigo (CharacterManager com tag Enemy)
    public bool IsMouseOverEnemy(out CharacterManager enemyCharacter)
    {
        enemyCharacter = null;
        GameObject clickedObject = GetClickedObject();

        if (clickedObject == null) return false;

        CharacterManager CharacterManager = clickedObject.GetComponent<CharacterManager>();
        if (CharacterManager != null && clickedObject.CompareTag("Enemy"))
        {
            enemyCharacter = CharacterManager;
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