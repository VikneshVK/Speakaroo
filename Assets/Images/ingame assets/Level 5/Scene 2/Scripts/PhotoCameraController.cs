using UnityEngine;

public class PhotoCameraController : MonoBehaviour
{
    public GameObject ground1; // Reference to ground 1
    public GameObject ground2; // Reference to ground 2
    public GameObject ground3; // Reference to ground 3
    public float panSpeed = 0.5f; // Speed of camera panning

    private GameObject leftGround;    // Ground object currently at the left
    private GameObject centerGround;  // Ground object currently at the center
    private GameObject rightGround;   // Ground object currently at the right
    private float groundWidth;        // The width of the ground objects

    private Vector3 targetPosition;
    private Vector2 startTouchPosition;
    private Vector2 currentTouchPosition;
    private bool isSwiping = false;

    void Start()
    {
        // Initialize the ground positions and calculate the width of one ground object
        leftGround = ground1;
        centerGround = ground2;
        rightGround = ground3;

        Renderer groundRenderer = ground1.GetComponent<Renderer>();
        groundWidth = groundRenderer.bounds.size.x;

        // Position the ground objects at the start of the game
        leftGround.transform.position = new Vector3(-groundWidth, 0, 0);
        centerGround.transform.position = new Vector3(0, 0, 0);
        rightGround.transform.position = new Vector3(groundWidth, 0, 0);

        // Initialize the camera target position
        targetPosition = transform.position;
    }

    void Update()
    {
        HandleSwipeInput();
        MoveCamera();
        HandleGroundCycling();
    }

    // Function to handle swipe input
    void HandleSwipeInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // Start of the swipe
                    startTouchPosition = touch.position;
                    isSwiping = true;
                    break;

                case TouchPhase.Moved:
                    // Detect swipe and update the camera's target position
                    if (isSwiping)
                    {
                        currentTouchPosition = touch.position;
                        float swipeDelta = startTouchPosition.x - currentTouchPosition.x;

                        // Adjust target position based on swipe distance
                        targetPosition.x += swipeDelta * panSpeed * Time.deltaTime;

                        // Update start touch position to the current position for smooth swiping
                        startTouchPosition = currentTouchPosition;
                    }
                    break;

                case TouchPhase.Ended:
                    // End of the swipe
                    isSwiping = false;
                    break;
            }
        }
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
        Vector3 cameraPosition = cam.transform.position;

        // Get the camera's half width in world units
        float cameraHalfWidth = cam.orthographicSize * cam.aspect;

        // Right side panning: If the camera reaches the right edge of the center ground
        if (cameraPosition.x + cameraHalfWidth > centerGround.transform.position.x + groundWidth / 2)
        {
            CycleGroundsRight();
        }

        // Left side panning: If the camera reaches the left edge of the center ground
        if (cameraPosition.x - cameraHalfWidth < centerGround.transform.position.x - groundWidth / 2)
        {
            CycleGroundsLeft();
        }
    }

    // Function to cycle the grounds to the right
    void CycleGroundsRight()
    {
        // Move the leftmost ground to the rightmost position
        leftGround.transform.position = new Vector3(rightGround.transform.position.x + groundWidth, 0, 0);

        // Reassign the ground references for the next cycle
        GameObject temp = leftGround;
        leftGround = centerGround;
        centerGround = rightGround;
        rightGround = temp;
    }

    // Function to cycle the grounds to the left
    void CycleGroundsLeft()
    {
        // Move the rightmost ground to the leftmost position
        rightGround.transform.position = new Vector3(leftGround.transform.position.x - groundWidth, 0, 0);

        // Reassign the ground references for the next cycle
        GameObject temp = rightGround;
        rightGround = centerGround;
        centerGround = leftGround;
        leftGround = temp;
    }
}
