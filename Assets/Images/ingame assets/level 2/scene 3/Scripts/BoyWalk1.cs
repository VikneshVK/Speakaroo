using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoyWalk1 : MonoBehaviour
{
    
    public Animator birdAnimator;
    public Transform stopPosition;
    public float moveSpeed = 2.0f;
    public GameObject Bag;  // The first object in the sequence
    public dragManager dragManager;  // Reference to DragManager
    public SubtitleManager subtitleManager;

    private bool isWalking;
    private bool reachedStopPosition;
   /* private bool birdFinishedTalking;*/
    private bool boyFinishedTalking;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;

    private Animator boyAnimator;
    

    private Collider2D bagCollider;  // Collider for the first object (Bag)
    private HelperPointer helperPointer;

    void Start()
    {
        boyAnimator = GetComponent<Animator>();        
        bagCollider = Bag.GetComponent<Collider2D>();
        bagCollider.enabled = false;  // Ensure the Bag's collider is disabled initially
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        helperPointer = FindObjectOfType<HelperPointer>();  // Find the HelperPointer in the scene
        if (helperPointer == null)
        {
            Debug.LogError("HelperPointer not found in the scene. Please ensure a HelperPointer script is attached to a GameObject.");
        }

        isWalking = false;
        reachedStopPosition = false;
        /*birdFinishedTalking = false;*/
        boyFinishedTalking = false;
    }

    void Update()
    {
        // Check if the boy can start walking (when the "Idle" animation is finished)
        if (boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.1f &&
            !isWalking)
        {
            boyAnimator.SetBool("canWalk", true);
            isWalking = true;
        }

        // Move the boy towards the stop position if walking
        if (isWalking && !reachedStopPosition)
        {
            MoveTowardsStopPosition();
        }

        // Check if the boy finished talking ("Dialogue 1" animation is nearly finished)
        if (boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dialouge 1") &&
            boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f &&
            !boyFinishedTalking)
        {
            Debug.Log("bird will talk bagTalk");
            boyAnimator.SetBool("canTalk", false);  // Stop the boy from talking again

            boyFinishedTalking = true;
            StartCoroutine(DelayedActions());
            
        }

        if (birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talk") &&
            birdAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            birdAnimator.SetTrigger("finalTalkComplete");
        }
    }

    private IEnumerator DelayedActions()
    {
        yield return new WaitForSeconds(1f);

        dragManager.PlayObjectAudio(0); 

        bagCollider.enabled = true; 

        var bagDragHandler = Bag.GetComponent<DragHandler>();
        if (bagDragHandler != null && helperPointer != null)
        {
            helperPointer.ScheduleHelperHand(bagDragHandler, dragManager);
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
            boyAnimator.SetBool("canWalk", false);
            
            boyAnimator.SetBool("canTalk", true);  // Start the "Dialogue 1" animation
            reachedStopPosition = true;  // Mark that the boy reached the stop position
            audioSource.Play();
            subtitleManager.DisplaySubtitle("Oh no..! Your room is messy too, Don't worry Kiki, Me and My Friend will help you clean it.", "JoJo", audioSource.clip);
        }
    }
    
}
