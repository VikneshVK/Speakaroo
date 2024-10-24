using System.Collections;
using TMPro;
using UnityEngine;

public class HelperHandController : MonoBehaviour
{
    public GameObject helperHandPrefab;
    public GameObject glowPrefab;  // Reference to the Glow GameObject
    public float helperDelay = 10f;
    public float helperMoveDuration = 1f;
    public float glowScaleTime = 0.5f;

    private GameObject helperHandInstance;
    private GameObject glowInstance; // Instance of the Glow object
    private PillowDragAndDrop currentPillow;

    private bool timerPaused = false;
    private bool isPingPongActive = false;
    private float halfHelperDelay;

    private AudioSource audioSource;
    private AudioClip audioClipBigPillow;
    private AudioClip audioClipSmallPillow;

    public Animator birdAnimator;
    public TextMeshProUGUI subtitleText;
    private bool isPlayingAudio = false;  // To track whether audio is currently playing

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();  // Get the AudioSource component attached to the game object

        // Load the audio clips from the Resources folder
        audioClipBigPillow = Resources.Load<AudioClip>("Audio/Helper Audio/BigPillowAudio");
        audioClipSmallPillow = Resources.Load<AudioClip>("Audio/Helper Audio/SmallPillowAudio");

        if (audioClipBigPillow == null || audioClipSmallPillow == null)
        {
            Debug.LogError("Audio clips not found in Resources. Please check the paths.");
        }

