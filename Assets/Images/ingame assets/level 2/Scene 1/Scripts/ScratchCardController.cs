using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class ScratchCardController : MonoBehaviour
{
    public GameObject maskPrefab;
    public TextMeshProUGUI feedbackText;
    public GameObject Card1;
    public GameObject Card2;
    public GameObject retryButton;
    public GameObject prefabInstance;
    public GameObject ST_Canvas;

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
    private Animator birdAnimator;

    private Button retryButtonComponent;

    private void Awake()
    {
        LeanTween.init(1500);
    }
    private void Start()
    {
        Debug.Log("Game Started: Initializing components.");

        InitializeAudioSources();
        InitializeColliders();
        InitializeRetryButton();
        InitializeBirdAnimator();
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
        retryButton.SetActive(true);
        retryButton.transform.localScale = Vector3.zero;

        retryButtonComponent = retryButton.GetComponent<Button>();
        if (retryButtonComponent != null)
        {
            retryButtonComponent.onClick.AddListener(Retry);
        }
    }

    private void InitializeBirdAnimator()
    {
        GameObject birdGameObject = GameObject.FindWithTag("Bird");
        if (birdGameObject != null)
        {
            birdAnimator = birdGameObject.GetComponent<Animator>();

            Debug.Log("Bird Animator component found and initialized.");
        }
        else
        {
            Debug.LogError("Bird GameObject not found.");
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
                StartCoroutine(SpawnPrefabs(hit.collider.transform));
            }
            else if (hit.collider != null && hit.collider.gameObject == Card2 && card1Processed && !card2Processed)
            {
                StopAllAudio(); // Stop any audio before playing new one
                LeanTween.scale(retryButton, Vector3.zero, 0.5f).setEase(LeanTweenType.easeOutBack);
                StartCoroutine(SpawnPrefabs(hit.collider.transform));
            }
        }
    }

    private IEnumerator SpawnPrefabs(Transform cardTransform)
    {
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
            AudioClip recordedClip = Microphone.Start(null, false, Mathf.CeilToInt(recordLength), 44100);
            yield return new WaitForSeconds(recordLength);

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
        AudioSource audioSourceToPlay = (cardTransform == Card1.transform) ? audio1 : audio2;
        audioSourceToPlay.pitch = highPitchFactor;
        audioSourceToPlay.clip = (cardTransform == Card1.transform) ? recAudio1 : recAudio2;
        audioSourceToPlay.Play();

        if (cardTransform == Card1.transform)
        {
            card1Processed = true;
            Card2.GetComponent<Collider2D>().enabled = true;
        }
        else if (cardTransform == Card2.transform)
        {
            card2Processed = true;
            StartCoroutine(WaitForRecordingToEndThenTweenAndFly(audioSourceToPlay));
        }

        LeanTween.scale(retryButton, Vector3.one, 0.5f).setEase(LeanTweenType.easeInBack);
    }

    private IEnumerator WaitForRecordingToEndThenTweenAndFly(AudioSource audioSourceToPlay)
    {
        yield return new WaitWhile(() => audioSourceToPlay.isPlaying);


        Transform[] childTransforms = prefabInstance.GetComponentsInChildren<Transform>();


        foreach (Transform child in childTransforms)
        {
            if (child != prefabInstance.transform) // Skip the parent prefabInstance itself
            {
                LeanTween.scale(child.gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInOutBack);
            }
        }

        // Wait for all children to finish tweening
        yield return new WaitForSeconds(0.5f);

        // Scale down the prefabInstance itself
        LeanTween.scale(prefabInstance, Vector3.zero, 1f).setEase(LeanTweenType.easeInOutBack);

        // Wait for the prefabInstance to finish tweening
        yield return new WaitForSeconds(1f);

        if (birdAnimator != null)
        {
            birdAnimator.SetBool("startWalking", true);
        }
        else
        {
            Debug.LogError("Bird Animator is not assigned or not found.");
        }
    }

    public void Retry()
    {
        StopAllCoroutines(); // Stop all coroutines to prevent conflicts and overlap.
        feedbackText.text = "Restarting..."; // Update feedback text to inform the user.

        if (card1Processed && !card2Processed)
        {
            // If card1 was processed but not card2, reset recAudio1 and start the process for card1 again.
            recAudio1 = null;
            StartCoroutine(PlayAudioAndRecord(Card1.transform, audio1));
        }
        else if (card2Processed)
        {
            // If card2 was processed, reset recAudio2 and start the process for card2 again.
            recAudio2 = null;
            StartCoroutine(PlayAudioAndRecord(Card2.transform, audio2));
        }

        // Reset the audio sources to ensure they are clean for a new start.
        ResetAudioSources();

        // Re-enable colliders accordingly.
        Card1.GetComponent<Collider2D>().enabled = true;
        Card2.GetComponent<Collider2D>().enabled = card1Processed; // Only enable Card2 if Card1 has been processed.

        retryButton.SetActive(true); // Make sure the retry button is visible.
        LeanTween.scale(retryButton, Vector3.one, 0.5f).setEase(LeanTweenType.easeInBack); // Animate the retry button to reappear smoothly.
    }


    private void StopAllAudio()
    {
        audio1.Stop();
        audio2.Stop();
    }

    private void ResetAudioSources()
    {
        audio1.Stop();
        audio2.Stop();
        audio1.clip = null;
        audio2.clip = null;
        audio1.pitch = 1;
        audio2.pitch = 1;
    }

    private IEnumerator PlayAudioAndRecord(Transform cardTransform, AudioSource audioSource)
    {
        if ( audioSource.clip != null)
        {
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length);
            StartCoroutine(CheckAndRecordAudio(cardTransform));
        }
    }

    private bool AlreadyHasMaskAtPoint(Vector3 point, Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (!string.IsNullOrEmpty(child.tag) && child.gameObject.CompareTag("Mask"))
            {
                float distance = Vector3.Distance(child.position, point);
                if (distance < 0.01f)
                    return true;
            }
        }
        return false;
    }
}
