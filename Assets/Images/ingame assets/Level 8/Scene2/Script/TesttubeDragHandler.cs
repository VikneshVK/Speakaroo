using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TesttubeDragHandler : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 resetPosition; // The position to reset the test tube to after tweening
    public Collider2D beakerCollider;
    private bool isCorrectDrop = false;
    private bool isResetPositionSet = false;
    private static bool isTestTube1Dropped = false;
    private static bool isTestTube2Dropped = false;

    [SerializeField] private Lvl8Sc2QuestManager questManager;
    [SerializeField] private SpriteRenderer beakerSpriteRenderer;
    [SerializeField] private Sprite BSprite1;
    [SerializeField] private Sprite BSprite2;
    [SerializeField] private Sprite BSprite3;
    [SerializeField] private Sprite BSprite4;
    [SerializeField] private SpriteRenderer testtubeSpriteRenderer;
    [SerializeField] private Color blinkColor = Color.red;
    [SerializeField] private float blinkDuration = 0.2f;
    [SerializeField] private int blinkCount = 2;
    [SerializeField] private BeakerDragHandler beakerDragHandler;

    void Update()
    {
        // Check if the beaker has been dropped and store the current position as reset position only once
        if (beakerDragHandler.isBeakerDropped && !isResetPositionSet)
        {
            resetPosition = transform.localPosition;
            isResetPositionSet = true;
            Debug.Log($"{gameObject.name}: Reset position set to {resetPosition}");
        }

        // Detect when the player clicks on the test tube
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
            }
        }

        // Handle releasing the mouse button and dropping the test tube
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            // Check if the test tube is dropped over the beaker's collider
            if (beakerCollider != null && beakerCollider.OverlapPoint(transform.position))
            {
                HandleDrop();
            }
            else
            {
                ResetToPosition();
            }
        }

        // Handle dragging the test tube
        if (isDragging)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector2(mousePosition.x, mousePosition.y);
        }
    }

    // Handles the logic when a test tube is dropped
    private void HandleDrop()
    {
        isCorrectDrop = false;

        // Check for ColoursFound = 0 scenario
        if (questManager.ColoursFound == 0)
        {
            if (gameObject.name == "TestTube2" && !isTestTube2Dropped)
            {
                isCorrectDrop = true;
                isTestTube2Dropped = true;
                beakerSpriteRenderer.sprite = BSprite1;
            }
            else if (gameObject.name == "TestTube1" && isTestTube2Dropped && !isTestTube1Dropped)
            {
                isCorrectDrop = true;
                isTestTube1Dropped = true;
                beakerSpriteRenderer.sprite = BSprite2;
            }
        }
        // Check for ColoursFound = 1 scenario
        else if (questManager.ColoursFound == 1)
        {
            if (gameObject.name == "TestTube1" && !isTestTube1Dropped)
            {
                isCorrectDrop = true;
                isTestTube1Dropped = true;
                beakerSpriteRenderer.sprite = BSprite3;
            }
            else if (gameObject.name == "TestTube2" && isTestTube1Dropped && !isTestTube2Dropped)
            {
                isCorrectDrop = true;
                isTestTube2Dropped = true;
                beakerSpriteRenderer.sprite = BSprite4;
            }
        }

        // If the drop is incorrect, blink and reset
        if (!isCorrectDrop)
        {
            StartCoroutine(BlinkAndReset());
        }
        else
        {
            ResetToPosition(); // Reset the test tube to its saved position after a correct drop
        }
    }

    // Coroutine to blink the test tube in red if dropped incorrectly
    private IEnumerator BlinkAndReset()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            testtubeSpriteRenderer.color = blinkColor;
            yield return new WaitForSeconds(blinkDuration);
            testtubeSpriteRenderer.color = Color.white;
            yield return new WaitForSeconds(blinkDuration);
        }
        ResetToPosition();
    }

    // Resets the test tube to its saved position
    private void ResetToPosition()
    {
        if (isResetPositionSet)
        {
            Debug.Log($"{gameObject.name}: Resetting to position {resetPosition}");
            transform.localPosition = resetPosition;
        }
    }

    
}
