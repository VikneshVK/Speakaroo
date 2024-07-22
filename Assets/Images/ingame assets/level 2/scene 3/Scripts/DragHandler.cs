using UnityEngine;

public class DragHandler : MonoBehaviour
{
    public Transform targetPosition;
    public GameObject nextGameObject;
    public Animator birdAnimator;
    public dragManager dragManager;

    private Vector2 originalPosition;
    private bool isDragging = false;
    private Collider2D objectCollider;
    private AnchorGameObject anchor;

    void Start()
    {
        objectCollider = GetComponent<Collider2D>();
        objectCollider.enabled = false; // Initially disable all colliders
        anchor = GetComponent<AnchorGameObject>();
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
        if (objectCollider == Physics2D.OverlapPoint(mousePosition))
        {
            isDragging = true;
            originalPosition = transform.position;
            if (anchor != null) 
            {
                anchor.enabled = false;
            }
        }
    }

    void OnMouseDrag()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector2(mousePosition.x, mousePosition.y);
    }

    void OnMouseUp()
    {
        isDragging = false;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Vector2.Distance(mousePosition, targetPosition.position) < 0.5f) // Adjust the threshold as needed
        {
            transform.position = targetPosition.position;
            OnSuccessfulDrop();
        }
        else
        {
            transform.position = originalPosition;
        }
    }

    void OnSuccessfulDrop()
    {
        objectCollider.enabled = false;
        dragManager.OnItemDropped();
        if (nextGameObject != null)
        {
            nextGameObject.GetComponent<Collider2D>().enabled = true;
        }
        Debug.Log("Total Correct Drops: " + dragManager.totalCorrectDrops);
        Destroy(targetPosition.gameObject);
    }
}

