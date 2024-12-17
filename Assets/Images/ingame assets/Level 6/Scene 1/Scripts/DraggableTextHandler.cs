using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

public class DraggableTextHandler : MonoBehaviour
{
    public Transform dropPosition; // Set this in the inspector for the drop target
    public AudioSource audioSource; // AudioSource attached to this text
    public AudioSource audioSource2;
    public Image ringImage; // The ring to show progress
    public GameObject panel; // Reference to the panel containing buttons and texts
    public GameObject prefabToSpawn; // Reference to the prefab to spawn after panel scales down
    public AudioMixer audioMixer;

    public AudioSource buttonAudioSource; // AudioSource attached to the button
    public GameObject childTextObject; // The TextMeshPro child object to enable and scale
    public Button retryButton;
    public Canvas canvas; // Reference to the canvas
    public TextMeshProUGUI tmpText; // Reference to the TMP component
    public TextMeshProUGUI textComponent;
    public TextMeshProUGUI textComponent2;
    public Image imageComponent; // Reference to the Image component to change sprite
    public string spriteName;

    private Vector3 initialPosition;
    private bool isDropped = false;
    private bool isBeingDragged = false;
    private static List<DraggableTextHandler> allDraggableTextHandlers = new List<DraggableTextHandler>(); // Static list of all instances

    /*private string recordedClipName = "RecordedAudio";*/

    public BeachBoxHandler beachBoxHandler;

     private const string musicVolumeParam = "MusicVolume";
    private const string AmbientVolumeParam = "AmbientVolume";

    private void Awake()
    {
        // Add this instance to the list of all draggable text handlers
        allDraggableTextHandlers.Add(this);
    }

    private void OnDestroy()
    {
        // Remove this instance from the list when destroyed
        allDraggableTextHandlers.Remove(this);
    }

    private void Start()
    {
        // Get the initial position dynamically from BeachBoxHandler once at start
        UpdateInitialPosition();
        if (ringImage != null) ringImage.fillAmount = 0;
    }

    private void UpdateInitialPosition()
    {
        if (beachBoxHandler != null)
        {
            initialPosition = beachBoxHandler.GetButtonPosition(gameObject.name);
            Debug.Log($"Updated initial position for {gameObject.name}: {initialPosition}");
        }
        else
        {
            initialPosition = transform.position; // Fallback to current position
            Debug.LogWarning($"BeachBoxHandler not assigned for {gameObject.name}. Using current position.");
        }
    }


