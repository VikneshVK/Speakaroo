using UnityEngine;

public class PhotoCameraController : MonoBehaviour
{
    public GameObject ground1;
    public GameObject ground2;
    public GameObject ground3;
    public float panSpeed = 0.5f;

    private GameObject leftGround;
    private GameObject centerGround;
    private GameObject rightGround;
    private float groundWidth;

    private Vector3 targetPosition;
    private Vector2 startTouchPosition;
    private Vector2 currentTouchPosition;
    private bool isSwiping = false;
    public bool canPan;
    public bool canMove = true; // Flag to control overall camera movement

    // Reference to LVL5Sc2HelperController
    public LVL5Sc2HelperController helperController;

    void Start()
    {
        leftGround = ground1;
        centerGround = ground2;
        rightGround = ground3;

        Renderer groundRenderer = ground1.GetComponent<Renderer>();
        groundWidth = groundRenderer.bounds.size.x;

        leftGround.transform.position = new Vector3(-groundWidth, 0, 0);
        centerGround.transform.position = new Vector3(0, 0, 0);
        rightGround.transform.position = new Vector3(groundWidth, 0, 0);

        targetPosition = transform.position;
        canPan = true;
    }

    void Update()
    {
        // Check if movement is allowed
        if (!canMove) return;

        HandleSwipeInput();
        MoveCamera();
        HandleGroundCycling();
    }

    // Function to handle swipe input
    void HandleSwipeInput()
    {
        if (!canPan) return;

#if UNITY_EDITOR
        // Use mouse input for testing in the editor
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
            isSwiping = true;
        }
        else if (Input.GetMouseButton(0) && isSwiping)
        {
            currentTouchPosition = Input.mousePosition;
            float swipeDelta = startTouchPosition.x - currentTouchPosition.x;
            targetPosition.x += swipeDelta * panSpeed * Time.deltaTime;
            startTouchPosition = currentTouchPosition;

            if (helperController != null)
            {
                helperController.DestroyHelperHand();
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isSwiping = false;
        }

#else
    // Use touch input for mobile
    if (Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                startTouchPosition = touch.position;
                isSwiping = true;
                break;

            case TouchPhase.Moved:
                if (isSwiping)
                {
                    currentTouchPosition = touch.position;
                    float swipeDelta = startTouchPosition.x - currentTouchPosition.x;
                    targetPosition.x += swipeDelta * panSpeed * Time.deltaTime;
                    startTouchPosition = currentTouchPosition;

                    if (helperController != null)
                    {
                        helperController.DestroyHelperHand();
                    }
                }
                break;

            case TouchPhase.Ended:
                isSwiping = false;
                break;
        }
    }
#endif
    }


    // Move the camera smoothly to the target position
    void MoveCamera()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * panSpeed);
    }

    // Handle cycling the ground objects based on the camera's position
    void HandleGroundCycling()
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("Main camera is not found!");
            return; // Early exit to prevent further errors
        }

        Vector3 cameraPosition = cam.transform.position;

        // Ensure ground references are not null
        if (leftGround == null || centerGround == null || rightGround == null)
        {
            Debug.LogError("Ground references are not assigned properly!");
            return;
        }

        float cameraHalfWidth = cam.orthographicSize * cam.aspect;

        if (cameraPosition.x + cameraHalfWidth > centerGround.transform.position.x + groundWidth / 2)
        {
            CycleGroundsRight();
        }
        if (cameraPosition.x - cameraHalfWidth < centerGround.transform.position.x - groundWidth / 2)
        {
            CycleGroundsLeft();
        }
    }

    // Function to cycle the grounds to the right
    void CycleGroundsRight()
    {
        leftGround.transform.position = new Vector3(rightGround.transform.position.x + groundWidth, 0, 0);
        GameObject temp = leftGround;
        leftGround = centerGround;
        centerGround = rightGround;
        rightGround = temp;
    }

    // Function to cycle the grounds to the left
    void CycleGroundsLeft()
    {
        rightGround.transform.position = new Vector3(leftGround.transform.position.x - groundWidth, 0, 0);
        GameObject temp = rightGround;
        rightGround = centerGround;
        centerGround = leftGround;
        leftGround = temp;
    }

    public void SetPanningEnabled(bool enabled)
    {
        canPan = enabled;
    }

    // Method to enable/disable camera movement
    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
    }
}
