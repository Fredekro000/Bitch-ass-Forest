using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rustling : MonoBehaviour
{
    private Animator animator;
    public AnimationClip rustleAnim;
    
    bool isRustling = false;
    bool isCooldown = false;
    const float cooldownDuration = 2f; 
    
    public AudioSource rustleSound;
    
    void Start()
    {
        rustleSound = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Foliage"))
        {

            if (isCooldown) return;

            if (!isRustling)
            {
                isRustling = true;
                rustleSound.Play();
            }

            StartCoroutine(StartCooldown());
        }
    }

    IEnumerator StartCooldown()
    {
        isCooldown = true; 
        yield return new WaitForSeconds(cooldownDuration);
        isRustling = false;
        isCooldown = false; 
    }
}
