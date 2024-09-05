using UnityEngine;

public class HelperPointer : MonoBehaviour
{
    public GameObject helperHandPrefab;
    public float helperDelay = 5f;  // Delay before showing the helper hand
    public float helperMoveDuration = 1f;  // Duration for the helper hand to move to the target
    public AudioSource audioSource;  // AudioSource attached to the HelperPointer game object

    private GameObject helperHandInstance;
    private DragHandler currentDragHandler;
    private dragManager currentDragManager;

    public void ScheduleHelperHand(DragHandler dragHandler, dragManager manager, float delay = 0f)
    {
        // Cancel any existing helper hand
        StopHelperHand();

        // Set the new current DragHandler and dragManager
        currentDragHandler = dragHandler;
        currentDragManager = manager;

        // Schedule the helper hand to appear after the delay
        CancelInvoke(nameof(StartHelperHand));
        Invoke(nameof(StartHelperHand), helperDelay + delay);  // Add delay parameter
    }

    private void StartHelperHand()
    {
        // Destroy any existing helper hand instance
        if (helperHandInstance != null)
        {
            Destroy(helperHandInstance);
        }

        // Only show the helper hand if the current DragHandler is not already being dragged and its collider is enabled
        if (currentDragHandler != null && currentDragHandler.GetComponent<Collider2D>().enabled && !currentDragHandler.IsDragged)
        {
            // Instantiate the helper hand at the DragHandler's position
            helperHandInstance = Instantiate(helperHandPrefab, currentDragHandler.transform.position, Quaternion.identity);

            // Start the helper hand's movement to the target position with looping
            StartHelperHandTween(currentDragHandler);

            // Play the audio for the helper hand using the AudioSource on HelperPointer
            if (audioSource != null && currentDragManager != null && currentDragManager.audioSource != null)
            {
                audioSource.clip = currentDragManager.audioSource.clip;  // Set the same clip from dragManager
                audioSource.Play();  // Play the audio using HelperPointer's AudioSource
                Debug.Log("Playing helper hand audio using HelperPointer's AudioSource.");
            }
            else
            {
                Debug.LogWarning("HelperPointer audioSource or dragManager's audioSource is not set.");
            }
        }
    }

    private void StartHelperHandTween(DragHandler dragHandler)
    {
        // Move the helper hand to the target position (object drop location)
        if (helperHandInstance != null)
        {
            LeanTween.move(helperHandInstance, dragHandler.targetPosition.position, helperMoveDuration)
                .setEaseInOutQuad()
                .setLoopClamp()  // Make the tween loop infinitely without resetting
                .setOnComplete(() =>
                {
                    Debug.Log("Helper hand is looping between positions.");
                });
        }
    }

    public void StopHelperHand()
    {
        // Cancel any active tweens and destroy the helper hand
        if (helperHandInstance != null)
        {
            LeanTween.cancel(helperHandInstance);  // Cancel any active tween
            Destroy(helperHandInstance);  // Destroy the helper hand instance
        }

        // Cancel any scheduled invocation of StartHelperHand
        CancelInvoke(nameof(StartHelperHand));
    }
}
