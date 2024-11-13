using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Redstag : MonoBehaviour
{
    public AudioClip deerSound; // Assign this in the Inspector
    public float minTimeBetweenSounds = 50f; // Minimum time between sounds
    public float maxTimeBetweenSounds = 150f; // Maximum time between sounds

    private AudioSource audioSource;
    private float timeToNextSound;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = deerSound;
        SetRandomTimeForNextSound();
        StartCoroutine(PlaySoundAtIntervals());
    }

    private void SetRandomTimeForNextSound()
    {
        timeToNextSound = Random.Range(minTimeBetweenSounds, maxTimeBetweenSounds);
    }

    private IEnumerator PlaySoundAtIntervals()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeToNextSound);
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
            SetRandomTimeForNextSound();
        }
    }
}
