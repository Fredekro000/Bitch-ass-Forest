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

    private bool isInFoliage = false;
    private bool foliageRustlingCooldown = false;
    private Vector3 previousHandPosition;
    private Vector3 currentVelocity;
    public float velocityThreshold = 0.5f; // Set a velocity threshold for rustling

    private Collider foliageCollider;

    void Start() { }

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
            foliageCollider = other;
            previousHandPosition = transform.position;
            StartCoroutine(PlayRustlingSounds(other.GetComponent<AudioSource>()));
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Foliage"))
        {
            isInFoliage = false;
            foliageCollider = null;
        }
    }

    void Update()
    {
        if (isInFoliage && foliageCollider != null)
        {
            // Calculate velocity based on position change over time
            currentVelocity = (transform.position - previousHandPosition) / Time.deltaTime;

            // Check if the hand's velocity exceeds the threshold
            if (currentVelocity.magnitude > velocityThreshold && !foliageRustlingCooldown)
            {
                AudioSource foliageAudioSource = foliageCollider.GetComponent<AudioSource>();
                if (foliageAudioSource != null && rustleSounds.Count > 0)
                {
                    int randomIndex = Random.Range(0, rustleSounds.Count);
                    foliageAudioSource.clip = rustleSounds[randomIndex];
                    foliageAudioSource.Play();
                    foliageRustlingCooldown = true;
                    StartCoroutine(RustlingSoundCooldown(foliageAudioSource.clip.length));
                }
            }

            previousHandPosition = transform.position; // Update hand position
        }
    }

    IEnumerator RustlingSoundCooldown(float duration)
    {
        yield return new WaitForSeconds(duration);
        foliageRustlingCooldown = false;
    }

    IEnumerator PlayRustlingSounds(AudioSource foliageAudioSource)
    {
        while (isInFoliage && foliageAudioSource != null)
        {
            if (!foliageRustlingCooldown && rustleSounds.Count > 0)
            {
                foliageRustlingCooldown = true;

                int randomIndex = Random.Range(0, rustleSounds.Count);
                foliageAudioSource.clip = rustleSounds[randomIndex];
                foliageAudioSource.Play();

                yield return new WaitForSeconds(foliageAudioSource.clip.length);
                foliageRustlingCooldown = false;
            }
            else
            {
                yield return null;
            }
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
