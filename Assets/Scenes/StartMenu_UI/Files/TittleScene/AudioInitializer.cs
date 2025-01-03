using System.Runtime.InteropServices;
using UnityEngine;

public class AudioInitializer : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void SetAudioToLoudSpeaker();

    void Awake()
    {
        // Set the audio route to loudspeaker at the start
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            SetAudioToLoudSpeaker();
        }
    }
}
