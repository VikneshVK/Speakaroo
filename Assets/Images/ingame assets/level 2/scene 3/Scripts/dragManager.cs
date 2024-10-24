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

    public Animator birdAnimator;
    public Animator boyAnimator;
    public GameObject[] gameObjects;  // The 6 game objects
    public string[] triggerNames = { "bagTalk", "sheetTalk", "rugTalk", "pillowTalk", "shoeTalk", "teddyTalk" };  // Triggers for each game object
    public string[] feedbackTriggers = { "rightDrop1", "rightDrop2", "wrongDrop" };  // Triggers for feedback audios
    public bool allDone = false;  // Flag to indicate all drops are done
    public AudioSource kikiAudio;
    public TextMeshProUGUI subtitleText;

    public string[] subtitles;  // Array to hold the subtitle text for each audio
    public float delayBetweenWords = 0.3f;  // Delay between words for subtitle reveal

    private HelperPointer helperPointer;
    private GameObject parrot;
    private SpriteRenderer interactableSpriteRender;

    void Start()
    {
        parrot = GameObject.FindGameObjectWithTag("Bird");
        helperPointer = FindObjectOfType<HelperPointer>();
        InitializeGameObjects();

        // Make sure the first object is active
        if (gameObjects.Length > 0)
        {
            ActivateNextObject(0);  // Activate the first object
        }

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
        kikiAudio.Play();  // Play the audio
        yield return new WaitUntil(() => !kikiAudio.isPlaying);  // Wait until the audio finishes playing

        allDone = true;  // Set allDone to true after audio finishes
        Debug.Log("All items dropped, and kikiAudio finished playing.");
    }

    // Play feedback and advance to the next object
    private IEnumerator PlayFeedbackAndAdvance(bool isCorrectDrop)
    {
        if (isCorrectDrop)
        {
            // Randomly select one of the two positive feedback audios
            int randomIndex = Random.Range(0, 2);  // Select either rightDrop1 or rightDrop2
            feedbackAudioSource.clip = feedbackClips[randomIndex];
            birdAnimator.SetTrigger(feedbackTriggers[randomIndex]);  // Trigger corresponding feedback animation
            boyAnimator.SetTrigger("rightDrop");

            feedbackAudioSource.Play();
            yield return new WaitUntil(() => !feedbackAudioSource.isPlaying);

            // Play the object-specific audio after feedback finishes
            PlayObjectAudio(totalCorrectDrops);

            // Enable the next object
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
            yield return new WaitUntil(() => !feedbackAudioSource.isPlaying);
        }
    }

    // Activate the next object by enabling its collider
    private void ActivateNextObject(int index)
    {
        if (index < gameObjects.Length)
        {
            gameObjects[index].GetComponent<Collider2D>().enabled = true;
            Debug.Log($"Activated {gameObjects[index].name} for dragging.");
        }
    }

    // Play the audio associated with the current object based on totalCorrectDrops
    public void PlayObjectAudio(int index)
    {
        if (index < objectAudioClips.Length && audioSource != null)
        {
            audioSource.clip = objectAudioClips[index];
            audioSource.Play();
            birdAnimator.SetTrigger(triggerNames[index]);  // Trigger the animation for the current object
            Debug.Log($"Playing audio for {triggerNames[index]}");

            // Display the subtitle for the respective object
            if (index < subtitles.Length)
            {
                StartCoroutine(RevealTextWordByWord(subtitles[index], delayBetweenWords));
            }
        }
        else
        {
            Debug.LogWarning("Invalid index or audioSource not set.");
        }
    }

    // Coroutine to reveal text word by word
    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";  // Clear the text before starting
        subtitleText.gameObject.SetActive(true);  // Ensure the subtitle text is active

        string[] words = fullText.Split(' ');  // Split the full text into individual words

        // Reveal words one by one
        for (int i = 0; i < words.Length; i++)
        {
            subtitleText.text = string.Join(" ", words, 0, i + 1);  // Show only the words up to the current index
            yield return new WaitForSeconds(delayBetweenWords);  // Wait before revealing the next word
        }

        // Hide the subtitle after the full text is shown
        subtitleText.gameObject.SetActive(false);
    }

    public void OnWrongDrop()
    {
        Debug.Log("Wrong drop made. Playing wrong feedback.");
        StartCoroutine(PlayFeedbackAndAdvance(false));
    }

    public void FinishLevelAfterDelay()
    {
        allDone = true;
    }
}
