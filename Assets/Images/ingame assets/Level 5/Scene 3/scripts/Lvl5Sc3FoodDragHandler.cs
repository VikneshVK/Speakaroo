using UnityEngine;

public class Lvl5Sc3FoodDragHandler : MonoBehaviour
{
    private Vector3 startPosition;
    private bool isDragging = false;
    private bool isRightFood;
    private Transform dropTarget;
    private Collider2D dropTargetCollider;
    private Lvl5Sc3FeedingManager feedingManager;
    private Vector3 offset;
    private Transform glowObject;

    private void Start()
    {
        startPosition = transform.position;
        feedingManager = FindObjectOfType<Lvl5Sc3FeedingManager>();

        isRightFood = gameObject.name.Contains("RightFood");

        dropTarget = transform.parent.Find("DropTarget");
        dropTargetCollider = dropTarget.GetComponent<Collider2D>();
        glowObject = transform.Find("Glow 2");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;

                offset = transform.position - (Vector3)mousePos;
                if (glowObject != null)
                {
                    LeanTween.scale(glowObject.gameObject, Vector3.zero, 0.3f).setEaseInOutQuad();
                }
            }

        }

        if (isDragging)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mousePos + (Vector2)offset; 
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;

            if (dropTargetCollider.OverlapPoint(transform.position))
            {
                feedingManager.OnFoodDropped(transform, isRightFood);
            }
            else
            {
                transform.position = startPosition;
                if (glowObject != null)
                {
                    LeanTween.scale(glowObject.gameObject, Vector3.one * 6, 0.3f).setEaseInOutQuad();
                }
            }
        }
    }
}
