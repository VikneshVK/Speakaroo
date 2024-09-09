using UnityEngine;

public class Kiki_actions : MonoBehaviour
{
    public Transform birdStopPosition;
    public Transform offScreenPosition;
    public Transform basketFinalPosition;
    public Transform TeddyFinalPosition;
    public Transform DinoFinalPosition;
    public Transform BunnyFinalPosition;
    public GameObject toysBasket;
    public GameObject Teddy;
    public GameObject Dino;
    public GameObject Bunny;
    public float flySpeed = 2f;
    public bool hasReachedOffScreen = false; // Boolean to track off-screen position

    private Animator animator;
    /*private SpriteRenderer spriteRenderer;*/

    private bool isFlying = false;
    private bool isIdleCompleted = false;
    public bool isReturning = false; // Flag for return trip

    private Vector3 targetPosition; // Store the current target position
    private AudioSource kikiAudio;

    void Start()
    {
        animator = GetComponent<Animator>();
        kikiAudio = GetComponent<AudioSource>();    
        /*spriteRenderer = GetComponent<SpriteRenderer>();*/
    }

    void Update()
    {
        if (isFlying)
        {
            FlyToPosition(targetPosition);
        }
        else if (!isReturning)
        {
            HandleIdleCompletion();
        }
    }

    private void HandleIdleCompletion()
    {
        if (!isIdleCompleted && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            isIdleCompleted = true;
            MoveToStopPosition(); // Start flying to the stop position after idle completes
        }
    }

    public void MoveToStopPosition()
    {
        /*spriteRenderer.flipX = true; // Ensure sprite is facing correct direction*/
        animator.SetBool("canFly", true);
        targetPosition = birdStopPosition.position;
        isFlying = true;
        isReturning = false;
    }

    public void MoveOffScreen()
    {
        /*spriteRenderer.flipX = false; // Flip X to true when flying off-screen*/
        animator.SetTrigger("canFly2");
        targetPosition = offScreenPosition.position;
        kikiAudio.Play();
        isFlying = true;
    }

    public void ReturnToStopPosition()
    {
        /*spriteRenderer.flipX = true; // Flip X to false when returning to stop position*/
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

            if (isReturning)
            {
                EnableAllColliders();
                NotifyHelperHand(); // Notify the helper hand controller
            }
            else if (targetPosition == birdStopPosition.position && !isReturning)
            {
                OnReachedStopPosition();
                TweenToysToPosition();
            }
            else if (targetPosition == offScreenPosition.position)
            {
                hasReachedOffScreen = true; // Set the boolean to true when reaching off-screen
            }
        }
    }

    private void TweenToysToPosition()
    {
        LeanTween.move(toysBasket, basketFinalPosition.position, 1f).setEase(LeanTweenType.easeOutBack);
        LeanTween.move(Teddy, TeddyFinalPosition.position, 1f).setEase(LeanTweenType.easeOutBack);
        LeanTween.move(Dino, DinoFinalPosition.position, 1f).setEase(LeanTweenType.easeOutBack);
        LeanTween.move(Bunny, BunnyFinalPosition.position, 1f).setEase(LeanTweenType.easeOutBack);
    }

    private void OnReachedStopPosition()
    {
        // This function is called when Kiki reaches the stop position
        Debug.Log("Kiki has reached the stop position.");
        // Add any additional logic here, such as enabling interaction, triggering animations, etc.
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

    private void NotifyHelperHand()
    {
        // Notify the HelpHandController for Kiki's action
        HelpHandController helperHand = FindObjectOfType<HelpHandController>();
        if (helperHand != null)
        {
            helperHand.StartHelperHandRoutineForKiki(new GameObject[]
            {
            GameObject.Find("wet_socK"),
            GameObject.Find("wet_shirT"),
            GameObject.Find("wet kuma"),
            GameObject.Find("wet_shorT"),
            GameObject.Find("wet dino"),
            GameObject.Find("wet bunny")
            });
        }
    }
}
