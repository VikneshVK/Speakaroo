using UnityEngine;
using System.Collections;

public class HelperPointer : MonoBehaviour
{
    public GameObject helperHandPrefab;
    public GameObject glowPrefab;  // Reference to the glow prefab
    public float helperDelay = 10f;  // Delay before showing the helper hand
    public float helperMoveDuration = 1f;  // Duration for the helper hand to move to the target
    public AudioSource audioSource;  // AudioSource attached to the HelperPointer game object

    private GameObject helperHandInstance;
    private GameObject glowInstance;
    private DragHandler currentDragHandler;
    private dragManager currentDragManager;
    private Coroutine helperCoroutine;  // To track the delay coroutine

    public void ScheduleHelperHand(DragHandler dragHandler, dragManager manager, float delay = 0f)
    {
        // Cancel any existing helper hand or coroutine
        StopHelperHand();

        // Set the new current DragHandler and dragManager
        currentDragHandler = dragHandler;
        currentDragManager = manager;

        // Start the coroutine to handle the delay and glow effect
        helperCoroutine = StartCoroutine(HandleHelperDelay(helperDelay + delay));
    }

    private IEnumerator HandleHelperDelay(float totalDelay)
    {
        // Wait for half of the delay
        float halfDelay = totalDelay / 2f;
        yield return new WaitForSeconds(halfDelay);

        // Spawn the glow object and tween its scale
        if (currentDragHandler != null && glowPrefab != null)
        {
            SpawnAndTweenGlow(currentDragHandler.transform.position);
        }

        // Wait for the glow tweening to finish (about 1.5 seconds total: 1 second pause + tween times)
        yield return new WaitForSeconds(1.5f);

        // Continue with the rest of the delay
        yield return new WaitForSeconds(halfDelay);

        // Now spawn the helper hand and the glow with ping-pong tweening
        StartHelperHand();
        SpawnAndTweenGlowPingPong(currentDragHandler.transform.position);
    }

    private void SpawnAndTweenGlow(Vector3 position)
    {
        // Destroy any existing glow instance
        if (glowInstance != null)
        {
            Destroy(glowInstance);
        }

        // Spawn the glow prefab at the current object's position
        glowInstance = Instantiate(glowPrefab, position, Quaternion.identity);

        // Tween the glow object's scale to 8, wait for 1 second, then scale back to 0
        LeanTween.scale(glowInstance, Vector3.one * 8, 0.5f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
        {
            LeanTween.scale(glowInstance, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInOutQuad).setDelay(1f).setOnComplete(() =>
            {
                Destroy(glowInstance);  // Destroy the glow object after tweening back to zero
            });
        });
    }

    private void SpawnAndTweenGlowPingPong(Vector3 position)
    {
        // Destroy any existing glow instance
        if (glowInstance != null)
        {
            Destroy(glowInstance);
        }

        // Spawn the glow prefab at the current object's position
        glowInstance = Instantiate(glowPrefab, position, Quaternion.identity);

        // Tween the glow object's scale with ping-pong effect (looping between 0 and 8)
        LeanTween.scale(glowInstance, Vector3.one * 8, 0.5f).setEase(LeanTweenType.easeInOutQuad).setLoopPingPong();
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

        // Destroy any glow instance
        if (glowInstance != null)
        {
            Destroy(glowInstance);
        }

        // Cancel any scheduled invocation of StartHelperHand
        CancelInvoke(nameof(StartHelperHand));

        // Stop the delay coroutine
        if (helperCoroutine != null)
        {
            StopCoroutine(helperCoroutine);
        }
    }
}
