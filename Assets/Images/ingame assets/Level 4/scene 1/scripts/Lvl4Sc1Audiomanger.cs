using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lvl4Sc1Audiomanger : MonoBehaviour
{
    private AudioSource audioSource;

    private void Start()
    {
        // Get the AudioSource component attached to this GameObject
        audioSource = GetComponent<AudioSource>();

        // Check if the AudioSource component exists
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource component missing on this GameObject. Please add one.");
        }
    }

    // Public method to play an audio clip by name
    public void PlayAudio(AudioClip clip)
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is not set up correctly.");
            return;
        }

        if (clip == null)
        {
            Debug.LogError("AudioClip is null. Please provide a valid AudioClip.");
            return;
        }

        // Play the provided audio clip
        audioSource.clip = clip;
        audioSource.Play();
    }
}
