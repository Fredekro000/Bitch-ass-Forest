using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fishingsounds : MonoBehaviour
{
    public List<AudioClip> audioClips = new List<AudioClip>();

    private AudioSource audioSource;
    private int lastPlayedIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(PlaySoundatRandomInterval());
    }

    private IEnumerator PlaySoundatRandomInterval()
    {
        while (true)
        {
            float randomInterval = Random.Range(5f, 10f);
            yield return new WaitForSeconds(randomInterval);
            PlayRandomSound();
        }
    }

    void PlayRandomSound()
    {
        if (audioClips.Count > 0) {
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