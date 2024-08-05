using UnityEngine;

public class DraggingController2 : MonoBehaviour
{
    private Vector3 offset;
    private Vector3 startPosition;
    private bool isDragging = false;
    private Vector3 originalScale;  // Store the original scale of the object
    private SpriteChangeController spriteChangeController;

    void Start()
    {
        originalScale = transform.localScale;  // Initialize original scale
        spriteChangeController = FindObjectOfType<SpriteChangeController>();  // Get the SpriteChangeController
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x + offset.x, mousePosition.y + offset.y, transform.position.z);
            transform.localScale = originalScale * 1.1f;  // Scale up by 10%
            spriteChangeController.ActivateBlenderSprite();  // Activate active blender sprite
        }
        else
        {
            transform.localScale = originalScale;  // Reset scale when not dragging
        }
    }

    void OnMouseDown()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - mousePosition;
        startPosition = transform.position;
        isDragging = true;
    }

    void OnMouseUp()
    {
        isDragging = false;
        transform.position = startPosition;
        spriteChangeController.ResetBlender();  // Reset blender sprite to original after dropping
        if (IsOverlappingBlenderJar())
        {
            spriteChangeController.UpdateBlenderJarSprite(gameObject.tag);  // Update blender sprite based on fruit tag
        }
    }

    private bool IsOverlappingBlenderJar()
    {
        Collider2D collider = GetComponent<Collider2D>();
        Collider2D blenderJarCollider = GameObject.FindGameObjectWithTag("Blender_Jar").GetComponent<Collider2D>();
        return collider.bounds.Intersects(blenderJarCollider.bounds);
    }
}