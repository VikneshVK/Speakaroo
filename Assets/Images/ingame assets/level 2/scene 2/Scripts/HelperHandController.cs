using System.Collections;
using TMPro;
using UnityEngine;

public class HelperHandController : MonoBehaviour
{
    public GameObject helperHandPrefab;
    public float helperDelay = 10f;
    public float helperMoveDuration = 1f;

    private GameObject helperHandInstance;
    private PillowDragAndDrop currentPillow;

    private AudioSource audioSource;
    private AudioClip audioClipBigPillow;
    private AudioClip audioClipSmallPillow;

    public Animator birdAnimator;
    public TextMeshProUGUI subtitleText;
    private bool isPlayingAudio = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Load the audio clips from the Resources folder
        audioClipBigPillow = Resources.Load<AudioClip>("Audio/Helper Audio/bigpillow");
        audioClipSmallPillow = Resources.Load<AudioClip>("Audio/Helper Audio/smallpillow");

        if (audioClipBigPillow == null || audioClipSmallPillow == null)
        {
            Debug.LogError("Audio clips not found in Resources. Please check the paths.");
        }
    }

    public void ScheduleHelperHand(PillowDragAndDrop pillow)
    {
        Debug.Log("Scheduling Helper Hand for: " + pillow.gameObject.name);
        currentPillow = pillow;
        CancelInvoke(nameof(StartHelperHandInternal));
        Invoke(nameof(StartHelperHandInternal), helperDelay); // Schedule helper hand after full delay
    }

    private void StartHelperHandInternal()
    {
        if (currentPillow != null && !currentPillow.HasInteracted)
        {
            Debug.Log("Starting Helper Hand for: " + currentPillow.gameObject.name);
            StartHelperHand(currentPillow);
        }
        else
        {
            Debug.Log("Pillow has already been interacted with or is null.");
        }
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
            Debug.Log("Audio called");
            StartCoroutine(PlayAudioWithDelay(pillow));
        }
    }
    public void StopHelperHand()
    {
        Debug.Log("Stopping Helper Hand");

        if (helperHandInstance != null)
        {
            LeanTween.cancel(helperHandInstance);
            Destroy(helperHandInstance);
            helperHandInstance = null;
        }

        StopAllCoroutines();

        isPlayingAudio = false;

        CancelInvoke(nameof(StartHelperHandInternal));

        Debug.Log("Helper hand has been fully cleaned up.");
    }


    private IEnumerator PlayAudioWithDelay(PillowDragAndDrop pillow)
    {
        isPlayingAudio = true;

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
            yield return new WaitForSeconds(audioSource.clip.length);
            yield return new WaitForSeconds(3f);
        }

        isPlayingAudio = false;
    }

    public void ScheduleNextPillow(PillowDragAndDrop nextPillow)
    {
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
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(true);

        string[] words = fullText.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            subtitleText.text = string.Join(" ", words, 0, i + 1);
            yield return new WaitForSeconds(delayBetweenWords);
        }

        subtitleText.gameObject.SetActive(false);
    }
}
