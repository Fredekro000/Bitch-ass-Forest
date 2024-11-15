using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GrassSoundTrigger : MonoBehaviour
{
   public List<AudioClip> audioClips = new List<AudioClip>(); 
   
       private AudioSource audioSource;
       private Vector3 lastPosition; 
       private float distanceThreshold = 0.5f; 
       private int lastPlayedIndex = -1;
       public bool inGrass = false;
   
       void Start()
       {
           
           audioSource = GetComponent<AudioSource>();
           
           lastPosition = gameObject.transform.position;
       }
   
       void Update()
       {
           // Calculate the distance moved since the last sound was played
           float distanceMoved = Vector3.Distance(gameObject.transform.position, lastPosition);

           if (inGrass)
           {
               if (distanceMoved >= distanceThreshold) {
                      lastPosition = gameObject.transform.position; // Update the last position
                      PlayRandomSound();
               }
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
       
       private void OnTriggerEnter(Collider other)
       {
           Debug.Log(other.gameObject);
           
           // Check if the object entering the collider has a specific tag or name
           if (other.CompareTag("CubeTerrain"))
           {
               // Perform the desired action
               Debug.Log("Object entered the collider!");
               inGrass = true;
           }
       }


       private void OnTriggerExit(Collider other)
       {
           if (other.CompareTag("CubeTerrain"))
           {
               Debug.Log("Object exited the collider!");
               inGrass = false;
           }
       }
           
   }