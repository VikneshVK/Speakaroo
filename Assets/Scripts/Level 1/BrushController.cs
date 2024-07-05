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
        HandleDragging();
        HandleMouseInput();
    }

    private void HandleDragging()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;
            transform.position = mousePosition;
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
        if (dropTarget != null && Vector3.Distance(transform.position, dropTarget.transform.position) < 0.5f)
        {
            Debug.Log("Drop successful.");
            HandleSuccessfulDrop(dropTarget);
        }
        else if (teeth != null && Vector3.Distance(transform.position, teeth.transform.position) < 0.5f)
        {
            Debug.Log("Drop on teeth successful.");
            HandleSuccessfulDrop(teeth);
        }
        else
        {
            Debug.Log("Drop failed, returning to initial position.");
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
        else if (target == teeth)
        {
            StartCoroutine(ZoomAndHandleFoam());
        }
    }

    private IEnumerator ZoomAndHandleFoam()
    {
        // Deactivate main camera and zoom camera viewport handler
        mainCamera.enabled = false;
        zoomCameraViewportHandler.enabled = false;

        yield return StartCoroutine(ZoomInOnTeeth());

        Foam.SetActive(true);
        Destroy(teeth);

        yield return new WaitForSeconds(10); // Wait for 10 seconds

        yield return StartCoroutine(ZoomOutCamera()); // First zoom out

        Foam.SetActive(false); // Then deactivate foam
        GetComponent<Collider2D>().enabled = false; // Deactivate foam collider

        // Reset brush to the rest position and disable dragging
        transform.position = restPosition.position;
        isDragging = false; // Ensure brush is not draggable

        boyAnimator.SetBool("isBrushed", true); // Set the animator parameter
        StartCoroutine(CheckTeethShineAnimation());
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

        Debug.Log("Zoom In Completed");
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

        Debug.Log("Zoom Out Completed");

        if (boyAnimator != null)
        {
            boyAnimator.SetBool("isZoomedOut", true);
        }

        collider.enabled = false;
    }

    private IEnumerator CheckTeethShineAnimation()
    {
        yield return new WaitUntil(() => boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("TeethShine") &&
                                         boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);

        Scene_Manager sceneManager = GameObject.Find("Scene_Manager").GetComponent<Scene_Manager>();
        if (sceneManager != null)
        {
            sceneManager.LoadLevel("Level 2");
        }
        else
        {
            Debug.LogError("SceneManager not found or Scene_Manager script not attached.");
        }
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
