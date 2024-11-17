using UnityEngine;
using System.Collections;

public class ScrubberController : MonoBehaviour
{
    public GameObject maskPrefab; // The scrubber dummy prefab
    public Vector3 originalPosition;
    public bool isInteracted = false;
    public bool scrubberTimerStarted = false;
    private GameObject scrubberDummyInstance; // Reference to the spawned scrubber dummy
    private float scrubberTimerDuration = 10f; // Timer duration
    public float scrubberTimer = 0f; // Timer variable
    private LV4DragManager dragManager;
    private DishController[] dishControllers; // Array to hold references to all DishController instances
    private bool positionConfirmed;
    private Coroutine scrubberTimerCoroutine;

    public GameObject scrubberBoundary; // Reference to the scrubber boundary

    private void Start()
    {
        dishControllers = FindObjectsOfType<DishController>(); // Get all DishController instances in the scene
        dragManager = FindAnyObjectByType<LV4DragManager>();
        positionConfirmed = false;
    }

    private void Update()
    {
        HandleScrubbing();
        CheckScrubberTimer();
    }

    private void HandleScrubbing()
    {
        if (dragManager.tweenComplete && !positionConfirmed)
        {
            originalPosition = transform.position;
            positionConfirmed = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isInteracted = true;
            }
        }

        if (isInteracted && Input.GetMouseButton(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Restrict the scrubber's position within the boundary
            if (scrubberBoundary != null)
            {
                // Get the bounds of the scrubber boundary
                Bounds boundaryBounds = scrubberBoundary.GetComponent<Collider2D>().bounds;

                // Clamp the mouse position within the boundary's bounds
                mousePosition.x = Mathf.Clamp(mousePosition.x, boundaryBounds.min.x, boundaryBounds.max.x);
                mousePosition.y = Mathf.Clamp(mousePosition.y, boundaryBounds.min.y, boundaryBounds.max.y);
            }

            transform.position = mousePosition;

            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (hit.collider != null && hit.collider.GetComponent<DishController>() != null)
            {
                hit.collider.GetComponent<DishController>().StartScrubbing();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isInteracted = false;
        }
    }

    private void CheckScrubberTimer()
    {
        if (!scrubberTimerStarted)
        {
            foreach (DishController dish in dishControllers)
            {
                if (dish.scrubbertimer)
                {
                    Debug.Log($"Scrubber timer started for game object: {dish.gameObject.name}");
                    scrubberTimerStarted = true; // Start the timer only once
                    scrubberTimerCoroutine = StartCoroutine(StartScrubberTimer(dish.gameObject)); // Store the coroutine reference
                    break; // Only start the timer once
                }
            }
        }
    }


    private IEnumerator StartScrubberTimer(GameObject dishGameObject)
    {
        scrubberTimer = 0f; // Reset timer
        Debug.Log($"Starting scrubber timer for: {dishGameObject.name}"); // Log the game object name

        while (scrubberTimer < scrubberTimerDuration)
        {
            scrubberTimer += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"Scrubber timer completed for: {dishGameObject.name}"); // Log when the timer completes
        SpawnAndTweenScrubberDummy(); // Spawn and tween the scrubber dummy once timer completes
    }

    private void SpawnAndTweenScrubberDummy()
    {
        if (maskPrefab != null && scrubberDummyInstance == null)
        {
            scrubberDummyInstance = Instantiate(maskPrefab, transform.position, Quaternion.identity);

            // Tween scrubber dummy to viewport center and loop it
            Vector3 centerScreenPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane + 5f));
            LeanTween.move(scrubberDummyInstance, centerScreenPosition, 1f).setLoopClamp();
        }
    }

    private void OnMouseDown()
    {
        // Always stop the scrubber timer regardless of scrubberDummy
        foreach (DishController dish in dishControllers)
        {
            dish.scrubbertimer = false; // Stop the scrubber timer
            scrubberTimer = 0;          // Reset the timer
        }

        Debug.Log("Scrubber timer stopped.");

        // Stop the running scrubber timer coroutine if it exists
        if (scrubberTimerCoroutine != null)
        {
            StopCoroutine(scrubberTimerCoroutine);
            scrubberTimerCoroutine = null;
        }

        // If a scrubber dummy is spawned, destroy it
        if (scrubberDummyInstance != null)
        {
            Destroy(scrubberDummyInstance);
            Debug.Log("Scrubber dummy destroyed.");
            scrubberDummyInstance = null;
        }

        // Reset scrubberTimerStarted so a new timer can be started if needed
        scrubberTimerStarted = false;
    }



    public void ResetPosition()
    {
        LeanTween.move(gameObject, originalPosition, 0.5f);
        isInteracted = false;
    }
}
