using UnityEngine;
using System.Collections;
using TMPro; // Ensure you have TMPro namespace included
using UnityEngine.UI; // Ensure you have UI namespace included

public class ST_AudioManager : MonoBehaviour
{
    public static ST_AudioManager Instance;
    public AudioSource audioSourceCard1;
    public AudioSource audioSourceCard2;
    public float recordLength = 5f;
    public float highPitchFactor = 1.5f;
    public GameObject ST_Canvas; // Assign the ST_Canvas in the Inspector
    private TextMeshProUGUI displayText;
    private Button retryButton;
    private AudioSource currentAudioSource;
    private string currentCardTag;
    private bool isRetrying = false;

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
            currentAudioSource = audioSourceCard1;
            StartCoroutine(PlayAudioAndRecord(currentAudioSource));
        }
        else if (cardTag == "Card_2")
        {
            currentAudioSource = audioSourceCard2;
            StartCoroutine(PlayAudioAndRecord(currentAudioSource));
        }
    }

    private IEnumerator PlayAudioAndRecord(AudioSource audioSource)
    {
        if (!isRetrying)
        {
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length);
        }

        displayText.text = "Listening ...!";
        AudioClip recordedClip = Microphone.Start(null, false, Mathf.CeilToInt(recordLength), 44100);
        yield return new WaitForSeconds(recordLength);
        Microphone.End(null);

        float[] samples = new float[Mathf.CeilToInt(44100 * recordLength)];
        recordedClip.GetData(samples, 0);

        bool detectedVoice = AnalyzeRecordedAudio(samples);

        if (detectedVoice)
        {
            displayText.text = "Did You Say?";
            PlayRecordingWithFunnyVoice(recordedClip);
        }
        else
        {
            displayText.text = "No sound detected.";
        }
    }

    private bool AnalyzeRecordedAudio(float[] samples)
    {
        foreach (float sample in samples)
        {
            if (Mathf.Abs(sample) > 0.01f) // Silence threshold
            {
                return true;
            }
        }
        return false;
    }

    private void PlayRecordingWithFunnyVoice(AudioClip recordedClip)
    {
        currentAudioSource.pitch = highPitchFactor;
        currentAudioSource.clip = recordedClip;
        currentAudioSource.Play();
    }

    private void OnRetryButtonClick()
    {
        isRetrying = true;
        StopAllCoroutines();
        PlayAudioAfterDestroy(currentCardTag);
        isRetrying = false;
    }
}
