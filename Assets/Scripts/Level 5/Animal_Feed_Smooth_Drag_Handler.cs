using UnityEngine;
using UnityEngine.EventSystems;

public class Animal_Feed_Smooth_Drag_Handler : MonoBehaviour
{
    public GameObject correctFood;
    public GameObject incorrectFood;
    public Transform dropZone;
    public float dropZoneThreshold = 1f;  // Adjust this value to increase or decrease the threshold

    private Vector3 offset;
    private Vector3 initialPosition;
    private bool isDragging = false;
    private bool isCorrectFood = false;

    private Level_5_Animal_Feed_Manager levelManager;

    private void Start()
    {
        initialPosition = transform.position;
        levelManager = FindObjectOfType<Level_5_Animal_Feed_Manager>();
        isCorrectFood = (gameObject == correctFood);
    }

    private void OnMouseDown()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mousePosition.x, mousePosition.y, 0f);
        isDragging = true;
        levelManager.OnFoodDragStarted();
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x, mousePosition.y, 0f) + offset;
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        if (IsDroppedInDropZone())
        {
            if (isCorrectFood)
            {
                levelManager.OnCorrectFoodDropped();
                gameObject.SetActive(false);
            }
            else
            {
                levelManager.OnIncorrectFoodDropped();
                gameObject.SetActive(false);
            }
        }
        else
        {
            ResetPosition();
            levelManager.OnFoodDragEnded(false);
        }
    }

    private bool IsDroppedInDropZone()
    {
        Collider2D foodCollider = GetComponent<Collider2D>();
        Collider2D dropZoneCollider = dropZone.GetComponent<Collider2D>();

        // Create a larger bounds around the drop zone
        Bounds largerDropZoneBounds = dropZoneCollider.bounds;
        largerDropZoneBounds.Expand(dropZoneThreshold);

        // Check if the food’s collider is within the expanded drop zone bounds
        return largerDropZoneBounds.Intersects(foodCollider.bounds);
    }

    private void ResetPosition()
    {
        transform.position = initialPosition;
    }
}
