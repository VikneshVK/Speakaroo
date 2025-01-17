using TMPro;
using UnityEngine;
using System.Collections;


public class dragManager : MonoBehaviour
{
    public static int totalCorrectDrops;  // Track the number of correct drops
    public AudioSource audioSource;  // The AudioSource for playing object-specific audios
    public AudioSource feedbackAudioSource;  // The AudioSource for playing feedback audios
    public AudioClip[] objectAudioClips;  // Array for each object's audio clip
    public AudioClip[] feedbackClips;  // Array for feedback audios (2 correct, 1 wrong)
    public GameObject[] interactables;
    public GameObject glowPrefab;
    public Animator birdAnimator;
    public Animator boyAnimator;
    public GameObject[] gameObjects;  // The 6 game objects
    public string[] triggerNames = { "bagTalk", "sheetTalk", "rugTalk", "pillowTalk", "shoeTalk", "teddyTalk" };  // Triggers for each game object
    public string[] feedbackTriggers = { "rightDrop1", "rightDrop2", "wrongDrop" };  // Triggers for feedback audios
    public bool allDone = false;  // Flag to indicate all drops are done
    public AudioSource kikiAudio;
    public SubtitleManager subtitleManager;

    public string[] subtitles;  // Array to hold the subtitle text for each audio    

    private HelperPointer helperPointer;
    private GameObject parrot;
    private SpriteRenderer interactableSpriteRender;
    private GameObject glowInstance;

    void Start()
    {
        parrot = GameObject.FindGameObjectWithTag("Bird");
        helperPointer = FindObjectOfType<HelperPointer>();
        InitializeGameObjects();

        /*// Make sure the first object is active
        if (gameObjects.Length > 0)
        {
            ActivateNextObject(0);  // Activate the first object
        }*/

        Debug.Log("Game Initialized. Ready to start.");
        totalCorrectDrops = 0;
    }

    // Initialize all game objects
    void InitializeGameObjects()
    {
        foreach (GameObject obj in gameObjects)
        {
            obj.GetComponent<Collider2D>().enabled = false;  // Disable all objects initially
            obj.GetComponent<DragHandler>().dragManager = this;  // Link dragManager
        }

        // Reset all triggers
        foreach (string trigger in triggerNames)
        {
            birdAnimator.ResetTrigger(trigger);
        }

        birdAnimator.SetBool("allDone", false);
    }

    // Called by DragHandler after a successful drop
    public void OnItemDropped(bool isCorrectDrop)
    {
        if (isCorrectDrop)
        {
            totalCorrectDrops++;  // Increment the correct drop counter

            Debug.Log("Total Correct Drops: " + totalCorrectDrops);

            if (totalCorrectDrops < triggerNames.Length)
            {
                // Play feedback audio for the right drop (either rightDrop1 or rightDrop2)
                StartCoroutine(PlayFeedbackAndAdvance(true));
            }
            else
            {
                // If all items are dropped, trigger the end condition
                birdAnimator.SetBool("alldone", true);
                boyAnimator.SetTrigger("AllDone");
                StartCoroutine(HandleAllItemsDropped());
                Debug.Log("All items dropped successfully.");
            }
        }
        else
        {
            // Handle incorrect drop
            StartCoroutine(PlayFeedbackAndAdvance(false));
        }
    }

    private IEnumerator HandleAllItemsDropped()
    {
        kikiAudio.Play();
        subtitleManager.DisplaySubtitle("Thank You, Jojo and Friend. My Room looks so clean now.", "Kiki", kikiAudio.clip);// Play the audio
        yield return new WaitUntil(() => !kikiAudio.isPlaying);  // Wait until the audio finishes playing

        allDone = true;  // Set allDone to true after audio finishes
        Debug.Log("All items dropped, and kikiAudio finished playing.");
    }

