using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;

public class BubbleSpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject[] bubblePrefabs; // Array to hold multiple bubble prefabs
    [SerializeField] private Transform bubbleCanvas; // Reference to the Bubble Canvas
    [SerializeField] private float minSpeed = 2f; // Minimum speed for bubbles
    [SerializeField] private float maxSpeed = 5f; // Maximum speed for bubbles
    [SerializeField] private GameObject LvlCompletePanel;
    [SerializeField] private GameObject LvlCompleteImage;   
    [SerializeField] private GameObject confetti1; // Reference to Confetti 1
    [SerializeField] private GameObject confetti2; // Reference to Confetti 2
    [SerializeField] private TextMeshProUGUI lvlCompleteText;
    private int[] weights = { 20, 16, 16, 16, 16, 16 };
    public event Action OnBubblesDestroyed;
    private Camera mainCamera;
    private List<GameObject> activeBubbles = new List<GameObject>();
    private void Awake()
    {
        ResetPanelAndImage(); // Reset everything at the start
    }

    private void Start()
    {
        mainCamera = Camera.main;/*
        StartBubbleSpawning();*/
    }
    public void StartBubbleSpawning()
    {
        SetInitialAlpha(LvlCompletePanel.GetComponent<Image>());
        SetInitialAlpha(LvlCompleteImage.GetComponent<Image>());

        LeanTween.alpha(LvlCompletePanel.GetComponent<RectTransform>(), 1f, 0.5f)
        .setOnComplete(() =>
        {
                LeanTween.alpha(LvlCompleteImage.GetComponent<RectTransform>(), 1f, 0.5f)
                .setOnComplete(() =>
                {
                       lvlCompleteText.GetComponent<AudioSource>().Play();
                       StartCoroutine(AnimateText(lvlCompleteText, "LEVEL COMPLETED"));
                       PlayConfettiEffects();
                       StartCoroutine(SpawnBubblesAtPositions());
                });
        });
            
    }

    private void SetInitialAlpha(Image image)
    {
        Color initialColor = image.color;
        initialColor.a = 0;
        image.color = initialColor; // Set the initial alpha to 0 (fully transparent)
    }

    private void PlayConfettiEffects()
    {
        if (confetti1 != null && confetti2 != null)
        {
            confetti1.SetActive(true);
            confetti2.SetActive(true);

            var confetti1ParticleSystem = confetti1.GetComponent<ParticleSystem>();
            var confetti2ParticleSystem = confetti2.GetComponent<ParticleSystem>();

            if (confetti1ParticleSystem != null) confetti1ParticleSystem.Play();
            if (confetti2ParticleSystem != null) confetti2ParticleSystem.Play();
        }
    }


    private IEnumerator AnimateText(TextMeshProUGUI textComponent, string textToDisplay)
    {
        textComponent.text = ""; // Clear the text initially
        foreach (char letter in textToDisplay)
        {
            textComponent.text += letter;
            yield return new WaitForSeconds(0.1f); // Wait for 0.1 seconds between each letter
        }
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

        float bubbleSpeed = UnityEngine.Random.Range(minSpeed, maxSpeed);
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
        foreach (GameObject bubble in activeBubbles)
        {
            Destroy(bubble);
        }

        activeBubbles.Clear();        

        OnBubblesDestroyed?.Invoke(); // Invoke the event

        LeanTween.alpha(LvlCompleteImage.GetComponent<RectTransform>(), 0f, 0.5f)
         .setOnComplete(() =>
         {
             LeanTween.alpha(LvlCompletePanel.GetComponent<RectTransform>(), 0f, 0.5f)
                 .setOnComplete(() =>
                 {
                     ResetPanelAndImage(); // Reset after fade-out completes
                 });
         });
    }

    private void ResetPanelAndImage()
    {
        // Reset the alpha
        SetInitialAlpha(LvlCompletePanel.GetComponent<Image>());
        SetInitialAlpha(LvlCompleteImage.GetComponent<Image>());

        // Reset the text
        lvlCompleteText.text = "";
    }

    private GameObject SelectPrefabBasedOnWeight()
    {
        int totalWeight = 0;
        foreach (int weight in weights)
        {
            totalWeight += weight;
        }

        int randomValue = UnityEngine.Random.Range(0, totalWeight);
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
