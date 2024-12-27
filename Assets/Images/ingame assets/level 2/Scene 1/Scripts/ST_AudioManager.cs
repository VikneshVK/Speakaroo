using UnityEngine;
using System;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;

public class ST_AudioManager : MonoBehaviour
{
    public static ST_AudioManager Instance;
    public AudioMixer audioMixer;
    public AudioSource audioSourceCard1;
    public AudioSource audioSourceCard2;
    public AudioSource recordedAudioSource;
    public string scratchAudioClipPath = "Audio/ScratchAudio";
    public string revealAudioClipPath = "Audio/RevealAudio";
    public float recordLength = 5f;
    public float highPitchFactor = 1f;
    private GameObject ST_Canvas;
    private TextMeshProUGUI displayText;
    private Button retryButton;
    public string currentCardTag;
    private RetryButton retryButtonScript;  // Reference to the RetryButton script.
    
    private Image Jojo;
    private Image Kiki;
    private Animator JojoAnimator;
    private Animator KikiAnimator;
    private const string musicVolumeParam = "MusicVolume";


    public event Action OnRecordingStart;
    public event Action<int> OnRecordingComplete;
    public event Action OnCard1PlaybackComplete;
    public event Action OnCard2Interaction;
    public event Action OnPlaybackComplete;
    public event Action OnPlaybackStart;

    // New events
    public event Action OnRecordingPlaybackStart;  // Triggered before the recorded audio plays
    public event Action<int> OnRecordingPlaybackEnd;    // Triggered after the recorded audio finishes playing

    // Added the OnRetryClicked event
    public event Action OnRetryClicked;

    public bool isCard1PlaybackComplete = false; // Tracks playback completion for Card 1
    public bool isCard2PlaybackComplete = false; // Tracks playback completion for Card 2

    private AudioSource SfxAudioSource;
    private AudioClip SfxAudio1;
    private AudioClip SfxAudio2;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // Find the RetryButton script on the STMechanics GameObject
            retryButtonScript = FindObjectOfType<RetryButton>();

            if (retryButtonScript == null)
            {
                Debug.LogError("RetryButton script not found on the STMechanics GameObject.");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        
        ST_Canvas = GameObject.FindGameObjectWithTag("STCanvas");
        displayText = ST_Canvas.transform.Find("UI_Fitter/DisplayText").gameObject.GetComponent<TextMeshProUGUI>();
        retryButton = ST_Canvas.transform.Find("UI_Fitter/RetryButton").gameObject.GetComponent<Button>();
        Jojo = ST_Canvas.transform.Find("UI_Fitter/char holder/Jojo").gameObject.GetComponent<Image>();
        Kiki = ST_Canvas.transform.Find("UI_Fitter/char holder/Kiki").gameObject.GetComponent<Image>();
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
        SfxAudio1 = Resources.Load<AudioClip>("Audio/sfx/Start Record");
        SfxAudio2 = Resources.Load<AudioClip>("Audio/sfx/Start Playback");
        JojoAnimator = Jojo.GetComponent<Animator>();
        KikiAnimator = Kiki.GetComponent<Animator>();

        retryButton.onClick.AddListener(OnRetryButtonClick);
        CheckMicrophonePermissions();
        InitializeAudioComponents();
        // Set initial feedback text
        displayText.text = "Scratch the Cards to Reveal the Word";
    }

