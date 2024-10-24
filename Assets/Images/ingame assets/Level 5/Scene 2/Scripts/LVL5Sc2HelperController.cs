using UnityEngine;

public class LVL5Sc2HelperController : MonoBehaviour
{
    public GameObject dirPrefab;
    public Transform dirPrefabRefL;
    public Transform dirPrefabRefR;

    private GameObject spawnedDirPrefab;
    private SpriteRenderer dirPrefabSpriteRenderer;

    private PhotoQuestManager photoQuestManager;
    public float delayTimer = 5f;
    private float currentDelay = 0f;
    private bool isHelperActive = false;

    // Reference to sprites corresponding to 6 animals
    public Sprite[] animalSprites; // Array to hold 6 sprites

    // References for the helper hand
    public GameObject helperHandPrefab;
    public Transform helperHandSpawnPosition;
    public Transform helperHandTargetPosition;

    private GameObject spawnedHelperHand;

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
        if (currentDelay >= delayTimer)
        {
            Vector3 currentAnimalPosition = photoQuestManager.GetCurrentAnimalPosition();
            Vector3 cameraPosition = Camera.main.transform.position;

            // Check if the animal is in the viewport
            if (Mathf.Abs(currentAnimalPosition.x - cameraPosition.x) >= Camera.main.orthographicSize * Camera.main.aspect)
            {
                if (!isHelperActive)
                {
                    ShowHelper(currentAnimalPosition, cameraPosition);
                }
                else
                {

                    UpdateHelperPosition(currentAnimalPosition, cameraPosition);
                }
            }
            else if (isHelperActive)
            {

                spawnedDirPrefab.SetActive(false);
                DestroyHelperHand();
                isHelperActive = false;
            }
        }
    }

    private void ShowHelper(Vector3 animalPosition, Vector3 cameraPosition)
    {
        // Determine where to spawn the prefab based on the position of the animal relative to the camera
        if (animalPosition.x < cameraPosition.x)
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

        // Spawn the helper hand and start tweening
        SpawnHelperHand();
    }

    private void UpdateHelperPosition(Vector3 animalPosition, Vector3 cameraPosition)
    {
        if (spawnedDirPrefab != null)
        {
            // Update the position of the helper continuously based on the animal's position relative to the camera
            if (animalPosition.x < cameraPosition.x)
            {
                // Update position to the left reference and flip the sprite
                spawnedDirPrefab.transform.position = dirPrefabRefL.position;
                dirPrefabSpriteRenderer.flipX = true;
            }
            else
            {
                // Update position to the right reference without flipping
                spawnedDirPrefab.transform.position = dirPrefabRefR.position;
                dirPrefabSpriteRenderer.flipX = false;
            }

            // Make sure the helper is active if it is not already
            if (!spawnedDirPrefab.activeSelf)
            {
                spawnedDirPrefab.SetActive(true);
            }
        }
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

        // Get the SpriteRenderer component of the child GameObject named "image"
        Transform imageTransform = spawnedDirPrefab.transform.Find("image");
        if (imageTransform != null)
        {
            SpriteRenderer imageSpriteRenderer = imageTransform.GetComponent<SpriteRenderer>();

            // Use the currentAnimalIndex from photoQuestManager to update the sprite of the image GameObject
            int currentAnimalIndex = photoQuestManager.currentAnimalIndex;
            if (currentAnimalIndex >= 0 && currentAnimalIndex < animalSprites.Length)
            {
                imageSpriteRenderer.sprite = animalSprites[currentAnimalIndex];
            }
        }
        else
        {
            Debug.LogError("No child GameObject named 'image' found in dirPrefab.");
        }

        // Flip the dirPrefab if needed
        dirPrefabSpriteRenderer = spawnedDirPrefab.GetComponent<SpriteRenderer>();
        dirPrefabSpriteRenderer.flipX = flipX;
    }

    private void SpawnHelperHand()
    {
        if (spawnedHelperHand != null)
        {
            Destroy(spawnedHelperHand);
        }

        // Instantiate the helper hand prefab
        spawnedHelperHand = Instantiate(helperHandPrefab, helperHandSpawnPosition.position, Quaternion.identity);

        // Tween the helper hand between spawn and target positions
        LeanTween.move(spawnedHelperHand, helperHandTargetPosition.position, 1.5f)
            .setEaseInOutSine()
            .setLoopPingPong();
    }

    // Public method to destroy the helper hand, callable from external scripts
    public void DestroyHelperHand()
    {
        if (spawnedHelperHand != null)
        {
            Destroy(spawnedHelperHand);
            spawnedHelperHand = null;
        }
    }

    public void ResetDelayTimer()
    {
        currentDelay = 0f;
    }

    // Call this method when validation is successful
    public void OnValidationSuccess()
    {
        if (spawnedDirPrefab != null)
        {
            Destroy(spawnedDirPrefab);
            spawnedDirPrefab = null;
        }

        // Reset the delay timer
        ResetDelayTimer();

        // Mark the helper as inactive so it will be spawned again for the next animal
        isHelperActive = false;
    }
}