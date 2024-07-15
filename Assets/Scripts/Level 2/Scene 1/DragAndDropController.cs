using UnityEngine;

public class DragAndDropController : MonoBehaviour
{
    public Transform correctDropZone;
    public Animator boyAnimator;
    public float blinkDuration = 0.1f;
    public int blinkCount = 2;
    public float dropOffset = 0.5f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isDragging = false;
    private Renderer objectRenderer;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        objectRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            CheckDrop();
        }

        if (isDragging)
        {
            DragObject();
        }
    }

    void DragObject()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePosition.x, mousePosition.y, originalPosition.z);
    }

    void CheckDrop()
    {
        if (IsCorrectDropZone())
        {
            boyAnimator.SetBool("isRightDrop", true);
            transform.position = correctDropZone.position;
            transform.rotation = correctDropZone.rotation; // Match the rotation
        }
        else
        {
            boyAnimator.SetBool("isWrongDrop", true);
            StartCoroutine(BlinkRedAndReset());
        }
    }

    bool IsCorrectDropZone()
    {
        // Check if the object is dropped within the specified offset range
        return Vector3.Distance(transform.position, correctDropZone.position) <= dropOffset;
    }

    System.Collections.IEnumerator BlinkRedAndReset()
    {
        Color originalColor = objectRenderer.material.color;

        for (int i = 0; i < blinkCount; i++)
        {
            objectRenderer.material.color = Color.red;
            yield return new WaitForSeconds(blinkDuration);
            objectRenderer.material.color = originalColor;
            yield return new WaitForSeconds(blinkDuration);
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation; // Reset to the original rotation
    }
}
