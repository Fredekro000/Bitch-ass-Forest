using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FishingRodState
{
    Idle,
    Casting,
    Reeling,
    ReelingFish
}

public class FishingRod : MonoBehaviour
{
    public Transform rodTip;
    public Transform lure;
    public LineRenderer lineR;

    private Rigidbody lureRb;
    public float castForce = 10f;
    public float reelSpeed = 5f;
    public float fishHookedReelSpeed = 3f;

    public int segments = 10;
    private Vector3[] points;

    public FishingRodState currentState = FishingRodState.Idle;

    public FishingLogic fishingLogic;

    void Start()
    {
        lureRb = lure.GetComponent<Rigidbody>();
        lineR.positionCount = segments;
        points = new Vector3[segments];
        
        fishingLogic.fishingRod = this;
    }

    void Update()
    {
        switch (currentState)
        {
            case FishingRodState.Idle:
                UpdateIdleState();
                break;
            case FishingRodState.Casting:
                UpdateCastingState();
                break;
            case FishingRodState.Reeling:
                UpdateReelingState();
                break;
            case FishingRodState.ReelingFish:
                UpdateReelingFishState();
                break;
        }

        if (Input.GetButtonDown("Fire1") && currentState == FishingRodState.Idle && fishingLogic.currentState == LureState.Idle)
        {
            CastLure();
        }

        if (Input.GetButtonUp("Fire1") && currentState == FishingRodState.Casting)
        {
            StopCasting();
        }

        if (Input.GetButtonDown("Fire2") && currentState != FishingRodState.Reeling)
        {
            StartReeling();
        }

        if (Input.GetButtonUp("Fire2") && currentState == FishingRodState.Reeling)
        {
            currentState = FishingRodState.Casting;
        }
        if (Input.GetButtonDown("Fire2") && currentState != FishingRodState.ReelingFish)
        {
            StartReelingFish();
        }

        if (Input.GetButtonUp("Fire2") && currentState == FishingRodState.ReelingFish)
        {
            currentState = FishingRodState.Casting;
        }

        UpdateLineRenderer();
    }

    void UpdateLineRenderer()
    {
        points[0] = rodTip.position;
        points[segments - 1] = lure.position;

        for (int i = 1; i < segments - 1; i++)
        {
            float t = (float)i / (segments - 1);
            points[i] = Vector3.Lerp(rodTip.position, lure.position, t);
        }

        lineR.SetPositions(points);
    }

    void UpdateIdleState()
    {
        lure.position = rodTip.position;
    }

    void UpdateCastingState()
    {
        // Casting logic if needed
    }

    void UpdateReelingState()
    {
        Vector3 targetPosition = rodTip.position;
        targetPosition.y = lure.position.y; // Lock the y-axis
        Vector3 direction = (targetPosition - lure.position).normalized;
        float distance = Vector3.Distance(lure.position, targetPosition);

        if (distance > 0.1f)
        {
            lureRb.MovePosition(Vector3.MoveTowards(lure.position, targetPosition, reelSpeed * Time.deltaTime));
        }
        else
        {
            currentState = FishingRodState.Idle;
            lureRb.isKinematic = true;
        }
    }


    void UpdateReelingFishState()
    {
        Vector3 targetPosition = rodTip.position;
        targetPosition.y = lure.position.y;
        float distance = Vector3.Distance(lure.position, targetPosition);

        if (distance > 0.1f && fishingLogic.currentStrain < fishingLogic.strainThreshold)
        {
            lureRb.MovePosition(Vector3.MoveTowards(lure.position, targetPosition, fishHookedReelSpeed * Time.deltaTime));
        }

        else
        {
            currentState = FishingRodState.Idle;
            lureRb.isKinematic = true;
            fishingLogic.StopFishStruggle();
        }
    }



    void CastLure()
    {
        currentState = FishingRodState.Casting;
        lureRb.isKinematic = false;
        lureRb.AddForce(rodTip.forward * castForce, ForceMode.Impulse);
    }

    void StopCasting()
    {
        currentState = FishingRodState.Casting;
    }

    void StartReeling()
    {
        currentState = FishingRodState.Reeling;
    }

    void StartReelingFish()
    {
        currentState = FishingRodState.ReelingFish;
    }
}
