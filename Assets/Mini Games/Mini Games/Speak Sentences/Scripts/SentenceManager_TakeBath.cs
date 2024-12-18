using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SentenceManager_TakeBath : MonoBehaviour
{
    public Animator objectAnimator; // Animator for the object
    public AudioClip[] wordAudioClips; // Audio clips for each word
    public AudioClip AmbientSounds; // Crocodile or specific animal sound
    public Button[] wordButtons; // Buttons for the words
    public AudioSource audioSource; // AudioSource to play audio
    public float buttonCooldown; // Cooldown time in seconds
    public ParticleSystem actionParticleEffect; // Particle system to play during "Action"

    private bool isIdle = false;
    private bool spawnFinished = false; // Flag to check if spawn is finished

    void Start()
    {
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
        // Check if the spawn animation has finished
        AnimatorStateInfo stateInfo = objectAnimator.GetCurrentAnimatorStateInfo(0);

        // Transition to idle if the spawn animation has finished
        if (stateInfo.IsName("Spawn") && stateInfo.normalizedTime >= 1f && !objectAnimator.IsInTransition(0))
        {
            spawnFinished = true; // Set flag indicating spawn is finished
            isIdle = true; // Set to idle state
            objectAnimator.SetTrigger("Idle"); // Transition to idle animation
        }

        // Handle the particle effect when in "Action" state
        if (stateInfo.IsName("Action"))
        {
            if (!actionParticleEffect.isPlaying)
            {
                actionParticleEffect.Play(); // Play particle effect if not already playing
            }
        }
        else if (stateInfo.IsName("Idle") && actionParticleEffect.isPlaying)
        {
            actionParticleEffect.Stop(); // Stop particle effect when in Idle state
        }
    }

    // This method will be called when the object is clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        OnCharacterClick();
    }

    // Coroutine to play word audio with a cooldown for the button
    private IEnumerator PlayWordAudioWithCooldown(int index)
    {
        SetButtonsInteractable(false); // Disable all buttons

        if (index == 2)
        {
            // Button 3: play animal sound and then full sentence
            yield return PlayActionAndWords();
        }
        else
        {
            // For button 1 and 2: play individual or paired clips
            yield return PlayWordAudioWithCooldownForButton(index);
        }

        SetButtonsInteractable(true); // Re-enable buttons after playback
    }

    // Coroutine to handle the cooldown and audio playback for button 1 and 2
    private IEnumerator PlayWordAudioWithCooldownForButton(int index)
    {
        if (index == 0)
        {
            PlayAudioClip(wordAudioClips[0]);
        }
        else if (index == 1)
        {
            yield return StartCoroutine(PlayMultipleAudioClips(new AudioClip[] { wordAudioClips[0], wordAudioClips[1] }));
        }

        // Wait for the cooldown after both clips have finished
        yield return new WaitForSeconds(buttonCooldown);
    }

    // Play the corresponding audio clip when a word button is clicked
    public void PlayWordAudio(int index)
    {
        if (index == 0)
        {
            PlayAudioClip(wordAudioClips[0]);
        }
        else if (index == 1)
        {
            StartCoroutine(PlayMultipleAudioClips(new AudioClip[] { wordAudioClips[0], wordAudioClips[1] }));
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

    // Method to play the action animation, animal sound, and the individual word audio clips in succession
    public IEnumerator PlayActionAndWords()
    {
        isIdle = false; // Set idle to false while playing action

        objectAnimator.SetTrigger("Action");

        // Play the animal sound first
        if (AmbientSounds != null)
        {
            audioSource.clip = AmbientSounds;
            audioSource.Play();
            yield return new WaitForSeconds(AmbientSounds.length);
        }

        // Play the words in sequence
        yield return StartCoroutine(PlayMultipleAudioClips(new AudioClip[] { wordAudioClips[0], wordAudioClips[1], wordAudioClips[2] }));

        AnimatorStateInfo stateInfo = objectAnimator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.IsName("Action") || objectAnimator.IsInTransition(0))
        {
            yield return null; // Wait until the action animation is finished
            stateInfo = objectAnimator.GetCurrentAnimatorStateInfo(0); // Update state info
        }

        isIdle = true; // Set idle to true after the sequence finishes
        objectAnimator.SetTrigger("Idle"); // Ensure to transition back to idle animation

        // Re-enable the buttons after the entire sequence finishes
        SetButtonsInteractable(true);
    }

    // Helper method to set buttons interactable
    private void SetButtonsInteractable(bool isInteractable)
    {
        foreach (Button button in wordButtons)
        {
            button.interactable = isInteractable;
        }
    }

    public void OnCharacterClick()
    {
        if (isIdle) // Ensure it's in idle state before restarting the action
        {
            StartCoroutine(PlayActionAndWords());
        }
    }
}
