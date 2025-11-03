using UnityEngine;

public class MouseManager : MonoBehaviour
{
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    public Vector3 GetMousePosition()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            return hitInfo.point;
        }

        return Vector3.zero;
    }

    public GameObject GetClickedObject()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            return hitInfo.collider.gameObject;
        }

        return null;
    }

    public bool CollisionDetected => GetClickedObject() != null;
}