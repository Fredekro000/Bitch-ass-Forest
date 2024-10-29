using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Radio : MonoBehaviour
{
    Animator animator;
    public AnimationClip RadioOn; 
    public AnimationClip RadioOff;
    bool RadioState = false;
    
    const float fadeTime = 1.5f;
    public AudioSource RadioSound;
    
    void Start()
    {
        RadioSound = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        RadioSound.volume = 0;  // Start with volume at 0
        RadioSound.Play();
        RadioSound.Pause();     // Start paused
    }

    // This function will be called by the XR button press
    public void ToggleRadio()
    {
        if (!RadioState)
        {
            RadioState = true;
            animator.SetBool("RadioMode", true);
            animator.Play(RadioOn.name);
            RadioSound.UnPause();
            StartCoroutine(FadeIn(RadioSound, fadeTime));
        }
        else
        {
            RadioState = false;
            animator.SetBool("RadioMode", false);
            animator.Play(RadioOff.name);
            StartCoroutine(FadeOut(RadioSound, fadeTime));
        }
    }

    IEnumerator FadeIn(AudioSource audioSource, float duration)
    {
        float startVolume = 0f;
        audioSource.volume = startVolume;

        while (audioSource.volume < 1f)
        {
            audioSource.volume += Time.deltaTime / duration;
            yield return null;
        }

        audioSource.volume = 1f; // Ensure volume is fully up at the end
    }

    IEnumerator FadeOut(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0f)
        {
            audioSource.volume -= Time.deltaTime / duration;
            yield return null;
        }

        audioSource.volume = 0f; // Ensure volume is completely off at the end
        audioSource.Pause();
    }
}