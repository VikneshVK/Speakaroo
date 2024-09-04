using UnityEngine;
using System.Collections;

public class dragManager : MonoBehaviour
{
    public static int totalCorrectDrops;
    public AudioSource audioSource;  // The AudioSource attached to the dragManager GameObject
    public AudioSource feedbackAudioSource;  // The AudioSource attached to the InteractionAudio GameObject
    public AudioClip[] objectAudioClips;  // Array to hold the audio clips for each game object

    public Animator birdAnimator;
    public GameObject[] gameObjects;
    public string[] triggerNames = { "bagTalk", "sheetTalk", "rugTalk", "pillowTalk", "shoeTalk", "teddyTalk" };
    public string allDoneBool = "allDone";

    private HelperPointer helperPointer;
    private GameObject parrot;

    void Start()
    {
        parrot = GameObject.FindGameObjectWithTag("Bird");
        helperPointer = FindObjectOfType<HelperPointer>();
        InitializeGameObjects();
        if (gameObjects.Length > 0)
        {
            var firstDragHandler = gameObjects[0].GetComponent<DragHandler>();
            if (firstDragHandler != null && helperPointer != null && gameObjects[0].GetComponent<Collider2D>().enabled)
            {
                helperPointer.ScheduleHelperHand(firstDragHandler, this);
            }
        }
        totalCorrectDrops = 0;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectParrotClick();
        }
    }

    void InitializeGameObjects()
    {
        foreach (GameObject obj in gameObjects)
        {
            obj.GetComponent<Collider2D>().enabled = false;
            obj.GetComponent<DragHandler>().dragManager = this;
        }
        if (gameObjects.Length > 0)
        {
            gameObjects[0].GetComponent<Collider2D>().enabled = true;
        }

        // Ensure no triggers are active at the start
        birdAnimator.ResetTrigger("bagTalk");
        birdAnimator.ResetTrigger("sheetTalk");
        birdAnimator.ResetTrigger("rugTalk");
        birdAnimator.ResetTrigger("pillowTalk");
        birdAnimator.ResetTrigger("shoeTalk");
        birdAnimator.ResetTrigger("teddyTalk");
        birdAnimator.SetBool(allDoneBool, false);
    }

    public void OnItemDropped()
    {
        totalCorrectDrops++;

        // Set the audio clip based on the current object
        if (totalCorrectDrops < objectAudioClips.Length)
        {
            audioSource.clip = objectAudioClips[totalCorrectDrops];
        }

        // Now play the feedback audio and then play the next audio
        StartCoroutine(PlayManagerAudioAfterFeedback());

        Debug.Log("Total Correct Drops: " + totalCorrectDrops);

        if (totalCorrectDrops < gameObjects.Length)
        {
            birdAnimator.SetTrigger(triggerNames[totalCorrectDrops]);
        }
        else
        {
            birdAnimator.SetBool(allDoneBool, true);
        }
    }

    private IEnumerator PlayManagerAudioAfterFeedback()
    {
        // Play feedback audio from InteractionAudio GameObject
        if (feedbackAudioSource != null && feedbackAudioSource.clip != null)
        {
            feedbackAudioSource.Play();
            Debug.Log("Playing feedback audio from InteractionAudio.");
        }
        else
        {
            Debug.LogWarning("feedbackAudioSource or clip is not set.");
        }

        // Wait until the feedback audio finishes playing
        yield return new WaitUntil(() => !feedbackAudioSource.isPlaying);

        // Play the next audio from the dragManager's AudioSource after the feedback audio has completed
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
            Debug.Log("Playing dragManager audio after feedback audio.");

            // Trigger the animation associated with the current totalCorrectDrops index
            OnTriggerActivated(triggerNames[totalCorrectDrops]);
        }
        else
        {
            Debug.LogWarning("dragManager audioSource or clip is not set.");
        }
    }

    public void OnTriggerActivated(string triggerName)
    {
        int index = System.Array.IndexOf(triggerNames, triggerName);
        if (index >= 0 && index < gameObjects.Length)
        {
            AnimateGameObject(gameObjects[index]);
        }
    }

    void DetectParrotClick()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D collider = Physics2D.OverlapPoint(mousePosition);

        if (collider != null && collider.gameObject == parrot)
        {
            OnParrotClicked();
        }
    }

    public void OnParrotClicked()
    {
        // Logic when the parrot is clicked
        if (birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle0"))
        {
            if (totalCorrectDrops < triggerNames.Length)
            {
                AnimateGameObject(gameObjects[totalCorrectDrops]);
                birdAnimator.SetTrigger(triggerNames[totalCorrectDrops]);
                PlayAudio(totalCorrectDrops);  // Play the audio for the current object
            }
        }
    }

    public void PlayAudio(int index)
    {
        if (index < objectAudioClips.Length && audioSource != null)
        {
            audioSource.clip = objectAudioClips[index];
            audioSource.Play();
            Debug.Log($"Playing audio for object {index}: {objectAudioClips[index].name}");
        }
        else
        {
            Debug.LogWarning("Invalid audio index or audioSource not set.");
        }
    }

    void AnimateGameObject(GameObject obj)
    {
        Vector3 originalScale = obj.transform.localScale;
        Vector3 targetScale = originalScale + new Vector3(0.35f, 0.35f, 0.35f);
        LeanTween.scale(obj, targetScale, 0.5f).setEaseInOutQuad().setOnComplete(() =>
        {
            LeanTween.scale(obj, originalScale, 0.5f).setEaseInOutQuad();
        });
    }
}
