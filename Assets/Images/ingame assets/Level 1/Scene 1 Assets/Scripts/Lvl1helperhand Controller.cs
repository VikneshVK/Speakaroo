using UnityEngine;

public class LVL1helperhandController : MonoBehaviour
{
    // Public variables
    public GameObject helperHandPrefab;
    public float waitTimer;

    private float currentTimer = 0f;
    private GameObject spawnedHelperHand;
    private bool timerActive = false; // Flag to control coroutine behavior

    // Public function to start the timer
    public void StartTimer(Vector3 spawnLocation, Vector3 targetLocation)
    {
        StopAllCoroutines(); // Stop any existing coroutines
        timerActive = true;  // Activate the timer
        StartCoroutine(TimerRoutine(spawnLocation, targetLocation));
        Debug.Log("helperTimerStarted");
    }

    // Public function to reset the timer
    public void ResetTimer()
    {
        timerActive = false; // Deactivate the timer
        currentTimer = 0f;

        if (spawnedHelperHand != null)
        {
            Destroy(spawnedHelperHand);
        }

        StopAllCoroutines(); // Ensure all coroutines are stopped
        Debug.Log("TimerEnded");
    }

    // Coroutine to manage the timer
    private System.Collections.IEnumerator TimerRoutine(Vector3 spawnLocation, Vector3 targetLocation)
    {
        while (currentTimer < waitTimer && timerActive)
        {
            currentTimer += Time.deltaTime;
            yield return null;
        }

        if (timerActive) // Check if timer is still active before spawning
        {
            // Spawn the helper hand prefab
            spawnedHelperHand = Instantiate(helperHandPrefab, spawnLocation, Quaternion.identity);

            // LeanTween movement loop
            LeanTween.move(spawnedHelperHand, targetLocation, 1.5f).setLoopClamp();
        }
    }
}
