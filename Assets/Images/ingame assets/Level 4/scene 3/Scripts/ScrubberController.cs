using UnityEngine;

public class ScrubberController : MonoBehaviour
{
    public GameObject maskPrefab;
    private Vector3 originalPosition; // Store the scrubber's original position

    private void Start()
    {
        // Store the initial position of the scrubber
        originalPosition = transform.position;
    }

    private void Update()
    {
        HandleScrubbing();
    }

    private void HandleScrubbing()
    {
        // Check if the dish is selected before handling scrubbing
        if (Input.GetMouseButton(0) && DishIsSelected())
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mousePosition;

            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (hit.collider != null && hit.collider.GetComponent<DishController>() != null)
            {
                hit.collider.GetComponent<DishController>().StartScrubbing();
            }
        }
    }

    // Function to reset the scrubber back to its original position
    public void ResetPosition()
    {
        LeanTween.move(gameObject, originalPosition, 0.5f); // Smoothly move scrubber back
    }

    // Check if any dish is selected
    private bool DishIsSelected()
    {
        DishController[] dishes = FindObjectsOfType<DishController>();
        foreach (DishController dish in dishes)
        {
            if (dish.isDishSelected) return true;
        }
        return false;
    }
}
