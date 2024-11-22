using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public class RecenterOrigin : MonoBehaviour
{
    public Transform head;
    public Transform origin;
    public Transform target;

    public InputActionProperty recenterButton;

    public void Recenter()
    {
        XROrigin xrOrigin = GetComponent<XROrigin>();
        
        // Preserve the current Y position of the origin
        Vector3 adjustedTargetPosition = new Vector3(target.position.x, head.position.y, target.position.z);
        
        xrOrigin.MoveCameraToWorldLocation(adjustedTargetPosition);
        xrOrigin.MatchOriginUpCameraForward(target.up, target.forward);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (recenterButton.action.WasPressedThisFrame())
        {
            Recenter();
        }
    }
}