        halfHelperDelay = helperDelay / 2f; // Calculate half of the helper delay
    }

    public void ScheduleHelperHand(PillowDragAndDrop pillow)
    {
        Debug.Log("Scheduling Helper Hand for: " + pillow.gameObject.name);
        currentPillow = pillow;
        CancelInvoke(nameof(StartHelperHandInternal));
        Invoke(nameof(StartHelperHandInternal), halfHelperDelay); // Schedule helper hand after half delay
    }

    private void StartHelperHandInternal()
    {
        if (currentPillow != null && !currentPillow.HasInteracted)
        {
            Debug.Log("Pausing timer and starting Glow effect for: " + currentPillow.gameObject.name);
            timerPaused = true;
            StartGlowEffect();
        }
        else
        {
            Debug.Log("Pillow has already been interacted with or is null.");
        }
    }

    private void StartGlowEffect()
    {
        // Teleport the glow object to the pillow
        if (glowPrefab != null)
        {
            if (glowInstance == null)
            {
                glowInstance = Instantiate(glowPrefab, currentPillow.transform.position, Quaternion.identity);
            }
            else
            {
                glowInstance.transform.position = currentPillow.transform.position;
            }

            // Scale glow up to 15
            LeanTween.scale(glowInstance, Vector3.one * 15f, glowScaleTime).setOnComplete(() =>
            {
                // Wait for 1 second, then scale back down
                StartCoroutine(ScaleDownGlow());
            });
        }
    }

    private IEnumerator ScaleDownGlow()
    {
        yield return new WaitForSeconds(1f);

        // Scale down glow to 0
        LeanTween.scale(glowInstance, Vector3.zero, glowScaleTime).setOnComplete(() =>
        {
            // Resume the timer after the glow effect is done
            Debug.Log("Resuming helper timer.");
            timerPaused = false;

            // After the timer resumes, complete the helper hand tweening
            Invoke(nameof(CompleteHelperHandTimer), halfHelperDelay);
        });
    }

    private void CompleteHelperHandTimer()
    {
        if (!currentPillow.HasInteracted)
        {
            StartHelperHand(currentPillow);

            // Start PingPong glow tweening along with the helper hand tweening
            StartGlowPingPongEffect();
        }
    }

    private void StartGlowPingPongEffect()
    {
        if (glowInstance != null)
        {
            // Set PingPong scaling tween for glow object (from scale 0 to 15)
            LeanTween.scale(glowInstance, Vector3.one * 15f, glowScaleTime).setLoopPingPong();
            isPingPongActive = true;
        }
    }

    public void StopHelperHand()
    {
        Debug.Log("Stopping Helper Hand");

        // Cancel and destroy the helper hand instance if it's still active
        if (helperHandInstance != null)
        {
            LeanTween.cancel(helperHandInstance);  // Cancel any active LeanTween on the helper hand
            Destroy(helperHandInstance);  // Destroy the helper hand instance
            helperHandInstance = null;  // Reset the reference to the helper hand instance
        }

        // Cancel and destroy the glow instance if it's still active
        if (glowInstance != null)
        {
            LeanTween.cancel(glowInstance);  // Cancel any active LeanTween on the glow object
            Destroy(glowInstance);  // Destroy the glow instance
            glowInstance = null;  // Reset the reference to the glow instance
        }

        // Stop any active coroutines related to helper hand or glow
        StopAllCoroutines();  // This will stop any ongoing coroutines, including glow and audio

        // Reset the isPingPongActive flag
        isPingPongActive = false;

        // Cancel any delayed invocations to StartHelperHandInternal
        CancelInvoke(nameof(StartHelperHandInternal));
    }


    private void StartHelperHand(PillowDragAndDrop pillow)
    {
        Debug.Log("Starting Helper Hand for: " + pillow.gameObject.name);

        if (helperHandInstance != null)
        {
            Destroy(helperHandInstance);
        }

        currentPillow = pillow;
        helperHandInstance = Instantiate(helperHandPrefab, pillow.transform.position, Quaternion.identity);
        

        PlayAudioForPillow(pillow);

        LeanTween.move(helperHandInstance, pillow.targetPosition.position, helperMoveDuration)
            .setOnComplete(() =>
            {
                helperHandInstance.transform.position = pillow.transform.position;
                StartHelperHand(pillow); // Loop hand movement if not interacted
            });
    }

    private void PlayAudioForPillow(PillowDragAndDrop pillow)
    {
        if (!isPlayingAudio && audioSource != null)
        {
            Debug.Log("audio called");
            StartCoroutine(PlayAudioWithDelay(pillow));
        }
    }

    private IEnumerator PlayAudioWithDelay(PillowDragAndDrop pillow)
    {
        isPlayingAudio = true;  // Mark that the audio is playing

        while (!pillow.HasInteracted)
        {
            if (pillow.IsBigPillow())
            {
                audioSource.clip = audioClipBigPillow;
                StartCoroutine(RevealTextWordByWord("Put the Big Pillow at the Back", 0.5f));
                if (birdAnimator != null) birdAnimator.SetTrigger("bigPillow");
            }
            else
            {
                audioSource.clip = audioClipSmallPillow;
                StartCoroutine(RevealTextWordByWord("Put the Small Pillow at the front of the big Pillow", 0.5f));
                if (birdAnimator != null) birdAnimator.SetTrigger("smallPillow");
            }

            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length);  // Wait for the audio to finish
            yield return new WaitForSeconds(3f);  // Wait for 3 seconds before playing the audio again
        }

        isPlayingAudio = false;  // Reset the flag when done
    }

    public void ScheduleNextPillow(PillowDragAndDrop nextPillow)
    {
        // This method schedules the helper hand for the next pillow in sequence.
        if (nextPillow != null)
        {
            Debug.Log($"Scheduling next helper hand for: {nextPillow.gameObject.name}");
            isPlayingAudio = false;
            ScheduleHelperHand(nextPillow);
        }
        else
        {
            Debug.LogWarning("Next pillow reference is null.");
        }
    }

    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";  // Clear the text before starting
        subtitleText.gameObject.SetActive(true);  // Ensure the subtitle text is active

        string[] words = fullText.Split(' ');  // Split the full text into individual words

        // Reveal words one by one
        for (int i = 0; i < words.Length; i++)
        {
            // Instead of appending, build the text up to the current word
            subtitleText.text = string.Join(" ", words, 0, i + 1);  // Show only the words up to the current index
            yield return new WaitForSeconds(delayBetweenWords);  // Wait before revealing the next word
        }
        subtitleText.gameObject.SetActive(false);
    }
}
