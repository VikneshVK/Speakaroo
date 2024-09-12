using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoyWalk1 : MonoBehaviour
{
    public GameObject walkRig;
    public GameObject normalRig;
    public Animator birdAnimator;
    public Transform stopPosition;
    public float moveSpeed = 2.0f;
    public GameObject Bag;  // The first object in the sequence
    public dragManager dragManager;  // Reference to DragManager

    private bool isWalking;
    private bool reachedStopPosition;
    private bool birdFinishedTalking;
    private bool boyFinishedTalking;
    private AudioSource audioSource;

    private Animator walkrigAnimator;
    private Animator normalrigAnimator;

    private Collider2D bagCollider;  // Collider for the first object (Bag)
    private HelperPointer helperPointer;

    void Start()
    {
        walkrigAnimator = walkRig.GetComponent<Animator>();
        normalrigAnimator = normalRig.GetComponent<Animator>();
        bagCollider = Bag.GetComponent<Collider2D>();
        bagCollider.enabled = false;  // Ensure the Bag's collider is disabled initially
        audioSource = GetComponent<AudioSource>();

        helperPointer = FindObjectOfType<HelperPointer>();  // Find the HelperPointer in the scene
        if (helperPointer == null)
        {
            Debug.LogError("HelperPointer not found in the scene. Please ensure a HelperPointer script is attached to a GameObject.");
        }

        isWalking = false;
        reachedStopPosition = false;
        birdFinishedTalking = false;
        boyFinishedTalking = false;
    }

    void Update()
    {
        // Check if the boy can start walking (when the "Idle" animation is finished)
        if (normalrigAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            normalrigAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f &&
            !isWalking)
        {
            normalRig.SetActive(false);  // Hide the normal rig
            walkRig.SetActive(true);     // Show the walking rig
            isWalking = true;
        }

        // Move the boy towards the stop position if walking
        if (isWalking && !reachedStopPosition)
        {
            MoveTowardsStopPosition();
        }

        // Check if the boy finished talking ("Dialogue 1" animation is nearly finished)
        if (normalrigAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dialouge 1") &&
            normalrigAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f &&
            !boyFinishedTalking)
        {
            Debug.Log("bird will talk bagTalk");
            normalrigAnimator.SetBool("canTalk", false);  // Stop the boy from talking again
            boyFinishedTalking = true;

            // Use dragManager to play the object-specific audio and activate the bag
            dragManager.PlayObjectAudio(0);  // Play audio for the first object (Bag)

            bagCollider.enabled = true;  // Enable the collider for the Bag object (first drop object)

            // Schedule the helper hand to point at the Bag
            var bagDragHandler = Bag.GetComponent<DragHandler>();
            if (bagDragHandler != null && helperPointer != null)
            {
                helperPointer.ScheduleHelperHand(bagDragHandler, dragManager);
            }
        }

        if(dragManager.allDone)
        {
            birdAnimator.SetTrigger("finalTalkComplete");
        }
    }

    // Move the boy towards the stop position
    void MoveTowardsStopPosition()
    {
        transform.position = new Vector2(
            Mathf.MoveTowards(transform.position.x, stopPosition.position.x, moveSpeed * Time.deltaTime),
            transform.position.y);

        // Check if the boy reached the stop position
        if (Mathf.Abs(transform.position.x - stopPosition.position.x) < 0.1f)
        {
            normalRig.SetActive(true);   // Switch back to the normal rig
            walkRig.SetActive(false);    // Disable the walking rig
            normalrigAnimator.SetBool("canTalk", true);  // Start the "Dialogue 1" animation
            reachedStopPosition = true;  // Mark that the boy reached the stop position
            audioSource.Play();
        }
    }
}
