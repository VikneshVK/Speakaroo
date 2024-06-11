using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DraggableObject : MonoBehaviour
{
    private Vector3 offset;
    private Vector3 originalPosition;
    private bool isDragging = false;
    private Camera mainCamera;

    private System.Action<string> onCorrectDrop;
    private System.Action onIncorrectDrop;

    private bool correctlyDropped = false;
    private float snapDistance = 1.0f; // Distance within which the object will snap to the drop point

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found. Ensure your camera is tagged as 'MainCamera'.");
        }
        Debug.Log($"DraggableObject Initialized: {name}, Tag: {tag}");
    }

    public void Initialize(System.Action<string> correctDropCallback, System.Action incorrectDropCallback)
    {
        onCorrectDrop = correctDropCallback;
        onIncorrectDrop = incorrectDropCallback;
        originalPosition = transform.position;
        Debug.Log($"DraggableObject Initialized: {name}, Tag: {tag}");
    }

    void OnMouseDown()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found.");
            return;
        }

        offset = transform.position - GetMouseWorldPosition();
        isDragging = true;
        Debug.Log($"DraggableObject OnMouseDown: {name}, Tag: {tag}");
    }

    void OnMouseUp()
    {
        isDragging = false;

        Debug.Log($"DraggableObject OnMouseUp: {name}, Tag: {tag}");

        Collider2D[] colliders = Physics2D.OverlapPointAll(GetMouseWorldPosition());
        foreach (var collider in colliders)
        {
            DropPoint dropPoint = collider.GetComponent<DropPoint>();
            if (dropPoint != null && dropPoint.CompareTag(tag))
            {
                Debug.Log($"Dropped {name} on {dropPoint.name}, Tag Match: {tag}");
                if (Vector3.Distance(transform.position, collider.transform.position) < snapDistance)
                {
                    transform.position = collider.transform.position;
                    correctlyDropped = true;
                    onCorrectDrop.Invoke(tag);
                    return;
                }
            }
        }

        transform.position = originalPosition;
        onIncorrectDrop.Invoke();
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = GetMouseWorldPosition() + offset;
            transform.position = new Vector3(mousePosition.x, mousePosition.y, transform.position.z);
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = mainCamera.WorldToScreenPoint(transform.position).z;
        return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
    }

    public bool IsCorrectlyDropped()
    {
        return correctlyDropped;
    }
}
