using UnityEngine;

public class LVL7Sc2HelperFunction : MonoBehaviour
{
    [Header("Helper Settings")]
    public float helpTimer = 5f; // Timer duration
    public GameObject helperHandPrefab; // Reference to the helper hand prefab

    private float timer = 0f;
    private GameObject activeHelperHand;

    // Variables to store spawn and end positions
    private Vector3 currentSpawnLocation;
    private Vector3 currentEndLocation;

    // Public method to start the timer
    public void StartTimer(Vector3 spawnLocation, Vector3 endLocation)
    {
        timer = 0f;
        currentSpawnLocation = spawnLocation;
        currentEndLocation = endLocation;
        InvokeRepeating(nameof(UpdateTimer), 0f, Time.deltaTime);
    }

    // Public method to reset the timer and destroy active helper hand
    public void ResetTimer()
    {
        timer = 0f;
        if (activeHelperHand != null)
        {
            Destroy(activeHelperHand);
        }
        CancelInvoke(nameof(UpdateTimer));
    }

    // Update the timer
    private void UpdateTimer()
    {
        timer += Time.deltaTime;
        if (timer >= helpTimer)
        {
            CancelInvoke(nameof(UpdateTimer));
            SpawnAndTweenHelperHand(currentSpawnLocation, currentEndLocation);
        }
    }

    // Spawn and tween the helper hand
    private void SpawnAndTweenHelperHand(Vector3 spawnLocation, Vector3 endLocation)
    {
        if (helperHandPrefab == null) return;

        if (activeHelperHand == null)
        {
            activeHelperHand = Instantiate(helperHandPrefab, spawnLocation, Quaternion.identity);
        }

        // Loop tweening
        LeanTween.move(activeHelperHand, endLocation, 1f).setOnComplete(() =>
        {
            activeHelperHand.transform.position = spawnLocation; // Teleport back
            SpawnAndTweenHelperHand(spawnLocation, endLocation); // Repeat the tweening
        });
    }
}
