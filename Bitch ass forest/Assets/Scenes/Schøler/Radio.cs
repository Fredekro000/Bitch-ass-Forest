using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Radio : MonoBehaviour
{
    public AnimationClip RadioOn; 
    public AnimationClip RadioOff;
    bool RadioState = false;
    bool isCooldown = false;
    
    const float fadeTime = 1.5f;
    public Material red;
    public Material green;
    public GameObject button;
   
    public List<AudioClip> rustleSounds = new List<AudioClip>();
    
    void Start() { }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Radio"))
        {
            // Get the components from the button object (assumes the button itself has these components)
            AudioSource radioAudioSource = other.GetComponent<AudioSource>();
            Animator radioAnimator = other.GetComponent<Animator>();
            Renderer buttonRenderer = other.GetComponent<Renderer>();

            // Ensure audio source is ready to play and starts playing only if it hasn't already been triggered
            if (radioAudioSource != null && !radioAudioSource.isPlaying)
            {
                radioAudioSource.volume = 0;
                radioAudioSource.Play(); // Start the audio if it's the first time being activated
            }
            
            if (isCooldown) return;

            // Toggle radio state
            if (!RadioState)
            {
                if (buttonRenderer != null) buttonRenderer.material = green;
                RadioState = true;
                
                if (radioAnimator != null)
                {
                    radioAnimator.SetBool("RadioMode", true);
                    radioAnimator.Play(RadioOn.name);
                }

                if (radioAudioSource != null)
                {
                    radioAudioSource.UnPause(); // Resume audio if it's paused
                    StartCoroutine(FadeIn(radioAudioSource, fadeTime));
                }
                if (button != null)
                {
                    Renderer otherRenderer = button.GetComponent<Renderer>();
                    if (otherRenderer != null) otherRenderer.material = green;
                }
            }
            else
            {
                if (buttonRenderer != null) buttonRenderer.material = red;
                RadioState = false;
                
                if (radioAnimator != null)
                {
                    radioAnimator.SetBool("RadioMode", false);
                    radioAnimator.Play(RadioOff.name);
                }
                if (button != null)
                {
                    Renderer otherRenderer = button.GetComponent<Renderer>();
                    if (otherRenderer != null) otherRenderer.material = red;
                }
                if (radioAudioSource != null)
                {
                    StartCoroutine(FadeOut(radioAudioSource, fadeTime));
                }
            }

            StartCoroutine(StartCooldown(2f));
        } 
        else if (other.gameObject.CompareTag("Foliage"))
        {
            if (isCooldown) return;

            // Get AudioSource from the foliage and play a random rustle sound
            AudioSource foliageAudioSource = other.GetComponent<AudioSource>();
            if (foliageAudioSource != null && rustleSounds.Count > 0)
            {
                int randomIndex = Random.Range(0, rustleSounds.Count);
                foliageAudioSource.clip = rustleSounds[randomIndex];
                foliageAudioSource.Play();
            }

            StartCoroutine(StartCooldown(0.5f));
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

        audioSource.volume = 1f;
    }

    IEnumerator FadeOut(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0f)
        {
            audioSource.volume -= Time.deltaTime / duration;
            yield return null;
        }

        audioSource.volume = 0f; 
        audioSource.Pause();
    }

    IEnumerator StartCooldown(float cooldownDuration)
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownDuration);
        isCooldown = false; 
    }
}
