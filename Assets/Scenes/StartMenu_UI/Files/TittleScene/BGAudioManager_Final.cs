using UnityEngine;

public class BGAudioManager_Final : MonoBehaviour
{
    public AudioClip audioClip1; // Learn to Speak
    public AudioClip audioClip2; // Following Directions
    public AudioClip audioClip3; // Learn Words
    public AudioClip audioClip4; // Learn Sentences
    private AudioSource audioSource;

    void Awake()
    {
        // Make this object persistent
        DontDestroyOnLoad(gameObject);

        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component missing on BGAudioManager!");
        }
    }

    public void PlayAudioForCategory(int category)
    {
        if (audioSource == null) return;

        // Stop the current audio if playing
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Select and play audio based on the category
        switch (category)
        {
            case 1:
                audioSource.clip = audioClip1;
                break;
            case 2:
                audioSource.clip = audioClip2;
                break;
            case 3:
                audioSource.clip = audioClip3;
                break;
            case 4:
                audioSource.clip = audioClip4;
                break;
            default:
                Debug.LogWarning("Invalid category for audio playback!");
                return;
        }

        // Play the new audio
        audioSource.Play();
    }
}
