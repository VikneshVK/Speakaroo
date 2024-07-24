using UnityEngine;

public class DragScript : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;
    private RotateOnDrag rotateOnDragScript;

    private void Start()
    {
        rotateOnDragScript = GetComponent<RotateOnDrag>();
        if (rotateOnDragScript != null)
        {
            rotateOnDragScript.enabled = false; // Ensure the script is disabled initially
        }
    }

    private void OnMouseDown()
    {
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
        if (isDragging)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition) + offset;

            transform.position = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
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
