using UnityEngine;

public class Bird_Controller : MonoBehaviour
{
    public Transform initialPosition; // Reference to the initial position from the Inspector
    public Transform target1; // Reference to the first small leaf (target 1)
    public Transform target2; // Reference to the second small leaf (target 2)
    public Transform dustbinPosition; // Reference to the dustbin position
    public Animator birdAnimator;

    public Collider2D bigLeaf1Collider; // Reference to the Big Leaf 1 collider
    public Collider2D bigLeaf2Collider; // Reference to the Big Leaf 2 collider

    private static int totalLeaves = 2; // Static variable to keep track of the number of small leaves
    private Transform currentTarget; // To track the current target position

    private bool isTalking = false;
    private bool isPickingLeaves = false;
    private bool isDroppingLeaves = false;
    private bool isReturningToInitialPosition = false;

    private void Awake()
    {
        LeanTween.init(2400);
    }

    void Start()
    {
        // Start the bird's journey by moving it to the initial position
        birdAnimator.SetBool("isFlying", true);
        birdAnimator.SetBool("canLand", false);
        MoveToPosition(initialPosition.position, "Idle");
    }

    void Update()
    {
        HandleAnimationStateTransitions();
    }

    void HandleAnimationStateTransitions()
    {
        AnimatorStateInfo stateInfo = birdAnimator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Idle") && !isTalking)
        {
            isTalking = true;
            birdAnimator.SetBool("canTalk", true);
        }

        if (stateInfo.IsName("Talk") && stateInfo.normalizedTime >= 0.95f && isTalking)
        {
            OnTalkComplete();  // Now only enables the colliders
        }

        if (stateInfo.IsName("Pick leaves") && isPickingLeaves && stateInfo.normalizedTime >= 0.95f)
        {
            OnPickLeavesComplete();
        }

        if (stateInfo.IsName("Kiki_Takeoff_mouth open") && stateInfo.normalizedTime >= 0.95f)
        {
            Debug.Log("Bird is flying to the dustbin with leaves.");
        }

        if (stateInfo.IsName("Kiki_flying_mouth open") && stateInfo.normalizedTime >= 0.95f)
        {
            birdAnimator.SetBool("isFlying", false);
            birdAnimator.SetBool("canLand", true);
        }

        if (stateInfo.IsName("Kiki_drop_trun") && stateInfo.normalizedTime >= 0.95f)
        {
            OnDropLeavesComplete();
        }

        if (stateInfo.IsName("Kiki_flying_mouth open_turn") && stateInfo.normalizedTime >= 0.95f)
        {
            OnReachedInitialPosition();
        }

        if (stateInfo.IsName("Kiki_landing_Turn") && stateInfo.normalizedTime >= 0.95f)
        {
            birdAnimator.SetBool("canTalk", true);
        }

        if (stateInfo.IsName("Idle1") && isReturningToInitialPosition)
        {
            OnReturnToInitialPositionComplete();
        }
    }

    void MoveToPosition(Vector3 targetPosition, string nextState)
    {
        LeanTween.move(gameObject, targetPosition, 2f).setOnComplete(() =>
        {
            Debug.Log($"Reached {targetPosition}, transitioning to {nextState}");
            if (nextState == "Idle")
            {
                birdAnimator.SetBool("isFlying", false);
                birdAnimator.SetBool("canLand", true);
            }
            else if (nextState == "Pick leaves")
            {
                isPickingLeaves = true;
                birdAnimator.SetBool("isFlying", false);
                birdAnimator.SetBool("canLand", true);
            }
            else if (nextState == "Kiki_Takeoff_mouth open")
            {
                birdAnimator.SetBool("isFlying", true);
                birdAnimator.SetBool("canLand", false);
            }
            else if (nextState == "Idle1")
            {
                isReturningToInitialPosition = true;
                birdAnimator.SetBool("isFlying", false);
                birdAnimator.SetBool("canLand", true);
            }
        });
    }

