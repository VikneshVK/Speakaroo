using UnityEngine;
using System.Collections;

public class InteractableObject : MonoBehaviour
{
    public float delayBeforeHelp = 10f;
    private float interactionTimer = 0f;
    public bool isInteracted = false;
    public bool needsHelp = false;    
    private bool isTrackingEnabled = false;
    private bool isGlowDone = false;    
    private bool isPaused = false; // New flag to track if the timer is paused
    private DragAndDropController dragAndDropController;

    void Start()
    {
        dragAndDropController = GetComponent<DragAndDropController>();
        EnableInteractionTracking(); // Start tracking immediately
    }

    void Update()
    {
        if (ColliderIsEnabled() && !isInteracted && isTrackingEnabled && !isPaused)
        {
            interactionTimer += Time.deltaTime;

            // Trigger glow effect halfway through the delay time
            if (interactionTimer >= delayBeforeHelp / 2 && !needsHelp && !isGlowDone)
            {
                isGlowDone = true;
                HelpPointerManager.Instance.SpawnGlowEffect(gameObject);  // Spawn glow prefab
                StartCoroutine(PauseInteractionTimer(2f));
            }

            // Trigger helper hand and additional glow at the full delay
            if (interactionTimer >= delayBeforeHelp && !needsHelp)
            {
                needsHelp = true;
                HelpPointerManager.Instance.CheckAndShowHelpForObject(gameObject, delayBeforeHelp);                
            }
        }
    }

    private IEnumerator PauseInteractionTimer(float pauseDuration)
    {
        isPaused = true;
        yield return new WaitForSeconds(pauseDuration);
        isPaused = false;
    }

    public void EnableInteractionTracking()
    {
        isTrackingEnabled = true;
        interactionTimer = 0f;
        needsHelp = false;
        isGlowDone = false;        
    }

    private bool ColliderIsEnabled()
    {
        Collider2D collider = GetComponent<Collider2D>();
        return collider != null && collider.enabled;
    }

    public void OnInteract()
    {
        isInteracted = true;
        needsHelp = false; // Reset needsHelp
        interactionTimer = 0f; // Reset the timer
        isTrackingEnabled = false; // Stop tracking after interaction
        HelpPointerManager.Instance.ClearHelpRequest(this);
        HelpPointerManager.Instance.StopHelpPointer(); // Stop the help pointer
        Debug.Log($"Interaction completed for {gameObject.name}, resetting timer and stopping help pointer.");
    }

    public Vector3 GetDropLocation()
    {
        return dragAndDropController != null ? dragAndDropController.GetDropLocation2() : transform.position;
    }
}