using UnityEngine;

public class DragScript : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;
    private RotateOnDrag rotateOnDragScript;
    public bool canDrag = true; // Flag to control dragging

    // Define the allowed drag area in normalized viewport coordinates (25% margins)
    private float minX = 0.25f; // 25% from the left of the viewport
    private float maxX = 0.5f; // 25% from the right of the viewport
    private float minY = 0.1f; // 25% from the bottom of the viewport
    private float maxY = 0.75f; // 25% from the top of the viewport

    private void Start()
    {
        rotateOnDragScript = GetComponent<RotateOnDrag>();
        rotateOnDragScript.enabled = false;
    }

    private void OnMouseDown()
    {
        if (!canDrag) return; // Prevent drag if not allowed

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        offset = transform.position - worldPosition;
        isDragging = true;

        if (rotateOnDragScript != null)
        {
            rotateOnDragScript.enabled = true; // Enable rotation script on drag
        }
    }

    private void OnMouseDrag()
    {
        if (isDragging && canDrag) // Check canDrag on drag as well
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition) + offset;

            Vector3 viewportPos = Camera.main.WorldToViewportPoint(worldPosition);

            viewportPos.x = Mathf.Clamp(viewportPos.x, minX, maxX);
            viewportPos.y = Mathf.Clamp(viewportPos.y, minY, maxY);

            Vector3 clampedWorldPos = Camera.main.ViewportToWorldPoint(viewportPos);
            clampedWorldPos.z = transform.position.z; // Preserve original Z position

            transform.position = clampedWorldPos;
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        if (rotateOnDragScript != null)
        {
            rotateOnDragScript.enabled = false; // Disable rotation script when drag ends
        }
    }
}
