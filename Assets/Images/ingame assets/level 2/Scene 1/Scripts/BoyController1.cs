using System.Collections;
using TMPro;
using UnityEngine;

public class BoyController1 : MonoBehaviour
{
    public Transform stopPosition;
    public float walkSpeed = 2f;

    public GameObject Bird;
    public GameObject Bus;
    public GameObject Whale;
    public GameObject Building;
    public AudioSource jojoAudio;
   /* public AudioSource kikiAudio; //temp addition,please change later*/
    public AudioClip[] audioClips; // Array to hold the audio clips
    public TextMeshProUGUI subtitleText;

    private Animator boyAnimator;
    private SpriteRenderer boyspriteRenderer;
    private Animator birdAnimator;
    private bool isWalking = false;
    private bool isIdleCompleted = false;
    private bool isWalkCompleted = false;
    private bool isTalking = false;
    private bool collidersEnabled = false;
    private bool isReturning = false;


    //for final audio
    private bool hasFinalAudioPlayed;

    void Start()
    {
        // Initialize animators for front and side views
        boyAnimator = GetComponent<Animator>();
        boyspriteRenderer = GetComponent<SpriteRenderer>();
        birdAnimator = Bird.GetComponent<Animator>();
        jojoAudio = GetComponent<AudioSource>();



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
        // Check if the idle animation on the front view is complete
        if (!isIdleCompleted && boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            isIdleCompleted = true;

            
            boyAnimator.SetBool("isWalking", true);
            isWalking = true;

            Debug.Log("Idle complete, switching to side view for walking.");
        }
    }

    private void HandleWalking()
    {
        if (isWalking && !isWalkCompleted)
        {
            boyspriteRenderer.flipX = true;
            WalkToStopPosition();
        }
    }

    private void WalkToStopPosition()
    {
        if (stopPosition != null)
        {
            Vector3 targetPosition = new Vector3(stopPosition.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);

            // Check if the boy has reached the stop position
            if (Mathf.Abs(transform.position.x - stopPosition.position.x) < 0.1f)
            {
                isWalking = false;
                isWalkCompleted = true;
                boyspriteRenderer.flipX = false;
                boyAnimator.SetBool("isWalking", false);
                StartCoroutine(DelayBeforeCanTalk());// Start coroutine to delay the "canTalk" animation                
            }
        }
    }

    private IEnumerator DelayBeforeCanTalk()
    {
        yield return new WaitForSeconds(0.3f);  // Wait for 1 second
        /*boyspriteRenderer.flipX = false;*/
        boyAnimator.SetBool("canTalk", true);  // Enable the "canTalk" animation after the delay
        PlayAudioByIndex(0);
        Debug.Log("Walking complete, switching back to front view.");
        if (subtitleText != null)
        {
            StartCoroutine(RevealTextWordByWord("Oh No..!, My Room is So Messy", 0.5f));  // Word by word reveal with 0.5s delay
        }
    }

    private void HandleTalking()
    {


        if (isWalkCompleted && !isTalking && boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dialouge1") &&
            boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            
            boyAnimator.SetBool("canTalk", false);
            isTalking = true;
            birdAnimator.SetBool("can talk", true);
            PlayAudioByIndex(3); //added temp, change after
            StartCoroutine(RevealTextWordByWord("Put the Toys on the Shelf", 0.5f));
            boyspriteRenderer.flipX = true;
        }
    }

    private void HandleCollidersEnabling()
    {
        if (isTalking && !collidersEnabled && birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Bird Talk") &&
            birdAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            EnableColliders();
            birdAnimator.SetBool("TalkCompleted", true);
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
            birdAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            if (!hasFinalAudioPlayed)
            {
                boyAnimator.SetTrigger("CleaningComplete");
                PlayAudioByIndex(2);  // Play audio
                if (subtitleText != null)
                {
                    StartCoroutine(RevealTextWordByWord("Wow.! My Room Looks so Clean. Thankyou Kiki and Friend", 0.5f));  // Word by word reveal with 0.5s delay
                }
                hasFinalAudioPlayed = true; // Mark as played
                Debug.Log("final audio playing once");
            }

        }

        if (boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dialouge2") &&
            boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            isReturning = true;
            isWalking = true;  // Ensure walking continues            
            boyAnimator.SetBool("isWalking", true);
        }

        if (isReturning && isWalking)
        {
            MoveBackOnXAxis();
        }
    }

    private void MoveBackOnXAxis()
    {
        boyspriteRenderer.flipX = false;
        transform.position += Vector3.right * walkSpeed * Time.deltaTime;
    }

    // Public method to play audio based on index
    public void PlayAudioByIndex(int index)
    {
        if (jojoAudio != null && audioClips != null && index >= 0 && index < audioClips.Length)
        {

            jojoAudio.clip = audioClips[index];
            jojoAudio.loop = false; // Ensure that the audio does not loop
            jojoAudio.Play();

        }
        else
        {
            Debug.LogWarning("AudioSource is missing, audio clip is out of range, or index is invalid.");
        }
    }

    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";  // Clear the text before starting
        subtitleText.gameObject.SetActive(true);  // Ensure the subtitle text is active

        string[] words = fullText.Split(' ');  // Split the full text into individual words

        // Reveal words one by one
        for (int i = 0; i < words.Length; i++)
        {
            // Instead of appending, build the text up to the current word
            subtitleText.text = string.Join(" ", words, 0, i + 1);  // Show only the words up to the current index
            yield return new WaitForSeconds(delayBetweenWords);  // Wait before revealing the next word
        }
        subtitleText.gameObject.SetActive(false);
    }

}
