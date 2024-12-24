using UnityEngine;

public class BGAudioManager_Final : MonoBehaviour
{
    public static BGAudioManager_Final Instance; // Singleton instance
    public AudioClip audioClip1; // Learn to Speak
    public AudioClip audioClip2; // Following Directions
    public AudioClip audioClip3; // Learn Words
    public AudioClip audioClip4; // Learn Sentences
    private AudioSource audioSource;
    public bool TurnonVolume = true; // Boolean to track if volume is on

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("AudioSource component missing on BGAudioManager!");
            }
        }
        else
        {
            Destroy(gameObject); // Enforce Singleton pattern
        }
    }

    public void SetVolume(bool state)
    {
        TurnonVolume = state;
        audioSource.volume = TurnonVolume ? 1.0f : 0.0f; // Adjust volume directly
    }

    public bool IsVolumeEnabled()
    {
        return TurnonVolume; // Provides global access to the volume state
    }

    public void PlayAudioForCategory(int category)
    {
        if (audioSource == null) return;

        if (!TurnonVolume)
        {
            Debug.Log("Volume is off, audio playback disabled.");
            return;
        }

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

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

        audioSource.Play();
    }
}
