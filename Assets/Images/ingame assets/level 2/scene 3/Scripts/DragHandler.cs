using UnityEngine;
using System.Collections;

public class DragHandler : MonoBehaviour
{
    public Transform targetPosition;  // The correct target position for the object
    public GameObject nextGameObject;  // Reference to the next object to be activated
    public dragManager dragManager;  // Reference to the dragManager
    private Collider2D objectCollider;  // Collider for this object
    private bool isDragging = false;  // Whether the object is being dragged
    private Vector2 originalPosition;  // The original position of the object
    private bool isDroppedSuccessfully = false;  // To prevent multiple drops
    public bool IsDragged => isDragging;
   /* private AnchorGameObject anchor;*/
    public HelperPointer helperPointer;

    void Awake()
    {
        objectCollider = GetComponent<Collider2D>();
        /*anchor = GetComponent<AnchorGameObject>();*/
        
    }

    void OnEnable()
    {
        // Ensure the HelperPointer and Collider2D are properly initialized
        if (helperPointer != null && objectCollider.enabled)
        {
            helperPointer.ScheduleHelperHand(this, dragManager);
        }
    }

    void Start()
    {
        objectCollider.enabled = false;  // Initially disable all colliders until activated by dragManager
       /* anchor = GetComponent<AnchorGameObject>();*/
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseDown();
        }
        if (Input.GetMouseButton(0) && isDragging)
        {
            OnMouseDrag();
        }
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            OnMouseUp();
        }
    }

    void OnMouseDown()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (objectCollider.enabled && objectCollider == Physics2D.OverlapPoint(mousePosition))
        {
            isDragging = true;
            originalPosition = transform.position;
            Debug.Log($"{gameObject.name} collider is active and interaction started.");
            
        }
        
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        transform.position = new Vector2(mousePosition.x, mousePosition.y);

        helperPointer.StopHelperHand();
        Debug.Log($"{gameObject.name} is being dragged to position {transform.position}");
    }

    void OnMouseUp()
    {
        isDragging = false;

        if (Vector2.Distance(transform.position, targetPosition.position) < 2.0f)  // Adjust threshold as needed
        {
            if (!isDroppedSuccessfully)
            {
                OnSuccessfulDrop();  
            }
        }
        else
        {
            OnFailedDrop();  
        }
    }

    void OnSuccessfulDrop()
    {
        if (isDroppedSuccessfully) return;  // Prevent multiple drops

        isDroppedSuccessfully = true;
        objectCollider.enabled = false;  // Disable this object's collider to prevent re-triggering

        // Move object to the target position
        LeanTween.move(gameObject, targetPosition.position, 0.5f).setEaseInOutQuad().setOnComplete(() =>
        {
            dragManager.OnItemDropped(true);  // Inform dragManager of successful drop
            Debug.Log($"{gameObject.name} dropped successfully.");
        });          

        Destroy(targetPosition.gameObject);  // Clean up target position
    }

    void OnFailedDrop()
    {
        // Return object to its original position if the drop fails
        LeanTween.move(gameObject, originalPosition, 0.5f).setEaseInOutQuad().setOnComplete(() =>
        {
            dragManager.OnItemDropped(false);  // Inform dragManager of failed drop
            Debug.Log($"{gameObject.name} failed to drop correctly.");
        });

        // Reschedule helper hand after failed drop
        if (helperPointer != null)
        {
            helperPointer.ScheduleHelperHand(this, dragManager, 10f);  // Reschedule helper hand after 10 seconds
        }

        Debug.Log("Drop failed, retry.");
    }
}
