using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
public class GhostHandleFollower : MonoBehaviour
{
    public Transform handle;
    //private Transform originalParent;
    private Quaternion initialRotationOffset;
    
    // Start is called before the first frame update
    void Start()
    {
        //originalParent = handle.parent;
        initialRotationOffset = Quaternion.Inverse(transform.rotation) * handle.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void LateUpdate()
    {
        // Update the ghost object's position and rotation to match the handle
        //transform.position = handle.position;
        //transform.rotation = handle.rotation;
        handle.rotation = transform.rotation * initialRotationOffset;
    }

    public void OnGrab(SelectEnterEventArgs args)
    {
        // Reparent the handle to the ghost object when grabbed
        //handle.SetParent(transform);
        
    }

    public void OnRelease(SelectExitEventArgs args)
    {
        // Reparent the handle back to its original parent when released
        //handle.SetParent(originalParent);
    }
}
