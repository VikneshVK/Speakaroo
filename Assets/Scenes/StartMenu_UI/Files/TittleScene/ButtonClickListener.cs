using UnityEngine;
using UnityEngine.UI;

public class ButtonClickListener : MonoBehaviour
{
    public Scene_Manager_Final sceneManager;

    // This will be assigned from the Inspector
    public string sceneName;
    public int sceneCategory;

    private AudioSource audioSource;

    void Start()
    {
        // Check if an AudioSource component exists; if not, add one
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure the AudioSource
        audioSource.playOnAwake = false;

        // Load the "Button Click" audio clip from Resources/Audio
        AudioClip buttonClickClip = Resources.Load<AudioClip>("Audio/Button Click");
        if (buttonClickClip != null)
        {
            audioSource.clip = buttonClickClip;
        }
        else
        {
            Debug.LogWarning("Audio clip 'Button Click' not found in Resources/Audio.");
        }
    }

    public void OnButtonClick()
    {
        // Play the audio clip when the button is clicked
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }

        // Call the LoadLevel function
        sceneManager.LoadLevel(sceneName, sceneCategory);
    }
}
