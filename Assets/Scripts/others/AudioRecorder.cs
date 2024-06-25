using UnityEngine;
using System.Collections;

public class AudioRecorder : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioClip recordedClip;
    private bool isRecording = false;
    private float recordLength = 5f; // Length of the audio recording in seconds

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    // Call this method when the record button is clicked
    public void StartRecording()
    {
        if (Microphone.IsRecording(null))
        {
            Debug.Log("Already recording...");
            return;
        }

        isRecording = true;
        recordedClip = Microphone.Start(null, false, Mathf.CeilToInt(recordLength), 44100);
        StartCoroutine(StopRecordingAfterTime(recordLength));
    }

    // Coroutine to stop recording after a set duration
    IEnumerator StopRecordingAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        if (isRecording)
        {
            Microphone.End(null);
            isRecording = false;
            Debug.Log("Recording stopped.");
        }
    }

    // Call this method when the play button is clicked
    public void PlayRecording()
    {
        if (recordedClip == null)
        {
            Debug.Log("No recording to play.");
            return;
        }

        audioSource.clip = recordedClip;
        audioSource.Play();
        Debug.Log("Playing the recorded audio.");
    }
}
