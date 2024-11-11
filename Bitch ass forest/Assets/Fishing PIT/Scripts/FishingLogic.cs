using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum LureState
{
    Idle,
    InWater,
    HookingFish,
    
}

public class FishingLogic : MonoBehaviour
{
    public Transform lure;

    public Transform fish;
    public GameObject fishPrefab;
    private Animator fishAnimator;
    public float attachDistance = 0.01f;

    public bool isFishAttached = false;

    public float glideSpeed = 0.5f;
    public bool isLureOnWater = false;
    
    public GameObject splashParticlePrefab;
    public float splashDelayMin = 2f;
    public float splashDelayMax = 5f;

    public LureState currentState = LureState.Idle;
    
    
    private GameObject currentSplashParticle;
    public bool splashParticleActive = false;

    public float currentStrain = 0f;
    public float strainThreshold = 100f;
    private Coroutine fishStruggleCoroutine;
    public FishingRod fishingRod;

    public bool isFishStruggling = false;
    
    // Start is called before the first frame update
    void Start()
    { 
        StartCoroutine(SplashEffectCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case LureState.Idle:
                UpdateIdleState();
                break;
            case LureState.InWater:
                UpdateInWaterState();
                break;
            case LureState.HookingFish:
                UpdateHookingFishState();
                break;
        }
        
        if (!isLureOnWater && IsLureOnWater())
        {
            currentState = LureState.InWater;
        }

        if (currentSplashParticle != null)
        {
            MoveSplashParticleTowardsLure();
        }

        if (Input.GetKeyDown(KeyCode.F) && fishingRod.currentState == FishingRodState.Idle)
        {
            RemoveFishFromLure();
        }
        /*
        if (isLureOnWater)
        {
            GlideLureOnWater();
        }
        
        if (!isFishAttached && IsLureOnWater())
        {
            AttachFishToLure();
        }*/
        
        // Increase strain when reeling in
        /*if (fishingRod != null && fishingRod.currentState == FishingRodState.ReelingFish)
        {
            IncreaseStrain();
        }*/
    }

    /*void IncreaseStrain()
    {
        // Increase strain based on some factor, e.g., time
        currentStrain += Time.deltaTime * 10f; // Adjust the multiplier as needed

        if (currentStrain > strainThreshold)
        {
            Debug.Log("Line Snapped!");
            TransitionToState(LureState.Idle);
        }
    }*/
    
    void UpdateIdleState()
    {
        Debug.Log("Idle State");
        if (IsLureOnWater())
        {
            TransitionToState(LureState.InWater);
        }
        
        if (isFishAttached && fishAnimator != null)
        {
            fishAnimator.SetBool("IsIdle", true);
        }
        //StopCoroutine(FishStruggleCoroutine());
    }

    void UpdateInWaterState()
    {
        Debug.Log("Water State");
        if (!IsLureOnWater())
        {
            TransitionToState(LureState.Idle);
        }
        else
        {
            RaycastHit hit;
            float sphereRadius = 0.01f;
            float castDistance = 0.01f;
            if (Physics.SphereCast(lure.position, sphereRadius, Vector3.down, out hit, castDistance))
            {
                if (hit.transform.CompareTag("Water"))
                {
                    // Set the lure's y-position to the water surface y-position
                    Vector3 waterSurfacePosition = hit.point;
                    waterSurfacePosition.y = hit.point.y; // Ensure y-position matches the water surface
                    lure.position = new Vector3(lure.position.x, waterSurfacePosition.y, lure.position.z);
                }
                else if (hit.transform.CompareTag("Terrain"))
                {
                    lure.position = hit.point;
                }
            }
        }
    }


    void UpdateHookingFishState()
    {
        //AttachFishToLure();
        Debug.Log("Hooking Fish State");
    }
    bool IsLureOnWater()
    {
        RaycastHit hit;
        float sphereRadius = 0.2f;
        float castDistance = 0.1f;
        
        Debug.DrawRay(lure.position, Vector3.down * castDistance, Color.red);
        
        if (Physics.SphereCast(lure.position, sphereRadius, Vector3.down, out hit, castDistance))
        {
            if (hit.transform.CompareTag("Water"))
            {
                print("lure is on water");
                return true;
            }
            else
            {
                Debug.Log("Hit something else: " + hit.transform.name);
            }
        }
        print("lure is no longer on water");
        return false;
    }

