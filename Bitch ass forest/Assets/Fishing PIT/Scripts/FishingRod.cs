using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.tvOS;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR;
using CommonUsages = UnityEngine.InputSystem.CommonUsages;

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
    public float reelSpeed = 10f;
    public float fishHookedReelSpeed = 3f;

    public int segments = 10;
    private Vector3[] points;

    public FishingRodState currentState = FishingRodState.Idle;

    public FishingLogic fishingLogic;
    
    private Color baseColor = Color.blue;
    private Color struggleColor = Color.red;

    public Transform[] rodBones;
    public float weight = 1.0f;

    public InputActionReference AButtonReference;

    public Material lineColor;
    public bool isHoldingA = false;
    private Vector3 initialPosition;
    public float throwThreshold = 1.0f;

    //public InputActionProperty AButton;

    void Start()
    {
        lureRb = lure.GetComponent<Rigidbody>();
        lineR.positionCount = segments;
        points = new Vector3[segments];
        
        fishingLogic.fishingRod = this;
        
        AButtonReference.action.performed += ctx => OnAPress();
        AButtonReference.action.canceled += ctx => OnARelease();
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

        if (isHoldingA)
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
        if (Input.GetButtonDown("Fire2") && currentState != FishingRodState.ReelingFish && fishingLogic.currentState == LureState.HookingFish)
        {
            StartReelingFish();
        }

        if (Input.GetButtonUp("Fire2") && currentState == FishingRodState.ReelingFish && fishingLogic.currentState == LureState.HookingFish)
        {
            currentState = FishingRodState.Casting;
        }

        UpdateLineRenderer();
        UpdateLineRendererColor();
        UpdateRodBending();

        /*Vector3 direction = lure.position - rodBones[rodBones.Length - 1].position;
        float distance = direction.magnitude;
        direction.Normalize();

        for (int i = 0; i < rodBones.Length; i++)
        {
            float influence = (float)(i + 1) / rodBones.Length;
            rodBones[i].localRotation = Quaternion.Slerp(rodBones[i].localRotation, Quaternion.LookRotation(direction),
                weight * influence * Time.deltaTime);
        }*/
    }

    void OnEnable()
    {
        AButtonReference.action.Enable();
    }

    void OnDisable()
    {
        AButtonReference.action.Disable();
    }

    void OnAPress()
    {
        isHoldingA = true;
        initialPosition = rodTip.position;
    }

    void OnARelease()
    {
        isHoldingA = false;
        Vector3 throwDirection = rodTip.position - initialPosition;
        if (throwDirection.magnitude > throwThreshold)
        {
            // Apply velocity to the fishing rod in the direction of the throw
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.velocity = throwDirection.normalized * throwDirection.magnitude;
        }
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
    
    void UpdateLineRendererColor()
    {
        float t = Mathf.Clamp01(fishingLogic.currentStrain / fishingLogic.strainThreshold); // Normalize currentStrain to a value between 0 and 1
        Color currentColor = Color.Lerp(baseColor, struggleColor, t); // Interpolate between baseColor and targetColor
        lineR.startColor = currentColor;
        lineR.endColor = currentColor;
        lineColor.color = currentColor;
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
    
    void UpdateRodBending()
    {
        if (rodBones == null || rodBones.Length == 0)
        {
            //Debug.LogError("Rod bones array is not set or empty.");
            return;
        }

        Vector3 direction = lure.position - rodBones[rodBones.Length - 1].position;
        direction.Normalize();

        for (int i = 0; i < rodBones.Length; i++)
        {
            if (rodBones[i] == null)
            {
                //Debug.LogError($"Rod bone at index {i} is null.");
                continue;
            }

            // Calculate influence based on the bone's position in the array
            float influence = Mathf.Pow((float)(i + 1) / rodBones.Length, 2); // Squaring the influence for a more pronounced effect

            // Calculate the target rotation for the current bone
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Apply the rotation with a weighted influence
            rodBones[i].localRotation = Quaternion.Slerp(rodBones[i].localRotation, targetRotation, weight * influence * Time.deltaTime);

            // Visualize bone positions
            Debug.DrawLine(rodBones[i].position, rodBones[i].position + rodBones[i].forward * 0.1f, Color.red);
        }
    }


}
