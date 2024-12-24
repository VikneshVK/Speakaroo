using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    public GameObject settingsPanel; // Reference to the Settings Panel
    public Button soundButton; // Reference to the Sound Button
    public Button closeButton; // Reference to the Close Button
    public Sprite sprite1; // Default sprite for "Sound On"
    public Sprite sprite2; // Alternate sprite for "Sound Off"
    public AudioMixer audioMixer;
    private bool soundOn = true; // Tracks the sound state (initially true)
    private const string musicVolumeParam = "MusicVolume";

    void Start()
    {
        // Assign listeners for button clicks
        soundButton.onClick.AddListener(ToggleSound);
        closeButton.onClick.AddListener(CloseSettings);

        // Initialize the sound state based on the BGAudioManager
        if (BGAudioManager_Final.Instance != null)
        {
            soundOn = BGAudioManager_Final.Instance.TurnonVolume;
            UpdateSoundButtonSprite();
        }
    }

    void ToggleSound()
    {
        soundOn = !soundOn; // Toggle the sound state

        if (BGAudioManager_Final.Instance != null)
        {
            BGAudioManager_Final.Instance.SetVolume(soundOn); // Update the global volume state
        }

        UpdateSoundButtonSprite(); // Update the button sprite

        if (audioMixer != null)
        {
            SetMusicVolume(soundOn ? -25f : -80f); // Adjust the audio mixer volume
        }
    }


    private void UpdateSoundButtonSprite()
    {
        soundButton.image.sprite = soundOn ? sprite1 : sprite2;
        Debug.Log($"Sound is {(soundOn ? "ON" : "OFF")}");
    }

    private void SetMusicVolume(float volume)
    {
        if (audioMixer != null)
        {
            bool result = audioMixer.SetFloat(musicVolumeParam, volume); // "MusicVolume" should match the exposed parameter name
            if (!result)
            {
                Debug.LogError($"Failed to set MusicVolume to {volume}. Is the parameter exposed?");
            }
        }
        else
        {
            Debug.LogError("AudioMixer is not assigned in the Inspector.");
        }
    }

    void CloseSettings()
    {
        settingsPanel.SetActive(false); // Deactivate the settings panel
        Debug.Log("Settings panel closed");
    }
}
