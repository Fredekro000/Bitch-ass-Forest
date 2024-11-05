using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FishingRodState
{
    Idle,
    Casting,
    Reeling
}
public class FishingRod : MonoBehaviour
{
    public Transform rodTip;
    public Transform lure;
    public LineRenderer lineR;
    //public Transform fishingRod;

    private Rigidbody lureRb;
    public float castForce = 10f;
    public float reelSpeed = 5f;

    //public bool isCasting = false;
    //public bool isReeling = false;
    //private Vector3 lureStartPosition;

    public int segments = 10;
    public float segmentLength = 0.1f;
    private Vector3[] points;
    
    private FishingRodState currentState = FishingRodState.Idle;
    
    private FishingLogic fishingLogic;
    
    // Start is called before the first frame update
    void Start()
    {
        //lineR.positionCount = 2;
        
        lureRb = lure.GetComponent<Rigidbody>();
        fishingLogic = GetComponent<FishingLogic>();

        lineR.positionCount = segments;

        //lureStartPosition = lure.position;
        
        points = new Vector3[segments];
        for (int i = 0; i < segments; i++)
        {
            points[i] = Vector3.Lerp(rodTip.position, lure.position, (float)i / (segments - 1));
        }

    }

    // Update is called once per frame
    void Update()
    {
        //UpdateLineRenderer();
        
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
        }

        if (Input.GetButtonDown("Fire1") && currentState == FishingRodState.Idle && fishingLogic.currentState == LureState.Idle)
        {
            Debug.Log("Fire1 pressed");
            CastLure();
        }
        
        if (Input.GetButtonUp("Fire1") && currentState == FishingRodState.Casting)
        {
            Debug.Log("Fire1 released");
            StopCasting();
        }

        if (Input.GetButtonDown("Fire2") && currentState != FishingRodState.Reeling)
        {
            Debug.Log("Fire2 pressed");
            StartReeling();
        }

        if (Input.GetButtonUp("Fire2") && currentState == FishingRodState.Reeling)
        {
            currentState = FishingRodState.Casting;
        }

        /*if (isReeling)
        {
            ReelInLure();
        }*/

        lineR.SetPositions(points);
        
        points[0] = rodTip.position;
        points[segments - 1] = lure.position;

        for (int i = 1; i < segments - 1; i++)
        {
            Vector3 velocity = points[i] - points[i - 1];
            points[i] += velocity * Time.deltaTime;
            points[i] = Vector3.Lerp(points[i], points[i + 1], 0.5f);
        }

    }

    /*void UpdateLineRenderer()
    {
        lineR.SetPosition(0, rodTip.position);
        lineR.SetPosition(1, lure.position);
    }*/
    
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
        lure.position = Vector3.MoveTowards(lure.position, targetPosition, reelSpeed * Time.deltaTime);
        if (Vector3.Distance(lure.position, targetPosition) < 0.1f)
        {
            currentState = FishingRodState.Idle;
            lureRb.isKinematic = true;
            Debug.Log("Reeling stopped");
        }
    }
    
    void CastLure()
    {
        currentState = FishingRodState.Casting;
        //isCasting = true;
        lureRb.isKinematic = false;
        Debug.Log("Casting started");
        lureRb.AddForce(rodTip.forward * castForce, ForceMode.Impulse);
        //Invoke("StopCasting", 1f); // Stop casting after 1 second
    }

    void StopCasting()
    {
        currentState = FishingRodState.Casting;
        //isCasting = false;
        print("Casting stopped");
    }

    void StartReeling()
    {
        currentState = FishingRodState.Reeling;
        //isReeling = true;
        //lureRb.isKinematic = true;
        print("reeling started");
    }

    /*void ReelInLure()
    {
        Vector3 targetPosition = rodTip.position;
        lure.position = Vector3.MoveTowards(lure.position, targetPosition, reelSpeed * Time.deltaTime);
        if (Vector3.Distance(lure.position, targetPosition) < 0.1f)
        {
            isReeling = false;
            print("reeling stopped");
        }
    }*/
        
        //this.lureRb.isKinematic = false;
        //Rigidbody lureRb = lure.GetComponent<Rigidbody>();
        
        
    
}
