using System;
using UnityEngine;

public class BoyController1 : MonoBehaviour
{
    public Transform stopPosition;
    public float walkSpeed = 2f;
    public GameObject FrontView;
    public GameObject SideView;
    public GameObject Bird;
    public GameObject Bus;
    public GameObject Whale;
    public GameObject Building;
    public AudioSource jojoAudio;
    public AudioSource kikiAudio; //temp addition,please change later
    public AudioClip[] audioClips; // Array to hold the audio clips

    private Animator frontViewAnimator;
    private Animator sideViewAnimator;
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
        frontViewAnimator = FrontView.GetComponent<Animator>();
        sideViewAnimator = SideView.GetComponent<Animator>();
        birdAnimator = Bird.GetComponent<Animator>();
        jojoAudio = GetComponent<AudioSource>();

        // Ensure the front view is active and the side view is inactive at the start
        FrontView.SetActive(true);
        SideView.SetActive(false);

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
        if (!isIdleCompleted && frontViewAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            frontViewAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            isIdleCompleted = true;

            // Switch to the side view for walking
            FrontView.SetActive(false);
            SideView.SetActive(true);

            // Enable walking animation on the side view
            sideViewAnimator.SetTrigger("canWalk");
            isWalking = true;

            Debug.Log("Idle complete, switching to side view for walking.");
        }
    }

    private void HandleWalking()
    {
        if (isWalking && !isWalkCompleted)
        {
            WalkToStopPosition();
        }
    }

    private void WalkToStopPosition()
    {
        if (stopPosition != null)
        {
            // Move the Boy Character towards the stop position
            Vector3 targetPosition = new Vector3(stopPosition.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);

            // Check if the boy has reached the stop position
            if (Mathf.Abs(transform.position.x - stopPosition.position.x) < 0.1f)
            {
                isWalking = false;
                isWalkCompleted = true;
                sideViewAnimator.SetTrigger("StopWalk");
                // Switch back to the front view
                SideView.SetActive(false);
                FrontView.SetActive(true);
                frontViewAnimator.SetTrigger("CanTalk");
                PlayAudioByIndex(0);
                Debug.Log("Walking complete, switching back to front view.");
                

            }
        }
    }

    private void HandleTalking()
    {
        

        if (isWalkCompleted && !isTalking && frontViewAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dialouge 1") &&
            frontViewAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            isTalking = true;
            birdAnimator.SetBool("can talk", true);
            PlayAudioByIndex(3); //added temp, change after
        }
    }

    private void HandleCollidersEnabling()
    {
        if (isTalking && !collidersEnabled && birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Bird Talk") &&
            birdAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
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
            if (!hasFinalAudioPlayed)
            {
                frontViewAnimator.SetTrigger("cleaningComplete");
                PlayAudioByIndex(2);  // Play audio
                hasFinalAudioPlayed = true; // Mark as played
                Debug.Log("final audio playing once");
            }

        }

        if (frontViewAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dialouge 2") &&
            frontViewAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.96f)
        {
            isReturning = true;
            isWalking = true;  // Ensure walking continues
            FrontView.SetActive(false);
            SideView.SetActive(true);
            sideViewAnimator.SetTrigger("canWalk2");
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

    
}
