using System.Collections;
using UnityEngine;

public class BrushController : MonoBehaviour
{
    private bool isDragging = false;

    private Vector3 initialPosition;
    private Animator brushAnimator;
    public Animator boyAnimator; // Ensure this is unique in the script
    public GameObject dropTarget;
    public GameObject teeth;
    public GameObject paste;
    public Sprite brushBackSprite;
    public GameObject Foam;
    public Transform restPosition;

    private SpriteRenderer spriteRenderer;

    public Camera mainCamera;                // Reference to the main camera
    public Camera zoomCamera;                // Reference to the zoom camera
    public CameraViewportHandler mainCameraViewportHandler; // Reference to the CameraViewportHandler script on the main camera
    public CameraViewportHandler zoomCameraViewportHandler; // Reference to the CameraViewportHandler script on the zoom camera

    public float zoomDuration = 2.0f;
    public float targetOrthographicSize = 5f;

    private bool isZooming;
    private bool isZoomedIn;
    private float originalOrthographicSize;
    private Vector3 originalCameraPosition;

    void Start()
    {
        brushAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialPosition = transform.position;
        brushBackSprite = Resources.Load<Sprite>("Images/brush back");
        CheckComponents();

        originalOrthographicSize = mainCamera.orthographicSize;
        originalCameraPosition = mainCamera.transform.position;
        zoomCamera.enabled = false; // Ensure zoom camera is disabled initially
    }

    void Update()
    {
        if (Camera.main == null)
        {
            return; // Skip Update if no main camera is found
        }

        HandleDragging();
        HandleMouseInput();
    }

    private void HandleDragging()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Ensure that the z-position is zero to match your 2D plane
            transform.position = mousePosition;

            // Perform raycast while dragging
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null)
                {
                    if (hit.collider.gameObject == teeth && hit.collider.gameObject.CompareTag("Teeth") && !isZoomedIn)
                    {
                        // Start zooming in if hovering over teeth
                        StartCoroutine(ZoomAndHandleFoam());
                        break;
                    }
                }
            }
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDragging();
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            CheckDrop();
        }
    }

    private void StartDragging()
    {
        if (Camera.main == null) return; // Guard clause to ensure Camera.main is not null

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);
        if (hitCollider != null && hitCollider.gameObject == gameObject)
        {
            isDragging = true;
            initialPosition = transform.position;
        }
    }

    private void CheckDrop()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);

        bool dropHandled = false;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                if (hit.collider.gameObject == dropTarget && hit.collider.gameObject.CompareTag("DropPoint"))
                {
                    HandleSuccessfulDrop(dropTarget);
                    dropHandled = true;
                    break;
                }
            }
        }

        if (!dropHandled)
        {
            transform.position = initialPosition;
        }
    }

    private void HandleSuccessfulDrop(GameObject target)
    {
        transform.position = target.transform.position;
        transform.rotation = Quaternion.Euler(0, 0, 0);

        if (target == dropTarget)
        {
            brushAnimator.SetTrigger("paste_On");
            teeth.SetActive(true);
            Destroy(paste);
            StartCoroutine(HandleAnimationCompletion());
        }
    }

    private IEnumerator ZoomAndHandleFoam()
    {
        isZoomedIn = true; // Set zoom state
        mainCamera.enabled = false;
        zoomCameraViewportHandler.enabled = false;

        yield return StartCoroutine(ZoomInOnTeeth());

        // Start brushing process
        float brushTimer = 10f; // Total time required for brushing
        bool isBrushing = false;

        while (brushTimer > 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartDragging();
            }

            if (isDragging)
            {
                // Perform raycast to check if the brush is over the teeth while dragging
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = 0; // Ensure that the z-position is zero to match your 2D plane

                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);
                bool isOverTeeth = false;
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider != null && hit.collider.gameObject == teeth)
                    {
                        isOverTeeth = true;
                        break;
                    }
                }

                if (isOverTeeth)
                {
                    if (!isBrushing)
                    {
                        isBrushing = true;
                        Foam.SetActive(true); // Activate foam when brushing starts
                    }
                    brushTimer -= Time.deltaTime; // Reduce timer if brushing is occurring
                }
                else
                {
                    if (isBrushing)
                    {
                        isBrushing = false;
                        Foam.SetActive(false); // Deactivate foam if the brush is not over the teeth
                    }
                }
            }
            else
            {
                if (isBrushing)
                {
                    isBrushing = false;
                    Foam.SetActive(false); // Deactivate foam if dragging stops
                }
            }

            yield return null; // Wait until the next frame
        }

        Foam.SetActive(false); // Ensure foam is deactivated after brushing is complete
        yield return StartCoroutine(ZoomOutCamera()); // Zoom out after brushing is done

        // Reset brush to the rest position
        transform.position = restPosition.position;
        boyAnimator.SetBool("isBrushed", true); // Set the animator parameter if brushing is considered complete
        isZoomedIn = false; // Reset zoom state
    }

    private IEnumerator ZoomInOnTeeth()
    {
        isZooming = true;
        zoomCamera.enabled = true;

        float elapsedTime = 0f;
        Vector3 targetPosition = new Vector3(teeth.transform.position.x, teeth.transform.position.y, originalCameraPosition.z); // Maintain original Z position
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
    }

    private IEnumerator ZoomOutCamera()
    {
        isZooming = true;
        Collider2D collider = this.GetComponent<Collider2D>();

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
        zoomCameraViewportHandler.enabled = true;
        zoomCamera.enabled = false;

        isZooming = false;
        isZoomedIn = false;

        if (boyAnimator != null)
        {
            boyAnimator.SetBool("isZoomedOut", true);
        }

        collider.enabled = false;
    }

    private IEnumerator HandleAnimationCompletion()
    {
        yield return new WaitUntil(() => brushAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f && !brushAnimator.IsInTransition(0));
    }

    private void CheckComponents()
    {
        if (dropTarget == null || teeth == null || paste == null || GetComponent<Collider2D>() == null || brushBackSprite == null)
        {
            Debug.LogError("One or more required components are missing or not assigned in the inspector.");
        }

        if (mainCamera == null || zoomCamera == null || mainCameraViewportHandler == null || zoomCameraViewportHandler == null)
        {
            Debug.LogError("Camera references or viewport handler is missing.");
        }
    }
}
