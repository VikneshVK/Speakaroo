using UnityEngine;
using System.Collections.Generic;

public class HelpPointerManager : MonoBehaviour
{
    public static HelpPointerManager Instance;
    public static bool IsAnyObjectBeingInteracted = false;
    public GameObject pointer;
    private List<InteractableObject> objectsNeedingHelp = new List<InteractableObject>();
    private InteractableObject currentObject = null;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CheckAndShowHelpForObject(GameObject reactivatedObject)
    {
        InteractableObject interactable = reactivatedObject.GetComponent<InteractableObject>();

        if (interactable != null && !interactable.isInteracted)
        {
            Collider2D objectCollider = reactivatedObject.GetComponent<Collider2D>();
            if (objectCollider != null && objectCollider.enabled)
            {
                if (!objectsNeedingHelp.Contains(interactable))
                {
                    objectsNeedingHelp.Add(interactable);
                }

                Debug.Log($"Activating help pointer for {interactable.gameObject.name}");
                ShowHelpForSpecificObject(interactable);
            }
        }
    }

    private void ShowHelpForSpecificObject(InteractableObject interactable)
    {
        if (IsAnyObjectBeingInteracted) return;

        Debug.Log($"Showing help pointer for {interactable.gameObject.name}");
        pointer.transform.position = interactable.transform.position;  // Start at the object's position
        pointer.SetActive(true);  // Activate the pointer

        if (interactable.IsDragAndDrop)  // If the object is draggable
        {
            Vector3 dropPosition = interactable.GetDropLocation();  // Get the drop location

            // Start the looping tween
            void LoopTween()
            {
                LeanTween.move(pointer, dropPosition, 1f)
                    .setEase(LeanTweenType.easeInOutSine)
                    .setOnComplete(() =>
                    {
                        pointer.transform.position = interactable.transform.position;  // Teleport back to the initial position
                        LoopTween();  // Start the tween again
                    });
            }

            LoopTween();  // Start the first tween
        }
        else
        {
            Debug.Log("Object is not draggable, no further movement.");
        }
    }



    public void StopHelpPointer()
    {
        LeanTween.cancel(pointer);  // Stop any ongoing tween
        pointer.SetActive(false);  // Deactivate the helping hand
        Debug.Log("Help pointer deactivated");
    }

    public void ClearHelpRequest(InteractableObject interactable)
    {
        if (objectsNeedingHelp.Contains(interactable))
        {
            objectsNeedingHelp.Remove(interactable);
        }
    }
}
