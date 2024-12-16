using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    public GameObject settingsPanel; // Reference to the Settings Panel
    public Button soundButton; // Reference to the Sound Button
    public Button closeButton; // Reference to the Close Button
    public Sprite sprite1; // Default sprite for "Sound On"
    public Sprite sprite2; // Alternate sprite for "Sound Off"
    private bool soundOn = true; // Tracks the sound state (initially true)

    void Start()
    {
        // Assign listeners for button clicks
        soundButton.onClick.AddListener(ToggleSound);
        closeButton.onClick.AddListener(CloseSettings);
    }

    void ToggleSound()
    {
        soundOn = !soundOn; // Toggle the sound state
        if (soundOn)
        {
            soundButton.image.sprite = sprite1; // Set to sprite1 for "Sound On"
            Debug.Log("Sound is ON");
        }
        else
        {
            soundButton.image.sprite = sprite2; // Set to sprite2 for "Sound Off"
            Debug.Log("Sound is OFF");
        }
    }

    void CloseSettings()
    {
        settingsPanel.SetActive(false); // Deactivate the settings panel
        Debug.Log("Settings panel closed");
    }
}
