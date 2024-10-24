using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class DraggableFoods : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public AudioSource audioSource; // Reference to the AudioSource attached to the GameObject
    public AudioClip recordedAudio; // Reference to the recorded audio
    public TextMeshProUGUI dropTargetText; // Reference to TextComponent2 (drop target)
    public TextMeshProUGUI playbackText; // Reference to TextComponent1 (for playback)
    public TextMeshProUGUI childText;
    public GameObject panel; // Reference to the panel for tweening
    public GameObject[] prefabsToSpawn; // Array of prefabs to spawn based on currentStopIndex (assign in inspector)
    public Transform foodDropPoint; // Reference to the food drop point
    public float tweenDuration = 0.5f; // Duration for tweening panel scale
    public Animator jojoAnimator; // Reference to Jojo animator to set 'canTalk' parameter
    public Button retryButton; // Reference to the Retry Button
    public Image ringImage; // Ring image for progress indication during recording/playback
    public Image foodContainerImage;
    public Canvas canvas;
    public FoodContainerController foodContainerController;
    public AudioClip textAudioClip; // Audio clip to play when text is enabled
    public Sprite[] foodSprites;
    public RectTransform targetPosition; // Target position to teleport when dropped correctly
    public float dropOffset = 75f; // Configurable offset for drop area detection


    private Vector3 initialPosition;
    
    private AudioClip micRecording;
    
    private bool isInitialPositionStored = false;    
    private bool recordingCompleteFlag = false;
    private bool revealText = false;



    void Start()
    {
        retryButton.gameObject.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        
        if (!isInitialPositionStored)
        {
            initialPosition = transform.localPosition;
            isInitialPositionStored = true;
        }        
        childText.gameObject.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Convert the screen position to a local position in the parent RectTransform
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        // Update the position of the object being dragged
        transform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
       

        // Check if the food image is dropped on the correct drop target (TextComponent 2)
        RectTransform dropTargetRect = dropTargetText.rectTransform;

        // Use the configurable offset to allow a drop within a certain range
        if (RectTransformUtility.RectangleContainsScreenPoint(dropTargetRect, Input.mousePosition, canvas.worldCamera, new Vector4(dropOffset, dropOffset, dropOffset, dropOffset)))
        {
            // Snap the dragged object to the target position
            transform.position = targetPosition.position; // Teleport the dragged object

            // Change the sprite based on currentStopIndex
            ChangeSpriteBasedOnStopIndex();

            playbackText.text = "I want to eat    ";
            StartCoroutine(HandleSuccessfulDrop());
        }
        else
        {
            // If dropped incorrectly, reset the position
            transform.localPosition = initialPosition;
            foodContainerController.clicked = false;            
            childText.gameObject.SetActive(true);
        }
    }


    private void ChangeSpriteBasedOnStopIndex()
    {
        int currentStopIndex = Lvl7Sc1JojoController.currentStopIndex;

        if (currentStopIndex >= 1 && currentStopIndex <= foodSprites.Length)
        {
            // Change the sprite to the one corresponding to the currentStopIndex
            GetComponent<Image>().sprite = foodSprites[currentStopIndex - 1]; // Array is 0-based, so subtract 1
        }
        else
        {
            Debug.LogError("Invalid stop index or sprite not assigned.");
        }
    }

    private IEnumerator HandleSuccessfulDrop()
    {
        // Enable Retry Button (not interactable) with default sprite
        retryButton.gameObject.SetActive(true);
        retryButton.interactable = false;
        retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/DefaultSprite");

        // Play the audio attached to the GameObject (initial drop audio)
        audioSource.Play();
        yield return new WaitWhile(() => audioSource.isPlaying);

        // Start recording using the microphone
        StartCoroutine(StartRecording());
        yield return new WaitForSeconds(5); // Record for 5 seconds
        StopRecording();

        // Get the AudioSource attached to TextComponent1 (playbackText)
        AudioSource playbackSource = playbackText.GetComponent<AudioSource>();

        if (playbackSource == null)
        {
            Debug.LogError("No AudioSource found on playbackText GameObject!");
            yield break; // Stop if there's no AudioSource
        }

        if (recordedAudio == null)
        {
            Debug.LogError("No recorded audio available!");
            yield break; // Stop if recording failed
        }

        // Assign the recorded audio to the AudioSource
        playbackSource.clip = recordedAudio;

        // Debug logs to ensure everything is working as expected
        Debug.Log("Playing recorded audio... Clip length: " + playbackSource.clip.length);

        // Play back the recorded audio
        playbackSource.Play();

        // Change Retry Button sprite to PlaybackSprite during audio playback
        retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/PlaybackSprite");

        // Track the playback progress and update the ringImage's fill amount
        float playbackDuration = playbackSource.clip.length;
        float playbackTimeElapsed = 0f;

        while (playbackTimeElapsed < playbackDuration)
        {
            ringImage.fillAmount = playbackTimeElapsed / playbackDuration; // Update fill amount
            playbackTimeElapsed += Time.deltaTime;
            yield return null; // Wait until the next frame
        }

        ringImage.fillAmount = 1; // Ensure the ring is fully filled at the end of playback

        // Wait for playback to complete
        yield return new WaitWhile(() => playbackSource.isPlaying);

        // Now, wait for 3 seconds and visually represent the wait time in the ringImage
        retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/RetrySprite");

        float waitTime = 3f; // 3 seconds wait time
        float timeElapsed = 0f;

        while (timeElapsed < waitTime)
        {

            retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/RetrySprite");
            retryButton.interactable = true;
            ringImage.fillAmount = 1 - (timeElapsed / waitTime); // Show countdown (decreasing fill amount)
            timeElapsed += Time.deltaTime;
            yield return null; // Wait until the next frame
        }

        ringImage.fillAmount = 0; // Reset ringImage fill amount


        ResetBooleans();
        // Tween the panel's scale to zero
        LeanTween.scale(panel.GetComponent<RectTransform>(), Vector3.zero, tweenDuration).setEaseInOutQuad();

        // Disable the panel after tweening
        yield return new WaitForSeconds(tweenDuration);
        panel.SetActive(false);

        // Spawn the prefab dynamically based on currentStopIndex
        int currentStopIndex = Lvl7Sc1JojoController.currentStopIndex;
        if (currentStopIndex >= 1 && currentStopIndex <= prefabsToSpawn.Length)
        {
            GameObject prefabToSpawn = prefabsToSpawn[currentStopIndex - 1]; // Adjust index to match array (0-based index)
            GameObject spawnedPrefab = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);

            LeanTween.move(spawnedPrefab, foodDropPoint.position, tweenDuration)
         .setEaseInOutQuad()
         .setOnComplete(() =>
         {
             // Destroy the prefab after the tweening finishes
             Destroy(spawnedPrefab);
         });
        }
        foodContainerImage.enabled = true;
        foodContainerController.ResetClickCount();

        // Set 'canTalk' animation parameter to true
        jojoAnimator.SetBool("canTalk", true);
    }

    private void ResetBooleans()
    {
        foodContainerController.clicked = false;
        isInitialPositionStored = false;
        transform.localPosition = initialPosition;
        AudioSource playbackSource = playbackText.GetComponent<AudioSource>();
        playbackSource.clip = null;
        retryButton.gameObject.SetActive(false);
        childText.gameObject.SetActive(true);
        playbackText.text = "What do you want to Eat?";
        playbackText.gameObject.SetActive(false);
        dropTargetText.gameObject.SetActive(false);
    }


    private IEnumerator StartRecording()
    {
        // Change Retry Button sprite to RetrySprite during recording
        retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/RetrySprite");

        int recordingDuration = 5;
        int frequency = 44100;

        audioSource.clip = Microphone.Start(null, false, recordingDuration, frequency);
        Debug.Log("Recording started...");

        float timeElapsed = 0f;
        while (timeElapsed < recordingDuration)
        {
            ringImage.fillAmount = timeElapsed / recordingDuration;
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        ringImage.fillAmount = 1;

        while (Microphone.IsRecording(null))
        {
            yield return null;
        }

        Debug.Log("Recording completed.");
    }

    private void StopRecording()
    {
        // Use the default microphone (null for default)
        int recordingLength = Microphone.GetPosition(null);
        Microphone.End(null); // End the default microphone

        if (recordingLength > 0)
        {
            // Assign the recorded audio clip
            recordedAudio = AudioClip.Create("RecordedAudio", recordingLength, 1, 44100, false);
            float[] data = new float[recordingLength];
            audioSource.clip.GetData(data, 0); // Copy the microphone data to the audio clip
            recordedAudio.SetData(data, 0);
            Debug.Log("Recorded audio captured. Length: " + recordedAudio.length);
        }
        else
        {
            Debug.LogError("Recording length is zero, something went wrong.");
        }
    }

}
