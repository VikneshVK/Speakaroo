using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class ZoomOnClick : MonoBehaviour
{
    public CameraZoom cameraZoom; // Assign this manually in the Inspector
    public float zoomInSize = 2f;
    public float zoomDuration = 2f;
    public GameObject speechBubblePrefab; // The prefab for the speech bubble

    public AudioClip zoomInAudio; // Audio clip to play when zoom in is done
    public AudioClip zoomOutAudio; // Audio clip to play when zoom out is done
    private AudioSource audioSource;

    private bool isZoomedIn = false;
    private Collider2D objectCollider;

    void Start()
    {
        objectCollider = GetComponent<Collider2D>();
        if (objectCollider == null)
        {
            Debug.LogError("Collider2D component not found on the game object.");
        }

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void OnMouseDown()
    {
        if (cameraZoom != null && objectCollider != null)
        {
            if (isZoomedIn)
            {
                StartCoroutine(ZoomOutCoroutine());
            }
            else
            {
                StartCoroutine(ZoomInCoroutine());
            }
            isZoomedIn = !isZoomedIn;
        }
        else
        {
            Debug.LogError("CameraZoom script reference is not assigned or Collider2D is missing.");
        }
    }

    IEnumerator ZoomInCoroutine()
    {
        objectCollider.enabled = false; // Disable the collider
        cameraZoom.ZoomTo(transform, zoomInSize, zoomDuration);
        yield return new WaitForSeconds(zoomDuration);
        PlayZoomInAudio();
        objectCollider.enabled = true; // Re-enable the collider
    }

    IEnumerator ZoomOutCoroutine()
    {
        objectCollider.enabled = false; // Disable the collider
        cameraZoom.ZoomOut(zoomDuration);
        yield return new WaitForSeconds(zoomDuration);
        yield return StartCoroutine(PlayZoomOutAudio());
        InstantiateSpeechBubble(); // Instantiate the speech bubble
        objectCollider.enabled = true; // Re-enable the collider
    }

    void PlayZoomInAudio()
    {
        if (zoomInAudio != null)
        {
            audioSource.PlayOneShot(zoomInAudio);
        }
    }

    IEnumerator PlayZoomOutAudio()
    {
        if (zoomOutAudio != null)
        {
            audioSource.PlayOneShot(zoomOutAudio);
            yield return new WaitWhile(() => audioSource.isPlaying); // Wait until the audio is finished
        }
    }

    void InstantiateSpeechBubble()
    {
        if (speechBubblePrefab != null)
        {
            // Instantiate the speech bubble at the desired position
            Instantiate(speechBubblePrefab, new Vector3(0.6f, 3, 0), Quaternion.identity); // Adjust the position as needed
        }
        else
        {
            Debug.LogError("SpeechBubblePrefab is not assigned.");
        }
    }
}
