using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Transform lookAt;
    public Vector3 relativePosition;

    void Awake()
    {
        if (target != null)
        {
            transform.position = target.position + relativePosition;
        }
    }
    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + relativePosition;
        }

        if (lookAt != null)
        {
            transform.LookAt(lookAt);
        }
    }
}
