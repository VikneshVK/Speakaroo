using UnityEngine;

public class HelpPointerManager : MonoBehaviour
{
    public static HelpPointerManager Instance;
    public static bool IsAnyObjectBeingInteracted = false;
    public GameObject pointer;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();  // Get the AudioSource component attached to the game object
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
            pointer.transform.position = interactable.transform.position;  // Start at the object's position
            pointer.SetActive(true);  // Activate the pointer

            // Play the audio when the helper hand is activated
            if (audioSource != null)
            {
                audioSource.Play();
            }

            // Assuming this is for drag-and-drop objects
            if (interactable.IsDragAndDrop)
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
        }
    }

    public void ClearHelpRequest(InteractableObject interactable)
    {
        // If the pointer is currently showing help for this object, stop it
        if (pointer.activeInHierarchy && pointer.transform.position == interactable.transform.position)
        {
            StopHelpPointer();
        }
    }

    public void StopHelpPointer()
    {
        LeanTween.cancel(pointer);  // Stop any ongoing tween
        pointer.SetActive(false);   // Deactivate the pointer

        if (audioSource != null)
        {
            audioSource.Stop();  // Stop the audio if the helper hand is deactivated
        }

        Debug.Log("Help pointer deactivated");
    }
}
