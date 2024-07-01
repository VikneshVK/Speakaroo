using UnityEngine;

public class BrushController : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 initialPosition;
    private Animator brushAnimator;
    private Animator characterAnimator;
    public GameObject boyGameObject; // Reference to the boy game object
    public GameObject openMouth; // Reference to the openMouth game object
    public LayerMask interactableLayer; // Layer mask to filter the raycast

    void Start()
    {
        Debug.Log("BrushController Start");

        // Get the Animator component attached to this game object (brush)
        brushAnimator = GetComponent<Animator>();

        // Ensure boyGameObject is assigned
        if (boyGameObject != null)
        {
            // Get the Animator component attached to the boy game object
            characterAnimator = boyGameObject.GetComponent<Animator>();
        }
        else
        {
            Debug.LogError("Boy GameObject not assigned in the inspector.");
        }

        // Ensure openMouth is assigned
        if (openMouth == null)
        {
            Debug.LogError("OpenMouth GameObject not assigned in the inspector.");
        }

        // Ensure Collider2D is attached
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogError("No Collider2D attached to the brush game object.");
        }
    }

    void Update()
    {
        if (isDragging)
        {
            Debug.Log("Dragging");
            // Update the position of the brush to follow the mouse
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f; // Ensure the brush stays on the same plane
            transform.position = mousePosition;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            // Raycast to check if the brush is clicked
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, interactableLayer);
            Debug.Log("Raycast hit: " + hit.collider);
            if (hit.collider != null && hit.collider.gameObject.CompareTag("brush"))
            {
                Debug.Log("Brush clicked");
                isDragging = true;
                initialPosition = transform.position;
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            Debug.Log("OnMouseUp called");
            // Stop dragging
            isDragging = false;

            // Check if dropped on the boy game object
            if (IsDroppedOnBoy())
            {
                
                transform.rotation = Quaternion.Euler(0, 0, 0);

                // Set the animation parameters
                brushAnimator.SetBool("pasteOn", true);
                characterAnimator.SetBool("openTeeth", true);
                openMouth.SetActive(true);                
            }
            else
            {
                
                transform.position = initialPosition;
            }
        }
    }

    private bool IsDroppedOnBoy()
    {
        // Get the bounds of the boy game object
        Collider2D boyCollider = boyGameObject.GetComponent<Collider2D>();
        if (boyCollider != null)
        {
            return boyCollider.bounds.Contains(transform.position);
        }
        else
        {
            Debug.LogError("Collider2D component not found on the boy GameObject.");
            return false;
        }
    }
}
