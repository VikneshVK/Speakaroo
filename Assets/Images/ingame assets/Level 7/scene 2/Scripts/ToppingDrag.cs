using UnityEngine;

public class ToppingDrag : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 startPosition;
    private Collider2D toppingCollider;
    public GameObject dropTarget;  // The pizza object where toppings are dropped
    public Lvl7Sc2DragManager dragManager;  // Reference to the drag manager

    // Sprites for the drop target (pizza) to be changed based on dropped toppings
    public Sprite sprite1;  // Sauce dropped
    public Sprite sprite2;  // Cheese dropped
    public Sprite sprite3;  // Toppings dropped
    public Sprite sprite4;  // Pepperoni dropped

    private SpriteRenderer dropTargetRenderer;
    private SpriteRenderer toppingRenderer;
    private int initialSortingOrder;  // Store the original sorting order of the topping

    void Start()
    {
        toppingCollider = GetComponent<Collider2D>();
        dragManager = FindObjectOfType<Lvl7Sc2DragManager>();  // Find the DragManager in the scene
        startPosition = transform.position;  // Store the initial position
        dropTargetRenderer = dropTarget.GetComponent<SpriteRenderer>();  // Get the SpriteRenderer for the pizza (drop target)
        toppingRenderer = GetComponent<SpriteRenderer>();  // Get the SpriteRenderer for the topping
        initialSortingOrder = toppingRenderer.sortingOrder;  // Save the original sorting order
    }

    void Update()
    {
        // Handle dragging
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;  // Set z to 0 to ensure it stays on the same plane
            transform.position = mousePosition;
        }
    }

    void OnMouseDown()
    {
        if (toppingCollider.enabled)  // Allow dragging only if the collider is enabled
        {
            isDragging = true;
            toppingRenderer.sortingOrder = 50;  // Bring the dragged object to the front
        }
    }

    void OnMouseUp()
    {
        isDragging = false;

        // Reset the sorting order after dragging
        toppingRenderer.sortingOrder = initialSortingOrder;

        // Check if the topping is dropped over the pizza (drop target)
        if (IsDroppedOnTarget())
        {
            // Change the sprite of the drop target (pizza) based on the topping that was dropped
            ChangeDropTargetSprite();

            // Reset the dragged object back to its starting position
            transform.position = startPosition;

            // Notify the DragManager that this topping has been dropped correctly
            dragManager.OnToppingDropped();

            // Disable the collider of this topping, so it can no longer be dragged
            toppingCollider.enabled = false;
        }
        else
        {
            // If not dropped on the target, return to the starting position
            transform.position = startPosition;
        }
    }

    // Check if the topping is dropped on the pizza (drop target)
    private bool IsDroppedOnTarget()
    {
        Collider2D pizzaCollider = dropTarget.GetComponent<Collider2D>();
        return toppingCollider.bounds.Intersects(pizzaCollider.bounds);  // Check if bounds intersect
    }

    // Change the sprite of the drop target (pizza) based on which topping is dropped
    private void ChangeDropTargetSprite()
    {
        if (gameObject.name == "Sauce")
        {
            dropTargetRenderer.sprite = sprite1;  // Change to sprite 1 for Sauce
        }
        else if (gameObject.name == "Cheese")
        {
            dropTargetRenderer.sprite = sprite2;  // Change to sprite 2 for Cheese
        }
        else if (gameObject.name == "Toppings")
        {
            dropTargetRenderer.sprite = sprite3;  // Change to sprite 3 for Toppings
        }
        else if (gameObject.name == "Pepperoni")
        {
            dropTargetRenderer.sprite = sprite4;  // Change to sprite 4 for Pepperoni
        }
    }
}
