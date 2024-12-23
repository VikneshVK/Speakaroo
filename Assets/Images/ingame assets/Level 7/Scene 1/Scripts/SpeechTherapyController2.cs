using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SpeechTherapyController2 : MonoBehaviour
{
    [Header("Text References")]
    public TextMeshProUGUI text1; // Reference to Text 1 component
    public TextMeshProUGUI text2; // Reference to Text 2 component

    [Header("Text References")]
    public Button Button1;
    public Button Button2;
    public Button Button3;


    [Header("Audio References")]
    public AudioClip audioClip1; // AudioClip to play when button1Click is called
    public AudioClip audioClip2;
    public AudioClip audioClip3;
    public AudioSource RecordedAudioSource;
    private AudioSource audioSource; // AudioSource on the GameObject

    [Header("Retrybutton")]
    public Button retryButton;
    private Image buttonImage;
    public Image ringImage;
    public Sprite Mic;
    public Sprite Speaker;
    public Sprite Retrysprite;
    private bool retryButtonWasClicked = false;

    private int retryCount;

    [Header("jojoController")]
    public Lvl7Sc1JojoController jojoController;

    [Header("SFX")]
    public AudioSource SfxAudioSource;
    private AudioClip SfxAudio1;
    private AudioClip SfxAudio2;

    private enum ButtonType { None, Button1, Button2, Button3 }
    private ButtonType retryButtonActivatedBy = ButtonType.None;


    private AudioClip recordedClip; // Holds the recorded audio

    private void Start()
    {
        retryCount = 0;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        buttonImage = retryButton.GetComponent<Image>();
        SfxAudio1 = Resources.Load<AudioClip>("Audio/sfx/Start Record");
        SfxAudio2 = Resources.Load<AudioClip>("Audio/sfx/Start Playback");
    }


    public void button1Click()
    {
        setButtonnotInteractable();
        retryButtonActivatedBy = ButtonType.Button1;
        StartCoroutine(HandleButton1Click());
    }

    private IEnumerator HandleButton1Click()
    {
        setButtonnotInteractable();
        text1.text = "I want to Eat";
        text2.text = "Grape Icecream";

        retryButton.interactable = false;
        retryButton.image.sprite = Speaker;

        if (audioClip1 != null)
        {
            audioSource.clip = audioClip1;
            audioSource.Play();
            Debug.Log("Playing audio clip...");

            yield return new WaitWhile(() => audioSource.isPlaying);
        }
        else
        {
            Debug.LogWarning("AudioClip1 is not assigned.");
        }

        retryButton.image.sprite = Mic;

        yield return StartCoroutine(RecordAudio(5)); // 5 seconds recording

        float[] samples = new float[recordedClip.samples];
        recordedClip.GetData(samples, 0);

        NormalizeAudio(samples);
        ApplyNoiseReduction(samples);
        ApplyBandpassFilter(samples, 100f, 3000f); // Bandpass filter between 100Hz and 3000Hz

        recordedClip.SetData(samples, 0);
        retryButton.image.sprite = Speaker;

        RecordedAudioSource.clip = recordedClip;
        SfxAudioSource.PlayOneShot(SfxAudio2);
        RecordedAudioSource.Play();
        Debug.Log("Playing the processed recorded audio...");

        float recordedAudioDuration = RecordedAudioSource.clip.length;
        float startTime = Time.time;
        while (Time.time - startTime < recordedAudioDuration)
        {
            ringImage.fillAmount = 1 - ((Time.time - startTime) / recordedAudioDuration); // Decrease fill proportionally to audio time
            yield return null;
        }

        retryButton.image.sprite = Retrysprite; // Change sprite to retrySprite        
        ringImage.fillAmount = 1f;
        StartCoroutine(WaitForRetryButtonClick());
    }

    private IEnumerator WaitForRetryButtonClick()
    {
        jojoController.audioPlaying = false;
        if (retryCount == 0)
        {
            retryCount++;

            retryButton.interactable = true;
            yield return new WaitForSeconds(3f);
            retryButton.interactable = false;

            // Check if the retry button was clicked before setting panel3Complete to true
            if (!retryButton.interactable && !retryButtonWasClicked)
            {
                jojoController.panel3Complete = true;
                UpdatePrefabToSpawn(); // Call the new method
            }
            retryButtonActivatedBy = ButtonType.None;
            retryButtonWasClicked = false; // Reset the flag
        }
        else
        {
            retryButton.interactable = false;
            // Check if the retry button was clicked before setting panel3Complete to true
            if (!retryButtonWasClicked)
            {
                jojoController.panel3Complete = true;
                UpdatePrefabToSpawn(); // Call the new method
            }
            retryButtonWasClicked = false; // Reset the flag
        }
    }

    private IEnumerator RecordAudio(int duration)
    {
        SetAlpha(buttonImage, 255);
        SetAlpha(ringImage, 255);
        Debug.Log("Recording audio...");
        SfxAudioSource.PlayOneShot(SfxAudio1);
        float recordingTime = duration;

        recordedClip = Microphone.Start(null, false, duration, 44100);
        while (recordingTime > 0f)
        {
            ringImage.fillAmount = recordingTime / 5f; // Update the fill based on remaining time
            recordingTime -= Time.deltaTime; // Decrease recording time
            yield return null;
        }
        Microphone.End(null);
        Debug.Log("Recording completed.");
    }


    private void NormalizeAudio(float[] samples)
    {
        Debug.Log("Normalizing audio...");
        float maxAmplitude = 0f;
        foreach (float sample in samples)
        {
            if (Mathf.Abs(sample) > maxAmplitude)
                maxAmplitude = Mathf.Abs(sample);
        }

        if (maxAmplitude > 0f)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] /= maxAmplitude;
            }
        }
        Debug.Log("Audio normalization completed.");
    }

    private void ApplyNoiseReduction(float[] samples)
    {
        Debug.Log("Applying noise reduction...");

        float noiseThreshold = Application.platform == RuntimePlatform.Android ? 0.05f : 0.02f;

        for (int i = 0; i < samples.Length; i++)
        {
            // Suppress low-level background noise
            if (Mathf.Abs(samples[i]) < noiseThreshold)
            {
                samples[i] = 0f; // Set very low values to zero
            }
        }
        Debug.Log("Noise reduction completed.");
    }

    private void ApplyBandpassFilter(float[] samples, float lowFreq, float highFreq)
    {
        Debug.Log("Applying bandpass filter...");

        float sampleRate = 44100f;
        float low = lowFreq / sampleRate;
        float high = highFreq / sampleRate;

        for (int i = 1; i < samples.Length - 1; i++)
        {
            samples[i] = samples[i] * (high - low);
        }
        Debug.Log("Bandpass filter applied.");
    }

    public void button2Click()
    {
        setButtonnotInteractable();
        retryButtonActivatedBy = ButtonType.Button2;
        StartCoroutine(HandleButton2Click());
    }
    private IEnumerator HandleButton2Click()
    {

        text1.text = "I want to Eat";
        text2.text = "BlueBerry Icecream";
        setButtonnotInteractable();
        retryButton.interactable = false;
        retryButton.image.sprite = Speaker;

        if (audioClip1 != null)
        {
            audioSource.clip = audioClip2;
            audioSource.Play();
            Debug.Log("Playing audio clip...");

            yield return new WaitWhile(() => audioSource.isPlaying);
        }
        else
        {
            Debug.LogWarning("AudioClip2 is not assigned.");
        }

        retryButton.image.sprite = Mic;
        yield return StartCoroutine(RecordAudio(5)); // 5 seconds recording


        float[] samples = new float[recordedClip.samples];
        recordedClip.GetData(samples, 0);

        NormalizeAudio(samples);
        ApplyNoiseReduction(samples);
        ApplyBandpassFilter(samples, 100f, 3000f); // Bandpass filter between 100Hz and 3000Hz

        recordedClip.SetData(samples, 0);
        retryButton.image.sprite = Speaker;
        RecordedAudioSource.clip = recordedClip;
        RecordedAudioSource.Play();
        Debug.Log("Playing the processed recorded audio...");

        float recordedAudioDuration = RecordedAudioSource.clip.length;
        float startTime = Time.time;
        while (Time.time - startTime < recordedAudioDuration)
        {
            ringImage.fillAmount = 1 - ((Time.time - startTime) / recordedAudioDuration); // Decrease fill proportionally to audio time
            yield return null;
        }

        retryButton.image.sprite = Retrysprite; // Change sprite to retrySprite
        StartCoroutine(WaitForRetryButtonClick());
        ringImage.fillAmount = 1f;
    }

    public void button3Click()
    {
        setButtonnotInteractable();
        retryButtonActivatedBy = ButtonType.Button3;
        StartCoroutine(HandleButton3Click());
    }

    private IEnumerator HandleButton3Click()
    {
        text1.text = "I want to Eat";
        text2.text = "Mango Icecream";
        setButtonnotInteractable();
        retryButton.interactable = false;
        retryButton.image.sprite = Speaker;

        if (audioClip1 != null)
        {
            audioSource.clip = audioClip3;
            audioSource.Play();
            Debug.Log("Playing audio clip...");

            yield return new WaitWhile(() => audioSource.isPlaying);
        }
        else
        {
            Debug.LogWarning("AudioClip3 is not assigned.");
        }

        retryButton.image.sprite = Mic;
        yield return StartCoroutine(RecordAudio(5)); // 5 seconds recording

        float[] samples = new float[recordedClip.samples];
        recordedClip.GetData(samples, 0);

        NormalizeAudio(samples);
        ApplyNoiseReduction(samples);
        ApplyBandpassFilter(samples, 100f, 3000f); // Bandpass filter between 100Hz and 3000Hz

        recordedClip.SetData(samples, 0);
        retryButton.image.sprite = Speaker;
        RecordedAudioSource.clip = recordedClip;
        RecordedAudioSource.Play();
        Debug.Log("Playing the processed recorded audio...");
        float recordedAudioDuration = RecordedAudioSource.clip.length;
        float startTime = Time.time;
        while (Time.time - startTime < recordedAudioDuration)
        {
            ringImage.fillAmount = 1 - ((Time.time - startTime) / recordedAudioDuration); // Decrease fill proportionally to audio time
            yield return null;
        }

        retryButton.image.sprite = Retrysprite; // Change sprite to retrySprite
        StartCoroutine(WaitForRetryButtonClick());
        ringImage.fillAmount = 1f;
    }


    public void setButtonnotInteractable()
    {
        Button1.interactable = false;
        Button2.interactable = false;
        Button3.interactable = false;
    }

    private void UpdatePrefabToSpawn()
    {
        switch (retryButtonActivatedBy)
        {
            case ButtonType.Button1:
                jojoController.PrefabToSpawn = 7;
                break;
            case ButtonType.Button2:
                jojoController.PrefabToSpawn = 8;
                break;
            case ButtonType.Button3:
                jojoController.PrefabToSpawn = 9;
                break;
            default:
                Debug.LogWarning("Unknown button type. PrefabToSpawn not updated.");
                break;
        }

        SetAlpha(buttonImage, 20);
        SetAlpha(ringImage, 20);
        Debug.Log($"PrefabToSpawn updated to {jojoController.PrefabToSpawn}");
    }
    public void retryButtonClick()
    {
        retryButtonWasClicked = true; // Set the flag to true when the retry button is clicked
        switch (retryButtonActivatedBy)
        {
            case ButtonType.Button1:
                StartCoroutine(HandleButton1Click());
                break;
            case ButtonType.Button2:
                StartCoroutine(HandleButton2Click());
                break;
            case ButtonType.Button3:
                StartCoroutine(HandleButton3Click());
                break;
            default:
                Debug.LogWarning("Unknown button type. Retry button click not handled.");
                break;
        }
    }

    private void SetAlpha(Image image, float alphaValue)
    {
        Color color = image.color;
        color.a = alphaValue / 255f;
        image.color = color;
    }
}
