using UnityEngine;
using System.Collections;

public class HelpHandController : MonoBehaviour
{
    public GameObject helpPointerPrefab; // Prefab for the helper hand
    public float delayTimer = 5f; // Timer duration before showing the helper hand
    public float tweenDuration = 1f; // Duration of the tween animation
    public Animator birdAnimator; // Reference to the bird's Animator

    private GameObject[] targetObjects; // Objects to monitor for interaction
    private GameObject currentHelpPointer; // Current instance of the helper hand
    private bool isHelperHandActive = false; // Flag to track if the helper hand is active
    private int currentTargetIndex = 0; // Index to track the current target object

    private Vector3 initialPosition; // Initial position of the helper hand
    private Vector3 targetPosition; // Target position for the helper hand

    private bool isForJojo; // Tracks whether the current routine is for Jojo or Kiki

    // AudioSource and Audio Clips
    private AudioSource audioSource;
    public AudioClip audioClip1; // Audio for Jojo
    public AudioClip audioClip2; // Audio for Kiki

    void Start()
    {
        // Get the AudioSource component attached to the same GameObject
        audioSource = GetComponent<AudioSource>();
    }

    // Method triggered by Jojo_action
    public void StartHelperHandRoutineForJojo(GameObject[] objectsToMonitor)
    {
        targetObjects = objectsToMonitor;
        currentTargetIndex = 0;
        isHelperHandActive = false;
        isForJojo = true; // Set context for Jojo

        StartCoroutine(CheckNextTarget(isForJojo));
    }

    // Method triggered by Kiki_actions
    public void StartHelperHandRoutineForKiki(GameObject[] objectsToMonitor)
    {
        targetObjects = objectsToMonitor;
        currentTargetIndex = 0;
        isHelperHandActive = false;
        isForJojo = false; // Set context for Kiki

        StartCoroutine(CheckNextTarget(isForJojo));
    }

    private IEnumerator CheckNextTarget(bool isForJojo)
    {
        if (currentTargetIndex < targetObjects.Length)
        {
            isHelperHandActive = false;
            yield return new WaitForSeconds(delayTimer);
            CheckInteraction(isForJojo);
        }
    }

    private void CheckInteraction(bool isForJojo)
    {
        GameObject targetObject = targetObjects[currentTargetIndex];

        if (targetObject != null)
        {
            ItemDragHandler itemDragHandler = targetObject.GetComponent<ItemDragHandler>();
            if (itemDragHandler != null && targetObject.GetComponent<Collider2D>().enabled)
            {
                // Debug log to check which object is being processed
                Debug.Log($"Processing {targetObject.name} for {(isForJojo ? "Jojo" : "Kiki")}.");

                // If the object has not been interacted with, spawn the helper hand
                SpawnHelperHand(targetObject, itemDragHandler, isForJojo);
            }
        }
    }

    private void SpawnHelperHand(GameObject targetObject, ItemDragHandler itemDragHandler, bool isForJojo)
    {
        if (helpPointerPrefab != null)
        {
            initialPosition = targetObject.transform.position;
            currentHelpPointer = Instantiate(helpPointerPrefab, initialPosition, Quaternion.identity);
            isHelperHandActive = true;

            // Play audio based on whether it's for Jojo or Kiki
            if (isForJojo && audioClip1 != null)
            {
                audioSource.clip = audioClip1;
                audioSource.Play();
                birdAnimator.SetTrigger("helper1"); // Trigger "helper1" for Jojo
            }
            else if (!isForJojo && audioClip2 != null)
            {
                audioSource.clip = audioClip2;
                audioSource.Play();
                birdAnimator.SetTrigger("helper2"); // Trigger "helper2" for Kiki
            }

            // Determine the target position for the tween based on the game object name and the action script that triggered the method
            if (isForJojo)
            {
                // Jojo's action: target position is the position where the toy should be placed
                switch (targetObject.name)
                {
                    case "wet kuma":
                        targetPosition = itemDragHandler.teddyPositionObject.transform.position;
                        Debug.Log($"Setting target position for wet kuma: {targetPosition}");
                        break;
                    case "wet dino":
                        targetPosition = itemDragHandler.dinoPositionObject.transform.position;
                        Debug.Log($"Setting target position for wet dino: {targetPosition}");
                        break;
                    case "wet bunny":
                        targetPosition = itemDragHandler.bunnyPositionObject.transform.position;
                        Debug.Log($"Setting target position for wet bunny: {targetPosition}");
                        break;
                    default:
                        // For clothes, Jojo's action uses the basketTransform
                        targetPosition = itemDragHandler.basketTransform.position;
                        Debug.Log($"Setting target position for clothes: {targetPosition}");
                        break;
                }
            }
            else
            {
                // Kiki's action: target position is the initial position where the toy should return to
                switch (targetObject.name)
                {
                    case "wet kuma":
                        targetPosition = itemDragHandler.teddyInitialPosition.transform.position;
                        Debug.Log($"Setting initial position for wet kuma: {targetPosition}");
                        break;
                    case "wet dino":
                        targetPosition = itemDragHandler.dinoInitalPosition.transform.position;
                        Debug.Log($"Setting initial position for wet dino: {targetPosition}");
                        break;
                    case "wet bunny":
                        targetPosition = itemDragHandler.bunnyInitialPosition.transform.position;
                        Debug.Log($"Setting initial position for wet bunny: {targetPosition}");
                        break;
                    case "wet_socK":
                    case "wet_shirT":
                    case "wet_shorT":
                        targetPosition = itemDragHandler.basketTransform.position;
                        Debug.Log($"Setting basket position for clothes: {targetPosition}");
                        break;
                    default:
                        targetPosition = itemDragHandler.basketTransform.position; // Fallback or other items
                        Debug.Log($"Setting fallback position: {targetPosition}");
                        break;
                }
            }

            // Start the looping tween animation
            StartTweenLoop();
        }
    }

    private void StartTweenLoop()
    {
        LeanTween.move(currentHelpPointer, targetPosition, tweenDuration).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
        {
            // Teleport the helper hand back to the initial position
            currentHelpPointer.transform.position = initialPosition;
            // Start the tween again
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
                // If the drop was correct, proceed to the next target
                DestroyHelperHand();
                currentTargetIndex++;
                StartCoroutine(CheckNextTarget(isForJojo));
            }
            else
            {
                // If the drop was incorrect, reset the timer and spawn the helper hand again for the same target
                DestroyHelperHand(); // Remove the current helper hand if active
                StartCoroutine(ResetTimerAndSpawnHelperHand());
            }
        }
    }

    private IEnumerator ResetTimerAndSpawnHelperHand()
    {
        // Wait for the delay timer before showing the helper hand again
        yield return new WaitForSeconds(delayTimer);

        // Spawn the helper hand for the current target object again
        CheckInteraction(isForJojo);
    }

    private void DestroyHelperHand()
    {
        if (currentHelpPointer != null)
        {
            Destroy(currentHelpPointer);
            isHelperHandActive = false;
        }
    }
}
