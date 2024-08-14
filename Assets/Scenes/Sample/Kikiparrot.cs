using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kikiparrot : MonoBehaviour
{
    public Transform positionA;  // Position A reference (Transform)
    public Transform positionB;  // Position B reference (Transform)
    public float speed = 2f;     // Speed of parrot movement

    public GameObject Kiki_Walk_Rig;  // Reference to the walking rig
    public GameObject Kiki_Fly_Rig;   // Reference to the flying rig

    private Animator walkAnimator;
    private Animator flyAnimator;
    private Vector3 targetPosition;

    private enum ParrotState { Walking, PickingUp, Flying, Dropping, Returning }
    private ParrotState currentState = ParrotState.Walking;

    void Start()
    {
        // Get the Animator components from the child rigs
        walkAnimator = Kiki_Walk_Rig.GetComponent<Animator>();
        flyAnimator = Kiki_Fly_Rig.GetComponent<Animator>();

        targetPosition = positionA.position;

        // Initially, activate the walking rig and deactivate the flying rig
        Kiki_Walk_Rig.SetActive(true);
        Kiki_Fly_Rig.SetActive(false);

        Debug.Log("ParrotController started: Walking towards Position A");
        walkAnimator.SetBool("isWalking", true);
    }

    void Update()
    {
        switch (currentState)
        {
            case ParrotState.Walking:
                Debug.Log("Current State: Walking");
                MoveToTarget(targetPosition);
                if (ReachedTarget(targetPosition))
                {
                    Debug.Log("Reached Position A, transitioning to PickUp");
                    walkAnimator.SetBool("isWalking", false);
                    walkAnimator.SetTrigger("isPickingUp");
                    currentState = ParrotState.PickingUp;
                }
                break;

            case ParrotState.PickingUp:
                Debug.Log("Current State: Picking Up");
                StartCoroutine(PickUpRoutine());
                break;

            case ParrotState.Flying:
                Debug.Log("Current State: Flying");
                MoveToTarget(targetPosition);
                if (ReachedTarget(targetPosition))
                {
                    Debug.Log("Reached Position B, transitioning to Drop");
                    flyAnimator.SetTrigger("isLanding");
                    currentState = ParrotState.Dropping;
                }
                break;

            case ParrotState.Dropping:
                Debug.Log("Current State: Dropping");
                StartCoroutine(DropRoutine());
                break;

            case ParrotState.Returning:
                Debug.Log("Current State: Returning");
                MoveToTarget(targetPosition);
                if (ReachedTarget(targetPosition))
                {
                    Debug.Log("Returned to initial position, transitioning to Walking");
                    SwitchToWalkRig();
                    walkAnimator.SetBool("isWalking", true);
                    currentState = ParrotState.Walking;
                }
                break;
        }
    }

    void MoveToTarget(Vector3 target)
    {
        Debug.Log("Moving towards target: " + target);
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    bool ReachedTarget(Vector3 targetPosition)
    {
        bool reached = Vector3.Distance(transform.position, targetPosition) < 0.1f;
        if (reached)
        {
            Debug.Log("Reached target position: " + targetPosition);
        }
        return reached;
    }

    IEnumerator PickUpRoutine()
    {
        yield return new WaitForSeconds(1f);  // Wait for pickup animation
        Debug.Log("Finished PickUp, switching to FlyRig and transitioning to Flying");
        SwitchToFlyRig();
        flyAnimator.SetTrigger("isFlyingOpenMouth");
        currentState = ParrotState.Flying;
        targetPosition = positionB.position;
    }

    IEnumerator DropRoutine()
    {
        yield return new WaitForSeconds(1f);  // Wait for drop animation
        Debug.Log("Finished Drop, transitioning to Returning");
        flyAnimator.SetTrigger("isFlying");
        currentState = ParrotState.Returning;
        targetPosition = positionA.position;  // Return to initial position (Position A)
    }

    void SwitchToWalkRig()
    {
        Debug.Log("Switching to WalkRig");
        Kiki_Walk_Rig.SetActive(true);
        Kiki_Fly_Rig.SetActive(false);
    }

    void SwitchToFlyRig()
    {
        Debug.Log("Switching to FlyRig");
        Kiki_Walk_Rig.SetActive(false);
        Kiki_Fly_Rig.SetActive(true);
    }
}
