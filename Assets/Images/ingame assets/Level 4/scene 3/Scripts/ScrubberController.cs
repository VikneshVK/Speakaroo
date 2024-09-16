using UnityEngine;
using System.Collections;

public class ScrubberController : MonoBehaviour
{
    public GameObject maskPrefab;
    private Vector3 originalPosition; // Store the scrubber's original position
    public bool isInteracted = false; // Boolean to track if scrubber has been interacted with

    private LVL4Sc3HelperHand helperHandManager; // Reference to the Helper Hand Manager
    private Coroutine helperHandCoroutine; // Coroutine for the helper hand delay
    private bool timerStarted = false; // Track if the timer has started

    private void Start()
    {
        // Store the initial position of the scrubber
        originalPosition = transform.position;

        // Find the helper hand manager in the scene using the "HelperHand" tag
        GameObject helperHandObject = GameObject.FindWithTag("HelperHand");
        if (helperHandObject != null)
        {
            helperHandManager = helperHandObject.GetComponent<LVL4Sc3HelperHand>();
        }
        else
        {
            Debug.LogError("Helper hand manager with tag 'HelperHand' not found in the scene.");
        }
    }

    private void Update()
    {
        HandleScrubbing();
    }

    private void HandleScrubbing()
    {
        // Start the helper hand delay timer only when a dish is selected and the timer hasn't started yet
        if (DishIsSelected() && !timerStarted)
        {
            StartHelperHandDelayTimer();
        }

        // Check if the scrubber itself is clicked
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            // If the clicked object is this scrubber, start the interaction
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isInteracted = true; // Set to true when scrubber is clicked
            }
        }

        // If the scrubber is being interacted with (clicked), follow the mouse
        if (isInteracted && Input.GetMouseButton(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mousePosition;

            // Check if the scrubber is interacting with a dish
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (hit.collider != null && hit.collider.GetComponent<DishController>() != null)
            {
                hit.collider.GetComponent<DishController>().StartScrubbing();

                // Destroy the helper hand and reset the timer for the next cycle
                helperHandManager.StopHelperHand();
                timerStarted = false; // Reset timer started flag
            }
        }

        // Reset interaction if the mouse button is released
        if (Input.GetMouseButtonUp(0))
        {
            isInteracted = false;
        }
    }

    // Function to reset the scrubber back to its original position
    public void ResetPosition()
    {
        LeanTween.move(gameObject, originalPosition, 0.5f); // Smoothly move scrubber back
        isInteracted = false; // Reset interaction state
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

    private void StartHelperHandDelayTimer()
    {
        if (helperHandCoroutine != null)
        {
            StopCoroutine(helperHandCoroutine);
        }
        helperHandCoroutine = StartCoroutine(HelperHandDelayTimer());
        timerStarted = true; // Set timer started flag
    }

    private IEnumerator HelperHandDelayTimer()
    {
        // Wait for the delay time before spawning the helper hand
        yield return new WaitForSeconds(helperHandManager.helperHandDelay);

        // If the scrubber has not been interacted with, spawn the helper hand
        if (!isInteracted)
        {
            // Find the first selected dish to tween towards
            DishController selectedDish = null;
            DishController[] dishes = FindObjectsOfType<DishController>();
            foreach (DishController dish in dishes)
            {
                if (dish.isDishSelected)
                {
                    selectedDish = dish;
                    break;
                }
            }

            // Spawn the helper hand at the scrubber's position and tween to the selected dish's position
            if (selectedDish != null)
            {
                helperHandManager.SpawnHelperHand(transform.position, selectedDish.transform.position);
            }
        }

        // Reset timer started flag to allow the timer to be started again in the future
        timerStarted = false;
    }
}
