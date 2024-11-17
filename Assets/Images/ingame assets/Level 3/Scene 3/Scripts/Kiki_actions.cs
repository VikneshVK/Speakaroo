using UnityEngine;

public class Kiki_actions : MonoBehaviour
{
    public Transform birdStopPosition;
    public Transform offScreenPosition;
  
    public float flySpeed = 2f;
    public bool hasReachedOffScreen = false; // Boolean to track off-screen position

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isFlying = false;
    private bool positionreached = false;
    private bool isIdleCompleted = false;
    public bool isReturning = false; // Flag for return trip

    private Vector3 targetPosition; // Store the current target position
    private AudioSource kikiAudio;

    void Start()
    {
        animator = GetComponent<Animator>();
        kikiAudio = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isFlying)
        {
            FlyToPosition(targetPosition);
        }
        else if (!isReturning && !positionreached)
        {
            MoveToStopPosition();
        }
    }

    

    public void MoveToStopPosition()
    {
        spriteRenderer.flipX = false; // Ensure sprite is facing correct direction
        animator.SetBool("canFly", true);
        targetPosition = birdStopPosition.position;
        isFlying = true;
        isReturning = false;
    }

    public void MoveOffScreen()
    {
        spriteRenderer.flipX = true; // Flip X to true when flying off-screen
        animator.SetBool("canFly2", true);
        targetPosition = offScreenPosition.position;
        /*kikiAudio.Play();*/
        isFlying = true;
    }

    public void ReturnToStopPosition()
    {
        spriteRenderer.flipX = false; // Flip X to false when returning to stop position
        isReturning = true;
        animator.SetBool("canFly", true);
        targetPosition = birdStopPosition.position;
        isFlying = true;
    }

    private void FlyToPosition(Vector3 targetPosition)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, flySpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
        {
            isFlying = false; // Stop flying
            animator.SetBool("canFly", false);
            animator.SetBool("canFly2", false);

            if (isReturning)
            {
                EnableAllColliders();
               /* NotifyHelperHand(); /*//*/ Notify the helper hand controller*/
            }
            else if (targetPosition == birdStopPosition.position && !isReturning)
            {
                positionreached = true;
                              
            }
            else if (targetPosition == offScreenPosition.position)
            {
                hasReachedOffScreen = true; // Set the boolean to true when reaching off-screen
            }
        }
    }

  


    private void EnableAllColliders()
    {
        // Enable all colliders in the scene
        string[] gameObjectNames = { "wet_socK", "wet_shirT", "wet kuma", "wet_shorT", "wet dino", "wet bunny" };
        foreach (string name in gameObjectNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Collider2D collider = obj.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }
        }
    }

   
}
