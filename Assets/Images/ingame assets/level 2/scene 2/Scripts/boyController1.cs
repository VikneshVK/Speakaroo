using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boyController1 : MonoBehaviour
{
    public Transform stopPosition;
    public Transform stopPosition2;
    public float walkSpeed = 2f;

    public GameObject pillowBigRight;
    public GameObject pillowBigLeft;
    public GameObject pillowSmallLeft;
    public GameObject pillowSmallRight;
    public GameObject bedsheet;
    public GameObject walkingRig;
    public GameObject normalRig;
    public GameObject Bird;
    public AudioSource birdAudiosource;

    // Reference to the PillowDragAndDrop script
    public PillowDragAndDrop pillowDragAndDrop;

    private Animator walkingAnimator;
    private Animator normalAnimator;
    private Animator birdAnimator;
    private bool isWalking;
    private bool hasReachedStopPosition;
    private bool shouldContinueWalking;
    private AudioSource audioSource;
    private bool isFinalWalk;
    private bool birdaudioplayed;

    private HelperHandController helperHandController;

    private bool hasPlayedAudio; // To ensure audio is played only once

    void Start()
    {
        walkingAnimator = walkingRig.GetComponent<Animator>();
        normalAnimator = normalRig.GetComponent<Animator>();
        birdAnimator = Bird.GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        normalRig.SetActive(true);
        walkingRig.SetActive(false);
        helperHandController = FindObjectOfType<HelperHandController>();

        if (stopPosition == null)
        {
            Debug.LogError("Stop position not set for BoyController.");
        }

        // Initially disable colliders for all interactive objects
        DisableColliders();

        birdaudioplayed = false;
        isWalking = false;
        hasReachedStopPosition = false;
        shouldContinueWalking = false;
        isFinalWalk = false;
        hasPlayedAudio = false;

    }

    void Update()
    {
        if (!isWalking && normalAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            normalAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f && !hasReachedStopPosition)
        {
            normalRig.SetActive(false);
            walkingRig.SetActive(true);
            walkingAnimator.SetTrigger("canWalk");
            isWalking = true;
        }

        if (isWalking && !hasReachedStopPosition)
        {
            MoveToStopPosition();
        }

        if (birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talk") &&
            birdAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.96f)
        {
            EnableBigPillowColliders();
        }

        if (PillowDragAndDrop.droppedPillowsCount == 4 && !hasPlayedAudio)
        {
            PlayAudioOnPillowsDropped();
            hasPlayedAudio = true; // Ensure the audio only plays once
            normalAnimator.SetTrigger("CanTalk"); // Set trigger to transition to Dialoge 1 state            
        }        

        if (normalAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dialoge 1") &&
            normalAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f) // When Dialoge 1 is about to end
        {
            normalRig.SetActive(false); // Switch to the walking rig
            walkingRig.SetActive(true);
            isWalking = true;
            isFinalWalk = true;
        }

        if (isWalking && isFinalWalk)
        {
            movetoEnd();
        }
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
        walkingAnimator.SetTrigger("canWalk2");
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
            walkingAnimator.SetTrigger("stopWalking");
            normalRig.SetActive(true);
            walkingRig.SetActive(false);
            birdAnimator.SetTrigger("canTalk");
            if (!birdaudioplayed)
            {
                birdaudioplayed = true;
                birdAudiosource.Play();
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
    }
}
