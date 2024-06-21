using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]

public class ZoomOnCharacter : MonoBehaviour
{
    public Transform character;  // Target character to zoom towards
    public Camera zoomCamera;
    public CameraViewportHandler viewportHandler;
    public float zoomDuration = 2.0f;
    public float targetOrthographicSize = 5f;  // Target size when zoomed in

    private Camera mainCamera;
    private Animator animator;
    private bool isZooming;
    private bool isZoomedIn;
    private float originalOrthographicSize;
    private Vector3 originalCameraPosition;  // Store the original camera position to reset after zoom out

    void Start()
    {
        mainCamera = Camera.main;
        originalOrthographicSize = mainCamera.orthographicSize;
        originalCameraPosition = mainCamera.transform.position;
        zoomCamera.enabled = false; // Ensure zoom camera is disabled initially
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.Log("No Animator component found on " + gameObject.name);
        }
    }

    void OnMouseDown()
    {
        if (isZooming) return;

        if (animator != null)
        {
            animator.SetBool("isClicked", true);
            Debug.Log("Object clicked, isClicked set to true");
        }

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
        Vector3 targetPosition = new Vector3(character.position.x, character.position.y, originalCameraPosition.z); // Maintain original Z position
        while (elapsedTime < zoomDuration)
        {
            zoomCamera.orthographicSize = Mathf.Lerp(originalOrthographicSize, targetOrthographicSize, elapsedTime / zoomDuration);
            zoomCamera.transform.position = Vector3.Lerp(originalCameraPosition, targetPosition, elapsedTime / zoomDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        zoomCamera.orthographicSize = targetOrthographicSize;
        zoomCamera.transform.position = targetPosition;
        isZooming = false;
        isZoomedIn = true;

        if (animator != null)
        {
            animator.SetBool("isZoomedIn", true);
            animator.SetBool("isClicked", false);
        }
    }

    private IEnumerator ZoomOutCamera()
    {
        isZooming = true;

        float elapsedTime = 0f;
        Vector3 startPosition = zoomCamera.transform.position;
        float startSize = zoomCamera.orthographicSize;

        while (elapsedTime < zoomDuration)
        {
            zoomCamera.orthographicSize = Mathf.Lerp(startSize, originalOrthographicSize, elapsedTime / zoomDuration);
            zoomCamera.transform.position = Vector3.Lerp(startPosition, originalCameraPosition, elapsedTime / zoomDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        zoomCamera.orthographicSize = originalOrthographicSize;
        zoomCamera.transform.position = originalCameraPosition;
        mainCamera.enabled = true;
        zoomCamera.enabled = false;
        viewportHandler.enabled = true;

        isZooming = false;
        isZoomedIn = false;

        if (animator != null)
        {
            animator.SetBool("isZoomedOut", true);
        }
    }
}
