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
    // Start is called before the first frame update
    void Start()
    {
        //grabInteractable = GetComponent<XRGrabInteractable>();
        lastRotation = handleTransform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (grabInteractable.isSelected)
        {
            Quaternion currentRotation = transform.rotation;
            float angleDifference = Quaternion.Angle(lastRotation, currentRotation);

            if (angleDifference > 0.1f) // Adjust sensitivity as needed
            {
                fishingRod.StartReeling();
            }

            lastRotation = currentRotation;
        }
    }
}
