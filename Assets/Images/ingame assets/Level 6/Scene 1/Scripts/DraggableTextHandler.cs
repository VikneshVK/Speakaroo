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
    private Image buttonImage;
    private int retryButtonCount = 0;
    private bool retryButtonClicked = false;
    public Canvas canvas; // Reference to the canvas
    public TextMeshProUGUI tmpText; // Reference to the TMP component
    public TextMeshProUGUI textComponent;
    
    public Image imageComponent; // Reference to the Image component to change sprite
    public string spriteName;

    private Vector3 initialPosition;
    private bool isDropped = false;
    private bool isBeingDragged = false;
    private static List<DraggableTextHandler> allDraggableTextHandlers = new List<DraggableTextHandler>(); // Static list of all instances

    /*private string recordedClipName = "RecordedAudio";*/
    private bool isButtonClicked = false;
    public BeachBoxHandler beachBoxHandler;
    private static DraggableTextHandler lastClickedHandler;

    private AudioSource SfxAudioSource;
    private AudioClip SfxAudio1;
    private AudioClip SfxAudio2;

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
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
        SfxAudio1 = Resources.Load<AudioClip>("Audio/sfx/Start Record");
        SfxAudio2 = Resources.Load<AudioClip>("Audio/sfx/Start Playback");
        if (ringImage != null) ringImage.fillAmount = 0;
        buttonImage = retryButton.GetComponent<Image>();
        SetAlpha(buttonImage, 20);
        SetAlpha(ringImage, 20);
    }
    

    private void UpdateInitialPosition()
    {
        if (beachBoxHandler != null)
        {
            initialPosition = beachBoxHandler.GetButtonPosition(gameObject.name);
            
        }
        else
        {
            initialPosition = transform.position; // Fallback to current position
            
        }
    }


    public void OnButtonClicked()
    {
        if (isButtonClicked)
            return;

        isButtonClicked = true;
        lastClickedHandler = this;
        retryButton.onClick.RemoveAllListeners();
        retryButton.onClick.AddListener(() => OnRetryButtonClick(this));

        // Deactivate other game objects
        foreach (var handler in allDraggableTextHandlers)
        {
            handler.GetComponent<Button>().interactable = false;
            if (handler != this)
            {                
                handler.gameObject.SetActive(false);
            }
        }

        // Tween the clicked object to the center of the viewport
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 centerPosition = canvas.GetComponent<RectTransform>().rect.center;

        LeanTween.move(rectTransform, centerPosition, 0.5f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                // Start the audio and recording coroutine once the tween is complete
                StartCoroutine(HandleAudioAndRecording());
            });

        // Update text based on the object name
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

        retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/speak-2");

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
        SfxAudioSource.PlayOneShot(SfxAudio2);
        yield return StartCoroutine(PlayRecordedAudio());

        retryButton.interactable = true;
        retryButtonClicked = false;

        retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/speak-1");

        float retryDuration = 5f;
        float elapsedTime = 0f;

        while (elapsedTime < retryDuration)
        {
            if (retryButtonClicked)
            {
                yield break; // Exit if the retry button was clicked
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ScaleDownAndSpawnPrefab();
    }

    public void OnRetryButtonClick(DraggableTextHandler handler)
    {
        if (lastClickedHandler != null)
        {
            if (retryButtonCount < 1)
            {
                retryButtonCount++;
                retryButtonClicked = true;
                lastClickedHandler.isButtonClicked = false; // Reset the flag
                lastClickedHandler.OnButtonClicked(); // Call OnButtonClicked on the last clicked handler
            }
            else
            {
                retryButton.interactable = false;
                lastClickedHandler.ScaleDownAndSpawnPrefab(); // Or any final action
            }
        }
        else
        {
            Debug.LogError("No last clicked handler found.");
        }
    }



    private IEnumerator StartRecording()
    {
        // Mute the music before recording
        SetMusicVolume(-80f);
        SetAmbientVolume(-80f);
        SetAlpha(buttonImage, 255);
        SetAlpha(ringImage, 255);
        retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/speak");

        int recordingDuration = 5;
        int frequency = 44100;
        SfxAudioSource.PlayOneShot(SfxAudio1);
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
       
        retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/speak-2");

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
        SetMusicVolume(-25f);
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
                // Check if prefab is spawned correctly
                GameObject spawnedPrefab;
                if (gameObject.name == "Beach Ball" || gameObject.name == "Frisbee")
                {
                    GameObject stCanvas = GameObject.Find("ST_Canvas");

                    if (stCanvas != null)
                    {
                        spawnedPrefab = Instantiate(prefabToSpawn, stCanvas.transform);
                    }
                    else
                    {
                        Debug.LogWarning("ST_Canvas not found in the scene. Prefab cannot be parented to it.");
                        return;
                    }
                }
                else
                {
                    spawnedPrefab = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity);
                }

                spawnedPrefab.transform.localScale = Vector3.zero;
                LeanTween.scale(spawnedPrefab, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);

                // Log and disable all button interactability
                foreach (var handler in allDraggableTextHandlers)
                {
                    Button button = handler.GetComponent<Button>();
                    if (button != null)
                    {
                        Debug.Log($"Disabling interactability for button: {handler.gameObject.name}");
                        button.interactable = false;
                    }
                    else
                    {
                        Debug.LogWarning($"Button component not found on {handler.gameObject.name}");
                    }
                }
            });

        textComponent.text = "What do we play next?";
        retryButtonCount = 0;
        SetAlpha(buttonImage, 20);
        SetAlpha(ringImage, 20);
        gameObject.SetActive(false);
        retryButton.interactable = false;

        // Re-enable dragging for the next playthrough
        EnableAllButtonsEventTriggers();
    }

    private void SetAlpha(Image image, float alphaValue)
    {
        Color color = image.color;
        color.a = alphaValue / 255f;
        image.color = color;
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
