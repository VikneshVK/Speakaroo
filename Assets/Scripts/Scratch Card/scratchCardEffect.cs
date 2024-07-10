using UnityEngine;
using System.Collections;
using TMPro;

public class ScratchCardEffect : MonoBehaviour
{
    public GameObject maskPrefab;
    public TextMeshProUGUI feedbackText;
    private GameObject instantiatedMask;
    private bool isSpawning = false;
    private bool recordingStarted = false;
    private AudioSource audio1;
    private AudioSource audio2;
    private AudioClip recordedClip;
    private float recordLength = 5f;
    private float highPitchFactor = 1.5f;
    private float silenceThreshold = 0.01f;
    private bool card1Processed = false;
    private bool card2Processed = false;

    public GameObject Card1;
    public GameObject Card2;
    public GameObject retryButton; // Reference to the retry button
    public GameObject prefabInstance; // Reference to the prefab instance

    public UiManager uiManager;
    private BirdController birdController;
    private Animator birdAnimator; // Add reference for Bird Animator

    private void Start()
    {
        audio1 = Card1.GetComponent<AudioSource>();
        audio2 = Card2.GetComponent<AudioSource>();

        if (audio1 == null || audio2 == null)
        {
            Debug.LogError("Audio Source not assigned to one or both cards.");
        }

        Collider2D collider1 = Card1.GetComponent<Collider2D>();
        Collider2D collider2 = Card2.GetComponent<Collider2D>();
        collider1.enabled = true;
        collider2.enabled = false;

        retryButton.SetActive(true); // Ensure retry button is enabled
        retryButton.transform.localScale = Vector3.zero; // Initialize the retry button scale to zero

        // Initialize BirdController and Animator reference
        InitializeBirdController();
    }

    private void InitializeBirdController()
    {
        GameObject parrotGameObject = GameObject.FindWithTag("Bird");
        if (parrotGameObject != null)
        {
            birdController = parrotGameObject.GetComponent<BirdController>();
            birdAnimator = parrotGameObject.GetComponent<Animator>();

            if (birdController != null)
            {
                Debug.Log("BirdController component found and initialized.");
            }
            else
            {
                Debug.LogError("BirdController component not found on Parrot GameObject");
            }
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
            if (Camera.main == null)
            {
                Debug.LogError("Main Camera is not set in the scene.");
                return;
            }

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == Card1 && !card1Processed)
            {
                StartCoroutine(SpawnPrefabs(hit.collider.transform));
            }
            else if (hit.collider != null && hit.collider.gameObject == Card2 && card1Processed && !card2Processed)
            {
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

        // Tween retry button to original scale when recording starts
        LeanTween.scale(retryButton, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);

        if (!Microphone.IsRecording(null))
        {
            recordedClip = Microphone.Start(null, false, Mathf.CeilToInt(recordLength), 44100);
            yield return new WaitForSeconds(recordLength);

            Microphone.End(null);
            float[] samples = new float[Mathf.CeilToInt(44100 * recordLength)];
            recordedClip.GetData(samples, 0);

            bool detectedVoice = false;
            foreach (float sample in samples)
            {
                if (Mathf.Abs(sample) > silenceThreshold)
                {
                    detectedVoice = true;
                    break;
                }
            }

            if (detectedVoice)
            {
                feedbackText.text = "Did you say...?";
                PlayRecording(cardTransform);
            }
            else
            {
                feedbackText.text = "Try saying the word!";
            }
        }

        recordingStarted = false;
    }

    private void PlayRecording(Transform cardTransform)
    {
        if (recordedClip != null)
        {
            AudioSource audioSourceToPlay = (cardTransform == Card1.transform) ? audio1 : audio2;

            audioSourceToPlay.pitch = highPitchFactor;
            audioSourceToPlay.clip = recordedClip;
            audioSourceToPlay.Play();
            Collider2D collider2 = Card2.GetComponent<Collider2D>();

            if (cardTransform == Card1.transform)
            {
                card1Processed = true;
                collider2.enabled = true;

                // Tween retry button back to zero when card 2 collider is enabled
                LeanTween.scale(retryButton, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInBack);
            }
            else if (cardTransform == Card2.transform)
            {
                card2Processed = true;
                StartCoroutine(WaitForRecordingToEndThenTweenAndFly(audioSourceToPlay));
            }

            Debug.Log(cardTransform.name + " playback complete");
        }
    }

    private IEnumerator WaitForRecordingToEndThenTweenAndFly(AudioSource audioSourceToPlay)
    {
        yield return new WaitWhile(() => audioSourceToPlay.isPlaying);

        // Scale the entire prefab to zero
        LeanTween.scale(prefabInstance, Vector3.zero, 0.1f).setEase(LeanTweenType.easeInOutBack);

        // Set the isFlying boolean to true
        birdAnimator.SetBool("IsFlying", true);
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
}
