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

        StartCoroutine(CheckNextTarget(isForJojo));
    }

    public void StartHelperHandRoutineForKiki(GameObject[] objectsToMonitor)
    {
        targetObjects = objectsToMonitor;
        currentTargetIndex = 0;
        isHelperHandActive = false;
        isForJojo = false;

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
            // Reset audio before spawning the helper hand
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            initialPosition = targetObject.transform.position;
            currentHelpPointer = Instantiate(helpPointerPrefab, initialPosition, Quaternion.identity);
            isHelperHandActive = true;

            // Play audio only if the object is valid and the helper hand is spawned
            PlayAudioForHelperHand(targetObject, isForJojo);

            targetPosition = GetTargetPosition(itemDragHandler, targetObject, isForJojo);
            StartTweenLoop();
        }
    }

    private void PlayAudioForHelperHand(GameObject targetObject, bool isForJojo)
    {
        // Play audio based on whether it's for Jojo or Kiki, but only once
        if (audioSource != null && !audioSource.isPlaying)
        {
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

            if (audioSource.clip != null)
            {
                audioSource.Play();
            }
        }
    }


    private Vector3 GetTargetPosition(ItemDragHandler itemDragHandler, GameObject targetObject, bool isForJojo)
    {
        Vector3 position = Vector3.zero;
        if (isForJojo)
        {
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
                    position = itemDragHandler.basketTransform.position;
                    break;
            }
        }
        else
        {
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
                    position = itemDragHandler.basketTransform.position;
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
        if (isHelperHandActive && interactedObject == targetObjects[currentTargetIndex])
        {
            if (isCorrectDrop)
            {
                DestroyHelperHand();
                currentTargetIndex++;
                Debug.Log("Object interacted, moving to next target and resetting timer.");
                StartCoroutine(CheckNextTarget(isForJojo)); // Restart timer
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
            StartCoroutine(CheckNextTarget(isForJojo)); // Restart timer for next object
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
