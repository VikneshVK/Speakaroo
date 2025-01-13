using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BubbleSpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject[] bubblePrefabs; // Array to hold multiple bubble prefabs
    [SerializeField] private Transform bubbleCanvas; // Reference to the Bubble Canvas
    [SerializeField] private float minSpeed = 2f; // Minimum speed for bubbles
    [SerializeField] private float maxSpeed = 5f; // Maximum speed for bubbles
    private int[] weights = { 15, 15, 15, 15, 10, 15, 15 };

    private Camera mainCamera;
    private List<GameObject> activeBubbles = new List<GameObject>(); 

    private void Start()
    {
        mainCamera = Camera.main;
        StartBubbleSpawning();
    }
    public void StartBubbleSpawning()
    {
        StartCoroutine(SpawnBubblesAtPositions());
    }
    private IEnumerator SpawnBubblesAtPositions()
    {
        float timer = 0f;
        yield return new WaitForSeconds(1f);
        
        while (timer < 15f)
        {
            for (int i = 0; i < 5; i++)
            {
                SpawnBubbleAtPosition(i);
            }

            timer += 1f; 
            yield return new WaitForSeconds(1.15f); // Adjust the spawn rate here (every 1 second)
        }

        // After 15 seconds, destroy all active bubbles
        DestroyAllBubbles();
    }

    private void SpawnBubbleAtPosition(int positionIndex)
    {
        GameObject selectedPrefab = SelectPrefabBasedOnWeight();

        Vector3 spawnPosition = GetExactPositionForSpawn(positionIndex);

        GameObject bubble = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity, bubbleCanvas);

        Button bubbleButton = bubble.GetComponent<Button>();
        if (bubbleButton != null)
        {
            bubbleButton.interactable = true;
            bubbleButton.onClick.AddListener(() => OnBubbleClick(bubble, spawnPosition)); 
        }

        float bubbleSpeed = Random.Range(minSpeed, maxSpeed);
        Vector3 targetPosition = new Vector3(spawnPosition.x, -10f, spawnPosition.z); 

        TweenBubble(bubble, spawnPosition, targetPosition, bubbleSpeed);

        activeBubbles.Add(bubble);
    }

    private void OnBubbleClick(GameObject bubble, Vector3 spawnPosition)
    {
        Debug.Log("Bubble clicked: " + bubble.name); 
        
        AudioSource audioSource = bubble.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();
        }

        Animator animator = bubble.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Pop"); 
        }

        Destroy(bubble, 1f); 

        activeBubbles.Remove(bubble);
    }

    private void TweenBubble(GameObject bubble, Vector3 startPosition, Vector3 targetPosition, float speed)
    {
        float travelTime = Vector3.Distance(startPosition, targetPosition) / speed;

        // Tween from start to target (bottom)
        LeanTween.move(bubble, targetPosition, travelTime)
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                Destroy(bubble); // Destroy the bubble after reaching the target position
                activeBubbles.Remove(bubble); // Remove from active list once destroyed
            });

        // No need to use Destroy(bubble, 15f) anymore, as destruction happens via tween completion
    }

    private void DestroyAllBubbles()
    {
        // Destroy all the active bubbles
        foreach (GameObject bubble in activeBubbles)
        {
            Destroy(bubble);
        }

        // Clear the active bubbles list
        activeBubbles.Clear();
    }

    private GameObject SelectPrefabBasedOnWeight()
    {
        int totalWeight = 0;
        foreach (int weight in weights)
        {
            totalWeight += weight;
        }

        int randomValue = Random.Range(0, totalWeight);
        int runningSum = 0;

        for (int i = 0; i < bubblePrefabs.Length; i++)
        {
            runningSum += weights[i];
            if (randomValue < runningSum)
            {
                return bubblePrefabs[i];
            }
        }

        // Fallback in case something goes wrong
        return bubblePrefabs[0];
    }

    private Vector3 GetExactPositionForSpawn(int positionIndex)
    {
        // Get the width of the first prefab's image
        Sprite bubbleSprite = bubblePrefabs[0].GetComponent<Image>().sprite;
        float imageWidth = bubbleSprite.rect.width; // Get the image width

        // Calculate the padding (half of the image width)
        float padding = imageWidth / 2f;

        // Calculate the range for bubble spawn positions
        float minX = padding / Screen.width; // Convert padding to a fraction of the screen width
        float maxX = (Screen.width - padding) / Screen.width; // The last position is reduced by padding

        // We will now manually assign the bubble's X position based on 5 sections
        float x = 0f;
        switch (positionIndex)
        {
            case 0:
                x = minX; // First position
                break;
            case 1:
                x = Mathf.Lerp(minX, maxX, 0.25f); // Second position
                break;
            case 2:
                x = Mathf.Lerp(minX, maxX, 0.5f); // Middle position
                break;
            case 3:
                x = Mathf.Lerp(minX, maxX, 0.75f); // Fourth position
                break;
            case 4:
                x = maxX; // Last position
                break;
        }

        // Always spawn at the top edge (1.1f) with padding
        float y = 1.1f;

        // Use a fixed Z value (10f is a good value to place the bubble a little in front of the camera)
        return mainCamera.ViewportToWorldPoint(new Vector3(x, y, 10f));
    }
}
