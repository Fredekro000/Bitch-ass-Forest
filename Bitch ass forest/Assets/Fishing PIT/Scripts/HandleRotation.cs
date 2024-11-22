using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HandleRotation : MonoBehaviour
{
    public XRGrabInteractable grabInteractable;
    public Transform handleTransform;
    private Quaternion lastRotation;
    public FishingRod fishingRod;
    
    [SerializeField] private Transform proxyHandle;
    private Vector3 originalLocalPosition;
    
    // Start is called before the first frame update
    void Start()
    {
        //grabInteractable = GetComponent<XRGrabInteractable>();
        //lastRotation = handleTransform.rotation;
        originalLocalPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = originalLocalPosition;

        // Synchronize the rotation of the handle with the proxy handle
        transform.localRotation = proxyHandle.localRotation;
        /*
        if (grabInteractable.isSelected)
        {
            Quaternion currentRotation = transform.rotation;
            float angleDifference = Quaternion.Angle(lastRotation, currentRotation);

            if (angleDifference > 0.1f) // Adjust sensitivity as needed
            {
                fishingRod.StartReeling();
            }

            lastRotation = currentRotation;
        }*/
    }
}
