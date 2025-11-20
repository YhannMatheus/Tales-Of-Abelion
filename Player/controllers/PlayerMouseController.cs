using System.Collections;
using UnityEngine;
using System;

[System.Serializable]
public class PlayerMouseController
{
    private GameObject targetObject;
    private Vector3 targetPosition;

    private Camera mainCamera;

    public void Initialize()
    {
        mainCamera = Camera.main;
        targetObject = null;
        targetPosition = Vector3.zero;
    }

    public void UpdateMouseTarget()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {

            targetObject = hit.collider.gameObject.GetComponent<CharacterManager>() != null ? hit.collider.gameObject : null;
            targetPosition = hit.point;
        }
    }

    public GameObject GetTargetObject()
    {
        if (targetObject == null) return null;

        if(targetObject.GetComponent<CharacterManager>() != null) return null;

        return targetObject;
    }
    
    public Vector3 GetTargetPosition()
    {
        return targetPosition;
    }
}