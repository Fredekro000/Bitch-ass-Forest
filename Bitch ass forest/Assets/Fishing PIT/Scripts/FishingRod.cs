using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
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
    public AudioSource fishBlob;
    public AudioSource lineThrow;

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
    
    private Color baseColor = Color.white;
    private Color struggleColor = Color.red;

    public Transform[] rodBones;
    public float weight = 1.0f;

    public InputActionReference AButtonReference;
    public InputActionReference BButtonReference;

    public Material lineColor;
    //public bool isHoldingA = false;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    public float throwThreshold = 1.0f;

    private bool isSwinging = false;
    public float maxThrowSpeed = 50f;
    public float forceMultiplier = 0.15f;

    public float slackFactor = 0.5f;
    //public float length;
    public float maxLineLength;
    
    private Quaternion[] defaultRotations;
    public Transform[] hoops;

    //public InputActionProperty AButton;

    void Start()
    {
        lureRb = lure.GetComponent<Rigidbody>();
        lineR.positionCount = segments;
        points = new Vector3[segments];
        
        fishingLogic.fishingRod = this;
        
        AButtonReference.action.performed += ctx => OnAPress();
        AButtonReference.action.canceled += ctx => OnARelease();
        
        BButtonReference.action.performed += ctx => OnBPress();
        BButtonReference.action.canceled += ctx => OnBRelease();
        
        if (rodBones == null || rodBones.Length == 0)
            return;

        defaultRotations = new Quaternion[rodBones.Length];

        for (int i = 0; i < rodBones.Length; i++)
        {
            if (rodBones[i] != null)
            {
                defaultRotations[i] = rodBones[i].localRotation;
            }
        }
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

        /*if (Input.GetButtonDown("Fire1") && currentState == FishingRodState.Idle && fishingLogic.currentState == LureState.Idle)
        {
            //CastLure();
            StartThrowLure();
            Debug.Log("Casting Lure");
        }
        if (Input.GetButtonUp("Fire1") && currentState == FishingRodState.Casting)
        {
            //StopCasting();
            ThrowLure();
            Debug.Log("Casting Lure");
        }*/
        
        /*if (isHoldingA)
        {
            StartThrowLure();
        }

        if (!isHoldingA)
        {
            ThrowLure();
            Debug.Log("prep to cast Lure");
        }*/
        /*
        if (Input.GetButtonDown("Fire2") && currentState != FishingRodState.Reeling)
        {
            StartReeling();
            Debug.Log("Starting Reeling");
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
        }*/

        UpdateLineRenderer();
        UpdateLineRendererColor();
        if (currentState == FishingRodState.ReelingFish && fishingLogic.isFishStruggling)
        {
            UpdateRodBending();
        }
        else
        {
            ResetRodBending();
        }
        

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
        BButtonReference.action.Enable();
    }

    void OnDisable()
    {
        AButtonReference.action.Disable();
        BButtonReference.action.Disable();
    }

    void OnAPress()
    {
        Debug.Log("A Button Pressed");
        isSwinging = true;
        initialPosition = rodTip.position;
        initialRotation = rodTip.rotation;
        StartThrowLure();
    }

    void OnARelease()
    {
        Debug.Log("A Button Released");
        //isSwinging = false;
        ThrowLure();
        StartCoroutine(ResetSwinging());
        /*Vector3 throwDirection = rodTip.position - initialPosition;
        if (throwDirection.magnitude > throwThreshold)
        {
            // Apply velocity to the fishing rod in the direction of the throw
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.velocity = throwDirection.normalized * throwDirection.magnitude;
        }*/
    }

    void OnBPress()
    {
        if (currentState != FishingRodState.Reeling && currentState == FishingRodState.Casting && fishingLogic.currentState != LureState.HookingFish)
        {
            Debug.Log("'b press reeling");
            StartReeling();
        }

        if (currentState != FishingRodState.ReelingFish && currentState == FishingRodState.Casting && fishingLogic.currentState == LureState.HookingFish)
        {
            Debug.Log("'b press reeling fish");
            StartReelingFish();
            Debug.Log("fishBlob");
        
            if (fishBlob.isPlaying == false)
            {
                fishBlob.Play();
            }
        }
        if (currentState == FishingRodState.Idle)
        {
            Debug.Log("'b press try to remove fish");
            fishingLogic.RemoveFishFromLure();
        }
    }

    void OnBRelease()
    {
        if (currentState == FishingRodState.Reeling)
        {
            currentState = FishingRodState.Casting;
        }

        if (currentState == FishingRodState.ReelingFish && fishingLogic.currentState == LureState.HookingFish)
        {
            currentState = FishingRodState.Casting;
        }
    }

    void StartThrowLure()
    {
        initialPosition = rodTip.position;
        initialRotation = rodTip.rotation;
        //lureRb.isKinematic = false;
        Debug.Log("Starting Throw Lure");
    }

    void ThrowLure()
    {
        Debug.Log("Throwing Lure");
        if (isSwinging && currentState == FishingRodState.Idle && fishingLogic.currentState == LureState.Idle)
        {
            Debug.Log("ThrowLure isSwinging is true");
            currentState = FishingRodState.Casting;
            lureRb.isKinematic = false;
            Vector3 swingDirection = rodTip.position - initialPosition;
            float swingSpeed = swingDirection.magnitude / Time.deltaTime;
            Debug.Log($"Original Swing Speed: {swingSpeed}");
            swingSpeed = Mathf.Min(swingSpeed, maxThrowSpeed);
            Debug.Log($"Clamped Swing Speed: {swingSpeed}");
            Vector3 throwForce = swingDirection.normalized * swingSpeed * forceMultiplier;
            Debug.Log($"Swing Direction: {swingDirection}, Swing Speed: {swingSpeed}, Throw Force: {throwForce}");
            
            /*if (throwForce.magnitude > maxThrowSpeed)
            {
                throwForce = throwForce.normalized / maxThrowSpeed;
            }*/
            lureRb.AddForce(throwForce, ForceMode.VelocityChange);
            
            lineThrow.Play();
            
        }
        else
        {
            Debug.Log("Throwlure isSwinging is false");
        }
    }
    
    IEnumerator ResetSwinging()
    {
        yield return new WaitForEndOfFrame();
        isSwinging = false;
    }
    
    void UpdateLineRenderer()
    {
        /*points[0] = rodTip.position;
        points[segments - 1] = lure.position;

        for (int i = 1; i < segments - 1; i++)
        {
            float t = (float)i / (segments - 1);
            points[i] = Vector3.Lerp(rodTip.position, lure.position, t);
        }

        lineR.SetPositions(points);*/
        
        
        
        points[0] = rodTip.position; // Start point
        points[segments - 1] = lure.position; // End point

        Vector3 start = rodTip.position;
        Vector3 end = lure.position;

        // Calculate the total length and direction of the line
        Vector3 direction = end - start;
        float length = direction.magnitude;
        Vector3 normalizedDirection = direction.normalized;

        for (int i = 1; i < segments - 1; i++)
        {
            float t = (float)i / (segments - 1); // Normalized position (0 to 1)

            // Interpolate between start and end points
            Vector3 point = Vector3.Lerp(start, end, t);
            slackFactor = Mathf.Clamp(length / maxLineLength, 0.1f, 0.5f);
            // Add slack using a parabolic sag
            float sagAmount = Mathf.Sin(Mathf.PI * t) * slackFactor; // Slack curve
            Vector3 sagOffset = Vector3.down * sagAmount; // Apply sag in the downward direction

            // Offset the point to create the slack
            points[i] = point + sagOffset;
        }

        // Update the LineRenderer
        lineR.SetPositions(points);
        
        
        /*
        // Define the total number of points: rod tip, hoops, reel, and lure
        int totalPoints = hoops.Length + 2; // Rod tip + hoops + reel + lure
        lineR.positionCount = totalPoints;

        // Update the points array to match the total number of points
        Vector3[] points = new Vector3[totalPoints];

        // Add the rod tip as the first point
        points[0] = rodTip.position;
        points[1] = lure.position;
        // Add the hoop positions
        for (int i = 0; i < hoops.Length; i++)
        {
            points[i + 2] = hoops[i].position;
        }

        // Add the lure as the final point
        

        // Optional: Apply slack to the line
        for (int i = 1; i < totalPoints - 1; i++)
        {
            // Interpolate between adjacent points to add slack
            Vector3 previous = points[i - 1];
            Vector3 next = points[i + 1];

            Vector3 midpoint = (previous + next) * 0.5f;
            float slackFactor = 0.1f; // Adjust for more/less slack
            Vector3 sagOffset = Vector3.down * slackFactor;

            // Apply the sag
            points[i] = midpoint + sagOffset;
        }

        // Update the LineRenderer with the calculated points
        lineR.SetPositions(points);*/
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

    public void StartReeling()
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
            return;
        }

        // Control parameters
        float maxBendAngle = -15f; // Maximum bending angle at the tip
        float bendSpeed = 2f;     // Speed of bending interpolation

        for (int i = 0; i < rodBones.Length; i++)
        {
            if (rodBones[i] == null)
            {
                continue;
            }

            // Calculate influence based on the bone's index (more bending at the tip)
            float influence = (float)(i + 1) / rodBones.Length;

            // Target downward bending angle (tip bends the most)
            float bendAngle = maxBendAngle * influence;

            // Create a rotation offset for downward bending
            Quaternion downwardBend = Quaternion.Euler(bendAngle, 0f, 0f);

            // Smoothly interpolate the bone's rotation to include the downward bend
            rodBones[i].localRotation = Quaternion.Slerp(
                rodBones[i].localRotation,
                downwardBend,
                bendSpeed * Time.deltaTime
            );

            // Visualize the bending (optional)
            Debug.DrawLine(rodBones[i].position, rodBones[i].position + rodBones[i].up * 0.1f, Color.green);
        }
    }
    void ResetRodBending()
    {
        Debug.Log("cannot Reset Rod Bending");
        if (rodBones == null || defaultRotations == null)
            return;
        Debug.Log("Reset Rod Bending");
        for (int i = 0; i < rodBones.Length; i++)
        {
            if (rodBones[i] != null)
            {
                rodBones[i].localRotation = Quaternion.Slerp(
                    rodBones[i].localRotation,
                    defaultRotations[i],
                    Time.deltaTime * 5f // Adjust speed for smooth resetting
                );
            }
        }
    }


}
