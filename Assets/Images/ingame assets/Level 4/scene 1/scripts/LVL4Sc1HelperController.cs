using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LVL4Sc1HelperController : MonoBehaviour
{
    public float delayTimer = 5f; // Time in seconds
    public GameObject fridge; // Fridge reference set in the inspector
    public GameObject helperHandPrefab; // Helper hand prefab set in the inspector
    public GameObject helperHandPrefab2;
    public GameObject glowPrefab; // Glow prefab set in the inspector
    public GameObject spawnPosition;
   
    private Lvl4Sc1Audiomanger lvl4Sc1Audiomanger;
    public GameObject AudioManager;
    public AudioClip Audio1;
    private float currentTimer;
    private bool isTimerRunning;
    private bool isGlowActive;
    private GameObject glowInstance;
    private GameObject helperHandInstance;
    private GameObject currentInteractable; // Track the current interactable for glow and helper hand

    private Dictionary<GameObject, bool> interactables = new Dictionary<GameObject, bool>();

    void Start()
    {
        // Initialize only the fridge at the start
        if (fridge != null)
        {
            interactables[fridge] = false;
        }
        lvl4Sc1Audiomanger = AudioManager.GetComponent<Lvl4Sc1Audiomanger>();
    }

    void Update()
    {
        // Iterate over a copy of the dictionary keys to avoid modifying the dictionary during enumeration
        foreach (var item in new List<GameObject>(interactables.Keys))
        {
            if (item != null && item.GetComponent<Collider2D>().enabled && !interactables[item])
            {
                StartDelayTimer(item);
                interactables[item] = true;
            }
        }

        if (isTimerRunning)
        {
            currentTimer -= Time.deltaTime;

            // Pause at halfway point, show glow effect (skip for fridge)
            if (!isGlowActive && currentTimer <= delayTimer / 2)
            {
                isGlowActive = true;

                // Only spawn glow if the current interactable is not the fridge
                if (currentInteractable != fridge)
                {
                    SpawnGlow(currentInteractable);
                }

                Invoke(nameof(HideGlow), 2f); // Hide glow after 2 seconds
            }

            // When timer reaches zero, spawn helper hand
            if (currentTimer <= 0)
            {
                isTimerRunning = false;
                SpawnHelperHand();
            }
        }
    }

    private void StartDelayTimer(GameObject interactable)
    {
        currentTimer = delayTimer;
        isTimerRunning = true;
        isGlowActive = false;
        currentInteractable = interactable; // Set the current interactable being processed
        Debug.Log("Timer started for interactable: " + interactable.name); // Debug log
    }

    private void SpawnGlow(GameObject interactable)
    {
        Debug.Log("Spawning glow for interactable: " + interactable.name); // Debug log
        glowInstance = Instantiate(glowPrefab, interactable.transform.position, Quaternion.identity);

        // Set the color of the glow to golden yellow
        SpriteRenderer glowSpriteRenderer = glowInstance.GetComponent<SpriteRenderer>();
        if (glowSpriteRenderer != null)
        {
            glowSpriteRenderer.color = new Color(1f, 0.84f, 0f, 1f); // Golden yellow color
        }

        // Scale the glow up to 8
        LeanTween.scale(glowInstance, Vector3.one * 8, 0.5f).setEase(LeanTweenType.easeInOutQuad);
    }

    private void HideGlow()
    {
        if (glowInstance != null)
        {
            LeanTween.scale(glowInstance, Vector3.zero, 0.5f).setOnComplete(() => Destroy(glowInstance));
            Debug.Log("Glow hidden for interactable: " + currentInteractable.name); // Debug log
        }
    }

    private void SpawnHelperHand()
    {
        if (currentInteractable == fridge)
        {
            Debug.Log("Spawning helper hand from outside viewport for fridge"); // Debug log
            /*Vector3 spawnPosition = Camera.main.ViewportToWorldPoint(new Vector3(1.1f, 0.5f, 10f)); // Right side of the viewport*/
            helperHandInstance = Instantiate(helperHandPrefab2, spawnPosition.transform.position, Quaternion.identity);
            lvl4Sc1Audiomanger.PlayAudio(Audio1);            
        }
        else
        {
            Debug.Log("Spawning helper hand on interactable: " + currentInteractable.name); // Debug log
            helperHandInstance = Instantiate(helperHandPrefab, currentInteractable.transform.position, Quaternion.identity);

            // Get the Draggable component to access the drop target position
            Draggable draggable = currentInteractable.GetComponent<Draggable>();
            if (draggable != null)
            {
                LeanTween.move(helperHandInstance, draggable.DropTargetPosition, 2f).setEase(LeanTweenType.easeInOutQuad).setLoopClamp();
            }
            else
            {
                Debug.LogWarning("Draggable component missing on interactable: " + currentInteractable.name);
            }
        }
    }


    public void RegisterInteractable(GameObject interactable)
    {
        if (!interactables.ContainsKey(interactable))
        {
            interactables[interactable] = false;
            Debug.Log("Interactable registered: " + interactable.name); // Debug log
        }
    }

    public void ResetTimer()
    {
        Debug.Log("ResetTimer called: Attempting to destroy helper hand and glow.");

        currentTimer = delayTimer;
        isTimerRunning = false;

        // Destroy glow instance if it exists
        if (glowInstance != null)
        {
            Destroy(glowInstance);
            glowInstance = null;
            Debug.Log("Glow instance destroyed.");
        }

        // Destroy helper hand if it exists
        if (helperHandInstance != null)
        {
            LeanTween.cancel(helperHandInstance); // Cancel any active tweens
            Destroy(helperHandInstance);
            helperHandInstance = null;
            Debug.Log("Helper hand instance destroyed.");
        }
    }   
}
