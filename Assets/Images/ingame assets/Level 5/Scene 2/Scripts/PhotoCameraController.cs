using UnityEngine;

public class PhotoCameraController : MonoBehaviour
{
    public GameObject ground1;
    public GameObject ground2;
    public GameObject ground3;
    public float panSpeed;
    public float inertiaDamping = 0.9f; // How quickly the inertia slows down

    private GameObject leftGround;
    private GameObject centerGround;
    private GameObject rightGround;
    private float groundWidth;

    private Vector3 velocity; // To track swipe speed for inertia
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

        canPan = true;
    }

    void Update()
    {
        if (!canMove) return;

        HandleSwipeInput();
        ApplyInertia();
        HandleGroundCycling();
    }

    void HandleSwipeInput()
    {
        if (!canPan) return;

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
            isSwiping = true;
            velocity = Vector3.zero; // Reset velocity
        }
        else if (Input.GetMouseButton(0) && isSwiping)
        {
            currentTouchPosition = Input.mousePosition;
            float swipeDelta = startTouchPosition.x - currentTouchPosition.x;

            transform.position += new Vector3(swipeDelta * panSpeed * Time.deltaTime, 0, 0);
            velocity = new Vector3(-swipeDelta * panSpeed, 0, 0); // Update velocity for inertia
            startTouchPosition = currentTouchPosition;

            helperController?.DestroyHelperHand();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isSwiping = false;
        }
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPosition = touch.position;
                    isSwiping = true;
                    velocity = Vector3.zero; // Reset velocity
                    break;

                case TouchPhase.Moved:
                    currentTouchPosition = touch.position;
                    float swipeDelta = startTouchPosition.x - currentTouchPosition.x;

                    transform.position += new Vector3(swipeDelta * panSpeed * Time.deltaTime, 0, 0);
                    velocity = new Vector3(-swipeDelta * panSpeed, 0, 0); // Update velocity for inertia
                    startTouchPosition = currentTouchPosition;

                    helperController?.DestroyHelperHand();
                    break;

                case TouchPhase.Ended:
                    isSwiping = false;
                    break;
            }
        }
#endif
    }

    void ApplyInertia()
    {
        if (!isSwiping && velocity.magnitude > 0.01f)
        {
            transform.position += velocity * Time.deltaTime;
            velocity *= inertiaDamping; // Reduce velocity over time
        }
        else
        {
            velocity = Vector3.zero;
        }
    }

    void HandleGroundCycling()
    {
        if (leftGround == null || centerGround == null || rightGround == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        float cameraHalfWidth = cam.orthographicSize * cam.aspect;

        if (transform.position.x + cameraHalfWidth > centerGround.transform.position.x + groundWidth / 2)
        {
            CycleGroundsRight();
        }
        if (transform.position.x - cameraHalfWidth < centerGround.transform.position.x - groundWidth / 2)
        {
            CycleGroundsLeft();
        }
    }

    void CycleGroundsRight()
    {
        leftGround.transform.position = new Vector3(rightGround.transform.position.x + groundWidth, 0, 0);
        GameObject temp = leftGround;
        leftGround = centerGround;
        centerGround = rightGround;
        rightGround = temp;
    }

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

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
    }
}
