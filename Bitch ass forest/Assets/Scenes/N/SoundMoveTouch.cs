using UnityEngine;
using System.Collections.Generic;

public class PlayRandomSoundOnMove : MonoBehaviour
{
    public List<AudioClip> audioClips = new List<AudioClip>(); 
    public GameObject cameraWalk; 

    private AudioSource audioSource;
    private Vector3 lastPosition; 
    private float distanceThreshold = 0.5f; 
    private int lastPlayedIndex = -1;

    void Start()
    {
        
        audioSource = GetComponent<AudioSource>();
        
        lastPosition = cameraWalk.transform.position;
    }

    void Update()
    {
        // Calculate the distance moved since the last sound was played
        float distanceMoved = Vector3.Distance(cameraWalk.transform.position, lastPosition);

        // Check if the object has moved more than or equal to the distance threshold
        if (distanceMoved >= distanceThreshold)
        {
            PlayRandomSound(); // Play a random sound from the list
            lastPosition = cameraWalk.transform.position; // Update the last position
        }
    }

    void PlayRandomSound()
    {
        if (audioClips.Count > 0)
        {
            int randomIndex;
            do
            {
                // Select a random index within the bounds of the audio clips list
                randomIndex = Random.Range(0, audioClips.Count);
            }
            while (randomIndex == lastPlayedIndex); // Repeat until a different index is selected

            // Play the selected audio clip
            audioSource.clip = audioClips[randomIndex];
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.Play();

            // Update lastPlayedIndex to the current one
            lastPlayedIndex = randomIndex;
        }
    }
}