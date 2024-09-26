using UnityEngine;

public class LVL5Sc2HelperController : MonoBehaviour
{
    public GameObject dirPrefab; // Reference to the directional prefab
    public Transform dirPrefabRefL; // Position reference for the left direction
    public Transform dirPrefabRefR; // Position reference for the right direction

    private GameObject spawnedDirPrefab; // Store the spawned prefab instance
    private SpriteRenderer dirPrefabSpriteRenderer; // Reference to the sprite renderer of the directional prefab

    private PhotoQuestManager photoQuestManager; // Reference to the PhotoQuestManager
    private float delayTimer = 5f; // Delay time before showing the helper
    private float currentDelay = 0f; // Track time elapsed since last interaction
    private bool isHelperActive = false; // To track if the helper is active

    void Start()
    {
        // Get the reference to PhotoQuestManager in the scene
        photoQuestManager = FindObjectOfType<PhotoQuestManager>();

        // Initialize the delay timer
        ResetDelayTimer();
    }

    void Update()
    {
        currentDelay += Time.deltaTime;

        // If the timer exceeds the delay and the helper is not active, show the helper
        if (currentDelay >= delayTimer && !isHelperActive)
        {
            ShowHelper();
        }

        // Check if the helper needs to be reactivated or deactivated based on the animal's position
        if (isHelperActive && spawnedDirPrefab != null)
        {
            Vector3 currentAnimalPosition = photoQuestManager.GetCurrentAnimalPosition();
            Vector3 cameraPosition = Camera.main.transform.position;

            // Deactivate helper if the animal is in the viewport
            if (Mathf.Abs(currentAnimalPosition.x - cameraPosition.x) < Camera.main.orthographicSize * Camera.main.aspect)
            {
                spawnedDirPrefab.SetActive(false);
                isHelperActive = false;
            }
            else if (!spawnedDirPrefab.activeSelf)
            {
                // Reactivate the helper if the animal is out of the viewport
                ShowHelper();
            }
        }
    }

    private void ShowHelper()
    {
        // Get the position of the current animal
        Vector3 currentAnimalPosition = photoQuestManager.GetCurrentAnimalPosition();
        Vector3 cameraPosition = Camera.main.transform.position;

        // Determine where to spawn the prefab based on the position of the animal relative to the camera
        if (currentAnimalPosition.x < cameraPosition.x)
        {
            // Spawn on the left and flip the sprite
            SpawnHelper(dirPrefabRefL.position, true);
        }
        else
        {
            // Spawn on the right without flipping
            SpawnHelper(dirPrefabRefR.position, false);
        }

        isHelperActive = true;
    }

    private void SpawnHelper(Vector3 position, bool flipX)
    {
        // If a prefab is already spawned, destroy it
        if (spawnedDirPrefab != null)
        {
            Destroy(spawnedDirPrefab);
        }

        // Instantiate the directional prefab at the specified position
        spawnedDirPrefab = Instantiate(dirPrefab, position, Quaternion.identity);

        // Get the SpriteRenderer component of the spawned prefab
        dirPrefabSpriteRenderer = spawnedDirPrefab.GetComponent<SpriteRenderer>();

        // Flip the sprite on the x-axis if needed
        dirPrefabSpriteRenderer.flipX = flipX;
    }

    public void ResetDelayTimer()
    {
        currentDelay = 0f;
    }

    // Call this method when validation is successful
    public void OnValidationSuccess()
    {
        // Destroy the spawned helper prefab if it exists
        if (spawnedDirPrefab != null)
        {
            Destroy(spawnedDirPrefab);
            spawnedDirPrefab = null;
        }

        // Reset the delay timer
        ResetDelayTimer();
        isHelperActive = false;
    }
}
