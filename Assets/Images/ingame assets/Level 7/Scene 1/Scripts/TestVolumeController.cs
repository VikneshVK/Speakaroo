using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;

public class TestVolumeController : MonoBehaviour
{
    // References to Sliders
    public Slider musicVolumeSlider;
    public Slider ambientVolumeSlider;
    public Slider dialogueVolumeSlider;

    // Reference to AudioMixer
    public AudioMixer speakarooMixer;

    // References to TextMeshProUGUI components for volume displays
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI ambientVolumeText;
    public TextMeshProUGUI dialogueVolumeText;

    // dB Range for Sliders
    public const float minDb = -80f;
    public const float maxDb = 20f;

    private void Start()
    {
        // Ensure all references are assigned in the Inspector
        if (!musicVolumeSlider || !ambientVolumeSlider || !dialogueVolumeSlider ||
            !speakarooMixer || !musicVolumeText || !ambientVolumeText || !dialogueVolumeText)
        {
            Debug.LogError("One or more references are not assigned in the Inspector.");
            return;
        }

        // Initialize slider values based on current mixer settings
        InitializeSliders();

        // Add listeners to sliders to update mixer volumes and text displays on change
        musicVolumeSlider.onValueChanged.AddListener(UpdateMusicVolume);
        ambientVolumeSlider.onValueChanged.AddListener(UpdateAmbientVolume);
        dialogueVolumeSlider.onValueChanged.AddListener(UpdateDialogueVolume);
    }

    private void InitializeSliders()
    {
        // Get current volumes from the mixer and apply to sliders and texts
        float musicVol; speakarooMixer.GetFloat("MusicVolume", out musicVol);
        float ambientVol; speakarooMixer.GetFloat("AmbientVolume", out ambientVol);
        float dialogueVol; speakarooMixer.GetFloat("DialogueVolume", out dialogueVol);

        // Map dB to Slider value (0 to 1)
        musicVolumeSlider.value = MapDbToSlider(musicVol);
        ambientVolumeSlider.value = MapDbToSlider(ambientVol);
        dialogueVolumeSlider.value = MapDbToSlider(dialogueVol);

        UpdateTextDisplays(musicVol, ambientVol, dialogueVol);
    }

    private void UpdateMusicVolume(float value)
    {
        // Map Slider value (0 to 1) to dB
        float dbValue = MapSliderToDb(value);
        speakarooMixer.SetFloat("MusicVolume", dbValue);
        UpdateTextDisplay(musicVolumeText, "Music Volume", dbValue);
    }

    private void UpdateAmbientVolume(float value)
    {
        // Map Slider value (0 to 1) to dB
        float dbValue = MapSliderToDb(value);
        speakarooMixer.SetFloat("AmbientVolume", dbValue);
        UpdateTextDisplay(ambientVolumeText, "Ambient Volume", dbValue);
    }

    private void UpdateDialogueVolume(float value)
    {
        // Map Slider value (0 to 1) to dB
        float dbValue = MapSliderToDb(value);
        speakarooMixer.SetFloat("DialogueVolume", dbValue);
        UpdateTextDisplay(dialogueVolumeText, "Dialogue Volume", dbValue);
    }

    private void UpdateTextDisplay(TextMeshProUGUI textComponent, string volumeType, float dbValue)
    {
        textComponent.text = $"{volumeType}: {dbValue:F1} dB"; // Display with one decimal place
    }

    private void UpdateTextDisplays(float musicVol, float ambientVol, float dialogueVol)
    {
        UpdateTextDisplay(musicVolumeText, "Music Volume", musicVol);
        UpdateTextDisplay(ambientVolumeText, "Ambient Volume", ambientVol);
        UpdateTextDisplay(dialogueVolumeText, "Dialogue Volume", dialogueVol);
    }

    // Helper function to map dB to Slider value (0 to 1)
    private float MapDbToSlider(float dbValue)
    {
        return (dbValue - minDb) / (maxDb - minDb);
    }

    // Helper function to map Slider value (0 to 1) to dB
    private float MapSliderToDb(float sliderValue)
    {
        return Mathf.Lerp(minDb, maxDb, sliderValue);
    }

    private void OnDestroy()
    {
        // Clean up listeners to prevent errors if this script is disabled/enabled at runtime
        musicVolumeSlider.onValueChanged.RemoveAllListeners();
        ambientVolumeSlider.onValueChanged.RemoveAllListeners();
        dialogueVolumeSlider.onValueChanged.RemoveAllListeners();
    }
}
