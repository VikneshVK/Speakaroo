using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SentenceManager_On_Off : MonoBehaviour
{
    [Header("On buttons")]
    public Button OnButton1;
    public Button OnButton2;

    [Header("Off buttons")]
    public Button OffButton1;
    public Button OffButton2;

    [Header("Audio ref")]
    public AudioClip LightOnAudioClip; // Audio clip for turning on lights
    public AudioClip LightOffAudioClip; // Audio clip for turning off lights
    public AudioSource L_audioSource; // AudioSource to play audio

    [Header("Backgrounds")]
    public GameObject LightsOn_BG; // Lights On GameObject
    public GameObject LightsOff_BG; // Lights Off GameObject

    private void Start()
    {
        // Set initial state to Lights Off
        StartCoroutine(InitializeLightsOffState());
    }

    private IEnumerator InitializeLightsOffState()
    {
        SetInteractable(false); // Disable buttons during initialization

        // Add a small delay before playing the audio to avoid abrupt starts
        yield return new WaitForSeconds(0.1f);

        // Play Lights Off audio
        PlayAudio(LightOffAudioClip);

        // Wait for the audio to finish
        yield return new WaitForSeconds(LightOffAudioClip.length);

        // Set the background to Lights Off
        SetLightsState(false);

        SetInteractable(true); // Re-enable buttons after initialization
    }

    private IEnumerator ToggleLight(bool isOn)
    {
        SetInteractable(false); // Disable buttons during the action

        if (isOn)
        {
            yield return StartCoroutine(OnSwitch());
        }
        else
        {
            yield return StartCoroutine(OffSwitch());
        }

        SetInteractable(true); // Re-enable buttons after action
    }

    private void PlayAudio(AudioClip clip)
    {
        if (clip != null && L_audioSource != null)
        {
            L_audioSource.clip = clip;
            L_audioSource.Play();
        }
    }

    private void SetInteractable(bool interactable)
    {
        OnButton1.interactable = interactable;
        OnButton2.interactable = interactable;
        OffButton1.interactable = interactable;
        OffButton2.interactable = interactable;
    }

    private void SetLightsState(bool isOn)
    {
        // Enable or disable Lights On and Lights Off GameObjects
        LightsOn_BG.SetActive(isOn);
        LightsOff_BG.SetActive(!isOn); // If it's on, turn off the "Lights Off" background
    }

    private IEnumerator OnSwitch()
    {
        // Play Lights On audio and show the lights on GameObject
        PlayAudio(LightOnAudioClip);
        yield return new WaitForSeconds(LightOnAudioClip.length);
        SetLightsState(true); // Lights are ON
    }

    private IEnumerator OffSwitch()
    {
        // Play Lights Off audio and show the lights off GameObject
        PlayAudio(LightOffAudioClip);
        yield return new WaitForSeconds(LightOffAudioClip.length);
        SetLightsState(false); // Lights are OFF
    }

    // Add functions to be called from buttons
    public void TurnOnLight()
    {
        StartCoroutine(ToggleLight(true));
    }

    public void TurnOffLight()
    {
        StartCoroutine(ToggleLight(false));
    }
}