    // Play feedback and advance to the next object
    private IEnumerator PlayFeedbackAndAdvance(bool isCorrectDrop)
    {
        if (isCorrectDrop)
        {
            // Play the feedback audio for a correct drop
            int randomIndex = Random.Range(0, 2);  // Select either rightDrop1 or rightDrop2 feedback
            feedbackAudioSource.clip = feedbackClips[randomIndex];
            birdAnimator.SetTrigger(feedbackTriggers[randomIndex]);  // Trigger corresponding feedback animation
            boyAnimator.SetTrigger("rightDrop");

            feedbackAudioSource.Play();
            yield return new WaitForSeconds(3f);

            // After feedback, play the object-specific audio
            PlayObjectAudio(totalCorrectDrops);
            yield return new WaitUntil(() => !audioSource.isPlaying);

            // Enable the next object collider after both audios have finished playing
            if (totalCorrectDrops < gameObjects.Length)
            {
                ActivateNextObject(totalCorrectDrops);  // Activate the next object
            }
        }
        else
        {
            // Play wrong drop audio and trigger the wrong drop animation
            feedbackAudioSource.clip = feedbackClips[2];  // Wrong feedback audio
            birdAnimator.SetTrigger(feedbackTriggers[2]);  // Trigger wrong feedback animation
            boyAnimator.SetTrigger("wrongDrop");

            feedbackAudioSource.Play();
            yield return new WaitForSeconds(2.5f);
            PlayObjectAudio(totalCorrectDrops);
        }
    }

    // Activate the next object by enabling its collider
    private void ActivateNextObject(int index)
    {
        if (index < gameObjects.Length)
        {
            GameObject nextObject = gameObjects[index];
            nextObject.GetComponent<Collider2D>().enabled = true;

            Debug.Log($"Activated {nextObject.name} for dragging.");

            // Schedule the helper hand for the next game object
            DragHandler dragHandler = nextObject.GetComponent<DragHandler>();
            if (dragHandler != null && helperPointer != null)
            {
                helperPointer.ScheduleHelperHand(dragHandler, this);
                Debug.Log($"Scheduled helper hand for {nextObject.name}.");
            }
            else
            {
                Debug.LogWarning($"Failed to schedule helper hand. DragHandler or HelperPointer is missing for {nextObject.name}.");
            }
        }
    }

    // Play the audio associated with the current object based on totalCorrectDrops
    public void PlayObjectAudio(int index)
    {
        if (index < objectAudioClips.Length && audioSource != null)
        {
            GameObject nextObject = gameObjects[index];
            SpawnAndTweenGlow(nextObject.transform.position);
            audioSource.clip = objectAudioClips[index];
            audioSource.Play();
            birdAnimator.SetTrigger(triggerNames[index]);  // Trigger the animation for the current object
            Debug.Log($"Playing audio for {triggerNames[index]}");

            // Display the subtitle for the respective object
            if (index < subtitles.Length)
            {
                subtitleManager.DisplaySubtitle(subtitles[index], "Kiki", audioSource.clip);
            }
        }
        else
        {
            Debug.LogWarning("Invalid index or audioSource not set.");
        }
    }

    // Coroutine to reveal text word by word
    

    public void OnWrongDrop()
    {
        Debug.Log("Wrong drop made. Playing wrong feedback.");
        StartCoroutine(PlayFeedbackAndAdvance(false));
    }

    public void FinishLevelAfterDelay()
    {
        allDone = true;
    }

    private void SpawnAndTweenGlow(Vector3 position)
    {
        if (glowInstance != null)
        {
            Destroy(glowInstance);
        }

        glowInstance = Instantiate(glowPrefab, position, Quaternion.identity);
        LeanTween.scale(glowInstance, Vector3.one * 8, 0.5f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
        {
            LeanTween.scale(glowInstance, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInOutQuad).setDelay(1f).setOnComplete(() =>
            {
                Destroy(glowInstance);
            });
        });
    }

}
