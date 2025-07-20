using System;
using System.Collections;
using System.Collections.Generic;
using CMF;
using UnityEngine;
using UnityEngine.EventSystems;

public class CamInput : MonoBehaviour, IDragHandler
{
    public CameraController cam;
    public bool locked;
    public float maxInput;

    public void OnDrag(PointerEventData eventData)
    {
        if(locked)
            return;
        
        Vector2 movement = eventData.delta;
        movement = new Vector2(Mathf.Clamp(movement.x / maxInput, -1, 1), Mathf.Clamp(movement.y / maxInput, -1, 1));
        cam.RotateCamera(movement.x, -movement.y);
    }
    
}
