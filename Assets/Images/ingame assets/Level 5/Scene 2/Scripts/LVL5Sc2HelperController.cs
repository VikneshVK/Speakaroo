using UnityEngine;
using System.Collections.Generic;

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

    public Sprite[] animalSprites; // Array to hold sprites for 6 animals

    // Helper hand references
    public GameObject helperHandPrefab;
    public Transform helperHandSpawnPosition;
    public Transform helperHandTargetPosition;

    private GameObject spawnedHelperHand;

    void Start()
    {
        // Reference to PhotoQuestManager in the scene
        photoQuestManager = FindObjectOfType<PhotoQuestManager>();
        ResetDelayTimer();
    }

    void Update()
    {
        currentDelay += Time.deltaTime;

        if (currentDelay >= delayTimer)
        {
            // Get positions for the target animal in all grounds
            List<Vector3> animalPositions = photoQuestManager.GetAnimalPositionsInAllGrounds(photoQuestManager.currentAnimalIndex);
            Vector3 cameraPosition = Camera.main.transform.position;

            // Check if any animal position is within the viewport
            bool anyAnimalInView = false;

            foreach (Vector3 position in animalPositions)
            {
                Vector3 viewportPosition = Camera.main.WorldToViewportPoint(position);

                // Log each animal's viewport position to check if it's within the viewport
                Debug.Log("Animal position: " + position + " | Viewport position: " + viewportPosition);

                // Check if the position is within the viewport (x and y between 0 and 1)
                if (viewportPosition.x >= 0 && viewportPosition.x <= 1 && viewportPosition.z > 0)
                {
                    anyAnimalInView = true;
                    Debug.Log("Animal is in view: " + position);
                    break;
                }
            }

            if (anyAnimalInView)
            {
                // Hide the helper if any instance of the animal is in the viewport
                if (isHelperActive)
                {
                    Debug.Log("Hiding helper as an animal instance is in view.");
                    HideHelper();
                }
            }
            else
            {
                // Show the helper if none of the instances are in the viewport
                Vector3 closestAnimalPosition = GetClosestPosition(animalPositions, cameraPosition);
                if (!isHelperActive)
                {
                    Debug.Log("Showing helper as all animal instances are out of view.");
                    ShowHelperOnEdge(closestAnimalPosition.x < cameraPosition.x);
                }
                else
                {
                    UpdateHelperPosition(closestAnimalPosition, cameraPosition);
                }
            }
        }
    }



    private Vector3 GetClosestPosition(List<Vector3> positions, Vector3 referencePosition)
    {
        Vector3 closestPosition = positions[0];
        float smallestDistance = Vector3.Distance(positions[0], referencePosition);

        foreach (var position in positions)
        {
            float distance = Vector3.Distance(position, referencePosition);
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                closestPosition = position;
            }
        }

        return closestPosition;
    }

    private void ShowHelperOnEdge(bool isLeft)
    {
        Vector3 edgePosition = isLeft ? dirPrefabRefL.position : dirPrefabRefR.position;
        SpawnHelper(edgePosition, isLeft);
        isHelperActive = true;
        SpawnHelperHand();
    }

    private void UpdateHelperPosition(Vector3 animalPosition, Vector3 cameraPosition)
    {
        if (spawnedDirPrefab != null)
        {
            spawnedDirPrefab.transform.position = animalPosition.x < cameraPosition.x ? dirPrefabRefL.position : dirPrefabRefR.position;
            dirPrefabSpriteRenderer.flipX = animalPosition.x < cameraPosition.x;

            if (!spawnedDirPrefab.activeSelf)
            {
                spawnedDirPrefab.SetActive(true);
            }
        }
    }

    private void SpawnHelper(Vector3 position, bool flipX)
    {
        if (spawnedDirPrefab != null)
        {
            Destroy(spawnedDirPrefab);
        }

        spawnedDirPrefab = Instantiate(dirPrefab, position, Quaternion.identity);

        Transform imageTransform = spawnedDirPrefab.transform.Find("image");
        if (imageTransform != null)
        {
            SpriteRenderer imageSpriteRenderer = imageTransform.GetComponent<SpriteRenderer>();
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

        dirPrefabSpriteRenderer = spawnedDirPrefab.GetComponent<SpriteRenderer>();
        dirPrefabSpriteRenderer.flipX = flipX;
    }

    private void SpawnHelperHand()
    {
        if (spawnedHelperHand != null)
        {
            Destroy(spawnedHelperHand);
        }

        spawnedHelperHand = Instantiate(helperHandPrefab, helperHandSpawnPosition.position, Quaternion.identity);

        LeanTween.move(spawnedHelperHand, helperHandTargetPosition.position, 1.5f)
            .setEaseInOutSine()
            .setLoopPingPong();
    }

    public void DestroyHelperHand()
    {
        if (spawnedHelperHand != null)
        {
            Destroy(spawnedHelperHand);
            spawnedHelperHand = null;
        }
    }

    public void DestroyDirPrefab()
    {
        if (spawnedDirPrefab != null)
        {
            Destroy(spawnedDirPrefab);
            spawnedDirPrefab = null;
        }
    }

    public void ResetDelayTimer()
    {
        currentDelay = 0f;
    }

    private void HideHelper()
    {
        if (spawnedDirPrefab != null)
        {
            spawnedDirPrefab.SetActive(false);
        }
        DestroyHelperHand();
        isHelperActive = false;
        ResetDelayTimer();
    }

    public void OnValidationSuccess()
    {
        DestroyHelperHand();
        DestroyDirPrefab();
        ResetDelayTimer();
        isHelperActive = false;
    }

    public void OnValidationFailed()
    {
        ResetDelayTimer();
        isHelperActive = false;
    }
}
