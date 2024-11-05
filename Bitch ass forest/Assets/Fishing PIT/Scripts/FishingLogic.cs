using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public enum LureState
{
    Idle,
    InWater,
    HookingFish
}

public class FishingLogic : MonoBehaviour
{
    public Transform lure;

    public Transform fish;
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
        /*
        if (isLureOnWater)
        {
            GlideLureOnWater();
        }
        
        if (!isFishAttached && IsLureOnWater())
        {
            AttachFishToLure();
        }*/
    }


    void UpdateIdleState()
    {
        Debug.Log("Idle State");
        if (IsLureOnWater())
        {
            TransitionToState(LureState.InWater);
        }
    }

    void UpdateInWaterState()
    {
        Debug.Log("Water State");
        StickLureToWater();
        if (!IsLureOnWater())
        {
            TransitionToState(LureState.Idle);
        }
        
        
    }

    void UpdateHookingFishState()
    {
        AttachFishToLure();
        Debug.Log("Hooking Fish State");
    }
    bool IsLureOnWater()
    {
        RaycastHit hit;
        float sphereRadius = 0.1f;
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
        isFishAttached = true;
        fish.position = lure.position;
        fish.SetParent(lure);
        fish.GetComponent<Rigidbody>().isKinematic = true;
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
                TransitionToState(LureState.HookingFish);
            }
        }
    }
    void TransitionToState(LureState newState)
    {
        currentState = newState;
        switch (newState)
        {
            case LureState.Idle:
                //isLureOnWater = false;
                break;
            case LureState.InWater:
                isLureOnWater = true;
                splashParticleActive = false;
                break;
            case LureState.HookingFish:
                StopCoroutine(SplashEffectCoroutine());
                break;
        }
    }
}
