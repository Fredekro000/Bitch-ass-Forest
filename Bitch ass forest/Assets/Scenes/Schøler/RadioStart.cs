using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioStart : MonoBehaviour
{
    public AudioSource radioSource;
    
    // Start is called before the first frame update
    void Start()
    {
        radioSource.Play();
        radioSource.Pause();
        radioSource.volume = 0.5f;
    }
}
