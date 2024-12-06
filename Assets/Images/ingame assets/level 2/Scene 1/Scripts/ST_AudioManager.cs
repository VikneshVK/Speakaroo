using UnityEngine;
using System;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;

public class ST_AudioManager : MonoBehaviour
{
    public static ST_AudioManager Instance;
    public AudioSource audioSourceCard1;
    public AudioSource audioSourceCard2;
    public AudioSource recordedAudioSource;
    public string scratchAudioClipPath = "Audio/ScratchAudio";
    public string revealAudioClipPath = "Audio/RevealAudio";
    public float recordLength = 5f;
    public float highPitchFactor = 1.2f;
    public GameObject ST_Canvas;
    private TextMeshProUGUI displayText;
    private Button retryButton;
    public string currentCardTag;
    private RetryButton retryButtonScript;  // Reference to the RetryButton script.
   

    public event Action OnRecordingStart;
    public event Action<int> OnRecordingComplete;
    public event Action OnCard1PlaybackComplete;
    public event Action OnCard2Interaction;
    public event Action OnPlaybackComplete;
    public event Action OnPlaybackStart;

    // New events
    public event Action OnRecordingPlaybackStart;  // Triggered before the recorded audio plays
    public event Action OnRecordingPlaybackEnd;    // Triggered after the recorded audio finishes playing

    // Added the OnRetryClicked event
    public event Action OnRetryClicked;

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
        displayText = ST_Canvas.transform.Find("DisplayText").GetComponent<TextMeshProUGUI>();
        retryButton = ST_Canvas.transform.Find("RetryButton").GetComponent<Button>();
        retryButton.onClick.AddListener(OnRetryButtonClick);

        // Set initial feedback text
        displayText.text = "Scratch the Cards to Reveal the Word";
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
        recordedAudioSource.clip = recordedClip;
        recordedAudioSource.pitch = highPitchFactor;
        recordedAudioSource.Play();
    }

    private IEnumerator RecordAndAnalyzeAudio(int cardNumber)
    {
        
        AudioClip recordedClip = Microphone.Start(null, false, Mathf.CeilToInt(recordLength), 44100);
        yield return StartCoroutine(WaitForSecondsRealtime(recordLength));
        Microphone.End(null);
        

        // Assign the recorded clip to the recordedAudioSource
        recordedAudioSource.clip = recordedClip;

        // Analyze the recorded audio
        float[] samples = new float[Mathf.CeilToInt(44100 * recordLength)];
        recordedClip.GetData(samples, 0);
        bool detectedVoice = AnalyzeRecordedAudio(samples);

        if (detectedVoice)
        {
            displayText.text = "Did You Say..?";

            // Trigger OnRecordingPlaybackStart event
            OnRecordingPlaybackStart?.Invoke();

            PlayRecordedClipWithFunnyVoice(recordedClip);
            yield return StartCoroutine(WaitForSecondsRealtime(recordedClip.length));

            // Trigger OnRecordingPlaybackEnd event
            OnRecordingPlaybackEnd?.Invoke();
        }
        else
        {
            displayText.text = "No sound detected. Please Retry";
        }

        // Update feedback text after recording and playback are complete
        if (cardNumber == 1)
        {
            displayText.text = "Good Job..! Scratch the cards to reveal word";
            OnCard1PlaybackComplete?.Invoke();
        }
        else if (cardNumber == 2)
        {
            displayText.text = "Good Job..!";
            OnPlaybackComplete?.Invoke();
        }

        // Trigger OnRecordingComplete event
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

    private bool AnalyzeRecordedAudio(float[] samples)
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

    private void OnRetryButtonClick()
    {
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
