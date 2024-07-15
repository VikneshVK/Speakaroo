using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Camera cam;
    public float zoomDuration = 2.0f; // Fixed duration for zoom in and out
    public float targetZoomSize = 5f; // Target size when zoomed in
    private float originalZoomSize; // Original zoom size to return to
    private Vector3 originalPosition; // Store the original camera position to reset after zoom out
    private CameraViewportHandler viewportHandler; // Reference to the ViewportHandler script

    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main; // Auto-assign the main camera if not set
        }
        originalZoomSize = cam.orthographicSize; // Store the original zoom size
        originalPosition = cam.transform.position; // Store the original position

        viewportHandler = GetComponent<CameraViewportHandler>(); // Get the ViewportHandler script attached to the same GameObject
        if (viewportHandler == null)
        {
            Debug.LogError("ViewportHandler script not found on the camera GameObject.");
        }
    }

    // Method to zoom in on a specific target
    public IEnumerator ZoomInOn(Transform target)
    {
        if (viewportHandler != null)
        {
            viewportHandler.enabled = false; // Disable ViewportHandler
            Debug.Log("ViewportHandler disabled.");
        }

        float elapsedTime = 0;
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, cam.transform.position.z);

        while (elapsedTime < zoomDuration)
        {
            float t = elapsedTime / zoomDuration;
            t = t * t * (3f - 2f * t); // Smooth step interpolation formula

            cam.orthographicSize = Mathf.Lerp(originalZoomSize, targetZoomSize, t);
            cam.transform.position = Vector3.Lerp(originalPosition, targetPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cam.orthographicSize = targetZoomSize;
        cam.transform.position = targetPosition;
    }

    // Method to zoom out to the original camera settings
    public IEnumerator ZoomOut()
    {
        float elapsedTime = 0;
        Vector3 startPosition = cam.transform.position;  // Capture the starting position
        float startSize = cam.orthographicSize;  // Capture the starting size

        while (elapsedTime < zoomDuration)
        {
            float t = elapsedTime / zoomDuration;
            t = t * t * (3f - 2f * t); // Applying smooth step formula for smoother interpolation

            cam.orthographicSize = Mathf.Lerp(startSize, originalZoomSize, t);
            cam.transform.position = Vector3.Lerp(startPosition, originalPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Explicitly set the final size and position to ensure they match exactly
        cam.orthographicSize = originalZoomSize;
        cam.transform.position = originalPosition;

        if (viewportHandler != null)
        {
            viewportHandler.enabled = true; // Enable ViewportHandler
            Debug.Log("ViewportHandler enabled.");
        }
    }
}
