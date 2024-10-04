using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HouseholdItemManager : MonoBehaviour
{
    //reference
    public Image itemImage;                 // Reference to the UI Image component
    public AudioSource audioSource;         // Reference to the AudioSource component
    public Sprite[] itemSprites;            // Array of item sprites
    public AudioClip[] itemAudioClips;      // Array of corresponding audio clips
    public Button nextButton;               // Reference to the Next button
    public Button previousButton;           // Reference to the Previous button  

    //mechanics
    private int currentIndex = 0;
    private bool[] rewardGiven;             // Array to track if reward has been given for each item
    private Vector3 initialPosition;        // Variable to store the initial position of the sprite
    private bool isAnimating = false;       // Track if an animation is currently playing
    public TextMeshProUGUI itemNameText;    //Name of the item name displayed

    //Progress
    public TextMeshProUGUI FinishedWordCount; //number of words finished
    public TextMeshProUGUI TotalWordCount;
    public Image progressBar;
    public int totalItems;
    public int finishedItems;


    // Swipe detection
    private Vector2 startTouchPosition;
    private Vector2 currentTouchPosition;
    private bool stopTouch = false;
    private float swipeRange = 50f;  // Minimum swipe distance to be considered a valid swipe
    //private float tapRange = 10f;
    //private bool isSwiping = false;
    public float swipeCooldown = 3f;
    private float lastSwipeTime;

    //congratulations
    private bool isCongratulating;
    public AudioSource congratulationsAudioSource;
    public AudioClip[] CongratulationsaudioClip;
    public GameObject confettiLeft;
    public GameObject confettiRight;
    public float sequenceDuration;

    //Baloon Pop miniGame
    public GameObject balloonPrefab;
    public Transform balloonSpawnPoint;
    public float balloonSpawnInterval = 0.5f;
    public float balloonFallSpeed = 2f;
    public int maxBalloonCount = 10;
    public int spawnedBalloonCount;
    public float gameplayDuration = 10f;  // Total duration for balloon gameplay
    public GameObject confettiPrefab;
    public bool isMiniGameActive;
    public GraphicRaycaster graphicRaycaster;  // Reference to the GraphicRaycaster attached to your Canvas
    public EventSystem eventSystem;


    public Transform confettiParent;

    public Scene_Manager miniSceneManager;

    ////Balloon
    //public GameObject balloonPrefab;  // Reference to the balloon prefab
    //public int balloonCount = 10;  // Number of balloons to spawn
    //public float balloonSpawnInterval = 0.3f;  // Time interval between spawning balloons
    //public float balloonSpeed = 2f;  // Speed at which balloons fall
    //public float sequenceDuration;  // Total duration for the balloon sequence
    //public Transform baloonSpawnPoint;


    void Start()
    {
        // Initialize the rewardGiven array based on the number of items
        rewardGiven = new bool[itemSprites.Length];

        // Store the initial position of the sprite
        initialPosition = itemImage.transform.position;
        totalItems = itemSprites.Length;

        TotalWordCount.text = totalItems.ToString("/" + totalItems);
        // Initialize with the first item
        DisplayItem(currentIndex, false);

        //AlignTextToImage();

        lastSwipeTime = -swipeCooldown;

        StartCoroutine(StartBalloonMiniGame());
    }

    void Update()
    {
        DetectSwipe();

        // Check if the audio is finished and no animation is happening
        if (!audioSource.isPlaying && !isAnimating && !rewardGiven[currentIndex])
        {
            RewardPlayer();
            Debug.Log("Audio finished, enabling buttons");

        }

        CheckUIRaycastBalloonPop();
    }
    // Time in seconds to debounce swipe input

    void DetectSwipe()
    {
        // Check cooldown
        if (Time.time - lastSwipeTime < swipeCooldown || isCongratulating)
        {
            Debug.Log("timer started & ended");
            return;  // Exit if the cooldown hasn't elapsed
        }

        // Touch input (mobile)
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
                stopTouch = false;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                currentTouchPosition = touch.position;
                Vector2 distance = currentTouchPosition - startTouchPosition;

                if (!stopTouch)
                {
                    if (distance.x < -swipeRange)
                    {
                        stopTouch = true;
                        Debug.Log("Swipe left detected");
                        NextItem();
                        lastSwipeTime = Time.time;  // Update the last swipe time
                    }
                    else if (distance.x > swipeRange)
                    {
                        stopTouch = true;
                        Debug.Log("Swipe right detected");
                        PreviousItem();
                        lastSwipeTime = Time.time;  // Update the last swipe time
                    }
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                stopTouch = true;
            }
        }
        // Mouse input (PC)
        else if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
            stopTouch = false;
        }
        else if (Input.GetMouseButton(0))
        {
            currentTouchPosition = Input.mousePosition;
            Vector2 distance = currentTouchPosition - startTouchPosition;

            if (!stopTouch)
            {
                if (distance.x < -swipeRange)
                {
                    stopTouch = true;
                    Debug.Log("Mouse swipe left detected");
                    NextItem();
                    lastSwipeTime = Time.time;  // Update the last swipe time
                }
                else if (distance.x > swipeRange)
                {
                    stopTouch = true;
                    Debug.Log("Mouse swipe right detected");
                    PreviousItem();
                    lastSwipeTime = Time.time;  // Update the last swipe time
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            stopTouch = true;
        }
    }


    public void DisplayItem(int index, bool animate = true)
    {
        Debug.Log("Displaying item: " + index + ", animate: " + animate);

        itemNameText.text = itemSprites[index].name;
        Debug.Log("Name update");

        if (animate)
        {
            isAnimating = true;

            // Move the current sprite out of the screen using LeanTween
            LeanTween.moveX(itemImage.gameObject, -Screen.width, 0.5f).setOnComplete(() =>
            {
                // After moving out, update the sprite and move the new one in
                itemImage.sprite = itemSprites[index];
                itemImage.transform.position = new Vector3(Screen.width, initialPosition.y, initialPosition.z);
                LeanTween.moveX(itemImage.gameObject, initialPosition.x, 0.5f).setOnComplete(() =>
                {
                    isAnimating = false;  // Animation finished
                });
            });

        }
        else
        {
            itemImage.sprite = itemSprites[index];
            itemImage.transform.position = initialPosition;
            isAnimating = false;
            Debug.Log("No animation, enabling buttons");

        }

        // Set the audio clip for the current item and play it
        audioSource.clip = itemAudioClips[index];
        audioSource.Play();
    }

    public void NextItem()
    {
        if (currentIndex < itemSprites.Length - 1)
        {
            currentIndex++;
            DisplayItem(currentIndex);
        }

        if (currentIndex == itemSprites.Length - 1 || currentIndex == 13)
        {
            Debug.Log("Mini game words finished");
            //StartCoroutine(miniGameEnd());


        }
    }

    public void PreviousItem()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            DisplayItem(currentIndex);
        }
    }


    void RewardPlayer()
    {
        if (!rewardGiven[currentIndex])
        {
            // Mark this item as rewarded
            rewardGiven[currentIndex] = true;

            // Increment finished items count
            finishedItems++;

            // Update the finished word count text
            Debug.Log($"Finished items: {finishedItems}");
            FinishedWordCount.text = $"{finishedItems}";

            // Calculate progress percentage
            float progress = (float)finishedItems / totalItems;

            // Update the progress bar's fill amount
            LeanTween.value(progressBar.gameObject, progressBar.fillAmount, progress, 0.5f).setOnUpdate((float val) =>
            {
                progressBar.fillAmount = val;
            });

            if (finishedItems % 5 == 0 && finishedItems > 0)
            {
                StartCoroutine(PlayCongratulatorySequence());
            }
        }
    }


    //Congratulations
    private IEnumerator PlayCongratulatorySequence()
    {
        isCongratulating = true;


        //for (int i = 0; i < balloonCount; i++)
        //{
        //    SpawnBalloon();
        //    yield return new WaitForSeconds(balloonSpawnInterval);
        //}


        // Play congratulatory audio
        int randomIndex = Random.Range(0, CongratulationsaudioClip.Length);
        congratulationsAudioSource.clip = CongratulationsaudioClip[randomIndex];
        congratulationsAudioSource.Play();

        // Play confetti particle effect
        confettiLeft.GetComponent<ParticleSystem>().Play();
        confettiRight.GetComponent<ParticleSystem>().Play();

        // Wait for the audio clip to finish
        yield return new WaitForSeconds(audioSource.clip.length);

        confettiLeft.GetComponent<ParticleSystem>().Stop();
        confettiRight.GetComponent<ParticleSystem>().Stop();
        // Re-enable buttons after the sequence
        yield return new WaitForSeconds(swipeCooldown + 5);
        isCongratulating = false;
    }


    // --- Balloon Mini-Game Section ---
    IEnumerator StartBalloonMiniGame()
    {
        isMiniGameActive = true;
        Debug.Log("Starting balloon mini-game");

        // Spawn balloons for the set duration or max count
        float endTime = Time.time + gameplayDuration;

        while (Time.time < endTime && spawnedBalloonCount < maxBalloonCount)
        {
            SpawnBalloon();
            spawnedBalloonCount++;
            yield return new WaitForSeconds(balloonSpawnInterval);
        }

        // After game end, you can call the end method (e.g., load a new scene or display a result)
        yield return new WaitForSeconds(gameplayDuration);
        StartCoroutine(PlayCongratulatorySequence());
    }

    void SpawnBalloon()
    {
        // X-axis: Spawn randomly between 0 and 1080 (Canvas width)
        float randomX = Random.Range(0, 1080);

        // Y-axis: Spawn just above the canvas, say at Y = 600
        Vector3 spawnPosition = new Vector3(randomX, 600, 0);

        // Convert screen space to canvas space (this assumes Screen Space - Overlay canvas)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)balloonSpawnPoint.parent, spawnPosition, null, out Vector2 localPoint);

        // Instantiate balloon at the calculated position
        GameObject balloon = Instantiate(balloonPrefab, balloonSpawnPoint);
        balloon.GetComponent<RectTransform>().anchoredPosition = localPoint;

        // Apply downward velocity for falling effect
        Rigidbody2D rb = balloon.GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(0, -balloonFallSpeed);  // Falls straight down

        // Destroy balloon after 10 seconds (or when it exits the bottom of the screen)
        Destroy(balloon, 10f);
    }


    // Raycast-based balloon popping
    void CheckUIRaycastBalloonPop()
    {
        if (Input.GetMouseButtonDown(0))  // Mouse or touch input
        {
            // Create a PointerEventData instance for the current mouse/touch position
            PointerEventData pointerData = new PointerEventData(eventSystem);
            pointerData.position = Input.mousePosition;

            // List to store the results of the raycast
            List<RaycastResult> results = new List<RaycastResult>();

            // Perform the UI raycast
            graphicRaycaster.Raycast(pointerData, results);

            // Loop through the raycast results to find any balloons
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.CompareTag("Balloon_MiniGame"))  // Check for the tag on UI element
                {
                    GameObject balloon = result.gameObject;
                    PopBalloon(balloon);
                    break;  // Stop after finding the first balloon
                }
            }
        }
    }

    // Balloon pop logic
    void PopBalloon(GameObject balloon)
    {
        StartCoroutine(PopBalloonRoutine(balloon));
    }

    IEnumerator PopBalloonRoutine(GameObject balloon)
    {
        // Scale down animation (using LeanTween or other tween engine)
        LeanTween.scale(balloon, Vector3.zero, 0.3f).setEaseInBack();

        // Wait for the animation to finish
        yield return new WaitForSeconds(0.3f);

        // Get the balloon's position in canvas space
        Vector3 balloonScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, balloon.transform.position);
        Vector3 confettiWorldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)confettiParent, balloonScreenPos, Camera.main, out confettiWorldPos);

        // Spawn confetti at the balloon's position in canvas space
        GameObject confetti = Instantiate(confettiPrefab, confettiWorldPos, Quaternion.identity, confettiParent);
        confetti.GetComponent<ParticleSystem>().Play();

        // Destroy the balloon object
        Destroy(balloon);

        // Destroy confetti after it plays
        Destroy(confetti, 2f);
    }


    IEnumerator miniGameEnd()
        {
            yield return new WaitForSeconds(2f);
            StartCoroutine(PlayCongratulatorySequence());
            yield return new WaitForSeconds(5.5f);
            miniSceneManager.LoadLevel("New_Level-Select");
        }

}
