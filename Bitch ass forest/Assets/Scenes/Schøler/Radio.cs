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

    private bool isInFoliage = false;  // Tracks if hand is in foliage
    private bool foliageRustlingCooldown = false; // Cooldown for rustling sounds
    private Vector3 lastPosition; // To track the previous position of the hand

    public float movementThreshold = 0.1f; // Minimum movement to trigger rustling sound
    public float movementCheckInterval = 0.5f; // Time between movement checks

    void Start() 
    {
        lastPosition = transform.position; // Set initial position of the hand object
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Radio"))
        {
            AudioSource radioAudioSource = other.GetComponent<AudioSource>();
            Animator radioAnimator = other.GetComponent<Animator>();
            Renderer buttonRenderer = other.GetComponent<Renderer>();

            if (radioAudioSource != null && !radioAudioSource.isPlaying)
            {
                radioAudioSource.volume = 0;
                radioAudioSource.Play();
            }

            if (isCooldown) return;

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
                    radioAudioSource.UnPause();
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

                if (radioAudioSource != null)
                {
                    StartCoroutine(FadeOut(radioAudioSource, fadeTime));
                }

                if (button != null)
                {
                    Renderer otherRenderer = button.GetComponent<Renderer>();
                    if (otherRenderer != null) otherRenderer.material = red;
                }
            }

            StartCoroutine(StartCooldown(2f));
        } 
        else if (other.gameObject.CompareTag("Foliage"))
        {
            isInFoliage = true;
            StartCoroutine(PlayRustlingSounds(other.GetComponent<AudioSource>()));
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Foliage"))
        {
            isInFoliage = false; // Set to false when hand leaves foliage
        }
    }

    IEnumerator PlayRustlingSounds(AudioSource foliageAudioSource)
    {
        while (isInFoliage && foliageAudioSource != null)
        {
            // Check for movement in the foliage
            if (Vector3.Distance(transform.position, lastPosition) > movementThreshold && !foliageRustlingCooldown)
            {
                lastPosition = transform.position; // Update last position
                if (!foliageRustlingCooldown && rustleSounds.Count > 0)
                {
                    foliageRustlingCooldown = true;

                    // Pick a random rustling sound
                    int randomIndex = Random.Range(0, rustleSounds.Count);
                    foliageAudioSource.clip = rustleSounds[randomIndex];
                    foliageAudioSource.pitch = Random.Range(0.9f, 1.1f);
                    foliageAudioSource.Play();

                    // Cooldown to prevent immediate replay of rustling sound
                    yield return new WaitForSeconds(foliageAudioSource.clip.length);
                    foliageRustlingCooldown = false;
                }
            }

            yield return new WaitForSeconds(movementCheckInterval); // Wait before checking again
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
