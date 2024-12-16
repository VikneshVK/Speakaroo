using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic; // Required for Dictionary

public class SpeechBubble : MonoBehaviour
{
    private GameObject mechanicsPrefab;
    private DragAndDropController controller;
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();
    private const string musicVolumeParam = "MusicVolume";
    private const string AmbientVolumeParam = "AmbientVolume";
    private GameObject stCanvas;
    public AudioMixer audioMixer;
    private int childrenScaled = 0;
    private int totalChildren = 2;

    /* private GameObject buttonToDeactivate;*/

    // List to hold all sprite masks with the tag "SpriteMask"


    public void Setup(GameObject prefab, DragAndDropController dragAndDropController)
    {
        mechanicsPrefab = prefab;
        controller = dragAndDropController;

        /*// Find the UICanvas and the Button within it
        GameObject uiCanvas = GameObject.FindGameObjectWithTag("UICanvas");
        if (uiCanvas != null)
        {
            buttonToDeactivate = uiCanvas.transform.Find("Button").gameObject; // Replace "ButtonName" with the actual name of your button
        }

        if (buttonToDeactivate != null)
        {
            buttonToDeactivate.SetActive(false); // Deactivate the button when the speech bubble is clicked
        }*/


    }

    private void Start()
    {
        stCanvas = GameObject.FindGameObjectWithTag("STCanvas");
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
    void OnMouseDown()
    {

        SpriteMaskManager.Instance.DeactivateMasks();

        foreach (Transform child in stCanvas.transform)
        {
            LeanTween.scale(child.gameObject, Vector3.one, 0.5f).setOnComplete(OnChildScaled);
        }
        Destroy(gameObject); // Destroy the speech bubble after spawning the prefab
    }
    private void OnChildScaled()
    {
        childrenScaled++;

        if (childrenScaled >= totalChildren)
        {
            SpawnAndAnimateMechanicsPrefab();
        }
    }

    void SpawnAndAnimateMechanicsPrefab()
    {
        SetMusicVolume(-80f);
        SetAmbientVolume(-80f);
        GameObject instantiatedPrefab = Instantiate(mechanicsPrefab, Vector3.zero, Quaternion.identity);

        // Save original scales and set scale to zero
        SaveAndResetScales(instantiatedPrefab);

        // Start animation of parent prefab
        LeanTween.scale(instantiatedPrefab, originalScales[instantiatedPrefab.transform], 0.5f)
                 .setEase(LeanTweenType.easeOutBack)
                 .setOnComplete(() => {
                     AnimateChildren(instantiatedPrefab.transform);
                 });
    }

    void SaveAndResetScales(GameObject root)
    {
        originalScales[root.transform] = root.transform.localScale;
        root.transform.localScale = Vector3.zero;

        foreach (Transform child in root.transform)
        {
            originalScales[child] = child.localScale;
            child.localScale = Vector3.zero;
        }
    }

    void AnimateChildren(Transform parentTransform)
    {
        foreach (Transform child in parentTransform)
        {
            if (originalScales.ContainsKey(child)) // Check if the original scale was stored
            {
                LeanTween.scale(child.gameObject, originalScales[child], 0.5f).setEase(LeanTweenType.easeOutBack);
            }
        }
    }
}
