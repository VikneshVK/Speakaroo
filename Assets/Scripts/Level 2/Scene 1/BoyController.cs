using UnityEngine;

public class BoyController : MonoBehaviour
{
    public Transform stopPosition;
    public Transform endPosition;
    public float walkSpeed = 2f;
    public GameObject Bus;
    public GameObject Whale;
    public GameObject Building;
    public GameObject Bird;

    private Animator animator;
    private Animator birdAnimator;
    private SpriteRenderer spriteRenderer;
    private bool isWalking = false;
    private bool isIdleCompleted = false;
    private bool isTalking = false;
    private bool collidersEnabled = false;
    private bool isWalkCompleted = false;
    private bool isReturning = false;


    void Start()
    {
        animator = GetComponent<Animator>();
        birdAnimator = Bird.GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (stopPosition == null)
        {
            Debug.LogError("Stop position not set for BoyController.");
        }
    }

    void Update()
    {
        HandleIdleCompletion();
        HandleWalking();
        HandleTalking();
        HandleCollidersEnabling();
        HandleFinalTalkAndReturn();
    }

    private void HandleIdleCompletion()
    {
        if (!isIdleCompleted && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            isIdleCompleted = true;
            isWalking = true;
            animator.SetBool("isWalking", true);
            Debug.Log("Idle complete, boy starts walking.");
        }
    }

    private void HandleWalking()
    {
        if (isWalking && !isReturning)
        {
            WalkToStopPosition();
        }
    }

    private void WalkToStopPosition()
    {
        if (stopPosition != null)
        {
            Vector3 targetPosition = new Vector3(stopPosition.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);

            if (Mathf.Abs(transform.position.x - stopPosition.position.x) < 0.1f)
            {
                isWalking = false;
                isWalkCompleted = true;
                animator.SetBool("isWalkCompleted", true);
                Debug.Log("Walking complete, boy stops.");
            }
        }
    }

    private void HandleTalking()
    {
        if (isWalkCompleted && !isTalking && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle_1") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            isTalking = true;
            birdAnimator.SetBool("can talk", true);
        }
    }

    private void HandleCollidersEnabling()
    {
        if (isTalking && !collidersEnabled && birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Bird Talk") &&
            birdAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            EnableColliders();
            birdAnimator.SetBool("talkCompleted", true);
            collidersEnabled = true;
        }
    }

    private void EnableColliders()
    {
        if (Bus != null && Whale != null && Building != null)
        {
            Collider2D busCollider = Bus.GetComponent<Collider2D>();
            Collider2D whaleCollider = Whale.GetComponent<Collider2D>();
            Collider2D buildingCollider = Building.GetComponent<Collider2D>();

            if (busCollider != null) busCollider.enabled = true;
            if (whaleCollider != null) whaleCollider.enabled = true;
            if (buildingCollider != null) buildingCollider.enabled = true;
        }
        else
        {
            Debug.LogError("One or more objects are missing.");
        }
    }

    private void HandleFinalTalkAndReturn()
    {
        if (birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("bird talk_") &&
            birdAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            animator.SetBool("cleaningComplete", true);
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("final talk") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.96f)
        {
            spriteRenderer.flipX = true;  // Flip sprite to walk from left to right
            isReturning = true;
            isWalking = true;  // Ensure walking continues
        }

        if (isReturning && isWalking)
        {
            MoveBackOnXAxis();
        }
    }


    private void MoveBackOnXAxis()
    {
        transform.position += Vector3.right * walkSpeed * Time.deltaTime;       
    }
}
