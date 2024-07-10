using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class ScratchCardEffect : MonoBehaviour
{
    public GameObject maskPrefab;
    public TextMeshProUGUI feedbackText;
    public GameObject Card1;
    public GameObject Card2;
    public GameObject retryButton;
    public GameObject prefabInstance;

    private GameObject instantiatedMask;
    private bool isSpawning = false;
    private bool recordingStarted = false;
    private AudioSource audio1;
    private AudioSource audio2;
    private AudioClip recAudio1;
    private AudioClip recAudio2;
    private float recordLength = 5f;
    private float highPitchFactor = 1.5f;
    private float silenceThreshold = 0.01f;
    private bool card1Processed = false;
    private bool card2Processed = false;

    public UiManager uiManager;
    private BirdController birdController;
    private Animator birdAnimator;

    private Button retryButtonComponent;

    private void Start()
    {
        Debug.Log("Game Started: Initializing components.");

        InitializeAudioSources();
        InitializeColliders();
        InitializeRetryButton();
        InitializeBirdController();
    }

    private void InitializeAudioSources()
    {
        audio1 = Card1.GetComponent<AudioSource>();
        audio2 = Card2.GetComponent<AudioSource>();

        if (audio1 == null || audio2 == null)
        {
            Debug.LogError("Audio Source not assigned to one or both cards.");
        }
    }

    private void InitializeColliders()
    {
        Collider2D collider1 = Card1.GetComponent<Collider2D>();
        Collider2D collider2 = Card2.GetComponent<Collider2D>();
        collider1.enabled = true;
        collider2.enabled = false;
    }

    private void InitializeRetryButton()
    {
        retryButton.SetActive(true); // Ensure retry button is enabled
        retryButton.transform.localScale = Vector3.zero; // Initialize the retry button scale to zero

        retryButtonComponent = retryButton.GetComponent<Button>();
        if (retryButtonComponent != null)
        {
            retryButtonComponent.onClick.AddListener(Retry);
        }
    }

    private void InitializeBirdController()
    {
        GameObject parrotGameObject = GameObject.FindWithTag("Bird");
        if (parrotGameObject != null)
        {
            birdController = parrotGameObject.GetComponent<BirdController>();
            birdAnimator = parrotGameObject.GetComponent<Animator>();

            Debug.Log("BirdController and Animator components found and initialized.");
        }
        else
        {
            Debug.LogError("Parrot GameObject not found");
        }
    }

    private void Update()
    {
        if (isSpawning || recordingStarted)
        {
            return; // Skip update if currently spawning or recording
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == Card1 && !card1Processed)
            {
                Debug.Log("Card 1 hit detected.");
                StartCoroutine(SpawnPrefabs(hit.collider.transform));
            }
            else if (hit.collider != null && hit.collider.gameObject == Card2 && card1Processed && !card2Processed)
            {
                Debug.Log("Card 2 hit detected.");
                LeanTween.scale(retryButton, Vector3.zero, 0.5f).setEase(LeanTweenType.easeOutBack);
                StartCoroutine(SpawnPrefabs(hit.collider.transform));
            }
        }
    }

    private IEnumerator SpawnPrefabs(Transform cardTransform)
    {
        Debug.Log("Spawning prefabs started.");
        isSpawning = true;

        for (float timer = 0; timer < 5f; timer += Time.deltaTime)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null && hit.collider.transform == cardTransform)
            {
                Vector3 spawnPos = new Vector3(hit.point.x, hit.point.y, hit.collider.transform.position.z);
                if (!AlreadyHasMaskAtPoint(spawnPos, hit.collider.transform))
                {
                    instantiatedMask = Instantiate(maskPrefab, spawnPos, Quaternion.identity);
                    instantiatedMask.transform.SetParent(hit.collider.transform);
                }
            }

            yield return null;
        }

        SpriteRenderer sr = cardTransform.GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        isSpawning = false;
        Debug.Log("Spawning prefabs ended.");

        AudioSource audioSourceToPlay = (cardTransform == Card1.transform) ? audio1 : audio2;
        if (!audioSourceToPlay.isPlaying && audioSourceToPlay.clip != null)
        {
            audioSourceToPlay.Play();
            yield return new WaitForSeconds(audioSourceToPlay.clip.length);
            StartCoroutine(CheckAndRecordAudio(cardTransform));
        }
    }

    private IEnumerator CheckAndRecordAudio(Transform cardTransform)
    {
        recordingStarted = true;
        feedbackText.text = "Recording...";
        Debug.Log("Recording started.");

        if (!Microphone.IsRecording(null))
        {
            // Start recording audio
            AudioClip recordedClip = Microphone.Start(null, false, Mathf.CeilToInt(recordLength), 44100);
            yield return new WaitForSeconds(recordLength);

            // Stop recording and analyze the audio
            Microphone.End(null);
            float[] samples = new float[Mathf.CeilToInt(44100 * recordLength)];
            recordedClip.GetData(samples, 0);

            bool detectedVoice = AnalyzeRecordedAudio(samples);

            if (detectedVoice)
            {
                feedbackText.text = "Did you say...?";
                if (cardTransform == Card1.transform)
                {
                    recAudio1 = recordedClip;
                }
                else if (cardTransform == Card2.transform)
                {
                    recAudio2 = recordedClip;
                }
                PlayRecording(cardTransform);
            }
            else
            {
                feedbackText.text = "Click Retry and try saying the word!";
                // Make retry button visible only if no voice detected
                LeanTween.scale(retryButton, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);
            }
        }

        recordingStarted = false;
        Debug.Log("Recording ended.");
    }

    private bool AnalyzeRecordedAudio(float[] samples)
    {
        foreach (float sample in samples)
        {
            if (Mathf.Abs(sample) > silenceThreshold)
            {
                return true;
            }
        }
        return false;
    }

    private void PlayRecording(Transform cardTransform)
    {
        if (cardTransform == Card1.transform && recAudio1 != null)
        {
            audio1.pitch = highPitchFactor;
            audio1.clip = recAudio1;
            audio1.Play();
            card1Processed = true;
            Card2.GetComponent<Collider2D>().enabled = true;
            LeanTween.scale(retryButton, Vector3.one, 0.5f).setEase(LeanTweenType.easeInBack);
        }
        else if (cardTransform == Card2.transform && recAudio2 != null)
        {
            audio2.pitch = highPitchFactor;
            audio2.clip = recAudio2;
            audio2.Play();
            card2Processed = true;
            LeanTween.scale(retryButton, Vector3.one, 0.5f).setEase(LeanTweenType.easeInBack);
            StartCoroutine(WaitForRecordingToEndThenTweenAndFly(audio2));
            
        }
    }

    private IEnumerator WaitForRecordingToEndThenTweenAndFly(AudioSource audioSourceToPlay)
    {
        yield return new WaitWhile(() => audioSourceToPlay.isPlaying);
        LeanTween.scale(prefabInstance, Vector3.zero, 1f).setEase(LeanTweenType.easeInOutBack);

        if (birdAnimator != null)
        {
            birdAnimator.SetBool("IsFlying", true);
        }
        else
        {
            Debug.LogError("Bird Animator is not assigned or not found.");
        }
    }

    private bool AlreadyHasMaskAtPoint(Vector3 point, Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (!string.IsNullOrEmpty(child.tag) && child.gameObject.CompareTag("Mask"))
            {
                float distance = Vector3.Distance(child.position, point);
                if (distance < 0.01f) // Adjust this threshold as necessary
                    return true;
            }
        }
        return false;
    }

    public void Retry()
    {
        StopAllCoroutines();
        feedbackText.text = "Restarting...";
        recordingStarted = false;
        isSpawning = false;

        if (card1Processed && !card2Processed)
        {
            recAudio1 = null;
            StartCoroutine(PlayAudioAndRecord(Card1.transform, audio1));
        }
        else if (card2Processed)
        {
            recAudio2 = null;
            StartCoroutine(PlayAudioAndRecord(Card2.transform, audio2));
        }

        ResetAudioSources();

        Card1.GetComponent<Collider2D>().enabled = true;
        Card2.GetComponent<Collider2D>().enabled = false;

        retryButton.SetActive(true);
        LeanTween.scale(retryButton, Vector3.one, 0.5f).setEase(LeanTweenType.easeInBack);
    }

    private void ResetAudioSources()
    {
        audio1.Stop();
        audio2.Stop();
        audio1.pitch = 1;
        audio2.pitch = 1;
    }

    private IEnumerator PlayAudioAndRecord(Transform cardTransform, AudioSource audioSource)
    {
        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length);
            StartCoroutine(CheckAndRecordAudio(cardTransform));
        }
    }
}
