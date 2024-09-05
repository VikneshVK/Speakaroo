using UnityEngine;
using System.Collections;

public class Bird_Controller : MonoBehaviour
{
    public Transform initialPosition; // Reference to the initial position from the Inspector
    public Transform target1; // Reference to the first small leaf (target 1)
    public Transform target2; // Reference to the second small leaf (target 2)
    public Collider2D dustbinCollider;
    public Animator birdAnimator;
    public Animator trashBinAnimator;
    public GameObject boyController;

    public Collider2D bigLeaf1Collider; // Reference to the Big Leaf 1 collider
    public Collider2D bigLeaf2Collider; // Reference to the Big Leaf 2 collider

    private static int totalLeaves; // Static variable to keep track of the number of small leaves
    private Transform currentTarget; // To track the current target position
    private JojoController1 boycontrolscript;

    private bool isTalking ;
    private bool isPickingLeaves ;
    private bool isDroppingLeaves;
    private bool isReturningToInitialPosition;
    private bool IsleavesDropped;

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
        isTalking = false;
        isPickingLeaves = false;
        isDroppingLeaves = false;
        isReturningToInitialPosition = false;
        IsleavesDropped = false;
        totalLeaves = 2;
        boycontrolscript = boyController.GetComponent<JojoController1>();
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
            // Bird is flying to the dustbin with leaves.
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

        if(stateInfo.IsName("Idle1") && totalLeaves == 0)
        {
            birdAnimator.SetBool("levelComplete", true) ;
        }
    }

    void MoveToPosition(Vector3 targetPosition, string nextState)
    {
        LeanTween.move(gameObject, targetPosition, 5f).setOnComplete(() =>
        {
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

    public void EnableBigLeavesColliders()
    {
        if (bigLeaf1Collider != null && bigLeaf2Collider != null)
        {
            bigLeaf1Collider.enabled = true;
            bigLeaf2Collider.enabled = true;

            // Start the inactivity timer in HP_HelperpointerController
            FindObjectOfType<HP_HelperpointerController>().StartInactivityTimer();
        }
    }


    public void OnBigLeafDropped()
    {
        birdAnimator.SetBool("isFlying", true);
        IsleavesDropped = false;
        DetermineNextTarget();
    }

    void DetermineNextTarget()
    {
        if (totalLeaves == 2 && !IsleavesDropped)
        {
            currentTarget = target1;
        }
        else if (totalLeaves == 1 && !IsleavesDropped)
        {
            currentTarget = target2;
        }

        MoveToPosition(currentTarget.position, "Pick leaves");
    }

    void OnPickLeavesComplete()
    {
        if (currentTarget != null && isPickingLeaves)
        {
            // Deactivate the Anchor script on the current target (leaves)
            var anchorScript = currentTarget.GetComponent<AnchorGameObject>();
            if (anchorScript != null)
            {
                anchorScript.enabled = false;
            }

            // Find the 'Mouth' GameObject within the bird's hierarchy
            Transform mouthTransform = transform.Find("RootBone/HeadBone/Mouth/LeafPosition");
            if (mouthTransform != null)
            {
                currentTarget.SetParent(mouthTransform);
                currentTarget.localPosition = Vector3.zero;
            }

            // Add a 1-second delay before updating the booleans
            StartCoroutine(DelayBeforeBooleanChange());
        }
    }

    IEnumerator DelayBeforeBooleanChange()
    {
        yield return new WaitForSeconds(1f); // Wait for 1 second

        // Update flags and set Animator parameters to transition to Kiki_Takeoff_mouth open
        isPickingLeaves = false;
        birdAnimator.SetBool("isFlying", true);
        birdAnimator.SetBool("canLand", false);
        trashBinAnimator.SetBool("binOpen", true);

        MoveToPosition(dustbinCollider.bounds.center, "Kiki_Takeoff_mouth open");
    }

    public void OnDropLeavesComplete()
    {
        if (currentTarget != null)
        {
            // Disable the SpriteRenderer to make the current target invisible
            SpriteRenderer spriteRenderer = currentTarget.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            // Remove the currentTarget from being a child of the bird
            currentTarget.SetParent(null);

            if (!IsleavesDropped)
            {
                totalLeaves--;
                IsleavesDropped = true;
            }


            // Log the change in totalLeaves
            Debug.Log($"Total leaves remaining: {totalLeaves}");

            // Reset the target to the initial position and prepare for the next cycle
            currentTarget = initialPosition;

            // Add a 1-second delay before updating the booleans
            StartCoroutine(DelayBeforeBooleanChangeAfterDrop());
        }
    }

    IEnumerator DelayBeforeBooleanChangeAfterDrop()
    {
        yield return new WaitForSeconds(1f); // Wait for 1 second

        // Update Animator parameters to transition to KIKI_takeoff state
        birdAnimator.SetBool("canLand", false);
        birdAnimator.SetBool("isFlying", true);
        trashBinAnimator.SetBool("binOpen", false);

        // Move the bird back to the initial position
        MoveToPosition(initialPosition.position, "Kiki_flying_mouth open_turn");
    }

    public void OnReachedInitialPosition()
    {
        // Add a 1-second delay before setting the Animator parameters
        StartCoroutine(DelayBeforeReachedInitialPosition());
    }

    IEnumerator DelayBeforeReachedInitialPosition()
    {
        yield return new WaitForSeconds(1f); // Wait for 1 second

        // Set Animator parameters to transition to Kiki_landing_Turn
        birdAnimator.SetBool("isFlying", false);
        birdAnimator.SetBool("canLand", true);
    }

    public void OnReturnToInitialPositionComplete()
    {
        birdAnimator.SetBool("canTalk", true);
        isReturningToInitialPosition = false;
    }

    public void OnTalkComplete()
    {
        // Start the coroutine to add a 2-second delay before enabling the big leaves colliders
        StartCoroutine(DelayedEnableBigLeavesColliders());
    }

    IEnumerator DelayedEnableBigLeavesColliders()
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds
        EnableBigLeavesColliders(); // Call the method to enable the colliders
    }
    
}
