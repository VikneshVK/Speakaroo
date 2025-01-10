using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class BoyController1 : MonoBehaviour
{
    public Transform stopPosition;
    public float walkSpeed = 2f;

    public GameObject Bird;
    private ParrotController parrotController;
    public GameObject Bus;
    public GameObject Whale;
    public GameObject Building;
    private AudioSource jojoAudio;
   /* public AudioSource kikiAudio; //temp addition,please change later*/
    public AudioClip[] audioClips; // Array to hold the audio clips
   /* public TextMeshProUGUI subtitleText;*/
   public SubtitleManager subtitleManager;

    [Header("SFX")]
    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;
    private bool walkingSfxPlayed = false;

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
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
        parrotController = Bird.GetComponent<ParrotController>();
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
            boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.1f)
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
            if (!walkingSfxPlayed)
            {
                SfxAudioSource.loop = true;
                walkingSfxPlayed = true;
                SfxAudioSource.clip = SfxAudio1;
                SfxAudioSource.Play();
            }
            // Check if the boy has reached the stop position
            if (Mathf.Abs(transform.position.x - stopPosition.position.x) < 0.1f)
            {
                SfxAudioSource.Stop();
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
        
        boyAnimator.SetBool("canTalk", true);  // Enable the "canTalk" animation after the delay
        PlayAudioWithSubtitles(0, "Oh no..!, My room is so messy.", "JoJo");
        Debug.Log("Walking complete, switching back to front view.");
        
    }

    private void HandleTalking()
    {
        if (isWalkCompleted && !isTalking && boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dialouge1") &&
            boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            
            boyAnimator.SetBool("canTalk", false);
            isTalking = true;
            birdAnimator.SetBool("can talk", true);
            parrotController.SpawnAndTweenGlowOnInteractableObjects();
            PlayAudioWithSubtitles(3, "Put the big toys on the shelf.", "Kiki" ); //added temp, change after
            
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
                PlayAudioWithSubtitles(2, "Wow, Thank you, Kiki and Friend. My room looks so clean.", "JoJo");  // Play audio
                
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
    public void PlayAudioWithSubtitles(int index, string subtitleText, string dialogueType)
    {
        if (jojoAudio != null && audioClips != null && index >= 0 && index < audioClips.Length)
        {
            // Set and play the audio clip
            jojoAudio.clip = audioClips[index];
            jojoAudio.loop = false; // Ensure that the audio does not loop
            jojoAudio.Play();

            // Display subtitles using the currently set audio clip
            subtitleManager.DisplaySubtitle(subtitleText, dialogueType, jojoAudio.clip);
        }
        else
        {
            Debug.LogWarning("AudioSource is missing, audio clip is out of range, or index is invalid.");
        }
    }



}
