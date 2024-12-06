using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class STMechanicsSpawner : MonoBehaviour
{
    public GameObject prefabToSpawn;
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();
    public AudioMixer audioMixer;
    private const string musicVolumeParam = "MusicVolume";


    void OnMouseDown()
    {
        SpawnAndAnimatePrefab();
        Destroy(gameObject); // Destroy this game object after spawning the prefab
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

    void SpawnAndAnimatePrefab()
    {
        SetMusicVolume(-80f);
        GameObject instantiatedPrefab = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity);

        SaveAndResetScales(instantiatedPrefab);

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
