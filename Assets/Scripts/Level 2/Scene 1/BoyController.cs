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
    private bool isTalking = false;
    private bool collidersEnabled = false;

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
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                isIdleCompleted = true;
                isWalking = true;
                animator.SetBool("isWalking", true);
                Debug.Log("Idle complete, boy starts walking.");
            }
        }

        if (isWalking)
        {
            WalkToStopPosition();
        }

        if (isWalkCompleted && !isTalking)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Talk 1") &&
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                isTalking = true;  // Marks the boy's talking phase as complete
                birdAnimator.SetBool("can talk", true);  // Enable the bird to start talking
                
            }
        }

        if (isTalking && !collidersEnabled)
        {
            if (birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Bird Talk") &&
                birdAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                enableColliders();
                birdAnimator.SetBool("talkCompleted", true);
                collidersEnabled = true;  // Marks that colliders are now enabled                
            }
            
        }
    }


    void WalkToStopPosition()
    {
        if (stopPosition != null)
        {
            Vector3 targetPosition = new Vector3(stopPosition.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);

            if (Mathf.Abs(transform.position.x - stopPosition.position.x) < 0.1f)
            {
                isWalking = false;
                isWalkCompleted = true;
                animator.SetBool("isWalkCompleted", true);  // Marks walk as complete and transitions to talking       
               
            }
        }
    }

    void enableColliders()
    {
        if (Bus != null && Whale != null && Building != null)
        {
            Collider2D busCollider = Bus.GetComponent<Collider2D>();
            Collider2D whaleCollider = Whale.GetComponent<Collider2D>();
            Collider2D buildingCollider = Building.GetComponent<Collider2D>();

            if (busCollider != null && whaleCollider != null && buildingCollider != null)
            {
                busCollider.enabled = true;
                whaleCollider.enabled = true;
                buildingCollider.enabled = true;                
            }
            else
            {
                Debug.LogError("One or more collider components are missing.");
            }
        }
        else
        {
            Debug.LogError("One or more objects are missing.");
        }
    }
}
