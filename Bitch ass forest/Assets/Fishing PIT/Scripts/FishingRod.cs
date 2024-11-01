using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingRod : MonoBehaviour
{
    public Transform rodTip;
    public Transform lure;
    public LineRenderer lineR;

    private Rigidbody lureRb;

    public float castForce = 10f;
    
    // Start is called before the first frame update
    void Start()
    {
        lureRb = lure.GetComponent<Rigidbody>();
        lineR.positionCount = 2;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLineRenderer();

        if (Input.GetButtonDown("Fire1"))
        {
            CastLure();
        }
    }

    void UpdateLineRenderer()
    {
        lineR.SetPosition(0, rodTip.position);
        lineR.SetPosition(1, lure.position);
    }

    void CastLure()
    {
        Rigidbody lureRb = lure.GetComponent<Rigidbody>();
        this.lureRb.isKinematic = false;
        lureRb.AddForce(transform.forward * castForce, ForceMode.Impulse);
    }
}
