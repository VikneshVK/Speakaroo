using System;
using System.Collections.Generic;
using UnityEngine;

public class Jojo_action1 : MonoBehaviour
{
    public Transform stopPosition;
    public Transform offScreenPosition;
    public float walkSpeed = 2f;
    public bool hasReachedOffScreen = false; // New boolean to track off-screen position
    public List<GameObject> objectsToEnable;
    public static event Action<GameObject[]> OnCollidersEnabled;

    private Animator walkAnimator;
    private Animator talkAnimator;

    public GameObject walkRig;
    public GameObject talkRig;

    // AudioSource and AudioClips
    private AudioSource audioSource;
    private AudioClip audioClip1;
    private AudioClip audioClip2;

    private bool isWalking = false;
    private bool isIdleCompleted = false;
    private bool collidersEnabled = false; // To ensure colliders are enabled only once
    private bool isReturning = false; // Flag for return trip
    private bool isTalk2Triggered = false; // Flag to track when canTalk2 is triggered

    private Vector3 targetPosition; // Store the current target position

    void Start()
    {
        walkAnimator = walkRig.GetComponent<Animator>();
        talkAnimator = talkRig.GetComponent<Animator>();

        // Get the AudioSource and load the audio clips from Resources
        audioSource = GetComponent<AudioSource>();
        audioClip1 = Resources.Load<AudioClip>("audio/lvl3sc3/Looks like some clothes are still wet");
        audioClip2 = Resources.Load<AudioClip>("audio/lvl3sc3/_You are right, Kiki. It is evening now the clothes and toys are dry");

        ActivateTalkRig(); // Start with talkRig active
    }

    void Update()
    {
        if (isWalking)
        {
            WalkToPosition(targetPosition);
        }
        else if (!isReturning)
        {
            HandleIdleCompletion();
        }
    }

    private void HandleIdleCompletion()
    {
        if (!isIdleCompleted && talkAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            talkAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            isIdleCompleted = true;
            MoveToStopPosition(); // Start moving to the stop position after idle completes
        }
    }

    public void MoveToStopPosition()
    {
        ActivateWalkRig(); // Activate walkRig
        targetPosition = stopPosition.position;
        isWalking = true;
        isReturning = false;
    }

    public void MoveOffScreen()
    {
        ActivateWalkRig(); // Activate walkRig
        targetPosition = offScreenPosition.position;
        walkAnimator.SetTrigger("walkBack");
        isWalking = true;
    }

    public void ReturnToStopPosition()
    {
        isReturning = true;
        ActivateWalkRig(); // Activate walkRig
        targetPosition = stopPosition.position;
        walkAnimator.SetTrigger("walk");
        isWalking = true;
        isTalk2Triggered = true; // Set flag to trigger canTalk2 when returning to stop position
    }

    private void WalkToPosition(Vector3 targetPosition)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
        {
            isWalking = false; // Stop walking

            if (targetPosition == offScreenPosition.position)
            {
                hasReachedOffScreen = true; // Reached off-screen

            }
            else if (targetPosition == stopPosition.position)
            {
                ActivateTalkRig(); // Switch to talkRig after reaching the stop position
                CheckAndEnableColliders();

                if (isTalk2Triggered)
                {
                    StartTalking("canTalk2"); // Trigger "canTalk2" if returning to stop position
                    isTalk2Triggered = false; // Reset flag
                }
                else
                {
                    StartTalking("canTalk"); // Regular talking trigger
                }
            }
        }
    }

    public void StartTalking(string talkTrigger)
    {
        ActivateTalkRig(); // Switch to talkRig
        talkAnimator.SetTrigger(talkTrigger); // Trigger the correct talk animation (canTalk or canTalk2)

        // Play the appropriate audio based on the trigger
        if (talkTrigger == "canTalk" && audioClip1 != null)
        {
            audioSource.clip = audioClip1;
            audioSource.Play();
        }
        else if (talkTrigger == "canTalk2" && audioClip2 != null)
        {
            audioSource.clip = audioClip2;
            audioSource.Play();
        }
    }

    private void CheckAndEnableColliders()
    {
        if (!collidersEnabled)
        {
            foreach (GameObject obj in objectsToEnable)
            {
                ItemDragHandler item = obj.GetComponent<ItemDragHandler>();
                if (item != null)
                {
                    Collider2D collider = obj.GetComponent<Collider2D>();
                    if (collider != null)
                    {
                        collider.enabled = true;
                    }
                }
            }
            collidersEnabled = true;

            // Notify the HelpHandController for Jojo's action
            HelpHandController helperHand = FindObjectOfType<HelpHandController>();
            if (helperHand != null)
            {
                helperHand.StartHelperHandRoutineForJojo(objectsToEnable.ToArray());
            }
        }
    }

    private void ActivateWalkRig()
    {
        walkRig.SetActive(true);
        talkRig.SetActive(false);
    }

    private void ActivateTalkRig()
    {
        talkRig.SetActive(true);
        walkRig.SetActive(false);
    }
}
