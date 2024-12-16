using UnityEngine;
using UnityEngine.Audio;

public class Speech_Bubble_Controller2 : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject STPrefab; 
    private GameObject stCanvas;
    public AudioMixer audioMixer;
    private bool isTapped = false;
    private const string musicVolumeParam = "MusicVolume";
    private const string AmbientVolumeParam = "AmbientVolume";
    private int childrenScaled = 0;
    private int totalChildren = 2;

    private void Start()
    {
        // Find STCanvas by tag
        stCanvas = GameObject.FindGameObjectWithTag("STCanvas");

        if (stCanvas == null)
        {
            Debug.LogError("STCanvas not found! Make sure it is tagged as 'STCanvas'.");
        }
    }

    private void Update()
    {
        if (!isTapped && Input.GetMouseButtonDown(0)) 
        {
            isTapped = true; 
            OnTapped();
        }
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
    private void SetAmbientVolume(float volume)
    {
        if (audioMixer != null)
        {
            bool result = audioMixer.SetFloat(AmbientVolumeParam, volume); // "MusicVolume" should match the exposed parameter name
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
    private void OnTapped()
    {
        if (stCanvas == null) return;

        SetMusicVolume(-80f);
        SetAmbientVolume(-80f);

        foreach (Transform child in stCanvas.transform)
        {
            LeanTween.scale(child.gameObject, Vector3.one, 0.5f).setOnComplete(OnChildScaled);
        }
    }

    private void OnChildScaled()
    {
        childrenScaled++;

        if (childrenScaled >= totalChildren)
        {
            SpawnSpeechBubble();
        }
    }

    private void SpawnSpeechBubble()
    {
        if (STPrefab != null && stCanvas != null)
        {
            GameObject bubble = Instantiate(STPrefab);

            bubble.transform.localPosition = Vector3.zero;

            Destroy(gameObject);
            Debug.Log("Speech bubble prefab spawned!");
        }
        else
        {
            Debug.LogError("Prefab or STCanvas is not set!");
        }
    }
}
