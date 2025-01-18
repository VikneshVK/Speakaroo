using UnityEngine;
using System.Collections;

public class InteractableObject : MonoBehaviour
{
    public bool isInteracted = false;
    public bool needsHelp = false;
    private bool isTrackingEnabled = false;
    private bool isGlowDone = false;
    private bool isPaused = false;
    private DragAndDropController dragAndDropController;

    public static bool isBusDropped = false;
    public static bool isWhaleDropped = false;
    public static bool isBlockDropped = false;
    public GameObject bus;  // Add reference to the Bus GameObject
    public GameObject whale;  // Add reference to the Whale GameObject
    public GameObject building;  // Add reference to the Building GameObject
    void Start()
    {
        dragAndDropController = GetComponent<DragAndDropController>();
        EnableInteractionTracking(); // Start tracking immediately
    }

   

    public void EnableInteractionTracking()
    {
        isTrackingEnabled = true;        
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
        isTrackingEnabled = false;
        /*HelpPointerManager.Instance.ClearHelpRequest(this); // Reset help when interacted*/
        Debug.Log($"Interaction completed for {gameObject.name}");
    }

    public void UpdateDropStatus(string objectName)
    {
        if (objectName == "Bus")
        {
            isBusDropped = true;

        }
        else if (objectName == "Whale")
        {
            isWhaleDropped = true;
        }
        else if (objectName == "Building")
        {
            isBlockDropped = true;
        }
    }


    public Vector3 GetDropLocation()
    {
        return dragAndDropController != null ? dragAndDropController.GetDropLocation2() : transform.position;
    }

}
