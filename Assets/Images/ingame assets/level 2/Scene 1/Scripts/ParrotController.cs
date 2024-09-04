using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

public class ParrotController : MonoBehaviour
{
    public GameObject mainBus;
    public GameObject mainWhale;
    public GameObject mainBuilding;
    public Transform finalPositionWhale;
    public Transform finalPositionBus;
    public Transform finalPositionBuilding;
    public Boolean cleaningCompleted;
    public float speed = 5.0f;
    public AudioSource audioSource;
    public GameObject referenceContainer;

    private AnchorGameObject anchor;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private GameObject pushedGameObject;
    private bool isReturning = false;
    private List<string> pushedObjects = new List<string>();
    private Dictionary<string, GameObject> mainObjects;
    private List<string> requiredObjects = new List<string> { "bus S", "whale s", "building blocks s" };
    private bool isWalkingCoroutineRunning = false;
    private bool audioPlayed = false;
    private bool stopWalking = false;
    private AudioSource ReferenceaudioSource;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        anchor = GetComponent<AnchorGameObject>();
        audioSource = GetComponent<AudioSource>();
        ReferenceaudioSource= referenceContainer.GetComponent<AudioSource>();
        cleaningCompleted = false;


        mainObjects = new Dictionary<string, GameObject>
        {
            {"bus S", mainBus},
            {"whale s", mainWhale},
            {"building blocks s", mainBuilding}
        };
        foreach (var obj in mainObjects.Values)
        {
            obj.GetComponent<Collider2D>().enabled = false;
        }
    }

    void Update()
    {
        if (animator.GetBool("startWalking") && !isReturning)
        {
            StartCoroutine(WaitAndWalk());
        }

        if (isReturning)
        {
            ReturnToStart();
        }
        if (pushedObjects.Count == requiredObjects.Count && !pushedObjects.Except(requiredObjects).Any())
        {
            animator.SetBool("cleaningDone", true); // All required objects have been pushed
            cleaningCompleted = true;
        }
    }

    IEnumerator WaitAndWalk()
    {
        isWalkingCoroutineRunning = true;
        anchor.enabled = false;
        yield return new WaitForSeconds(1f);

        float duration = 0.5f; // Set the duration for the movement
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = transform.position + new Vector3(1f, 0f, 0f); // Move by 1 unit on the x-axis

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition; // Ensure the final position is exactly the target position
        isWalkingCoroutineRunning = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Pushable") && !pushedObjects.Contains(other.gameObject.name))
        {
            stopWalking = true; // Stop the WaitAndWalk movement
            animator.SetBool("startWalking", false);
            animator.SetTrigger("canKnock");
            pushedGameObject = other.gameObject;
            other.GetComponent<Collider2D>().enabled = false;
            pushedObjects.Add(pushedGameObject.name);
            StartCoroutine(WaitForKnockToComplete());
            UpdateColliderStatus(pushedGameObject.name);
        }
    }

    IEnumerator WaitForKnockToComplete()
    {
        // Wait until the "Bird Knock" animation is being played
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Bird Knock"));
        // Wait until the "Bird Knock" animation is complete
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);

        MovePushedObject();
    }

    private void MovePushedObject()
    {
        Transform finalPosition = null;
        if (pushedGameObject.name == "bus S")
            finalPosition = finalPositionBus;
        else if (pushedGameObject.name == "whale s")
            finalPosition = finalPositionWhale;
        else if (pushedGameObject.name == "building blocks s")
            finalPosition = finalPositionBuilding;

        if (finalPosition != null)
        {
            LeanTween.move(pushedGameObject, finalPosition.position, 1f).setOnComplete(OnCompleteMove);
        }
        else
        {
            Debug.LogError("No matching final position found for: " + pushedGameObject.name);
        }
    }

    private void OnCompleteMove()
    {
        animator.SetBool("walkBack", true);
        isReturning = true;

    }

    private void ReturnToStart()
    {
        transform.position = Vector3.MoveTowards(transform.position, startPosition, speed * Time.deltaTime);

        if (transform.position == startPosition)
        {
            if(pushedObjects.Count < requiredObjects.Count)
            {
                ReferenceaudioSource.Play();
            }
            
            ResetAnimatorBooleans();
            anchor.enabled = true;
            isReturning = false;
            stopWalking = false;

            // Re-enable the correct colliders based on which objects have been dropped
            EnableCorrectColliders();

            // Reset timers for the next objects
            ResetTimersForNextObjects();
        }
    }


    private void EnableCorrectColliders()
    {
        bool busDropped = pushedObjects.Contains("bus S");
        bool whaleDropped = pushedObjects.Contains("whale s");
        bool buildingDropped = pushedObjects.Contains("building blocks s");

        // Enable colliders based on the state of other objects
        if (busDropped && whaleDropped && !buildingDropped)
        {
            mainBuilding.GetComponent<Collider2D>().enabled = true;
        }
        else if (busDropped && !whaleDropped && buildingDropped)
        {
            mainWhale.GetComponent<Collider2D>().enabled = true;
        }
        else if (!busDropped && whaleDropped && buildingDropped)
        {
            mainBus.GetComponent<Collider2D>().enabled = true;
        }
        else
        {
            if (!busDropped) mainBus.GetComponent<Collider2D>().enabled = true;
            if (!whaleDropped) mainWhale.GetComponent<Collider2D>().enabled = true;
            if (!buildingDropped) mainBuilding.GetComponent<Collider2D>().enabled = true;
        }
    }

    private void ResetTimersForNextObjects()
    {
        var otherKeys = mainObjects.Keys.Where(k => !pushedObjects.Contains(k)).ToList();

        foreach (var key in otherKeys)
        {
            var interactable = mainObjects[key].GetComponent<InteractableObject>();
            if (interactable != null)
            {
                interactable.EnableInteractionTracking();  // This will reset the timer and start tracking
                Debug.Log($"Timer reset and tracking started for {key} after Kiki returned.");
            }
        }
    }

    private void ResetAnimatorBooleans()
    {
        animator.SetBool("resetPosition", true);
        animator.SetBool("walkBack", false);
        animator.SetBool("startWalking", false);
        animator.SetBool("canKnock", false);
        animator.SetBool("cleaningDone", false);
    }

    private void UpdateColliderStatus(string pushedName)
    {
        var otherKeys = mainObjects.Keys.Where(k => k != pushedName).ToList();

        foreach (var key in otherKeys)
        {
            if (!pushedObjects.Contains(key))
            {
                var gameObject = mainObjects[key];
                gameObject.GetComponent<Collider2D>().enabled = true;

                // Start interaction tracking for the newly enabled object
                InteractableObject interactable = gameObject.GetComponent<InteractableObject>();
                if (interactable != null)
                {
                    interactable.EnableInteractionTracking(); // This already resets the timer
                    Debug.Log($"Collider enabled and tracking started for {key}");
                }
            }
        }
    }

    // This method will be called by the Animation Event
    public void PlayAnimationAudio(AudioClip clip)
    {
        if (audioSource != null && clip != null && !audioPlayed)
        {
            audioSource.clip = clip;
            audioSource.loop = false; // Ensure that the audio does not loop
            audioSource.Play();
            audioPlayed = true; // Set the flag to true after playing the audio
        }
        else if (audioPlayed)
        {
            Debug.Log("Audio has already been played once.");
        }
        else
        {
            Debug.LogWarning("AudioSource or AudioClip is missing.");
        }
    }
    public void ResetAudioPlayed()
    {
        audioPlayed = false;
    }
}
