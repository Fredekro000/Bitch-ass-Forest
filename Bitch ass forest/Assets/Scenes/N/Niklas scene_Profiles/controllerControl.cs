using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controllerControl : MonoBehaviour
{
    public GameObject Controllercontrol;
    public GameObject XRControl;

    private bool active = false; // Initialize to false for clarity

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("x") && active)
        {
            Controllercontrol.SetActive(false);
            XRControl.SetActive(true);
            active = false;
        }

        if (Input.GetKey("c") && !active)
        {
            Controllercontrol.SetActive(true);
            XRControl.SetActive(false);
            active = true;
        }
    }
}