using UnityEngine;

public class DishdragController : MonoBehaviour
{
    public Transform dropTarget; // The target location to drop the object
    public Sprite[] bowlSprites; // Array of sprites for when bowls are dropped
    public Sprite newSprite; // The new sprite for glass and plates (non-bowl items)
    public Animator birdAnimator; // Reference to the bird Animator

    private Vector3 startPosition; // Store the start position for drag-resetting
    private bool isDragging = false;
    private static int bowlDropCount = 0; // Static counter to track how many bowls have been dropped
    public static int dishesArranged = 0; // Static variable to count total dishes arranged

    private void Start()
    {
        startPosition = transform.position; // Store the initial position for reset if needed

        // Initialize the static counter
        if (dishesArranged == 0)
        {
            dishesArranged = 0;
        }
    }

    void Update()
    {
        HandleMouseInput(); // Update function to handle mouse/touch input
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseDown();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            OnMouseDrag();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            OnMouseUp();
        }
    }

    private void OnMouseDown()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);
        if (hitCollider != null && hitCollider.gameObject == gameObject)
        {
            isDragging = true; // Start dragging when touched or clicked
        }
    }

    private void OnMouseDrag()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePosition.x, mousePosition.y, transform.position.z);
    }

    private void OnMouseUp()
    {
        isDragging = false;

        // Check if the object was dropped in the correct location (dropTarget)
        if (Vector3.Distance(transform.position, dropTarget.position) < 1.0f)
        {
            OnDropped();
        }
        else
        {
            // Reset position if not dropped correctly
            transform.position = startPosition;
        }
    }

    private void OnDropped()
    {
        SpriteRenderer targetSpriteRenderer = dropTarget.GetComponent<SpriteRenderer>();

        if (gameObject.name.Contains("Bowl"))
        {
            // If the dropped object is a bowl, handle bowl-specific logic
            HandleBowlDrop(targetSpriteRenderer);
        }
        else
        {
            // For other objects (non-bowls), just change the sprite and destroy the object
            if (targetSpriteRenderer != null)
            {
                targetSpriteRenderer.sprite = newSprite; // Set the new sprite for plates/glasses
            }
            Destroy(gameObject); // Destroy the dropped object

            // Increment the arranged dishes counter
            dishesArranged++;
            CheckDishesArranged();
        }
    }

    private void HandleBowlDrop(SpriteRenderer targetSpriteRenderer)
    {
        // Increase the bowl drop count
        bowlDropCount++;

        // Change the sprite of the target (bowl-sillout) based on how many bowls have been dropped
        if (bowlDropCount <= bowlSprites.Length)
        {
            // Update the sprite based on how many bowls have been dropped
            targetSpriteRenderer.sprite = bowlSprites[bowlDropCount - 1]; // Set sprite based on drop count
        }

        // Destroy the current bowl object after it's dropped
        Destroy(gameObject);

        // Increment the arranged dishes counter
        dishesArranged++;
        CheckDishesArranged();
    }

    private void CheckDishesArranged()
    {
        if (dishesArranged == 6)
        {
            // Trigger the LvlComplete animation when all 6 dishes are arranged
            birdAnimator.SetTrigger("LvlComplete");
            Debug.Log("Level Completed! All dishes arranged.");
        }
    }
}
