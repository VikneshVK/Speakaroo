using UnityEngine;

public class DraggingController2 : MonoBehaviour
{
    private Vector3 offset;
    private Vector3 startPosition;
    private bool isDragging = false;
    private Transform blenderTransform;
    private SpriteRenderer blenderSpriteRenderer;
    private Sprite originalBlenderSprite;
    private Sprite activeBlenderSprite;

    void Start()
    {
        blenderTransform = GameObject.FindGameObjectWithTag("Blender").transform;
        blenderSpriteRenderer = blenderTransform.GetComponent<SpriteRenderer>();
        originalBlenderSprite = blenderSpriteRenderer.sprite;
        activeBlenderSprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/blender_active");
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x + offset.x, mousePosition.y + offset.y, transform.position.z);
            blenderSpriteRenderer.sprite = activeBlenderSprite; 
        }
    }

    private void OnMouseDown()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - mousePosition;
        startPosition = transform.position; 
        isDragging = true;
    }

    private void OnMouseUp()
    {
        isDragging = false;
        blenderSpriteRenderer.sprite = originalBlenderSprite; 
        if (IsOverlappingBlender())
        {
            UpdateBlenderSprite(); 
            Debug.Log(gameObject.name + " dropped on the blender and blended!");
        }
        transform.position = startPosition; 
    }

    private bool IsOverlappingBlender()
    {
        Collider2D collider = GetComponent<Collider2D>();
        Collider2D blenderCollider = blenderTransform.GetComponent<Collider2D>();

        return collider.bounds.Intersects(blenderCollider.bounds);
    }

    private void UpdateBlenderSprite()
    {
        string spritePath = "Images/LVL 4 scene 2/" + gameObject.name.ToLower() + "_blender";
        Sprite newBlenderSprite = Resources.Load<Sprite>(spritePath);
        if (newBlenderSprite != null)
        {
            blenderSpriteRenderer.sprite = newBlenderSprite;
        }
        else
        {
            Debug.LogError("Sprite not found at path: " + spritePath);
            blenderSpriteRenderer.sprite = originalBlenderSprite;
        }
    }
}
