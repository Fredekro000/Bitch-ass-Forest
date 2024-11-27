using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollCheck : MonoBehaviour
{
    public bool inArea = false;

    public GameObject xrOrigin;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider col) {
        // Detect if xrOrigin enters a collider tagged as "Omrade"
        if (col.gameObject == xrOrigin) {
            inArea = true; // Set inArea to true
            Debug.Log("xrOrigin entered the Omrade area.");
        }
        
    }

    void OnTriggerExit(Collider col) {
        // Detect if xrOrigin exits a collider tagged as "Omrade"
       if (col.gameObject == xrOrigin) {
            inArea = false; // Set inArea to false
            Debug.Log("xrOrigin exited the Omrade area.");
        }

    }
}
