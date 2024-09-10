using UnityEngine;
using System.Collections;

public class HelpHandController : MonoBehaviour
{
    public GameObject helpPointerPrefab;
    public float delayTimer = 5f;
    public float tweenDuration = 1f;
    public Animator birdAnimator;

    private GameObject[] targetObjects;
    private GameObject currentHelpPointer;
    private bool isHelperHandActive = false;
    private int currentTargetIndex = 0;

    private Vector3 initialPosition;
    private Vector3 targetPosition;

    private bool isForJojo;

    private AudioSource audioSource;
    public AudioClip audioClip1;
    public AudioClip audioClip2;
    public AudioClip audioClip3;

    // New flag to prevent audio from playing twice
    private bool hasAudioPlayed = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void StartHelperHandRoutineForJojo(GameObject[] objectsToMonitor)
    {
        targetObjects = objectsToMonitor;
        currentTargetIndex = 0;
        isHelperHandActive = false;
        isForJojo = true;
        hasAudioPlayed = false; // Reset flag for Jojo

        StartCoroutine(CheckNextTarget(isForJojo));
    }

    public void StartHelperHandRoutineForKiki(GameObject[] objectsToMonitor)
    {
        targetObjects = objectsToMonitor;
        currentTargetIndex = 0;
        isHelperHandActive = false;
        isForJojo = false;
        hasAudioPlayed = false; // Reset flag for Kiki

        StartCoroutine(CheckNextTarget(isForJojo));
    }

    private IEnumerator CheckNextTarget(bool isForJojo)
    {
        // Ensure we are within the bounds of the array and skip null objects
        while (currentTargetIndex < targetObjects.Length && targetObjects[currentTargetIndex] == null)
        {
            currentTargetIndex++;
        }

        if (currentTargetIndex < targetObjects.Length)
        {
            isHelperHandActive = false;
            hasAudioPlayed = false; // Reset flag when moving to next target
            Debug.Log("Delay timer started for next object.");
            yield return new WaitForSeconds(delayTimer);
            CheckInteraction(isForJojo);
        }
        else
        {
            Debug.Log("No more target objects to process.");
            yield break;
        }
    }

    private void CheckInteraction(bool isForJojo)
    {
        // Ensure that currentTargetIndex is within bounds
        if (currentTargetIndex >= targetObjects.Length)
        {
            Debug.LogWarning("currentTargetIndex is out of bounds. Ending interaction.");
            return;
        }

        GameObject targetObject = targetObjects[currentTargetIndex];

        if (targetObject != null)
        {
            ItemDragHandler itemDragHandler = targetObject.GetComponent<ItemDragHandler>();
            // Ensure the object has not been interacted with and its collider is enabled
            if (itemDragHandler != null && targetObject.GetComponent<Collider2D>().enabled && !itemDragHandler.HasInteracted)
            {
                DestroyHelperHand();
                SpawnHelperHand(targetObject, itemDragHandler, isForJojo);
            }
            else
            {
                // Move to the next object if the current one is interacted with or not valid
                currentTargetIndex++;
                StartCoroutine(CheckNextTarget(isForJojo));
            }
        }
        else
        {
            // If the object is null, skip to the next one
            currentTargetIndex++;
            StartCoroutine(CheckNextTarget(isForJojo));
        }
    }

    private void SpawnHelperHand(GameObject targetObject, ItemDragHandler itemDragHandler, bool isForJojo)
    {
        if (helpPointerPrefab != null)
        {
            // Ensure the audio stops if it's playing before spawning a new helper hand
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            initialPosition = targetObject.transform.position;
            currentHelpPointer = Instantiate(helpPointerPrefab, initialPosition, Quaternion.identity);
            isHelperHandActive = true;

            // Play audio once when helper hand is spawned
            if (!hasAudioPlayed)
            {
                PlayAudioForHelperHand(targetObject, isForJojo);
            }

            targetPosition = GetTargetPosition(itemDragHandler, targetObject, isForJojo);
            StartTweenLoop();
        }
    }

