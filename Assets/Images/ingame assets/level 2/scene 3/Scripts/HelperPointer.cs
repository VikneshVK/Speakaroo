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
        currentDragHandler = dragHandler;
        currentDragManager = manager;  // Store the dragManager instance
        CancelInvoke(nameof(StartHelperHand));
        Invoke(nameof(StartHelperHand), helperDelay + delay);  // Add delay parameter
    }

    private void StartHelperHand()
    {
        if (helperHandInstance != null)
        {
            Destroy(helperHandInstance);
        }

        if (currentDragHandler != null && currentDragHandler.GetComponent<Collider2D>().enabled && !currentDragHandler.IsDragged)
        {
            helperHandInstance = Instantiate(helperHandPrefab, currentDragHandler.transform.position, Quaternion.identity);
            StartHelperHandTween(currentDragHandler);

            // Use the AudioSource attached to the HelperPointer itself
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
        if (helperHandInstance != null)
        {
            LeanTween.move(helperHandInstance, dragHandler.targetPosition.position, helperMoveDuration).setOnComplete(() =>
            {
                helperHandInstance.transform.position = dragHandler.transform.position;
                StartHelperHandTween(dragHandler);
            });
        }
    }

    public void StopHelperHand()
    {
        if (helperHandInstance != null)
        {
            LeanTween.cancel(helperHandInstance);
            Destroy(helperHandInstance);
        }

        CancelInvoke(nameof(StartHelperHand));
    }
}
