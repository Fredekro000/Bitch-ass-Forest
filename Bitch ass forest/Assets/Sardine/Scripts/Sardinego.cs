using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sardinego : MonoBehaviour
{
    public GameObject fish;
    public GameObject swimTarget;
    public float speed;
    private Vector3 startposition;

    private void Start()
    {
        startposition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        fish.transform.position = Vector3.MoveTowards(fish.transform.position, swimTarget.transform.position, speed);
        if (Vector3.Distance(transform.position, swimTarget.transform.position) < 0.2f)
        {
            transform.position = startposition;
        }
    }
}
