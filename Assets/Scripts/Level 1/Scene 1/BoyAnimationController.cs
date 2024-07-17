using UnityEngine;
using System.Collections;

public class BoyAnimationController : MonoBehaviour
{
    private Animator animator;
    private Camera mainCamera;
    private Vector3 targetPosition;
    public float walkDuration = 2f;
    public float zoomSize = 5f;
    public float zoomDuration = 2f;

    public GameObject speechBubblePrefab;
    public Transform speechBubbleContainer;

    private float originalOrthographicSize;
    private CameraViewportHandler viewportHandler;

    private void Start()
    {
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;

        if (animator == null)
        {
            Debug.LogError("Animator component missing on Boy object.");
            return;
        }

        if (speechBubblePrefab == null || speechBubbleContainer == null)
        {
            Debug.LogError("Speech bubble prefab or container is not assigned.");
            return;
        }

        viewportHandler = mainCamera.GetComponent<CameraViewportHandler>();
        originalOrthographicSize = mainCamera.orthographicSize;
        targetPosition = new Vector3(0, transform.position.y, transform.position.z); // Center of the viewport

        // Start the scene by walking to the center
        StartCoroutine(WalkToCenter());
    }

    private IEnumerator WalkToCenter()
    {
        animator.SetBool("isWalking", true);
        Vector3 startPosition = transform.position;
        float elapsedTime = 0;

        while (elapsedTime < walkDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / walkDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        animator.SetBool("isWalking", false);

        // Transition from Idle to Talking controlled by exit time
        yield return new WaitForSeconds(1f); // Allow time for transition to idle
        OnTalkAnimationEnd();
    }

    // This method should be called by an Animation Event at the end of the Talk animation
    public void OnTalkAnimationEnd()
    {
        StartCoroutine(HandlePostTalk());
    }

    private IEnumerator HandlePostTalk()
    {
        yield return new WaitForSeconds(2.5f); // Wait for 2 seconds after talk animation

        // Start zooming in
        StartCoroutine(ZoomIn());
    }

    private IEnumerator ZoomIn()
    {
        float startOrthographicSize = mainCamera.orthographicSize;
        viewportHandler.enabled = false;
        float elapsedTime = 0;

        while (elapsedTime < zoomDuration)
        {
            mainCamera.orthographicSize = Mathf.Lerp(startOrthographicSize, zoomSize, elapsedTime / zoomDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.orthographicSize = zoomSize;

        // Show teeth animation
        animator.SetBool("showTeeth", true);

        yield return new WaitForSeconds(2f); // Assuming show teeth animation duration

        

        // Start zooming out
        StartCoroutine(ZoomOut());
    }

    private IEnumerator ZoomOut()
    {
        float startOrthographicSize = mainCamera.orthographicSize;
        float elapsedTime = 0;

        while (elapsedTime < zoomDuration)
        {
            mainCamera.orthographicSize = Mathf.Lerp(startOrthographicSize, originalOrthographicSize, elapsedTime / zoomDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.orthographicSize = originalOrthographicSize;
        viewportHandler.enabled = true;
        animator.SetBool("showTeeth", false);

        // Continue the animation sequence
        yield return new WaitForSeconds(7f); // Assuming final talk animation duration

        // Spawn the speech bubble
        SpawnSpeechBubble();
    }

    private void SpawnSpeechBubble()
    {
        if (speechBubblePrefab != null && speechBubbleContainer != null)
        {
            Instantiate(speechBubblePrefab, speechBubbleContainer.position, Quaternion.identity, speechBubbleContainer);
        }
        else
        {
            Debug.LogError("Speech bubble prefab or container is not assigned.");
        }
    }
}
