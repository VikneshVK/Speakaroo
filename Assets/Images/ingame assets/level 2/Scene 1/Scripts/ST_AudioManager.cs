using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System;

public class ST_AudioManager : MonoBehaviour
{
    public static ST_AudioManager Instance;
    public AudioSource audioSourceCard1;
    public AudioSource audioSourceCard2;
    public AudioSource recordedAudioSource;
    public string scratchAudioClipPath = "Audio/ScratchAudio";
    public string revealAudioClipPath = "Audio/RevealAudio";
    public float recordLength = 5f;
    public float highPitchFactor = 1.5f;
    public GameObject ST_Canvas;
    private TextMeshProUGUI displayText;
    private Button retryButton;
    public string currentCardTag;
    private AudioSource audioSource;
    private AudioClip scratchAudioClip;
    private AudioClip revealAudioClip;

    public event Action OnRecordingStart;
    public event Action<int> OnRecordingComplete;
    public event Action OnCard1PlaybackComplete;
    public event Action OnCard2Interaction;
    public event Action OnPlaybackComplete;
    public event Action OnRetryClicked;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = gameObject.AddComponent<AudioSource>();

            scratchAudioClip = Resources.Load<AudioClip>(scratchAudioClipPath);
            revealAudioClip = Resources.Load<AudioClip>(revealAudioClipPath);
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
        if (scratchAudioClip != null)
        {
            audioSource.clip = scratchAudioClip;
            audioSource.Play();
        }
    }

    public void PlayRevealAudio(string cardTag)
    {
        if (revealAudioClip != null)
        {
            StartCoroutine(PlayRevealAudioWithDelay(cardTag));
        }
    }

    private IEnumerator PlayRevealAudioWithDelay(string cardTag)
    {
        audioSource.clip = revealAudioClip;
        audioSource.Play();
        yield return new WaitForSeconds(2f); // Wait for 2 seconds

        // Update feedback text after the card is destroyed
        displayText.text = "Nice..!";

        // After reveal audio, play the original card audio
        PlayAudioAfterDestroy(cardTag);
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
        originalAudioSource.Play();
        yield return StartCoroutine(WaitForSecondsRealtime(originalAudioSource.clip.length));

        // Trigger the OnRecordingStart event and update feedback text
        OnRecordingStart?.Invoke();

        displayText.text = "Listening ...!";
        StartCoroutine(RecordAndAnalyzeAudio(cardNumber));
    }

    private IEnumerator RecordAndAnalyzeAudio(int cardNumber)
    {
        AudioClip recordedClip = Microphone.Start(null, false, Mathf.CeilToInt(recordLength), 44100);
        yield return StartCoroutine(WaitForSecondsRealtime(recordLength));
        Microphone.End(null);

        float[] samples = new float[Mathf.CeilToInt(44100 * recordLength)];
        recordedClip.GetData(samples, 0);

        bool detectedVoice = AnalyzeRecordedAudio(samples);

        if (detectedVoice)
        {
            displayText.text = "Did You Say..?";
            PlayRecordedClipWithFunnyVoice(recordedClip);
            yield return StartCoroutine(WaitForSecondsRealtime(recordedClip.length));
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

    private void PlayRecordedClipWithFunnyVoice(AudioClip recordedClip)
    {
        recordedAudioSource.pitch = highPitchFactor;
        recordedAudioSource.clip = recordedClip;
        recordedAudioSource.Play();
    }

    private void OnRetryButtonClick()
    {
        OnRetryClicked?.Invoke();
        StopAllCoroutines();
        PlayAudioAfterDestroy(currentCardTag);
    }

    public void TriggerRecordingStart()
    {
        OnRecordingStart?.Invoke();
    }
}
