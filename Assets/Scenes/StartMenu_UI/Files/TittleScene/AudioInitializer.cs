using System.Runtime.InteropServices;
using UnityEngine;

public class AudioInitializer : MonoBehaviour
{
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void SetAudioToLoudSpeaker();
#endif

    void Awake()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS
            SetAudioToLoudSpeaker();
#endif
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            // Implement Android-specific behavior here if needed
            Debug.Log("Android platform detected, no action needed.");
        }
    }
}
