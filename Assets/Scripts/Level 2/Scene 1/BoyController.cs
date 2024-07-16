using UnityEngine;

public class BoyController : MonoBehaviour
{
    public Transform stopPosition;
    public float walkSpeed = 2f;
    public bool isWalkCompleted = false;
    public GameObject Bus;
    public GameObject Whale;
    public GameObject Building;
    public GameObject Bird;

    private Animator animator;
    private Animator birdAnimator;
    private bool isWalking = false;
    private bool isIdleCompleted = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        birdAnimator = Bird.GetComponent<Animator>();

        if (stopPosition == null)
        {
            Debug.LogError("Stop position not set for BoyController.");
        }
    }

    void Update()
    {
        if (!isIdleCompleted)
        {
            // Check if the idle animation is done (assuming the idle animation sets this bool when done)
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                isIdleCompleted = true;
                isWalking = true;
                animator.SetBool("isWalking", true); // Assuming you have an animation parameter to start walking
            }
        }

        if (isWalking)
        {
            WalkToStopPosition();
        }
    }

    void WalkToStopPosition()
    {
        if (stopPosition != null)
        {
            // Only move along the x-axis
            Vector3 targetPosition = new Vector3(stopPosition.position.x, transform.position.y, transform.position.z);
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);

            // Check if the boy has reached the stop position
            if (Mathf.Abs(transform.position.x - stopPosition.position.x) < 0.1f)
            {
                isWalking = false;
                isWalkCompleted = true;
                animator.SetBool("isWalkCompleted", true); // Assuming you have an animation parameter to transition
                birdAnimator.SetBool("can talk", true) ;
                /*enableParrotTalk();*/
                enableColliders();
                Debug.Log("Walk completed.");
            }
        }
    }

   /* void enableParrotTalk()
    {
        if (birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Bird Talk") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            enableColliders();
        }
    }*/
    void enableColliders()
    {
        Collider2D busCollider = Bus.GetComponent<Collider2D>();
        Collider2D whaleCollider = Whale.GetComponent<Collider2D>();
        Collider2D buildingCollider = Building.GetComponent<Collider2D>();

        busCollider.enabled = true;
        whaleCollider.enabled = true;   
        buildingCollider.enabled = true;
    }
}
