using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class SpeechBubbleController : MonoBehaviour
{
    public float zoomInSize = 2f;
    public float zoomDuration = 2f;

    [Header("Game Objects")]
    public GameObject firstObjectPrefab; // Prefab for the first game object
    public GameObject secondObjectPrefab; // Prefab for the second game object
    public Transform firstSpawnContainer; // Container for the first game object
    public Transform secondSpawnContainer; // Container for the second game object

    [Header("Drop Points")]
    public GameObject firstDropPointPrefab; // Prefab for the first drop point
    public GameObject secondDropPointPrefab; // Prefab for the second drop point
    public Transform firstDropPointContainer; // Container for the first drop point
    public Transform secondDropPointContainer; // Container for the second drop point

    [Header("Toothbrush and Toothpaste")]
    public GameObject toothbrushPrefab; // Prefab for the toothbrush
    public GameObject toothpastePrefab; // Prefab for the toothpaste
    /*public Transform toothbrushContainer; // Container for the toothbrush
    public Transform toothpasteContainer; // Container for the toothpaste*/

    [Header("Audio Clips")]
    public AudioClip firstObjectDropAudio;
    public AudioClip secondObjectDropAudio;
    public AudioClip allCorrectDropAudio;
    public AudioClip incorrectDropAudio;
    private AudioSource audioSource;

    private CameraZoom cameraZoom;
    private bool isZoomedIn = false;

    private DraggableObject firstObject;
    private DraggableObject secondObject;

    private DropPoint firstDropPoint;
    private DropPoint secondDropPoint;

    private int repeatCount = 0;
    private const int maxRepeats = 3;

    void Start()
    {
        Collider2D bubbleCollider = GetComponent<Collider2D>();
        if (bubbleCollider == null)
        {
            Debug.LogError("Collider2D component not found on the speech bubble.");
            return;
        }
        bubbleCollider.isTrigger = true;

        audioSource = gameObject.AddComponent<AudioSource>();
        SetupCameraZoomComponent();
    }

    void SetupCameraZoomComponent()
    {
        GameObject mainCamera = GameObject.Find("Main Camera");
        if (mainCamera != null)
        {
            cameraZoom = mainCamera.GetComponent<CameraZoom>();
            if (cameraZoom == null)
            {
                Debug.LogError("CameraZoom component not found on the main camera.");
            }
        }
        else
        {
            Debug.LogError("Main Camera not found.");
        }
    }

    void OnMouseDown()
    {
        if (cameraZoom != null && !isZoomedIn)
        {
            StartCoroutine(ZoomInCoroutine());
            isZoomedIn = true;
        }
    }

    IEnumerator ZoomInCoroutine()
    {
        cameraZoom.ZoomTo(transform, zoomInSize, zoomDuration);
        yield return new WaitForSeconds(zoomDuration);
        SpawnNewObjectsAndDropPoints();
    }

    void SpawnNewObjectsAndDropPoints()
    {
        firstObject = SpawnDraggableObject(firstObjectPrefab, secondSpawnContainer, "bring");
        secondObject = SpawnDraggableObject(secondObjectPrefab, firstSpawnContainer, "toothpaste");

        firstDropPoint = SpawnDropPoint(firstDropPointPrefab, firstDropPointContainer, "bring");
        secondDropPoint = SpawnDropPoint(secondDropPointPrefab, secondDropPointContainer, "toothpaste");
    }

    DraggableObject SpawnDraggableObject(GameObject prefab, Transform container, string tag)
    {
        GameObject instance = Instantiate(prefab, container);
        instance.tag = tag;
        DraggableObject draggable = instance.AddComponent<DraggableObject>();
        draggable.Initialize(OnCorrectDrop, OnIncorrectDrop);
        Debug.Log($"Spawned GameObject: {instance.name}, Tag: {tag}");
        return draggable;
    }

    DropPoint SpawnDropPoint(GameObject prefab, Transform container, string tag)
    {
        GameObject instance = Instantiate(prefab, container);
        instance.tag = tag;
        DropPoint dropPoint = instance.AddComponent<DropPoint>();
        Debug.Log($"Spawned DropPoint: {instance.name}, Tag: {tag}");
        return dropPoint;
    }

    void OnCorrectDrop(string tag)
    {
        if (tag == "bring")
        {
            audioSource.PlayOneShot(firstObjectDropAudio);
        }
        else if (tag == "toothpaste")
        {
            audioSource.PlayOneShot(secondObjectDropAudio);
        }

        if (CheckAllCorrect())
        {
            StartCoroutine(PlayAllCorrectAudioWithDelay());
        }
    }

    IEnumerator PlayAllCorrectAudioWithDelay()
    {
        yield return new WaitForSeconds(1); // Wait for 1 second before playing the audio
        audioSource.PlayOneShot(allCorrectDropAudio);
        yield return new WaitForSeconds(allCorrectDropAudio.length); // Wait for the all correct audio to finish
        ProcessRepeats();
    }

    void ProcessRepeats()
    {
        repeatCount++;
        if (repeatCount < maxRepeats)
        {
            ResetForNextRound();
        }
        else
        {
            StartCoroutine(ZoomOutCoroutine());
        }
    }

    void OnIncorrectDrop()
    {
        audioSource.PlayOneShot(incorrectDropAudio);
    }

    bool CheckAllCorrect()
    {
        return firstObject.IsCorrectlyDropped() && secondObject.IsCorrectlyDropped();
    }

    void ResetForNextRound()
    {
        Destroy(firstObject.gameObject);
        Destroy(secondObject.gameObject);
        Destroy(firstDropPoint.gameObject);
        Destroy(secondDropPoint.gameObject);
        SpawnNewObjectsAndDropPoints();
    }

    IEnumerator ZoomOutCoroutine()
    {
        yield return new WaitForSeconds(1); // Wait for the audio to finish
        cameraZoom.ZoomOut(zoomDuration);
        yield return new WaitForSeconds(zoomDuration);
        isZoomedIn = false;

        // Instantiate toothbrush at (-4.81, -4.18, 0) with rotation on z-axis 22 degrees
        Vector3 toothbrushPosition = new Vector3(-4.81f, -4.18f, 0f);
        Quaternion toothbrushRotation = Quaternion.Euler(0, 0, 22);
        Instantiate(toothbrushPrefab, toothbrushPosition, toothbrushRotation);

        // Instantiate toothpaste at (5, -4.05, 0) with rotation on x-axis 35 degrees
        Vector3 toothpastePosition = new Vector3(5f, -4.05f, 0f);
        Quaternion toothpasteRotation = Quaternion.Euler(0, 0, 35);
        Instantiate(toothpastePrefab, toothpastePosition, toothpasteRotation);

        Debug.Log("Toothbrush and toothpaste instantiated inside their containers.");

       /* if (toothpastePrefab.GetComponent<DraggableToothpaste>() != null)
        {
            toothpastePrefab.GetComponent<DraggableToothpaste>().Initialize(cameraZoom, toothpastePrefab.transform);
        }*/

        // Destroy the speech bubble and its children
        Destroy(gameObject);
    }
}
