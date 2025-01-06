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
        if (ColliderIsEnabled() && !isInteracted && isTrackingEnabled && !needsHelp)
        {
            interactionTimer += Time.deltaTime;

            if (interactionTimer >= delayBeforeHelp)
            {
                needsHelp = true;
                HelpPointerManager.Instance.CheckAndShowHelpForObject(gameObject, delayBeforeHelp);
            }
        }
    }

    /* private IEnumerator PauseInteractionTimer(float pauseDuration)
     {
         isPaused = true;
         yield return new WaitForSeconds(pauseDuration);
         isPaused = false;
     }*/

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
        needsHelp = false;
        interactionTimer = 0f;
        isTrackingEnabled = false;
        HelpPointerManager.Instance.ClearHelpRequest(this); // Reset help when interacted
        Debug.Log($"Interaction completed for {gameObject.name}");
    }

    public Vector3 GetDropLocation()
    {
        return dragAndDropController != null ? dragAndDropController.GetDropLocation2() : transform.position;
    }
}