    private void Update()
    {
        if (!isDropped && isBeingDragged)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out Vector3 worldPoint
            );
            transform.position = worldPoint;
        }
    }

    public void OnMouseDown()
    {
        if (!isDropped)
        {
            buttonAudioSource.Play();
            isBeingDragged = true;
            tmpText.enabled = false;

            // Disable EventTrigger on other buttons to prevent dragging
            DisableOtherButtonsEventTriggers();
        }
    }

    public void OnMouseUp()
    {
        isBeingDragged = false;

        Vector2 screenDropPosition = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, dropPosition.position);
        Vector2 screenObjectPosition = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, transform.position);

        float distance = Vector2.Distance(screenObjectPosition, screenDropPosition);

        if (distance < 250f) // Adjust threshold as needed
        {
            // Dropped correctly
            isDropped = true;

            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvas.transform as RectTransform,
                screenDropPosition,
                canvas.worldCamera,
                out Vector3 worldPoint
            );

            transform.position = worldPoint;
            textComponent2.enabled = false;

            // Disable the sprite image and set text based on the object name
            imageComponent.enabled = false;

            switch (gameObject.name)
            {
                case "Beach Ball":
                    textComponent.text = "I want to play Ball";
                    break;
                case "Bubbles":
                    textComponent.text = "I want to play Bubbles";
                    break;
                case "Frisbee":
                    textComponent.text = "I want to play Frisbee";
                    break;
                default:
                    textComponent.text = "What do you want to Play?";
                    break;
            }

            audioSource.Play();
            StartCoroutine(HandleAudioAndRecording());
        }
        else
        {
            // Dropped incorrectly: Reset to initial position
            transform.position = initialPosition; // Reset position
            tmpText.enabled = true;
            EnableAllButtonsEventTriggers(); // Re-enable dragging for all buttons
            Debug.Log($"Reset position to {initialPosition} for {gameObject.name}");
        }
    }

    private void SetMusicVolume(float volume)
    {
        if (audioMixer != null)
        {
            bool result = audioMixer.SetFloat(musicVolumeParam, volume); // "MusicVolume" should match the exposed parameter name
            if (!result)
            {
                Debug.LogError($"Failed to set MusicVolume to {volume}. Is the parameter exposed?");
            }
        }
        else
        {
            Debug.LogError("AudioMixer is not assigned in the Inspector.");
        }
    }
    private void SetAmbientVolume(float volume)
    {
        if (audioMixer != null)
        {
            bool result = audioMixer.SetFloat(AmbientVolumeParam, volume);
            if (!result)
            {
                Debug.LogError($"Failed to set MusicVolume to {volume}. Is the parameter exposed?");
            }
        }
        else
        {
            Debug.LogError("AudioMixer is not assigned in the Inspector.");
        }
    }

    private void DisableOtherButtonsEventTriggers()
    {
        foreach (var handler in allDraggableTextHandlers)
        {
            if (handler != this) // Disable EventTrigger only on other buttons
            {
                EventTrigger eventTrigger = handler.GetComponent<EventTrigger>();
                if (eventTrigger != null)
                {
                    eventTrigger.enabled = false; // Disable EventTrigger to prevent dragging
                }
            }
        }
    }

    private void EnableAllButtonsEventTriggers()
    {
        foreach (var handler in allDraggableTextHandlers)
        {
            EventTrigger eventTrigger = handler.GetComponent<EventTrigger>();
            if (eventTrigger != null)
            {
                eventTrigger.enabled = true; // Re-enable EventTrigger for all buttons
            }
        }
    }

    private IEnumerator HandleAudioAndRecording()
    {
        retryButton.gameObject.SetActive(true);
        retryButton.interactable = false;

        retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/DefaultSprite");

        yield return new WaitForSeconds(audioSource.clip.length);

        yield return StartCoroutine(StartRecording());

        float[] recordedSamples = AnalyzeRecordedAudio(audioSource2.clip);
        if (DetectVoicePresence(recordedSamples))
        {
            Debug.Log("Voice detected in recording.");
        }
        else
        {
            Debug.Log("No significant voice detected.");
        }

        yield return StartCoroutine(PlayRecordedAudio());

        retryButton.interactable = true;

        retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/RetrySprite");

        yield return new WaitForSeconds(3f);

        ScaleDownAndSpawnPrefab();
    }

    private IEnumerator StartRecording()
    {
        // Mute the music before recording
        SetMusicVolume(-80f);
        SetAmbientVolume(-80f);

        retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/RetrySprite");

        int recordingDuration = 5;
        int frequency = 44100;

        audioSource2.clip = Microphone.Start(null, false, recordingDuration, frequency);
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

        // Restore the music volume after recording
        
    }


    private IEnumerator PlayRecordedAudio()
    {
       
        retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/PlaybackSprite");

        audioSource2.Play();
        Debug.Log("Playing back recorded audio...");

        float playbackDuration = audioSource2.clip.length;
        float timeElapsed = 0f;

        while (timeElapsed < playbackDuration)
        {
            ringImage.fillAmount = 1 - (timeElapsed / playbackDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        ringImage.fillAmount = 0;

        Debug.Log("Playback completed.");

        yield return new WaitForSeconds(1f);

        retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/RetrySprite");

        // Restore the music volume after playback
        SetMusicVolume(-35f);
        SetAmbientVolume(-10f);
    }

    private float[] AnalyzeRecordedAudio(AudioClip recordedClip)
    {
        float[] samples = new float[recordedClip.samples];
        recordedClip.GetData(samples, 0);

        NormalizeAudio(samples);
        ApplyNoiseReduction(samples);
        ApplyBandpassFilter(samples, 80, 3000);

        return samples;
    }
    private void NormalizeAudio(float[] samples)
    {
        float maxAmplitude = Mathf.Max(samples);
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] /= maxAmplitude;
        }
    }

    private void ApplyNoiseReduction(float[] samples)
    {
        float noiseThreshold = 0.02f;
        for (int i = 0; i < samples.Length; i++)
        {
            if (Mathf.Abs(samples[i]) < noiseThreshold)
            {
                samples[i] = 0f;
            }
        }
    }

    private void ApplyBandpassFilter(float[] samples, float lowFreq, float highFreq)
    {
        float sampleRate = 44100f;
        float low = lowFreq / sampleRate;
        float high = highFreq / sampleRate;

        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] *= (high - low);
        }
    }

    private bool DetectVoicePresence(float[] samples)
    {
        foreach (float sample in samples)
        {
            if (Mathf.Abs(sample) > 0.01f)
            {
                return true;
            }
        }
        return false;
    }


    private void ScaleDownAndSpawnPrefab()
    {
        if (panel == null || prefabToSpawn == null)
        {
            Debug.LogWarning("Panel or Prefab to spawn is not assigned.");
            return;
        }

        LeanTween.scale(panel, Vector3.zero, 0.5f)
            .setEase(LeanTweenType.easeInBack)
            .setOnComplete(() =>
            {
                GameObject spawnedPrefab = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity);
                spawnedPrefab.transform.localScale = Vector3.zero;
                LeanTween.scale(spawnedPrefab, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);
                Debug.Log("Prefab Instantiated and tweened to scale (1, 1, 1)");
            });

        textComponent.text = "What do we play Next?";
        gameObject.SetActive(false);

        // Re-enable dragging for all buttons for the next playthrough
        EnableAllButtonsEventTriggers();
    }

    public void UpdateInitialPositionFromHandler()
    {
        if (beachBoxHandler != null)
        {
            initialPosition = beachBoxHandler.GetButtonPosition(gameObject.name);
            Debug.Log($"Updated initial position for {gameObject.name}: {initialPosition}");
        }
    }
}
