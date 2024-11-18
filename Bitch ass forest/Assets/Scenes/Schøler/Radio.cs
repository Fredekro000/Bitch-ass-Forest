using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandInteraction : MonoBehaviour
{
    [Header("Radio Settings")]
    public AnimationClip RadioOn;
    public AnimationClip RadioOff;
    public Material red;
    public Material green;
    public GameObject button;
    public float fadeTime = 1.5f;

    private bool RadioState = false;
    private bool isCooldown = false;
    private bool firstTime = true;

    [Header("Foliage Settings")]
    public List<AudioClip> rustleSounds = new List<AudioClip>();
    public float interactionWindSpeed = 2f;  // Wind speed when interacting
    public float defaultWindSpeed = 0.5f;    // Normal wind speed
    public float windTransitionSpeed = 2f;   // Speed of wind transition

    private bool isInFoliage = false;  // Tracks if hand is in foliage
    private Vector3 lastPosition; // To track the previous position of the hand
    private MaterialPropertyBlock propertyBlock;

    public float movementThreshold = 0.1f;  // Minimum movement to trigger rustling sound
    public float movementCheckInterval = 0.1f;  // Time between movement checks

    void Start()
    {
        lastPosition = transform.position; // Set initial position of the hand object
        propertyBlock = new MaterialPropertyBlock();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Radio"))
        {
            HandleRadioInteraction(other);
        }
        else if (other.gameObject.CompareTag("Foliage"))
        {
            isInFoliage = true;
            Renderer foliageRenderer = other.GetComponent<Renderer>();
            StartCoroutine(HandleFoliageRustling(other.GetComponent<AudioSource>(), interactionWindSpeed, foliageRenderer));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Foliage"))
        {
            isInFoliage = false;
        }
    }

    private void HandleRadioInteraction(Collider other)
    {
        AudioSource radioAudioSource = other.GetComponent<AudioSource>();
        Animator radioAnimator = other.GetComponent<Animator>();
        Renderer buttonRenderer = other.GetComponent<Renderer>();

        if (isCooldown) return;

        if (!RadioState)
        {
            ToggleRadio(true, buttonRenderer, radioAnimator, radioAudioSource);
        }
        else
        {
            ToggleRadio(false, buttonRenderer, radioAnimator, radioAudioSource);
        }

        StartCoroutine(StartCooldown(2f));
    }

    private void ToggleRadio(bool state, Renderer buttonRenderer, Animator radioAnimator, AudioSource radioAudioSource)
    {
        Material buttonMaterial = state ? green : red;
        AnimationClip animationClip = state ? RadioOn : RadioOff;

        if (buttonRenderer != null) buttonRenderer.material = buttonMaterial;
        if (radioAnimator != null)
        {
            radioAnimator.SetBool("RadioMode", state);
            radioAnimator.Play(animationClip.name);
        }

        if (state) // If turning the radio on
        {
            if (firstTime)
            {
                radioAudioSource.Play();
                firstTime = false;
            }
            else if (radioAudioSource != null)
            {
                radioAudioSource.UnPause();
                StartCoroutine(FadeIn(radioAudioSource, fadeTime));
            }
        }
        else // If turning the radio off
        {
            StartCoroutine(FadeOut(radioAudioSource, fadeTime));
        }

        RadioState = state;

        if (button != null)
        {
            Renderer otherRenderer = button.GetComponent<Renderer>();
            if (otherRenderer != null)
                otherRenderer.material = buttonMaterial;
        }
    }

    private IEnumerator HandleFoliageRustling(AudioSource foliageAudioSource, float targetWindWaveScale, Renderer foliageRenderer)
    {
        while (isInFoliage && foliageAudioSource != null)
        {
            // Continuously check for hand movement
            if (Vector3.Distance(transform.position, lastPosition) > movementThreshold)
            {
                lastPosition = transform.position;

                // Continuously adjust the wind wave scale based on hand movement
                float currentWindWaveScale = foliageRenderer.material.GetFloat("_WindWavesScale");
                currentWindWaveScale = Mathf.Lerp(currentWindWaveScale, targetWindWaveScale, Time.deltaTime * windTransitionSpeed);
                propertyBlock.SetFloat("_WindWavesScale", currentWindWaveScale);
                foliageRenderer.SetPropertyBlock(propertyBlock);

                // Play rustling sound if not already playing one
                if (!foliageAudioSource.isPlaying && rustleSounds.Count > 0)
                {
                    int randomIndex = Random.Range(0, rustleSounds.Count);
                    foliageAudioSource.clip = rustleSounds[randomIndex];
                    foliageAudioSource.pitch = Random.Range(0.9f, 1.1f);
                    foliageAudioSource.Play();
                }
            }

            yield return new WaitForSeconds(movementCheckInterval);
        }
    }

    private IEnumerator FadeIn(AudioSource audioSource, float duration)
    {
        audioSource.volume = 0f;
        while (audioSource.volume < 1f)
        {
            audioSource.volume += Time.deltaTime / duration;
            yield return null;
        }
        audioSource.volume = 1f;
    }

    private IEnumerator FadeOut(AudioSource audioSource, float duration)
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

    private IEnumerator StartCooldown(float cooldownDuration)
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownDuration);
        isCooldown = false;
    }
}
