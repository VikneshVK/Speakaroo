using UnityEngine;
using System.Collections;

public class HelperPointer : MonoBehaviour
{
    public GameObject helperHandPrefab;
    public GameObject glowPrefab;
    public float helperDelay = 10f;
    public float helperMoveDuration = 1f;
    public AudioSource audioSource;

    private GameObject helperHandInstance;
    private GameObject glowInstance;
    private DragHandler currentDragHandler;
    private dragManager currentDragManager;
    private Coroutine helperCoroutine;

    public void ScheduleHelperHand(DragHandler dragHandler, dragManager manager, float delay = 0f)
    {
        // Stop any existing helper hand or coroutine
        StopHelperHand();

        // Set references to the drag handler and manager
        currentDragHandler = dragHandler;
        currentDragManager = manager;

        // Start the delay coroutine
        helperCoroutine = StartCoroutine(HandleHelperDelay(helperDelay + delay));
    }

    private IEnumerator HandleHelperDelay(float totalDelay)
    {
        float halfDelay = totalDelay / 2f;
        yield return new WaitForSeconds(halfDelay);

        // Spawn the glow effect
        if (currentDragHandler != null && glowPrefab != null)
        {
            SpawnAndTweenGlow(currentDragHandler.transform.position);
        }

        // Wait for glow tweening
        yield return new WaitForSeconds(1.5f);

        // Complete the remaining delay
        yield return new WaitForSeconds(halfDelay);

        // Spawn the helper hand and play the Kiki animation
        StartHelperHand();
        PlayKikiAnimation();
    }

    private void PlayKikiAnimation()
    {
        if (currentDragManager != null && currentDragManager.birdAnimator != null)
        {
            int index = dragManager.totalCorrectDrops;

            if (index < currentDragManager.triggerNames.Length)
            {
                string triggerName = currentDragManager.triggerNames[index];
                currentDragManager.birdAnimator.SetTrigger(triggerName);
                Debug.Log($"Playing Kiki animation: {triggerName}");
            }
            else
            {
                Debug.LogWarning("No trigger available for this index.");
            }
        }
    }

    private void SpawnAndTweenGlow(Vector3 position)
    {
        if (glowInstance != null)
        {
            Destroy(glowInstance);
        }

        glowInstance = Instantiate(glowPrefab, position, Quaternion.identity);
        LeanTween.scale(glowInstance, Vector3.one * 8, 0.5f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
        {
            LeanTween.scale(glowInstance, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInOutQuad).setDelay(1f).setOnComplete(() =>
            {
                Destroy(glowInstance);
            });
        });
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

            if (audioSource != null && currentDragManager != null && currentDragManager.audioSource != null)
            {
                audioSource.clip = currentDragManager.audioSource.clip;
                audioSource.Play();
                Debug.Log("Playing helper hand audio.");
            }
            else
            {
                Debug.LogWarning("AudioSource not set.");
            }
        }
    }

    private void StartHelperHandTween(DragHandler dragHandler)
    {
        if (helperHandInstance != null)
        {
            LeanTween.move(helperHandInstance, dragHandler.targetPosition.position, helperMoveDuration)
                .setEaseInOutQuad()
                .setLoopClamp();
        }
    }

    public void StopHelperHand()
    {
        if (helperHandInstance != null)
        {
            LeanTween.cancel(helperHandInstance);
            Destroy(helperHandInstance);
            helperHandInstance = null;
        }

        if (glowInstance != null)
        {
            LeanTween.cancel(glowInstance);
            Destroy(glowInstance);
            glowInstance = null;
        }

        StopAllCoroutines();
        if (helperCoroutine != null)
        {
            StopCoroutine(helperCoroutine);
        }
    }
}
