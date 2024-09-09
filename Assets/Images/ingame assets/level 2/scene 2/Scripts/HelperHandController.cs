using System.Collections;
using UnityEngine;

public class HelperHandController : MonoBehaviour
{
    public GameObject helperHandPrefab;
    public float helperDelay = 5f;
    public float helperMoveDuration = 1f;

    private GameObject helperHandInstance;
    private PillowDragAndDrop currentPillow;

    private PillowDragAndDrop bigPillowLeft;
    private PillowDragAndDrop bigPillowRight;

    private AudioSource audioSource;
    private AudioClip audioClipBigPillow;
    private AudioClip audioClipSmallPillow;

    [SerializeField]
    private float helperAudioDelay;
    private bool canPlayHelperAudioAfterDelay;

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
    }

    public void InitializePillows(PillowDragAndDrop left, PillowDragAndDrop right)
    {
        bigPillowLeft = left;
        bigPillowRight = right;

        // Start with either big pillow left or big pillow right
        if (bigPillowLeft != null && bigPillowLeft.GetComponent<Collider2D>().enabled)
        {
            ScheduleHelperHand(bigPillowLeft);
        }
        else if (bigPillowRight != null && bigPillowRight.GetComponent<Collider2D>().enabled)
        {
            ScheduleHelperHand(bigPillowRight);
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


        //canPlayHelperAudioAfterDelay = true;  
        PlayAudioForPillow(pillow);  // Play the appropriate audio clip once
        Debug.Log("check how many times this is calling");
            
    
        LeanTween.move(helperHandInstance, pillow.targetPosition.position, helperMoveDuration)
            .setOnComplete(() =>
            {
                helperHandInstance.transform.position = pillow.transform.position;
                StartHelperHand(pillow);
            });
    }

    public void ScheduleHelperHand(PillowDragAndDrop pillow)
    {
        Debug.Log("Scheduling Helper Hand for: " + pillow.gameObject.name);

        currentPillow = pillow;
        CancelInvoke(nameof(StartHelperHandInternal));
        Invoke(nameof(StartHelperHandInternal), helperDelay);
    }

    private void StartHelperHandInternal()
    {
        if (currentPillow != null && !currentPillow.HasInteracted)
        {
            Debug.Log("Helper Hand delay ended. Spawning for: " + currentPillow.gameObject.name);
            StartHelperHand(currentPillow);
        }
        else
        {
            Debug.Log("Pillow has already been interacted with or is null.");
        }
    }

    public void StopHelperHand()
    {
        Debug.Log("Stopping Helper Hand");

        if (helperHandInstance != null)
        {
            LeanTween.cancel(helperHandInstance);
            Destroy(helperHandInstance);
        }

        CancelInvoke(nameof(StartHelperHandInternal));
    }


    public void ScheduleNextPillow(PillowDragAndDrop nextPillow)
    {
        ScheduleHelperHand(nextPillow);
    }

    public void ResetAndScheduleHelperHand(PillowDragAndDrop pillow)
    {
        // Stop the current helper hand
        StopHelperHand();

        // Schedule the helper hand with a delay for the given pillow
        Invoke(nameof(ScheduleHelperHand), 10f);
    }


    private void PlayAudioForPillow(PillowDragAndDrop pillow)
    {
        if (audioSource != null)
        {
            if (pillow.IsBigPillow())  // Assuming you have a way to determine if a pillow is big
            {
                //StartCoroutine(PlayHelperHandAudioPeriodically(helperAudioDelay));
                audioSource.clip = audioClipBigPillow;
            }
            else
            {
                audioSource.clip = audioClipSmallPillow;
            }

            if (!audioSource.isPlaying)  // Ensure the audio only plays once
            {
                audioSource.Play();
            }


        }
    }

    //private IEnumerator PlayHelperHandAudioPeriodically(float helperAudioDelay)
    //{
    //    canPlayHelperAudioAfterDelay = false;// Prevent further audio from playing until after the delay
    //    yield return new WaitForSeconds(helperAudioDelay);  // Wait for the delay                                                  // Play the helper hand audio here
    //    Debug.Log("Playing helper hand audio after delay" + helperAudioDelay);
    //    // Example: PlayAudio(helperHandAudioClip);
    //    canPlayHelperAudioAfterDelay = true;  // Allow the audio to play again after the delay
    //}

}
