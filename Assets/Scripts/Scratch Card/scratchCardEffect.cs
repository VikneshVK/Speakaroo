using UnityEngine;
using System.Collections;
using TMPro; // Required for TextMeshPro elements

public class ScratchCardEffect : MonoBehaviour
{
    public GameObject maskPrefab;
    public TextMeshProUGUI feedbackText; // Assigned via inspector (TextMeshPro text component)
    private GameObject instantiatedMask;
    private bool timerStarted = false;
    private bool audioPlayed = false;
    private AudioSource audioSource;
    private AudioClip recordedClip;
    /*private bool isRecording = false;*/
    private float recordLength = 5f;
    private float highPitchFactor = 1.5f;
    private float silenceThreshold = 0.01f;
    public UiManager uiManager;  // Reference to the UIManager

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = false;

        // Debugging to check if the clip is assigned
        if (audioSource.clip == null)
        {
            Debug.LogError("Audio Clip not assigned to AudioSource component");
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (Camera.main == null)
            {
                Debug.LogError("Main Camera is not set in the scene.");
                return; // Exit if there's no main camera
            }

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider == null)
            {
                Debug.Log("Raycast did not hit any collider.");
                return; // Exit if no object was hit
            }

            if (hit.collider.gameObject == gameObject &&
                (gameObject.tag == "Card_1" || gameObject.tag == "Card_2"))
            {
                Vector3 spawnPos = new Vector3(hit.point.x, hit.point.y, hit.collider.transform.position.z);

                if (!AlreadyHasMaskAtPoint(spawnPos, hit.collider.transform))
                {
                    instantiatedMask = Instantiate(maskPrefab, spawnPos, Quaternion.identity);
                    instantiatedMask.transform.SetParent(hit.collider.transform);

                    if (!timerStarted && uiManager != null)
                    {
                        timerStarted = true;
                        Invoke("DisableSpriteAndStartRecording", 5f);
                        uiManager.ShowButtons(); // Show retry and close buttons through the UI manager
                    }
                    else if (uiManager == null)
                    {
                        Debug.LogError("UIManager not found. Ensure it is in the scene and correctly referenced.");
                    }
                }
            }
        }
    }


    private void DisableSpriteAndStartRecording()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;
        if (!audioPlayed && audioSource.clip != null)
        {
            audioSource.Play();
            audioPlayed = true;
            Invoke("CheckAndRecordAudio", audioSource.clip.length);
        }
        else
        {
            Debug.LogError("no audio found");
        }
        timerStarted = false;
    }

    private void CheckAndRecordAudio()
    {
        if (!Microphone.IsRecording(null))
        {
            recordedClip = Microphone.Start(null, false, Mathf.CeilToInt(recordLength), 44100);
            StartCoroutine(AnalyzeRecording());
        }
    }

    IEnumerator AnalyzeRecording()
    {
        float[] samples = new float[Mathf.CeilToInt(44100 * recordLength)];
        bool detectedVoice = false;

        yield return new WaitForSeconds(recordLength);

        Microphone.End(null);
        recordedClip.GetData(samples, 0);

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
            PlayRecording();
        }
        else
        {
            feedbackText.text = "Try saying the word!";
        }
    }

    private void PlayRecording()
    {
        if (recordedClip != null)
        {
            audioSource.pitch = highPitchFactor;
            audioSource.clip = recordedClip;
            audioSource.Play();
        }
    }

    public void PerformRetry()
    {
        // Reset state for retry
        feedbackText.text = "";
        audioPlayed = false;
        CheckAndRecordAudio();
    }

    public void PerformClose()
    {
        // Perform the close action
        Debug.Log("Close action triggered");

        // Find the "parrot" GameObject and get its Animator component
        GameObject parrot = GameObject.Find("parrot");
        if (parrot != null)
        {
            Animator parrotAnimator = parrot.GetComponent<Animator>();
            if (parrotAnimator != null)
            {
                // Set the "startFlying" parameter to true
                parrotAnimator.SetBool("startFlying", true);
            }
            else
            {
                Debug.LogError("Animator component not found on 'parrot'");
            }
        }
        else
        {
            Debug.LogError("GameObject 'parrot' not found");
        }

        // Find and destroy the "ST_Mechanics" GameObject
        GameObject stMechanics = GameObject.Find("ST_Mechanics(Clone)");
        if (stMechanics != null)
        {
            Destroy(stMechanics);
        }
        else
        {
            Debug.LogError("GameObject 'ST_Mechanics' not found");
        }
    }


    private bool AlreadyHasMaskAtPoint(Vector3 point, Transform parent)
    {
        foreach (Transform child in parent)
        {
            // First, check if the child has any tag assigned to avoid "Untagged" errors
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
