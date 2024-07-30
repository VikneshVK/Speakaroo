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
    public float recordLength = 5f;
    public float highPitchFactor = 1.5f;
    public GameObject ST_Canvas;
    private TextMeshProUGUI displayText;
    private Button retryButton;
    private string currentCardTag;
    /*private bool isRetrying = false;*/

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
    }

    public void PlayAudioAfterDestroy(string cardTag)
    {
        currentCardTag = cardTag;
        displayText.text = "Let's Try Saying the Word";

        if (cardTag == "Card_1")
        {
            StartCoroutine(PlayOriginalClipAndRecord(audioSourceCard1, 1));
        }
        else if (cardTag == "Card_2")
        {
            OnCard2Interaction?.Invoke();
            StartCoroutine(PlayOriginalClipAndRecord(audioSourceCard2, 2));
        }
    }

    private IEnumerator PlayOriginalClipAndRecord(AudioSource originalAudioSource, int cardNumber)
    {
        originalAudioSource.Play();
        yield return StartCoroutine(WaitForSecondsRealtime(originalAudioSource.clip.length));

        if (cardNumber == 1)
        {
            displayText.text = "Listening ...!";
            StartCoroutine(RecordAndAnalyzeAudio(cardNumber));
        }
        else if (cardNumber == 2)
        {
            displayText.text = "Listening ...!";
            StartCoroutine(RecordAndAnalyzeAudio(cardNumber));
        }
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
            displayText.text = "Did You Say?";
            PlayRecordedClipWithFunnyVoice(recordedClip);
            yield return StartCoroutine(WaitForSecondsRealtime(recordedClip.length));
        }
        else
        {
            displayText.text = "No sound detected.";
        }

        if (cardNumber == 1)
        {
            OnCard1PlaybackComplete?.Invoke();
        }
        else if (cardNumber == 2)
        {
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
}