    public void TriggerRecordingStart()
    {
        // Reduce the volume of the "Music" group when recording starts
        SetMusicVolume(-80f); // Set to minimum volume (mute)
        OnRecordingStart?.Invoke();
    }
    void CheckMicrophonePermissions()
    {
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            StartCoroutine(RequestMicrophonePermission());
        }
        else
        {
            Debug.Log("Microphone permission granted.");
        }
    }

    IEnumerator RequestMicrophonePermission()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);

        if (Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            Debug.Log("Microphone permission granted.");
        }
        else
        {
            Debug.LogError("Microphone permission denied.");
        }
    }

    private void InitializeAudioComponents()
    {
        // Set up audio components and ensure microphone devices are detected
        if (Microphone.devices.Length > 0)
        {
            Debug.Log("Available Microphones: " + string.Join(", ", Microphone.devices));
        }
        else
        {
            Debug.LogError("No microphones detected.");
        }
    }

    public void TriggerRecordingStop()
    {
        // Restore the volume of the "Music" group when recording stops
        SetMusicVolume(0f); // Set back to the default volume
    }

    private void SetMusicVolume(float volume)
    {
        if (audioMixer != null)
        {
            bool result = audioMixer.SetFloat(musicVolumeParam, volume); // Use "Volume"
            Debug.Log($"SetFloat executed: {result}, Volume: {volume}");
        }
        else
        {
            Debug.LogError("AudioMixer is not assigned in the Inspector.");
        }
    }

   
    public void PlayScratchAudio()
    {
        if (retryButtonScript != null)
        {
            retryButtonScript.PlayScratchAudio();
        }
        else
        {
            Debug.LogError("RetryButton script is not assigned.");
        }
    }

    public void PlayRevealAudio(string cardTag)
    {
        if (retryButtonScript != null)
        {
            retryButtonScript.PlayRevealAudioWithCardAudio(cardTag);
        }
        else
        {
            Debug.LogError("RetryButton script is not assigned.");
        }
    }

    public void PlayRecordedClipWithFunnyVoice(AudioClip recordedClip)
    {
        KikiAnimator.SetBool("StartHearing", true);
        recordedAudioSource.clip = recordedClip; // Already set, but this ensures consistency
        recordedAudioSource.pitch = highPitchFactor;
        recordedAudioSource.Play();
    }

    private IEnumerator RecordAndAnalyzeAudio(int cardNumber)
    {
        int minFreq, maxFreq;
        Microphone.GetDeviceCaps(null, out minFreq, out maxFreq); // Get supported frequencies
        int chosenFreq = (maxFreq == 0 || maxFreq < 44100) ? Mathf.Clamp(44100, minFreq, maxFreq) : 44100;

        // Start recording
        JojoAnimator.SetBool("StartRecording",true);
        SfxAudioSource.PlayOneShot(SfxAudio1);
        AudioClip recordedClip = Microphone.Start(null, false, Mathf.CeilToInt(recordLength), chosenFreq);
        yield return StartCoroutine(WaitForSecondsRealtime(recordLength));
        Microphone.End(null);
       /* TriggerRecordingStop();*/

        // Process the recorded audio
        float[] samples = new float[Mathf.CeilToInt(chosenFreq * recordLength)];
        recordedClip.GetData(samples, 0);

        // Analyze and process audio
        float[] processedSamples = AnalyzeRecordedAudio(samples);

        // Create processed audio clip
        AudioClip processedClip = AudioClip.Create("ProcessedAudio", processedSamples.Length, 1, chosenFreq, false);
        processedClip.SetData(processedSamples, 0);

        // Assign to the recordedAudioSource
        recordedAudioSource.clip = processedClip;

        // Voice detection and playback logic
        if (DetectVoicePresence(processedSamples))
        {
            displayText.text = "Did You Say..?";
            OnRecordingPlaybackStart?.Invoke();
            JojoAnimator.SetBool("StartRecording", false);
            SfxAudioSource.PlayOneShot(SfxAudio2);
            PlayRecordedClipWithFunnyVoice(processedClip); // Play processed clip
            yield return StartCoroutine(WaitForSecondsRealtime(processedClip.length));
            KikiAnimator.SetBool("StartHearing", false);
            OnRecordingPlaybackEnd?.Invoke(cardNumber);
        }
        else
        {
            displayText.text = "No sound detected. Please Retry";
        }

        // Post-recording feedback
        if (cardNumber == 1)
        {
            displayText.text = "Good Job..! Scratch the cards to reveal word";
            OnCard1PlaybackComplete?.Invoke();
            isCard1PlaybackComplete = true;
        }
        else if (cardNumber == 2)
        {
            displayText.text = "Good Job..! Scratch the cards to reveal word";
            OnPlaybackComplete?.Invoke();
            isCard2PlaybackComplete = true;
        }

        OnRecordingComplete?.Invoke(cardNumber);
    }




    private IEnumerator WaitForSecondsRealtime(float time)
    {
        float counter = 0;
        while (counter < time)
        {
            counter += Time.deltaTime;
            yield return null;
        }
    }

    private float[] AnalyzeRecordedAudio(float[] samples)
    {
        NormalizeAudio(samples);
        Debug.Log($"Raw Sample [0]: {samples[0]}");
        ApplyNoiseReduction(samples);
        Debug.Log($"Processed Sample [0]: {samples[0]}");
        ApplyBandpassFilter(samples, 80, 3000);

        return samples; // Return the processed samples
    }

    private bool DetectVoicePresence(float[] samples)
    {
        foreach (float sample in samples)
        {
            if (Mathf.Abs(sample) > 0.01f) // Example threshold for vocal detection
            {
                return true;
            }
        }
        return false;
    }

    private void NormalizeAudio(float[] samples)
    {
        float maxAmplitude = 0f;
        // Find the maximum amplitude
        foreach (float sample in samples)
        {
            if (Mathf.Abs(sample) > maxAmplitude)
                maxAmplitude = Mathf.Abs(sample);
        }

        // Normalize if necessary
        if (maxAmplitude > 0f)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] /= maxAmplitude; // Scale all samples down by the max amplitude
            }
        }
    }

    private void ApplyNoiseReduction(float[] samples)
    {
        // Adjust noise threshold dynamically for Android
        float noiseThreshold = Application.platform == RuntimePlatform.Android ? 0.05f : 0.02f;

        for (int i = 0; i < samples.Length; i++)
        {
            // Suppress low-level background noise
            if (Mathf.Abs(samples[i]) < noiseThreshold)
            {
                samples[i] = 0f; // Set very low values to zero
            }
        }
    }


    private void ApplyBandpassFilter(float[] samples, float lowFreq, float highFreq)
    {
        // Example implementation using a simple bandpass algorithm
        // Note: For high-quality filtering, consider using DSP libraries (e.g., SoX, Unity's audio filters)
        float sampleRate = 44100f; // Assuming 44.1 kHz sample rate
        float low = lowFreq / sampleRate;
        float high = highFreq / sampleRate;

        for (int i = 1; i < samples.Length - 1; i++)
        {
            // Simple bandpass filter logic
            samples[i] = samples[i] * (high - low);
        }
    }

    private void OnRetryButtonClick()
    {
        Debug.Log("Retry Button Clicked: Invoking OnRetryClicked event");
        isCard1PlaybackComplete = false;
        isCard2PlaybackComplete = false;
        OnRetryClicked?.Invoke();  // Trigger the OnRetryClicked event
        StopAllCoroutines();
        PlayAudioAfterDestroy(currentCardTag);
    }

    /*public void TriggerRecordingStart()
    {
        OnRecordingStart?.Invoke();
    }*/

    // Method to trigger the OnPlaybackComplete event
    public void TriggerOnPlaybackComplete()
    {
        OnPlaybackComplete?.Invoke();
    }

    public void PlayAudioAfterDestroy(string cardTag)
    {
        currentCardTag = cardTag;

        // Determine which card was destroyed and set feedback text
        if (cardTag == "Card_1")
        {
            displayText.text = "Try Saying the Word";
            StartCoroutine(PlayOriginalClipAndRecord(audioSourceCard1, 1));
        }
        else if (cardTag == "Card_2")
        {
            displayText.text = "Try Saying the Word";
            OnCard2Interaction?.Invoke();
            StartCoroutine(PlayOriginalClipAndRecord(audioSourceCard2, 2));
        }
    }

    private IEnumerator PlayOriginalClipAndRecord(AudioSource originalAudioSource, int cardNumber)
    {
        OnPlaybackStart?.Invoke();

        originalAudioSource.Play();
        yield return StartCoroutine(WaitForSecondsRealtime(originalAudioSource.clip.length));

        // Trigger the OnRecordingStart event and update feedback text
        OnRecordingStart?.Invoke();

        displayText.text = "Listening ...!";
        StartCoroutine(RecordAndAnalyzeAudio(cardNumber));
    }
}