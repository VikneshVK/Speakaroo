using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DraggableToothpaste : MonoBehaviour
{
    public GameObject pasteGelPrefab; // Prefab for the paste gel
    private Vector3 offset;
    private Vector3 originalPosition;
    private bool isDragging = false;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found. Ensure your camera is tagged as 'MainCamera'.");
        }
        originalPosition = transform.position;
    }

    private void OnMouseDown()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found.");
            return;
        }

        offset = transform.position - GetMouseWorldPosition();
        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePosition = GetMouseWorldPosition() + offset;
            transform.position = new Vector3(mousePosition.x, mousePosition.y, transform.position.z);
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;

        Debug.Log("Mouse released, checking for colliders...");

        Collider2D[] colliders = Physics2D.OverlapPointAll(GetMouseWorldPosition());
        foreach (var collider in colliders)
        {
            Debug.Log($"Detected collider with tag: {collider.tag}");
            if (collider.CompareTag("Paste"))
            {
                Debug.Log("Brush detected, instantiating paste gel.");
                // Instantiate the paste gel at the specific position with the specified rotation
                Vector3 pasteGelPosition = new Vector3(4.15f, -4.1f, 0f);
                Quaternion pasteGelRotation = Quaternion.Euler(0, 0, 22);
                Instantiate(pasteGelPrefab, pasteGelPosition, pasteGelRotation);
                transform.position = originalPosition;
                return; // Exit after instantiating to avoid returning to the original position
            }
        }

        Debug.Log("No brush detected, returning to original position.");
        transform.position = originalPosition; // Return to original position if not over the toothbrush
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = mainCamera.WorldToScreenPoint(transform.position).z;
        return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
    }
}
