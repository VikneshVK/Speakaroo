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
        canPan = true;
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
        if (!canPan) return; // Do nothing if panning is disabled

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began: // Start of the swipe

                    startTouchPosition = touch.position;
                    isSwiping = true;
                    break;

                case TouchPhase.Moved: // Detect swipe and update the camera's target position

                    if (isSwiping)
                    {
                        currentTouchPosition = touch.position;
                        float swipeDelta = startTouchPosition.x - currentTouchPosition.x;                        
                        targetPosition.x += swipeDelta * panSpeed * Time.deltaTime; // Adjust target position based on swipe distance                       
                        startTouchPosition = currentTouchPosition; // Update start touch position to the current position for smooth swiping
                    }
                    break;

                case TouchPhase.Ended:                    
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
        // Move the leftmost ground to the rightmost position
        leftGround.transform.position = new Vector3(rightGround.transform.position.x + groundWidth, 0, 0);        
        GameObject temp = leftGround;   // Reassign the ground references for the next cycle
        leftGround = centerGround;
        centerGround = rightGround;
        rightGround = temp;
    }

    // Function to cycle the grounds to the left
    void CycleGroundsLeft()
    {
        // Move the rightmost ground to the leftmost position
        rightGround.transform.position = new Vector3(leftGround.transform.position.x - groundWidth, 0, 0);        
        GameObject temp = rightGround;  // Reassign the ground references for the next cycle
        rightGround = centerGround;
        centerGround = leftGround;
        leftGround = temp;
    }

    public void SetPanningEnabled(bool enabled)
    {
        canPan = enabled;
    }

}
