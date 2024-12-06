using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SpeechBubble_Balloons : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public GameObject[] balloonPrefabs; // Array to hold different balloon prefabs
    public int balloonCount = 20; // Number of balloons to spawn
    public float balloonSpeed = 5f; // Speed at which balloons move (increased for faster movement)
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
                     StartCoroutine(SpawnBalloons(() => AnimateChildren(instantiatedPrefab.transform)));
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

    IEnumerator SpawnBalloons(System.Action onComplete)
    {
        for (int i = 0; i < balloonCount; i++)
        {
            // Randomize spawn position along the bottom of the viewport
            Vector3 spawnPosition = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(0f, 1f), 0f, Camera.main.nearClipPlane + 5f));
            spawnPosition.z = 0; // Ensure the balloons are at the correct depth
            GameObject balloonPrefab = balloonPrefabs[Random.Range(0, balloonPrefabs.Length)];
            GameObject balloon = Instantiate(balloonPrefab, spawnPosition, Quaternion.identity);

            // Set the target position directly above the spawn position
            Vector3 moveTarget = new Vector3(spawnPosition.x, Camera.main.ViewportToWorldPoint(new Vector3(spawnPosition.x, 1.2f, Camera.main.nearClipPlane + 5f)).y, 0);

            // Use balloonSpeed as the duration directly for faster movement
            LeanTween.move(balloon, moveTarget, 1f / balloonSpeed).setOnComplete(() => Destroy(balloon));
            yield return new WaitForSeconds(0.2f); // Decreased delay between balloon spawns
        }
        onComplete?.Invoke(); // Call the onComplete action after all balloons are spawned
    }
}