    void AttachFishToLure()
    {
        if (fishPrefab != null)
        {
            GameObject newFish = Instantiate(fishPrefab, lure.position, Quaternion.identity);
            isFishAttached = true;
            newFish.transform.SetParent(lure);
            newFish.GetComponent<Rigidbody>().isKinematic = true;
            fish = newFish.transform; // Update the fish reference to the new fish
            
            fishAnimator = newFish.GetComponent<Animator>();
            if (fishAnimator != null)
            {
                fishAnimator.SetBool("isResting", false);
            }
        }
        else
        {
            Debug.LogError("Fish prefab is not assigned.");
        }
    }


    void StickLureToWater()
    {
        isLureOnWater = true;
        lure.GetComponent<Rigidbody>().isKinematic = true;
        lure.position = new Vector3(lure.position.x, lure.position.y, lure.position.z);
    }
    
    IEnumerator SplashEffectCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(splashDelayMin, splashDelayMax));
            if (isLureOnWater && currentState == LureState.InWater && !splashParticleActive)
            {
                Vector3 splashPosition = lure.position + Random.insideUnitSphere * 3f;
                splashPosition.y = lure.position.y;
                currentSplashParticle = Instantiate(splashParticlePrefab, splashPosition, quaternion.identity);
                currentSplashParticle.transform.rotation = Quaternion.LookRotation(Vector3.up);
                splashParticleActive = true;
            }
        }
    }

    void MoveSplashParticleTowardsLure()
    {
        if (currentSplashParticle != null)
        {
            currentSplashParticle.transform.position = Vector3.MoveTowards(currentSplashParticle.transform.position,
                lure.position, Time.deltaTime * glideSpeed);
            if (Vector3.Distance(currentSplashParticle.transform.position, lure.position) < attachDistance)
            {
                Destroy(currentSplashParticle);
                currentSplashParticle = null;
                splashParticleActive = false;
                AttachFishToLure();
                TransitionToState(LureState.HookingFish);
            }
            else if (!isLureOnWater)
            {
                Destroy(currentSplashParticle);
                currentSplashParticle = null;
                splashParticleActive = false;
                Debug.Log("Fish gone lure out of water");
            }
        }
    }
    void TransitionToState(LureState newState)
    {
        if (currentState == LureState.HookingFish && fishStruggleCoroutine != null)
        {
            StopCoroutine(fishStruggleCoroutine);
        }
        
        currentState = newState;
        switch (newState)
        { 
            case LureState.Idle:
                isLureOnWater = false;
                currentStrain = 0f;
                break;
            case LureState.InWater:
                isLureOnWater = true;
                splashParticleActive = false;
                StickLureToWater();
                break;
            case LureState.HookingFish:
                StopCoroutine(SplashEffectCoroutine());
                fishStruggleCoroutine = StartCoroutine(FishStruggleCoroutine());
                break;
        }
    }

    IEnumerator FishStruggleCoroutine()
    {
        while (currentState == LureState.HookingFish)
        {
            yield return new WaitForSeconds(Random.Range(3f, 5f));
            Vector3 struggleDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            float struggleDuration = Random.Range(1f, 2f);
            Debug.Log("Fish is struggling!");

            isFishStruggling = true;
            
            StartCoroutine(ApplyStrain(struggleDirection, struggleDuration));
        }
    }

    public IEnumerator ApplyStrain(Vector3 direction, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            if (currentState != LureState.HookingFish) yield break;

            // Apply strain based on the player's reeling action
            if (fishingRod.currentState == FishingRodState.ReelingFish && isFishStruggling)
            {
                currentStrain += Time.deltaTime * 10f;
                Debug.Log("Applying Strain: " + currentStrain);
                if (currentStrain > strainThreshold)
                {
                    Debug.Log("Line Snapped!");
                    currentState = LureState.InWater;
                    TransitionToState(LureState.Idle);
                    yield break;
                }
            }

            lure.position += direction * Time.deltaTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isFishStruggling = false;
    }
    
    public void StopFishStruggle()
    {
        if (fishStruggleCoroutine != null)
        {
            StopCoroutine(fishStruggleCoroutine);
            fishStruggleCoroutine = null;
            isFishStruggling = false;
        }
    }
    
    public void RemoveFishFromLure()
    {
        if (isFishAttached)
        {
            isFishAttached = false;
            fish.SetParent(null);
            fish.GetComponent<Rigidbody>().isKinematic = false;
            currentState = LureState.Idle;
            Debug.Log("Fish removed from lure.");
        }
    }

}
