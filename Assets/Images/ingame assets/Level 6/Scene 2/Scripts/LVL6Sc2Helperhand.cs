using UnityEngine;
using System.Collections;

public class LVL6Sc2Helperhand : MonoBehaviour
{
    [Header("Helper Hand Settings")]
    [SerializeField] private GameObject helperHandPrefab; // Prefab for the helper hand
    [SerializeField] private float delayTimer = 5.0f; // Delay timer from inspector

    private GameObject currentQuestObject; // The target quest object
    private GameObject spawnedHelperHand; // The instantiated helper hand
    private Coroutine timerCoroutine; // Reference to the delay timer coroutine
    private bool isTweenBirdBackCalled = false; // Flag to track if TweenBirdandback was called

    // Starts the delay timer
    public void StartDelayTimer(GameObject questObject)
    {
        currentQuestObject = questObject;
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        timerCoroutine = StartCoroutine(DelayTimerCoroutine());
    }

    // Coroutine to handle the delay timer
    private IEnumerator DelayTimerCoroutine()
    {
        
        yield return new WaitForSeconds(delayTimer);
                
        if (!isTweenBirdBackCalled)
        {
            SpawnHelperHand();
        }
    }

    // Spawns the helper hand and tweens it to the current quest object in a loop
    private void SpawnHelperHand()
    {
        
        spawnedHelperHand = Instantiate(helperHandPrefab, transform.position, Quaternion.identity);
        
        LeanTween.move(spawnedHelperHand, currentQuestObject.transform.position, 2f)
            .setLoopClamp()
            .setEase(LeanTweenType.linear);
    }

    // Destroys the helper hand and resets the delay timer
    public void DestroyAndResetTimer()
    {
        if (spawnedHelperHand != null)
        {
            Destroy(spawnedHelperHand);
        }

        isTweenBirdBackCalled = false; // Reset flag
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    // Called when TweenBirdandback is triggered; manages helper hand destruction based on timing
    public void OnTweenBirdAndBackCalled()
    {
        
        if (timerCoroutine != null && !isTweenBirdBackCalled)
        {
            isTweenBirdBackCalled = true;
        }
        else if (spawnedHelperHand != null)
        {            
            Destroy(spawnedHelperHand);
        }
    }
}