    void EnableBigLeavesColliders()
    {
        if (bigLeaf1Collider != null && bigLeaf2Collider != null)
        {
            bigLeaf1Collider.enabled = true;
            bigLeaf2Collider.enabled = true;
        }
        else
        {
            Debug.LogError("Big leaf colliders are not assigned in the inspector!");
        }
    }

    public void OnBigLeafDropped()
    {
        Debug.Log("Big leaf dropped, determining next target");
        birdAnimator.SetBool("isFlying", true);
        DetermineNextTarget();
    }

    void DetermineNextTarget()
    {
        if (totalLeaves == 2)
        {
            currentTarget = target1;
        }
        else if (totalLeaves == 1)
        {
            currentTarget = target2;
        }

        Debug.Log($"Next target determined: {currentTarget.name}");
        MoveToPosition(currentTarget.position, "Pick leaves");
    }

    public void OnPickLeavesComplete()
    {
        if (currentTarget != null && isPickingLeaves)
        {
            Debug.Log($"Picking leaves complete, setting {currentTarget.name} as child of bird");

            // Deactivate the Anchor script on the current target (leaves)
            var anchorScript = currentTarget.GetComponent<AnchorGameObject>();
            if (anchorScript != null)
            {
                anchorScript.enabled = false;
                Debug.Log("Anchor script deactivated on " + currentTarget.name);
            }
            else
            {
                Debug.LogWarning("Anchor script not found on " + currentTarget.name);
            }

            // Make the small leaves a child of the bird
            currentTarget.SetParent(this.transform);

            // Update flags and set Animator parameters to transition to Kiki_Takeoff_mouth open
            isPickingLeaves = false;
            birdAnimator.SetBool("isFlying", true);
            birdAnimator.SetBool("canLand", false);

            MoveToPosition(dustbinPosition.position, "Kiki_Takeoff_mouth open");
        }
        else
        {
            Debug.LogError("currentTarget is null during OnPickLeavesComplete or isPickingLeaves is false");
        }
    }

    public void OnDropLeavesComplete()
    {
        if (currentTarget != null)
        {
            Debug.Log("Dropping leaves complete, disabling currentTarget SpriteRenderer and removing from bird");

            // Disable the SpriteRenderer to make the current target invisible
            SpriteRenderer spriteRenderer = currentTarget.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
            else
            {
                Debug.LogWarning("SpriteRenderer not found on " + currentTarget.name);
            }

            // Remove the currentTarget from being a child of the bird
            currentTarget.SetParent(null);

            // Decrement the total leaves counter
            totalLeaves--;

            // Reset the target to the initial position and prepare for the next cycle
            currentTarget = initialPosition;

            // Update Animator parameters to transition to KIKI_tackup state
            birdAnimator.SetBool("canLand", false);
            birdAnimator.SetBool("isFlying", true);

            // Move the bird back to the initial position
            MoveToPosition(initialPosition.position, "Kiki_flying_mouth open_turn");
        }
        else
        {
            Debug.LogError("currentTarget is null during OnDropLeavesComplete");
        }
    }

    public void OnReachedInitialPosition()
    {
        Debug.Log("Bird has returned to the initial position.");

        // Set Animator parameters to transition to Kiki_landing_Turn
        birdAnimator.SetBool("isFlying", false);
        birdAnimator.SetBool("canLand", true);
    }

    public void OnReturnToInitialPositionComplete()
    {
        birdAnimator.SetBool("canTalk", true);
        isReturningToInitialPosition = false;
        Debug.Log("Returned to initial position, ready to talk again");
    }

    public void OnTalkComplete()
    {
        Debug.Log("Talk animation completed. Enabling big leaf colliders.");

        // Enable the colliders of the big leaves
        EnableBigLeavesColliders();

        // No need to change 'canTalk' or other Animator parameters here
    }
}
