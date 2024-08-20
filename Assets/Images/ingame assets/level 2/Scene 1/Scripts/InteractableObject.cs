using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public float delayBeforeHelp = 10f;
    private float interactionTimer = 0f;
    public bool isInteracted = false;
    public bool needsHelp = false;
    public bool IsDragAndDrop = true;
    private bool isTrackingEnabled = false;
    private DragAndDropController dragAndDropController;

    void Start()
    {
        dragAndDropController = GetComponent<DragAndDropController>();
        EnableInteractionTracking(); // Start tracking immediately
    }

    void Update()
    {
        /*Debug.Log($"{gameObject.name}: Timer: {interactionTimer}, isInteracted: {isInteracted}, needsHelp: {needsHelp}, ColliderEnabled: {ColliderIsEnabled()}");
*/
        if (ColliderIsEnabled() && !isInteracted && isTrackingEnabled)
        {
            interactionTimer += Time.deltaTime;

            if (interactionTimer >= delayBeforeHelp && !needsHelp)
            {
                needsHelp = true;
                Debug.Log("Object needs help: " + gameObject.name);
                HelpPointerManager.Instance.CheckAndShowHelpForObject(gameObject);
            }
        }
    }
    public void EnableInteractionTracking()
    {
        isTrackingEnabled = true;
        interactionTimer = 0f;  // Reset the timer when tracking starts
        needsHelp = false;      // Ensure the help request is reset
        Debug.Log("Tracking enabled and timer reset for " + gameObject.name);
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
        return dragAndDropController != null ? dragAndDropController.GetDropLocation() : transform.position;
    }
}
