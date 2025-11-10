using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Configurações")]
    public Transform Target;
    public Transform LookAt;
    public Vector3 RelativePosition;


    void Awake()
    {
        if (Target != null)
        {
            transform.position = Target.position + RelativePosition;
        }
    }

    void LateUpdate()
    {
        // segue o alvo
        if (Target != null)
        {
            transform.position = Target.position + RelativePosition;
        }

        // olhe para o LookAt se definido
        if (LookAt != null)
        {
            transform.LookAt(LookAt);
        }
    }

}
