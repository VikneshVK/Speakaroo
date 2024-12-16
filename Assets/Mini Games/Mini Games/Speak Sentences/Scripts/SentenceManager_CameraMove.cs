using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SentenceManager_CameraMove : MonoBehaviour
{
    public Animator objectAnimator; // Animator for the object
    public AudioClip[] wordAudioClips; // Audio clips for each word
    public AudioClip AmbientSounds; // Crocodile or specific animal sound
    public Button[] wordButtons; // Buttons for the words
    public AudioSource audioSource; // AudioSource to play audio
    public float buttonCooldown; // Cooldown time in seconds

    // Background Movement
    public RectTransform backgroundTransform; // RectTransform of the first background
    public GameObject backgroundPrefab; // Prefab for duplicating the background
    public float backgroundScrollSpeed = 100f; // Speed of the background movement
    public Vector2 backgroundSize; // Size of the background for infinite scrolling
    private RectTransform currentBackground;

    private bool isIdle = false; // Tracks whether the object is idle
    private bool isActionState = false; // Tracks if the animator is in the "Action" state

    void Start()
    {
        backgroundSize = backgroundTransform.sizeDelta;
        currentBackground = backgroundTransform; // Initial background

        // Ensure the backgroundTransform is assigned correctly in the Inspector.
        if (backgroundTransform == null)
        {
            Debug.LogError("Background Transform not assigned.");
            return;
        }

        // Get the background size from the RectTransform of the background
        backgroundSize = backgroundTransform.sizeDelta;
        Debug.Log($"Background Size: {backgroundSize}");

        // Disable word buttons at the start
        SetButtonsInteractable(false);

        // Start by playing the spawn animation
        objectAnimator.SetTrigger("Spawn");
        StartCoroutine(PlayActionAndWords()); // Play the action and words automatically

        // Add listeners to word buttons
        for (int i = 0; i < wordButtons.Length; i++)
        {
            int index = i; // Capture the index for the listener
            wordButtons[i].onClick.AddListener(() => StartCoroutine(PlayWordAudioWithCooldown(index)));
        }
    }

    void Update()
    {
        ScrollBackground();

        AnimatorStateInfo stateInfo = objectAnimator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Action") && !objectAnimator.IsInTransition(0))
        {
            isActionState = true;
            
        }
        else
        {
            isActionState = false;
        }
    }

    // This method will be called when the object is clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        OnCharacterClick();
    }

    private IEnumerator PlayWordAudioWithCooldown(int index)
    {
        SetButtonsInteractable(false);

        if (index == 2)
        {
            yield return PlayActionAndWords();
        }
        else
        {
            if (index == 0)
            {
                PlayAudioClip(wordAudioClips[0]);
            }
            else if (index == 1)
            {
                yield return PlayMultipleAudioClips(new AudioClip[] { wordAudioClips[0], wordAudioClips[1] });
            }
            yield return new WaitForSeconds(buttonCooldown);
        }

        SetButtonsInteractable(true);
    }

    public IEnumerator PlayActionAndWords()
    {
        isIdle = false; // Set idle to false
        objectAnimator.SetTrigger("Action");

        if (AmbientSounds != null)
        {
            audioSource.clip = AmbientSounds;
            audioSource.Play();
            yield return new WaitForSeconds(AmbientSounds.length);
        }

        yield return PlayMultipleAudioClips(wordAudioClips);

        isIdle = true;
        objectAnimator.SetTrigger("Idle");
        SetButtonsInteractable(true);
    }

    public void PlayAudioClip(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    private IEnumerator PlayMultipleAudioClips(AudioClip[] clips)
    {
        foreach (var clip in clips)
        {
            ScrollBackground();
            PlayAudioClip(clip);
            yield return new WaitForSeconds(clip.length);

        }
    }

    private void ScrollBackground()
    {
        Vector2 newPosition = backgroundTransform.anchoredPosition;

        // Move the background horizontally
        newPosition.x -= backgroundScrollSpeed * Time.deltaTime;

        // Update the position of the background
        backgroundTransform.anchoredPosition = newPosition;
    }

    private void SetButtonsInteractable(bool isInteractable)
    {
        foreach (var button in wordButtons)
        {
            button.interactable = isInteractable;
        }
    }

    public void OnCharacterClick()
    {
        if (isIdle)
        {
            //ScrollBackground();
            StartCoroutine(PlayActionAndWords());
        }
    }

    // This method will be called by the threshold collider when the background reaches the threshold.
    public void HandleThresholdTrigger()
    {
        // Instantiate a new background prefab
        GameObject newBackground = Instantiate(backgroundPrefab);

        // Get the RectTransform component of the new background
        RectTransform newBgTransform = newBackground.GetComponent<RectTransform>();

        // Set the new background's parent to match the original background's parent
        newBgTransform.SetParent(backgroundTransform.parent);

        // Position the new background to the right of the current background
        newBgTransform.anchoredPosition = new Vector2(backgroundSize.x, 0);

        // Destroy the current background (the one that's moving out of view)
        Destroy(currentBackground.gameObject);

        // Update the reference to the new background
        currentBackground = newBgTransform;
    }
}
