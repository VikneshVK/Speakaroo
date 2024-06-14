using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class SpeechBubbleController : MonoBehaviour
{
    public float zoomInSize = 2f;
    public float zoomDuration = 2f;

    [Header("Game Objects")]
    public GameObject firstObjectPrefab;
    public GameObject secondObjectPrefab;
    public Transform firstSpawnContainer;
    public Transform secondSpawnContainer;

    [Header("Drop Points")]
    public GameObject firstDropPointPrefab;
    public GameObject secondDropPointPrefab;
    public Transform firstDropPointContainer;
    public Transform secondDropPointContainer;

    [Header("Toothbrush")]
    public GameObject toothbrushPrefab;

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

    private Collider2D speechBubbleCollider;

    void Start()
    {
        speechBubbleCollider = GetComponent<Collider2D>();
        if (speechBubbleCollider == null)
        {
            Debug.LogError("Collider2D component not found on the speech bubble.");
            return;
        }
        speechBubbleCollider.isTrigger = true;

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

        speechBubbleCollider.enabled = false; // Disable collider to avoid interference
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
        yield return new WaitForSeconds(1);
        audioSource.PlayOneShot(allCorrectDropAudio);
        yield return new WaitForSeconds(allCorrectDropAudio.length);
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
        yield return new WaitForSeconds(1);
        cameraZoom.ZoomOut(zoomDuration);
        yield return new WaitForSeconds(zoomDuration);
        isZoomedIn = false;

        GameObject toothbrushContainerObj = GameObject.Find("brush Container");
        if (toothbrushContainerObj != null && toothbrushContainerObj.CompareTag("brush"))
        {
            Transform toothbrushContainer = toothbrushContainerObj.transform;
            Instantiate(toothbrushPrefab, toothbrushContainer.position, toothbrushContainer.rotation);
            Debug.Log("Toothbrush instantiated inside its container.");
        }
        else
        {
            Debug.LogError("Toothbrush container not found or tag mismatch.");
        }

        Destroy(gameObject);
    }
}
