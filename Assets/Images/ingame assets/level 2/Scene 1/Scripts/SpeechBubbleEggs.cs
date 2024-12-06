using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SpeechBubbleEggs : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public GameObject[] eggPrefabs; // Array to hold different egg prefabs
    public int eggCount = 10; // Number of eggs to spawn
    public float eggSpeed = 2f; // Speed at which eggs move
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();

    private Collider2D objectCollider;
    private SpriteRenderer objectSpriteRenderer;

    public AudioMixer audioMixer;
    private const string musicVolumeParam = "MusicVolume";

    void Start()
    {
        objectCollider = GetComponent<Collider2D>();
        objectSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        // Disable the collider and sprite renderer
        if (objectCollider != null)
            objectCollider.enabled = false;

        if (objectSpriteRenderer != null)
            objectSpriteRenderer.enabled = false;

        // Spawn and animate the prefab
        SpawnAndAnimatePrefab();
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
                     StartCoroutine(SpawnEggs(() => AnimateChildren(instantiatedPrefab.transform)));
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
            if (originalScales.ContainsKey(child))
            {
                LeanTween.scale(child.gameObject, originalScales[child], 0.5f).setEase(LeanTweenType.easeOutBack);
            }
        }
    }

    IEnumerator SpawnEggs(System.Action onComplete)
    {
        for (int i = 0; i < eggCount; i++)
        {
            // Randomize spawn position along the left side of the viewport
            Vector3 spawnPosition = Camera.main.ViewportToWorldPoint(new Vector3(0f, Random.Range(0f, 1f), Camera.main.nearClipPlane + 5f));
            spawnPosition.z = 0; // Ensure the eggs are at the correct depth
            GameObject eggPrefab = eggPrefabs[Random.Range(0, eggPrefabs.Length)];
            GameObject egg = Instantiate(eggPrefab, spawnPosition, Quaternion.identity);

            // Randomize scale between 2 and 4
            float randomScale = Random.Range(0.5f, 2f);
            egg.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

            // Set the target position directly to the right of the spawn position
            Vector3 moveTarget = new Vector3(Camera.main.ViewportToWorldPoint(new Vector3(1.2f, spawnPosition.y, Camera.main.nearClipPlane + 5f)).x, spawnPosition.y, 0);

            // Use eggSpeed as the duration directly for slower movement
            LeanTween.move(egg, moveTarget, 1f / eggSpeed).setOnComplete(() => Destroy(egg));

            // Rotate on the Z axis while moving
            LeanTween.rotateAround(egg, Vector3.forward, -360f, 1f / eggSpeed);

            yield return new WaitForSeconds(0.1f); // Reduced delay between egg spawns for faster spawning
        }
        onComplete?.Invoke(); // Call the onComplete action after all eggs are spawned
    }
}
