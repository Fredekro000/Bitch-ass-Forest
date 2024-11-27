using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioStart : MonoBehaviour
{
    public AudioClip[] audioClips; // Array to hold audio clips
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioClips.Length > 0)
        {
            StartCoroutine(PlaySequentialAudio());
        }
        else
        {
            Debug.LogWarning("No audio clips assigned to the SequentialAudioPlayer!");
        }
    }

    private IEnumerator PlaySequentialAudio()
    {
        foreach (AudioClip clip in audioClips)
        {
            audioSource.clip = clip;
            audioSource.Play();

            // Wait until the current audio clip finishes playing
            yield return new WaitForSeconds(clip.length);
        }
    }
    
    //public AudioSource radioSource;
    //public AudioSource radioSource2;
    
    // Start is called before the first frame update
    //void Start()
    //{
    //    radioSource.Play();
    //    radioSource.Pause();
    //    radioSource.volume = 0.5f;
    //}
}
