using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingLogic : MonoBehaviour
{
    public Transform lure;

    public Transform fish;
    public float attachDistance = 1.0f;

    private bool isFishAttached = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFishAttached && IsLureOnWater())
        {
            AttachFishToLure();
        }
    }

    bool IsLureOnWater()
    {
        RaycastHit hit;
        if (Physics.Raycast(lure.position, Vector3.down, out hit, attachDistance))
        {
            if (hit.transform.CompareTag("Water"))
            {
                return true;
            }
        }

        return false;
    }

    void AttachFishToLure()
    {
        isFishAttached = true;
        fish.position = lure.position;
        fish.SetParent(lure);
        fish.GetComponent<Rigidbody>().isKinematic = true;
    }
}
