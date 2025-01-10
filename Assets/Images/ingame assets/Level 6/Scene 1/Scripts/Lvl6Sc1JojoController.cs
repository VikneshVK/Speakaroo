using UnityEngine;
using System.Collections;
using TMPro;
using static UnityEngine.Rendering.DebugUI;

public class Lvl6Sc1JojoController : MonoBehaviour
{
    public Transform stopPosition;
    public float moveSpeed = 2f;
    public GameObject speechBubblePrefab;
    public Transform prefabSpawnPosition;
    public GameObject stCanvasPrefab;
    public GameObject panel;
   

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isWalking;
    private bool dialougeCompleted;
    private bool hasReachedStopPosition;
    private bool isTalking;
    /*private bool dialougeDone;*/
    public bool hasSpawnedPrefab;
    private bool isFinalTalkCompleted;
    public SubtitleManager subtitleManager;

    public AudioClip audio1;
    public AudioClip audio2;
    public AudioClip audio3;

    public AudioSource audioSource;

    private GameObject spawnedPrefab;
    private MiniGameController miniGameController;

    // Kiki and its Animator reference
    public GameObject kiki;
    private Animator kikiAnimator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        miniGameController = FindObjectOfType<MiniGameController>(); // Get reference to MiniGameController
        audioSource = GetComponent<AudioSource>();
        // Initialize Kiki's Animator
        if (kiki != null)
        {
            kikiAnimator = kiki.GetComponent<Animator>();
        }

        isWalking = false;
        hasReachedStopPosition = false;
        isTalking = false;
        hasSpawnedPrefab = false;
        isFinalTalkCompleted = false;
        dialougeCompleted = false;

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
            animator.SetBool("canTalk", false);
            isTalking = false;
            if (kikiAnimator != null)
            {
                kikiAnimator.SetTrigger("Dialogue1");
                audioSource.clip = audio1;
                audioSource.Play();
                subtitleManager.DisplaySubtitle("What do you want to play with?", "Kiki", audio1);
            }
            StartCoroutine(WaitForKikiTalkAnimation());
        }

        // Check if the "Talk 0" animation is complete and handle it
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Talk 0") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            if (miniGameController != null && miniGameController.CompletedMiniGamesCount >= 3)
            {
                animator.SetBool("finalTalk", true);
                kikiAnimator.SetTrigger("finalTalk");
                audioSource.clip = audio3;
                audioSource.Play();
                subtitleManager.DisplaySubtitle("The beach was a blast", "JoJo", audio3);
                Debug.Log("All mini-games completed, triggering final talk animation.");
            }
            else
            {
                animator.SetBool("canTalk", false);

                if (!hasSpawnedPrefab && !dialougeCompleted)
                {
                    dialougeCompleted = true;
                    /*dialougeDone = false;*/
                    kikiAnimator.SetTrigger("Dialogue1"); // Trigger Kiki's Talk animation
                    audioSource.clip = audio1;
                    audioSource.Play();
                    subtitleManager.DisplaySubtitle("What do you want to play with?", "Kiki", audio1);
                    StartCoroutine(WaitForKikiTalkAnimation());
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
            audioSource.clip = audio2;
            audioSource.Play();
            subtitleManager.DisplaySubtitle("I love the beach ", "JoJo", audio2);
            isTalking = true;
        }
    }

    private void OnTalkAnimationComplete()
    {
        isTalking = false;
        animator.SetBool("canTalk", false);
        SpawnSpeechBubble();
    }

    private IEnumerator WaitForKikiTalkAnimation()
    {
        // Wait until Kiki's talk animation is complete
        yield return new WaitUntil(() => kikiAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talk") && kikiAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f);

        if (!hasSpawnedPrefab)
        {
            SpawnSpeechBubble();
            hasSpawnedPrefab = true;
            dialougeCompleted = false;
            Debug.Log("Talk 0 completed, spawning speech bubble once.");
        }
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
        LeanTween.scale(panel, Vector3.one, 0.5f)
            .setEase(LeanTweenType.easeInBack);
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
