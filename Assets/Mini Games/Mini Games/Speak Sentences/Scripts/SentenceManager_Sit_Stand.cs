using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SentenceManager_Sit_Stand : MonoBehaviour
{
    public Animator objectAnimator; // Animator for the object
    public AudioClip[] leftPhraseClips; // Audio clips for left-side phrases
    public AudioClip[] rightPhraseClips; // Audio clips for right-side phrases
    public AudioClip sitAudioClip; // Audio clip for sit state
    public AudioClip standAudioClip; // Audio clip for stand state
    public AudioSource audioSource; // AudioSource to play audio
    public Button[] leftPhraseButtons; // Buttons for the left-side phrases
    public Button[] rightPhraseButtons; // Buttons for the right-side phrases

    private bool isSitting = true; // Tracks the character's current state (sitting or standing)
    private bool isIdle = false; // Tracks whether the character is idle

    void Start()
    {
        // Add listeners to left-side buttons
        foreach (var button in leftPhraseButtons)
        {
            button.onClick.AddListener(() => StartCoroutine(PlayPhraseWithAnimation(true)));
        }

        // Add listeners to right-side buttons
        foreach (var button in rightPhraseButtons)
        {
            button.onClick.AddListener(() => StartCoroutine(PlayPhraseWithAnimation(false)));
        }

        // Default idle state and play the sit audio and left phrase
        StartCoroutine(PlayInitialSitState());
    }

    // Coroutine to play the sit audio and full left phrase at the start
    private IEnumerator PlayInitialSitState()
    {
        SetButtonsInteractable(false); // Disable buttons during initial playback

        // Play sit state audio
        PlaySitStateAudio();

        // Wait for sit audio to finish
        if (sitAudioClip != null)
        {
            yield return new WaitForSeconds(sitAudioClip.length);
        }

        // Play the full left phrase
        yield return PlayPhrase(leftPhraseClips);

        SetButtonsInteractable(true); // Re-enable buttons after playback
    }


    void Update()
    {
        // Ensure idle state is updated correctly
        AnimatorStateInfo stateInfo = objectAnimator.GetCurrentAnimatorStateInfo(0);
        if (!objectAnimator.IsInTransition(0))
        {
            isIdle = stateInfo.IsName("Idle");
        }
    }

    // Coroutine to handle phrase button audio and animations
    private IEnumerator PlayPhraseWithAnimation(bool isLeft)
    {
        SetButtonsInteractable(false); // Disable all buttons

        if (isLeft)
        {
            if (!isSitting)
            {
                PlaySitStateAudio();
                objectAnimator.SetTrigger("Sit");
                isSitting = true;
            }
            yield return PlayPhrase(leftPhraseClips);
        }
        else
        {
            if (isSitting)
            {
                PlayStandStateAudio();
                objectAnimator.SetTrigger("Stand");
                isSitting = false;
            }
            yield return PlayPhrase(rightPhraseClips);
        }

        // After playback, set the final idle state
        objectAnimator.SetTrigger("Idle");
        SetButtonsInteractable(true); // Re-enable buttons after playback
    }

    // Play a set of phrases sequentially
    private IEnumerator PlayPhrase(AudioClip[] phraseClips)
    {
        foreach (var clip in phraseClips)
        {
            PlayAudioClip(clip);
            yield return new WaitForSeconds(clip.length); // Wait for each clip to finish
        }
    }

    // Play sit state audio
    private void PlaySitStateAudio()
    {
        PlayAudioClip(sitAudioClip);
    }

    // Play stand state audio
    private void PlayStandStateAudio()
    {
        PlayAudioClip(standAudioClip);
    }

    // Play an individual audio clip
    public void PlayAudioClip(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    // Enable or disable all buttons
    private void SetButtonsInteractable(bool isInteractable)
    {
        foreach (var button in leftPhraseButtons)
        {
            button.interactable = isInteractable;
        }
        foreach (var button in rightPhraseButtons)
        {
            button.interactable = isInteractable;
        }
    }
}
