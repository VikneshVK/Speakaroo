using UnityEngine;
using System.Collections;

public class AudioRecorder : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioClip recordedClip;
    private bool isRecording = false;
    private float recordLength = 5f; // Length of the audio recording in seconds
    private float highPitchFactor = 1.5f; // Higher pitch factor

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    // Call this method when you want to start recording
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

    // Call this method when you want to play the recording
    public void PlayRecording()
    {
        if (recordedClip == null)
        {
            Debug.Log("No recording to play.");
            return;
        }

        audioSource.clip = recordedClip;
        audioSource.pitch = highPitchFactor; // Set the pitch higher before playing
        audioSource.Play();
        Debug.Log("Playing the recorded audio at a higher pitch.");
    }
    public void DestroyGameObject()
    {
        GameObject targetObject = GameObject.Find("ST_Mechanics");
        if (targetObject != null)
        {
            Destroy(targetObject);
            Debug.Log("ST_Mechanics destroyed.");
        }
        else
        {
            Debug.Log("ST_Mechanics not found.");
        }
    }
}