    private void PlayAudioForHelperHand(GameObject targetObject, bool isForJojo)
    {
        if (audioSource != null && !hasAudioPlayed)
        {
            // Play appropriate audio based on Jojo or Kiki
            if (isForJojo)
            {
                if (audioClip3 != null && (targetObject.name == "wet kuma" || targetObject.name == "wet dino" || targetObject.name == "wet bunny"))
                {
                    audioSource.clip = audioClip3;
                }
                else if (audioClip1 != null)
                {
                    audioSource.clip = audioClip1;
                }
            }
            else if (!isForJojo && audioClip2 != null)
            {
                audioSource.clip = audioClip2;
            }

            // Play audio and ensure it plays only once
            if (audioSource.clip != null)
            {
                audioSource.loop = false; // Disable looping
                audioSource.Play();
                hasAudioPlayed = true; // Mark the audio as played to prevent it from playing again
            }
        }
    }

    private Vector3 GetTargetPosition(ItemDragHandler itemDragHandler, GameObject targetObject, bool isForJojo)
    {
        Vector3 position = Vector3.zero;

        if (isForJojo)
        {
            // Define positions for Jojo's interaction with specific objects
            switch (targetObject.name)
            {
                case "wet kuma":
                    position = itemDragHandler.teddyPositionObject.transform.position;
                    break;
                case "wet dino":
                    position = itemDragHandler.dinoPositionObject.transform.position;
                    break;
                case "wet bunny":
                    position = itemDragHandler.bunnyPositionObject.transform.position;
                    break;
                default:
                    position = itemDragHandler.basketTransform.position; // Default to basket position
                    break;
            }
        }
        else
        {
            // Define positions for Kiki's interaction with specific objects
            switch (targetObject.name)
            {
                case "wet kuma":
                    position = itemDragHandler.teddyInitialPosition.transform.position;
                    break;
                case "wet dino":
                    position = itemDragHandler.dinoInitalPosition.transform.position;
                    break;
                case "wet bunny":
                    position = itemDragHandler.bunnyInitialPosition.transform.position;
                    break;
                default:
                    position = itemDragHandler.basketTransform.position; // Default to basket position
                    break;
            }
        }

        return position;
    }

    private void StartTweenLoop()
    {
        LeanTween.move(currentHelpPointer, targetPosition, tweenDuration).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
        {
            currentHelpPointer.transform.position = initialPosition;
            if (isHelperHandActive)
            {
                StartTweenLoop();
            }
        });
    }

    public void OnObjectInteracted(GameObject interactedObject, bool isCorrectDrop)
    {
        // Check if currentTargetIndex is within bounds
        if (currentTargetIndex >= targetObjects.Length)
        {
            Debug.LogWarning("currentTargetIndex is out of bounds in OnObjectInteracted. Ending interaction.");
            return; // Prevent out-of-bounds access
        }

        if (isHelperHandActive && interactedObject == targetObjects[currentTargetIndex])
        {
            if (isCorrectDrop)
            {
                DestroyHelperHand();
                currentTargetIndex++;
                Debug.Log("Object interacted, moving to next target and resetting timer.");

                // Check if the new currentTargetIndex is within bounds before proceeding
                if (currentTargetIndex < targetObjects.Length)
                {
                    StartCoroutine(CheckNextTarget(isForJojo)); // Restart timer
                }
                else
                {
                    Debug.Log("No more target objects to process.");
                }
            }
            else
            {
                DestroyHelperHand();
                StartCoroutine(ResetTimerAndSpawnHelperHand());
            }
        }
        else if (interactedObject == targetObjects[currentTargetIndex] && !isHelperHandActive)
        {
            // Stop any audio playing and reset timer
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            currentTargetIndex++;

            // Check if the new currentTargetIndex is within bounds before proceeding
            if (currentTargetIndex < targetObjects.Length)
            {
                StartCoroutine(CheckNextTarget(isForJojo)); // Restart timer for next object
            }
            else
            {
                Debug.Log("No more target objects to process.");
            }
        }
    }


    private IEnumerator ResetTimerAndSpawnHelperHand()
    {
        Debug.Log("Incorrect drop, resetting delay timer.");
        yield return new WaitForSeconds(delayTimer);
        CheckInteraction(isForJojo);
    }

    private void DestroyHelperHand()
    {
        if (currentHelpPointer != null)
        {
            Destroy(currentHelpPointer);
            currentHelpPointer = null;
            isHelperHandActive = false;

            // Stop the audio when the helper hand is destroyed
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}
