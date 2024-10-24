using UnityEngine;
using System.Collections;


public class Lvl6Sc1JojoController : MonoBehaviour
{
    public Transform stopPosition;
    public float moveSpeed = 2f;
    public GameObject speechBubblePrefab;
    public Transform prefabSpawnPosition;
    public GameObject stCanvasPrefab;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isWalking;
    private bool hasReachedStopPosition;
    private bool isTalking;
    public bool hasSpawnedPrefab;
    private bool isFinalTalkCompleted;

    private GameObject spawnedPrefab;
    private MiniGameController miniGameController;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        miniGameController = FindObjectOfType<MiniGameController>(); // Get reference to MiniGameController


        isWalking = false;
        hasReachedStopPosition = false;
        isTalking = false;
        hasSpawnedPrefab = false;
        isFinalTalkCompleted = false;

        MoveToStopPosition();
    }

    private void Update()
    {
        if (isWalking && !hasReachedStopPosition)
        {
            MoveCharacter();
        }

        // Check if the "Talk" animation has completed
        if (isTalking && IsAnimationStateComplete("Talk"))
        {
            OnTalkAnimationComplete();
        }

        // Check if the "Talk 0" animation is complete and handle it
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Talk 0") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            if (miniGameController != null && miniGameController.CompletedMiniGamesCount >= 3)
            {
                animator.SetBool("finalTalk", true);
                Debug.Log("All mini-games completed, triggering final talk animation.");
            }
            else
            {
                animator.SetBool("canTalk", false);

                if (!hasSpawnedPrefab)
                {
                    SpawnSpeechBubble();
                    hasSpawnedPrefab = true;
                    Debug.Log("Talk 0 completed, spawning speech bubble once.");
                }

            }
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Final Talk") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f && !isFinalTalkCompleted)
        {
            isFinalTalkCompleted = true; // Ensure this block runs only once
            OnFinalTalkComplete();
        }
    }

    private void MoveToStopPosition()
    {
        isWalking = true;
        animator.SetBool("canWalk", true); // Start walking animation

        // Flip the sprite based on the direction to the stop position
        spriteRenderer.flipX = stopPosition.position.x < transform.position.x;
    }

    private void MoveCharacter()
    {
        // Move Jojo towards the stop position
        transform.position = Vector3.MoveTowards(transform.position, stopPosition.position, moveSpeed * Time.deltaTime);

        // Check if Jojo has reached the stop position
        if (Vector3.Distance(transform.position, stopPosition.position) < 0.1f)
        {
            spriteRenderer.flipX = false;
            hasReachedStopPosition = true;
            isWalking = false;
            animator.SetBool("canWalk", false);
            animator.SetBool("canTalk", true); // Start talking animation            
            isTalking = true;
        }
    }

    private void OnTalkAnimationComplete()
    {
        isTalking = false;
        animator.SetBool("canTalk", false);
        SpawnSpeechBubble();
    }

    private void SpawnSpeechBubble()
    {
        if (speechBubblePrefab != null)
        {

            spawnedPrefab = Instantiate(speechBubblePrefab, prefabSpawnPosition.position, Quaternion.identity);

            BoxCollider2D collider = spawnedPrefab.AddComponent<BoxCollider2D>();

            var prefabHandler = spawnedPrefab.AddComponent<PrefabTouchHandler>();
            prefabHandler.Initialize(stCanvasPrefab);

            prefabHandler.OnPrefabTapped = () => OnPrefabTapped();
        }
    }

    private void OnPrefabTapped()
    {
        // Activate the ST canvas
        stCanvasPrefab.SetActive(true);
    }

    // Check if a specific animation state is complete
    private bool IsAnimationStateComplete(string stateName)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
    }

    private void OnFinalTalkComplete()
    {
        // Start the final walk
        animator.SetTrigger("FinalWalk");
        Debug.Log("Final Talk completed, starting final walk.");

        // Move character to the right
        MoveCharacterRight();
    }

    private void MoveCharacterRight()
    {
        isWalking = true;

        // Flip the sprite to face right
        spriteRenderer.flipX = false;

        // Move to the right by continuously updating position
        StartCoroutine(MoveRightCoroutine());
    }

    private IEnumerator MoveRightCoroutine()
    {
        // Move continuously to the right
        while (isWalking)
        {
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
            yield return null;
        }
    }
}
