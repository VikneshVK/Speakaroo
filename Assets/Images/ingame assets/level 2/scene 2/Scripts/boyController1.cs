using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class boyController1 : MonoBehaviour
{
    public Transform stopPosition;
    public Transform stopPosition2;
    public float walkSpeed = 2f;
    public GameObject nextaudiosoucre;
    public GameObject pillowBigRight;
    public GameObject pillowBigLeft;
    public GameObject pillowSmallLeft;
    public GameObject pillowSmallRight;
    public GameObject bedsheet;
    /* public GameObject walkingRig;
     public GameObject normalRig;*/
    public GameObject Bird;
    public AudioSource birdAudiosource;

    // Reference to the PillowDragAndDrop script
    public PillowDragAndDrop pillowDragAndDrop;
    public TextMeshProUGUI subtitleText;
    /* private Animator walkingAnimator;
     private Animator normalAnimator;*/
    private Animator BoyAnimator;
    private Animator birdAnimator;
    private SpriteRenderer boySprite;
    private bool pillowAudioPlayer;
    private bool isWalking;
    private bool hasReachedStopPosition;
    /*private bool shouldContinueWalking;*/
    private AudioSource audioSource;
    private bool isFinalWalk;
    private bool birdaudioplayed;
   
    private AudioClip audioClipBigPillow;
    private HelperHandController helperHandController;

    private bool hasPlayedAudio; // To ensure audio is played only once

    void Start()
    {
        /*walkingAnimator = walkingRig.GetComponent<Animator>();
        normalAnimator = normalRig.GetComponent<Animator>();*/
        birdAnimator = Bird.GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        BoyAnimator = GetComponent<Animator>();
        boySprite = GetComponent<SpriteRenderer>();
        /*normalRig.SetActive(true);
        walkingRig.SetActive(false);*/
        helperHandController = FindObjectOfType<HelperHandController>();
        audioClipBigPillow = Resources.Load<AudioClip>("Audio/Helper Audio/bigpillow");
        if (stopPosition == null)
        {
            Debug.LogError("Stop position not set for BoyController.");
        }

        // Initially disable colliders for all interactive objects
        DisableColliders();

        birdaudioplayed = false;
        isWalking = false;
        hasReachedStopPosition = false;
        /*shouldContinueWalking = false;*/
        isFinalWalk = false;
        hasPlayedAudio = false;
        
    }

    void Update()
    {
        if (!isWalking && BoyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            BoyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.1f && !hasReachedStopPosition)
        {
            /*normalRig.SetActive(false);
            walkingRig.SetActive(true);*/
            BoyAnimator.SetTrigger("canWalk");
            isWalking = true;
        }

        if (isWalking && !hasReachedStopPosition)
        {
            MoveToStopPosition();
        }

        if (birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talk") &&
            birdAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            StartCoroutine(DelayedBigPillowTrigger());
        }

        if (PillowDragAndDrop.droppedPillowsCount == 4 && !hasPlayedAudio)
        {
            BoyAnimator.SetTrigger("CanTalk");
            birdAnimator.SetTrigger("allDone");
            StartCoroutine(RevealTextWordByWord("WoW..! My Bed looks so Clean, Thank you Kiki and Friend", 0.5f));
            PlayAudioOnPillowsDropped();
            hasPlayedAudio = true; // Ensure the audio only plays once                         
        }

        if (BoyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dialoge 1") &&
            BoyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f) // When Dialoge 1 is about to end
        {
            /*normalRig.SetActive(false); // Switch to the walking rig
            walkingRig.SetActive(true);*/
            isWalking = true;
            isFinalWalk = true;
        }

        if (isWalking && isFinalWalk)
        {
            movetoEnd();
        }
    }

    private IEnumerator DelayedBigPillowTrigger()
    {
        yield return new WaitForSeconds(2.5f); // Add 1-second delay     
       
        if (!pillowAudioPlayer)
        {
            birdAnimator.SetTrigger("bigPillow");
            AudioSource audioSource = nextaudiosoucre.GetComponent<AudioSource>();
            audioSource.PlayOneShot(audioClipBigPillow);
            StartCoroutine(RevealTextWordByWord("Put the Big Pillow at the Back", 0.5f));
            pillowAudioPlayer = true;
        }

        EnableBigPillowColliders();
    }

    private void PlayAudioOnPillowsDropped()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource is not assigned.");
        }
    }

    private void movetoEnd()
    {
        boySprite.flipX = true;
        BoyAnimator.SetTrigger("allDone");
        Vector3 targetPosition = new Vector3(stopPosition2.position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);
    }

    private void MoveToStopPosition()
    {
        Vector3 targetPosition = new Vector3(stopPosition.position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x - stopPosition.position.x) < 0.1f)
        {
            Debug.Log("positionReached");
            hasReachedStopPosition = true;
            isWalking = false;
            BoyAnimator.SetBool("canWalk", false);
            /*normalRig.SetActive(true);
            walkingRig.SetActive(false);*/
            birdAnimator.SetTrigger("canTalk");
            if (!birdaudioplayed)
            {
                birdaudioplayed = true;
                birdAudiosource.Play();
                StartCoroutine(RevealTextWordByWord("Oh JoJo, Your Bed sure does look Messy, Don't worry my Friend, and I will help you ", 0.5f));
            }
        }
    }

    private void DisableColliders()
    {
        if (pillowBigRight != null) pillowBigRight.GetComponent<Collider2D>().enabled = false;
        if (pillowBigLeft != null) pillowBigLeft.GetComponent<Collider2D>().enabled = false;
        if (pillowSmallLeft != null) pillowSmallLeft.GetComponent<Collider2D>().enabled = false;
        if (pillowSmallRight != null) pillowSmallRight.GetComponent<Collider2D>().enabled = false;
    }

    private void EnableBigPillowColliders()
    {
        if (pillowBigRight != null)
        {
            pillowBigRight.GetComponent<Collider2D>().enabled = true;
            var pillowRightScript = pillowBigRight.GetComponent<PillowDragAndDrop>();
            if (pillowRightScript != null)
            {
                helperHandController.ScheduleHelperHand(pillowRightScript);
            }
        }

        if (pillowBigLeft != null)
        {
            pillowBigLeft.GetComponent<Collider2D>().enabled = true;
            var pillowLeftScript = pillowBigLeft.GetComponent<PillowDragAndDrop>();
            if (pillowLeftScript != null)
            {
                helperHandController.ScheduleHelperHand(pillowLeftScript);
            }
        }

        PillowDragAndDrop.canDrag = true;
    }

    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(true);

        string[] words = fullText.Split(' ');

        // Reveal words one by one
        for (int i = 0; i < words.Length; i++)
        {
            subtitleText.text = string.Join(" ", words, 0, i + 1);
            yield return new WaitForSeconds(delayBetweenWords);
        }
        subtitleText.text = "";
    }
}
