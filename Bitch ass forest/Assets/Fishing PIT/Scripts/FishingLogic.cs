using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

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
    
    // Start is called before the first frame update
    void Start()
    { 
        StartCoroutine(SplashEffectCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLureOnWater && IsLureOnWater())
        {
            StickLureToWater();
        }

        if (isLureOnWater)
        {
            GlideLureOnWater();
        }
        
        if (!isFishAttached && IsLureOnWater())
        {
            AttachFishToLure();
        }
    }

    bool IsLureOnWater()
    {
        RaycastHit hit;
        float sphereRadius = 0.1f;
        if (Physics.SphereCast(lure.position, sphereRadius, Vector3.down, out hit, attachDistance))
        {
            if (hit.transform.CompareTag("Water"))
            {
                return true;
            }
        }

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

    void GlideLureOnWater()
    {
        //lure.position += Vector3.right * glideSpeed * Time.deltaTime;
    }

    IEnumerator SplashEffectCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(splashDelayMin, splashDelayMax));
            if (isLureOnWater)
            {
                Vector3 splashPosition = lure.position + Random.insideUnitSphere * 1f;
                splashPosition.y = lure.position.y;
                Instantiate(splashParticlePrefab, splashPosition, quaternion.identity);
            }
        }
    }
}
