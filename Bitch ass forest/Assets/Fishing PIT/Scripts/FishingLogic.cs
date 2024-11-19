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
    LayerMask waterLayer;
    
    // Start is called before the first frame update
    void Start()
    { 
        StartCoroutine(SplashEffectCoroutine());
        waterLayer = LayerMask.GetMask("Water");
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
            fishAnimator.SetBool("isIdle", true);
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
            SetLurePositionOnWater();
        }
    }

    void UpdateHookingFishState()
    {
        // AttachFishToLure();
        Debug.Log("Hooking Fish State");
    }

    bool IsLureOnWater()
    {
        Debug.Log("Checking if lure is on water...");
        isLureOnWater = CheckForWaterSimple(lure.position);
        Debug.Log($"IsLureOnWater result: {isLureOnWater}");
        return isLureOnWater;
    }

    void SetLurePositionOnWater()
    {
        RaycastHit hit;
        float sphereRadius = 0.5f;
        float castDistance = 0.5f;

        // Perform multiple spherecasts around the lure
        Vector3[] offsets = { Vector3.zero, Vector3.left * 0.1f, Vector3.right * 0.1f, Vector3.forward * 0.1f, Vector3.back * 0.1f };
        bool waterDetected = false;

        foreach (var offset in offsets)
        {
            if (Physics.BoxCast(lure.position + offset, Vector3.one * sphereRadius, Vector3.down, out hit, Quaternion.identity, castDistance))
            {
                if (hit.transform.CompareTag("Water"))
                {
                    //Debug.Log($"BoxCast hit: {hit.transform.name}, Tag: {hit.transform.tag}");
                    waterDetected = true;
                    // Set the lure's y-position to the water surface y-position
                    Vector3 waterSurfacePosition = hit.point;
                    waterSurfacePosition.y = hit.point.y;
                    lure.position = new Vector3(lure.position.x, waterSurfacePosition.y, lure.position.z);
                    break;
                }
                else if (hit.transform.CompareTag("Terrain"))
                {
                    lure.position = hit.point;
                }
            }
        }

        if (!waterDetected)
        {
            TransitionToState(LureState.Idle);
        }
    }
    bool CheckForWaterSimple(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position, Vector3.down, out hit, 0.5f, waterLayer))
        {
            //Debug.Log($"Raycast hit: {hit.collider.name}, Tag: {hit.collider.tag}");
            return hit.collider.CompareTag("Water");
            return true;
        }
        //Debug.Log("Raycast did not hit water.");
        return false;
    }
    bool CheckForWater(Vector3 position, float boxSize, float castDistance)
    {
        RaycastHit hit;
        Debug.DrawLine(position, position + Vector3.down * castDistance, Color.red, 1f);

        if (Physics.BoxCast(position, Vector3.one * boxSize, Vector3.down, out hit, Quaternion.identity, castDistance, waterLayer))
        {
            if (hit.collider.CompareTag("Water"))
            {
                //Debug.Log("Lure is on water");
                return true;
            }
            else
            {
                Debug.Log("Hit something else: " + hit.transform.name);
            }
        }
        Debug.Log("Lure is no longer on water");
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
            //Debug.LogError("Fish prefab is not assigned.");
        }
    }


    /*void StickLureToWater()
    {
        isLureOnWater = true;
        lure.GetComponent<Rigidbody>().isKinematic = true;
        lure.position = new Vector3(lure.position.x, lure.position.y, lure.position.z);
    }*/
    
    IEnumerator SplashEffectCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(splashDelayMin, splashDelayMax));
            Debug.Log($"SplashEffectCoroutine - isLureOnWater: {isLureOnWater}, currentState: {currentState}, splashParticleActive: {splashParticleActive}");
            if (isLureOnWater && currentState == LureState.InWater && !splashParticleActive)
            {
                Vector3 splashPosition = lure.position + Random.insideUnitSphere * 3f;
                splashPosition.y = lure.position.y;
                currentSplashParticle = Instantiate(splashParticlePrefab, splashPosition, quaternion.identity);
                currentSplashParticle.transform.rotation = Quaternion.LookRotation(Vector3.up);
                splashParticleActive = true;
                Debug.Log("Splash effect created!");
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
                //Debug.Log("Fish gone lure out of water");
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
                //StickLureToWater();
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
            //Debug.Log("Fish is struggling!");

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
                //Debug.Log("Applying Strain: " + currentStrain);
                if (currentStrain > strainThreshold)
                {
                    //Debug.Log("Line Snapped!");
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
            fishAnimator.SetBool("isResting", true);
            //Debug.Log("Fish removed from lure.");
        }
    }
}
