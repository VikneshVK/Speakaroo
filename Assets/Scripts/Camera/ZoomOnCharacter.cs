using UnityEngine;
using System.Collections;

public class ZoomOnCharacter : MonoBehaviour
{
    public Transform character;
    public Camera zoomCamera;
    public CameraViewportHandler viewportHandler;
    public float zoomDuration = 2.0f;
    public float targetOrthographicSize = 5f;  // Target size when zoomed in

    private Camera mainCamera;
    private bool isZooming;
    private bool isZoomedIn;
    private float originalOrthographicSize;

    void Start()
    {
        mainCamera = Camera.main;
        originalOrthographicSize = mainCamera.orthographicSize;
        zoomCamera.enabled = false; // Ensure zoom camera is disabled initially
    }

    void OnMouseDown()
    {
        if (isZooming) return;

        if (isZoomedIn)
        {
            StartCoroutine(ZoomOutCamera());
        }
        else
        {
            StartCoroutine(ZoomInOnCharacter());
        }
    }

    private IEnumerator ZoomInOnCharacter()
    {
        isZooming = true;
        zoomCamera.enabled = true;
        mainCamera.enabled = false;
        viewportHandler.enabled = false;  // Disable viewport handler during zoom

        float elapsedTime = 0f;
        while (elapsedTime < zoomDuration)
        {
            zoomCamera.orthographicSize = Mathf.Lerp(originalOrthographicSize, targetOrthographicSize, elapsedTime / zoomDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        zoomCamera.orthographicSize = targetOrthographicSize;
        isZooming = false;
        isZoomedIn = true;
    }

    private IEnumerator ZoomOutCamera()
    {
        isZooming = true;

        float elapsedTime = 0f;
        while (elapsedTime < zoomDuration)
        {
            zoomCamera.orthographicSize = Mathf.Lerp(targetOrthographicSize, originalOrthographicSize, elapsedTime / zoomDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        zoomCamera.orthographicSize = originalOrthographicSize;
        mainCamera.enabled = true;
        zoomCamera.enabled = false;
        viewportHandler.enabled = true;  // Re-enable viewport handler after zoom
        isZooming = false;
        isZoomedIn = false;
    }
}
