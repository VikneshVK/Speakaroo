using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SentenceManager_Numbers : MonoBehaviour
{

    [Header("UI Object Configuration")]
    public RectTransform uiObject;       // UI object to be tweened in (must be set as a UI RectTransform)
    public Vector3 targetPosition;       // Target position where the UI object should tween to

    [Header("Animation Settings")]
    public Animator objectAnimator;      // Animator component for handling Idle and Action states

    [Header("Audio and UI Elements")]
    public AudioClip[] wordAudioClips;   // Array of audio clips for each word
    public Button[] wordButtons;         // Buttons that trigger each word's audio (match array length with wordAudioClips)
    public AudioSource audioSource;      // AudioSource for playing word audio

    private bool isIdle = false;         // Flag to check if the object is in the idle state

    void Start()
    {
        // Set initial position above screen for tween effect
        uiObject.position = new Vector3(targetPosition.x, Screen.height + uiObject.rect.height, targetPosition.z);

        // Tween the object from above to the target position
        LeanTween.move(uiObject, targetPosition, 1.5f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() =>
        {
            // Once tween is complete, enter Idle state
            isIdle = true;
            objectAnimator.SetTrigger("Idle");
            StartCoroutine(PlayActionAndWords());
        });

        // Add listeners to each word button for audio playback
        for (int i = 0; i < wordButtons.Length; i++)
        {
            int index = i; // Capture the index for the listener
            wordButtons[i].onClick.AddListener(() => PlayWordAudio(index));
        }
    }

    // This method will be called when the UI object is clicked
    private void OnMouseDown()
    {
        if (isIdle) // Check if it's in the Idle state
        {
            StartCoroutine(PlayActionAndWords()); // Trigger action and audio sequence
        }
    }

    // Play the corresponding audio clip when a word button is clicked
    public void PlayWordAudio(int index)
    {
        if (index == 0)
        {
            PlayAudioClip(wordAudioClips[0]); // Play only the first word audio (e.g., "Hippo")
        }
        else if (index == 1)
        {
            StartCoroutine(PlayMultipleAudioClips(new AudioClip[] { wordAudioClips[0], wordAudioClips[1] }));
        }
        else if (index == 2)
        {
            StartCoroutine(PlayActionAndWords()); // Play the action and third word
        }
    }

    // Method to play an individual audio clip
    public void PlayAudioClip(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    // Coroutine to play multiple audio clips in sequence
    private IEnumerator PlayMultipleAudioClips(AudioClip[] clips)
    {
        foreach (AudioClip clip in clips)
        {
            audioSource.clip = clip;
            audioSource.Play();
            yield return new WaitForSeconds(clip.length); // Wait until the current clip finishes
        }
    }

    // Method to play the action animation and word audio clips in succession
    private IEnumerator PlayActionAndWords()
    {
        isIdle = false; // Set idle to false during action state
        objectAnimator.SetTrigger("Action"); // Trigger action animation

        // Play word audio clips in succession
        yield return StartCoroutine(PlayMultipleAudioClips(new AudioClip[] { wordAudioClips[0], wordAudioClips[1], wordAudioClips[2] }));

        // Wait until the Action animation finishes
        AnimatorStateInfo stateInfo = objectAnimator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.IsName("Action") || objectAnimator.IsInTransition(0))
        {
            yield return null;
            stateInfo = objectAnimator.GetCurrentAnimatorStateInfo(0); // Update state info
        }

        // Return to idle state
        isIdle = true;
        objectAnimator.SetTrigger("Idle");
    }

    public void playAll()
    {
        StartCoroutine(PlayActionAndWords());
    }
}
