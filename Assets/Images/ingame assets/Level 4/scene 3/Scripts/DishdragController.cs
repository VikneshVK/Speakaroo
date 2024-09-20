using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DishdragController : MonoBehaviour
{
    public static List<DishdragController> allDishControllers = new List<DishdragController>(); // List of all DishdragController objects
    public Transform dropTarget; // The target location to drop the object
    public LVL4Sc3HelperHand helperHandManager; // Reference to the Helper Hand Manager
    public float helperHandDelay = 5f; // Delay time for the helper hand
    public bool isDroppedCorrectly = false; // Track if the object was dropped correctly

    public Sprite[] bowlSprites; // Array of sprites for when bowls are dropped
    public Sprite newSprite; // The new sprite for glass and plates (non-bowl items)
    public Animator birdAnimator; // Reference to the bird Animator

    private static int bowlDropCount = 0; // Static counter to track how many bowls have been dropped
    public static int dishesArranged = 0; // Static variable to count total dishes arranged

    private Vector3 startPosition; // Store the start position for reset if needed
    private bool isDragging = false;
    private Coroutine helperHandCoroutine; // Coroutine for the helper hand delay

    private void Start()
    {
        startPosition = transform.position; // Store the initial position for reset if needed

        // Add this object to the static list
        allDishControllers.Add(this);

        // Disable the collider initially (until enabled by LV4DragManager)
        GetComponent<Collider2D>().enabled = false;
    }

    private void Update()
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

            // Stop the helper hand if it's active
            if (helperHandManager != null)
            {
                helperHandManager.StopHelperHand();
            }
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
            OnDropped(true); // Correct drop
        }
        else
        {
            // Reset position if not dropped correctly
            transform.position = startPosition;

            OnDropped(false); // Wrong drop
        }
    }

    private void OnDropped(bool correctDrop)
    {
        SpriteRenderer targetSpriteRenderer = dropTarget.GetComponent<SpriteRenderer>();

        if (correctDrop)
        {
            // Correct Drop
            if (gameObject.name.Contains("Bowl"))
            {
                // Handle bowl-specific logic
                HandleBowlDrop(targetSpriteRenderer);
            }
            else
            {
                // For other objects (non-bowls), change the sprite and destroy the object
                if (targetSpriteRenderer != null)
                {
                    targetSpriteRenderer.sprite = newSprite; // Set the new sprite for plates/glasses
                }

                // Mark the object as dropped correctly and destroy it
                isDroppedCorrectly = true;
                Destroy(gameObject);

                // Increment the arranged dishes counter
                dishesArranged++;
                CheckDishesArranged();
            }

            // Notify that the helper hand should stop
            if (helperHandManager != null)
            {
                helperHandManager.StopHelperHand();
            }

            // Start helper hand delay for the next undropped object
            StartHelperHandCheckForAll();
        }
        else
        {
            // Wrong Drop
            // Reset and start the helper hand timer for the current object
            StartHelperHandTimer();
        }
    }

    private void HandleBowlDrop(SpriteRenderer targetSpriteRenderer)
    {
        // Increase the bowl drop count
        bowlDropCount++;

        // Change the sprite of the target (bowl silhouette) based on how many bowls have been dropped
        if (bowlDropCount <= bowlSprites.Length)
        {
            // Update the sprite based on how many bowls have been dropped
            targetSpriteRenderer.sprite = bowlSprites[bowlDropCount - 1]; // Set sprite based on drop count
        }

        // Mark the object as dropped correctly and destroy it
        isDroppedCorrectly = true;
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

    // Start the helper hand delay timer
    public void StartHelperHandTimer()
    {
        if (helperHandCoroutine != null)
        {
            StopCoroutine(helperHandCoroutine);
        }
        helperHandCoroutine = StartCoroutine(HelperHandDelayTimer());
    }

    private IEnumerator HelperHandDelayTimer()
    {
        // Wait for the delay time before spawning the helper hand
        yield return new WaitForSeconds(helperHandDelay);

        // If the object was not dragged and dropped correctly, spawn the helper hand
        if (!isDroppedCorrectly)
        {
            // Spawn the helper hand at the current position and tween it to the drop target
            helperHandManager.SpawnHelperHand(transform.position, dropTarget.position);
        }
    }

    // Static method to initiate the check for all dish controllers
    public static void StartHelperHandCheckForAll()
    {
        foreach (var dishController in allDishControllers)
        {
            if (!dishController.isDroppedCorrectly)
            {
                dishController.StartHelperHandTimer();
                break; // Only start for the first undropped item
            }
        }
    }
}
