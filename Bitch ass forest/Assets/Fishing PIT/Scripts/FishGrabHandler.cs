using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FishGrabHandler : MonoBehaviour
{
    private XRGrabInteractable fishGrabInteractable;
    private FishingRod fishingRod;
    private FishingLogic fishingLogic;
    private GameObject ghostFishObject;
    
    void Awake()
    {
        fishGrabInteractable = GetComponent<XRGrabInteractable>();
        if (fishGrabInteractable == null)
        {
            Debug.LogError("XRGrabInteractable component not found on " + gameObject.name);
        }

        fishingRod = FindObjectOfType<FishingRod>();
        if (fishingRod == null)
        {
            Debug.LogError("FishingRod component not found in the scene.");
        }

        fishingLogic = FindObjectOfType<FishingLogic>();
        if (fishingLogic == null)
        {
            Debug.LogError("FishingLogic component not found in the scene.");
        }
        
        ghostFishObject = new GameObject("GhostObject");
        ghostFishObject.transform.position = transform.position;
        ghostFishObject.transform.rotation = transform.rotation;
    }

    void OnEnable()
    {
        if (fishGrabInteractable != null)
        {
            fishGrabInteractable.selectEntered.AddListener(OnGrab);
            fishGrabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    void OnDisable()
    {
        if (fishGrabInteractable != null)
        {
            fishGrabInteractable.selectEntered.RemoveListener(OnGrab);
            fishGrabInteractable.selectExited.RemoveListener(OnRelease);
        }
    }
    
    void LateUpdate()
    {
        // Update the ghost object's position and rotation to match the original object
        if (ghostFishObject != null)
        {
            ghostFishObject.transform.position = transform.position;
            ghostFishObject.transform.rotation = transform.rotation;
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (fishingLogic.isFishAttached)
        {
            Debug.Log("Remove fish from lure logic");
            fishingLogic.RemoveFishFromLure();
        }
    }
    private void OnRelease(SelectExitEventArgs args)
    {
        // Ensure the fish is not a child of the interactor and is not kinematic
        transform.SetParent(null);
        GetComponent<Rigidbody>().isKinematic = false;
    }
}

