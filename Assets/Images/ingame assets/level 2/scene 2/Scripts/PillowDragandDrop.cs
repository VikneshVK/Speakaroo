using UnityEngine;

public class PillowDragAndDrop : MonoBehaviour
{
    public Transform targetPosition;
    public Collider2D nextCollider; // The next collider to enable after this pillow is successfully dropped
    public GameObject dust; // Reference to the dust game object, assigned from the inspector
    public GameObject bedsheet; // Reference to the bedsheet game object, assigned from the inspector

    private bool isDragging = false;
    private Vector3 startPosition;
    private Vector3 offset;
    private int originalSortingOrder;
    private SpriteRenderer spriteRenderer;
    private static int droppedPillowsCount = 0;
    private static int totalPillows = 4; // Total number of pillow objects

    private static bool bigPillowLeftDropped = false;
    private static bool bigPillowRightDropped = false;
    private static bool smallPillowLeftDropped = false;
    private static bool smallPillowRightDropped = false;

    private static bool leftSideDropped = false;
    private static bool rightSideDropped = false;

    void Start()
    {
        startPosition = transform.position;
        GetComponent<Collider2D>().enabled = false; // Disable collider initially

        if (dust != null)
        {
            dust.SetActive(false); // Initially deactivate the dust game object
        }
        else
        {
            Debug.LogError("Dust game object reference not set in the inspector.");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }
        else
        {
            Debug.LogError("SpriteRenderer component not found.");
        }
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mousePosition + offset;
        }
    }

    void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            offset = transform.position - mousePosition;

            // Change sorting order to 10 while dragging
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 10;
            }
        }
    }

    void OnMouseUp()
    {
        if (isDragging)
        {
            isDragging = false;

            // Reset sorting order to original
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = originalSortingOrder;
            }

            if (Vector3.Distance(transform.position, targetPosition.position) < 0.5f) // Adjust distance threshold as needed
            {
                transform.position = targetPosition.position;
                transform.rotation = Quaternion.identity; // Reset rotation to Quaternion.identity
                EnableNextCollider();
                ActivateDust();
                Debug.Log("dust activated");
                targetPosition.gameObject.SetActive(false); // Deactivate the target game object
                droppedPillowsCount++;
                Debug.Log("Pillow dropped. Total dropped: " + droppedPillowsCount);
                UpdateBedsheetSprite();
                if (droppedPillowsCount >= totalPillows)
                {
                    DisableAllColliders();
                    ChangeBedsheetSprite("bedsheet");
                }
            }
            else
            {
                transform.position = startPosition;
                transform.rotation = Quaternion.identity; // Reset rotation to original
            }
        }
    }

    private void EnableNextCollider()
    {
        // Disable all other colliders except the next one
        PillowDragAndDrop[] allPillows = FindObjectsOfType<PillowDragAndDrop>();
        foreach (PillowDragAndDrop pillow in allPillows)
        {
            if (pillow != this && pillow.GetComponent<Collider2D>() != nextCollider)
            {
                pillow.GetComponent<Collider2D>().enabled = false;
            }
        }

        if (nextCollider != null)
        {
            nextCollider.enabled = true;
        }
    }

    private void ActivateDust()
    {
        if (dust != null)
        {
            dust.SetActive(true); // Activate the dust game object

            Animator dustAnimator = dust.GetComponent<Animator>();
            dustAnimator.enabled = true;
            /*if (dustAnimator != null)
            {
                *//*dustAnimator.SetTrigger("dusting");*//* // Trigger the dusting animation
                StartCoroutine(DeactivateDustAfterAnimation(dustAnimator));
            }*/
            }
        }

    private System.Collections.IEnumerator DeactivateDustAfterAnimation(Animator animator)
    {
        // Wait for the dusting animation to complete
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        dust.SetActive(false); // Deactivate the dust game object
    }

    private void UpdateBedsheetSprite()
    {
        if (bedsheet != null)
        {
            Debug.Log("Updating bedsheet sprite...");

            if (gameObject.name == "pillow Big Left")
            {
                bigPillowLeftDropped = true;
                Debug.Log("Big Pillow Left Dropped");
            }
            else if (gameObject.name == "pillow Big right")
            {
                bigPillowRightDropped = true;
                Debug.Log("Big Pillow Right Dropped");
            }
            else if (gameObject.name == "Pillow Small Left")
            {
                smallPillowLeftDropped = true;
                Debug.Log("Small Pillow Left Dropped");
            }
            else if (gameObject.name == "Pillow Small Right")
            {
                smallPillowRightDropped = true;
                Debug.Log("Small Pillow Right Dropped");
            }

            Debug.Log("bigPillowLeftDropped: " + bigPillowLeftDropped);
            Debug.Log("bigPillowRightDropped: " + bigPillowRightDropped);
            Debug.Log("smallPillowLeftDropped: " + smallPillowLeftDropped);
            Debug.Log("smallPillowRightDropped: " + smallPillowRightDropped);

            // Check if left side or right side pillows have been dropped
            if (bigPillowLeftDropped && smallPillowLeftDropped)
            {
                leftSideDropped = true;
                Debug.Log("Left Side Dropped");
            }
            if (bigPillowRightDropped && smallPillowRightDropped)
            {
                rightSideDropped = true;
                Debug.Log("Right Side Dropped");
            }

            Debug.Log("leftSideDropped: " + leftSideDropped);
            Debug.Log("rightSideDropped: " + rightSideDropped);

            // Update the bedsheet sprite based on the drop sequence
            if (leftSideDropped || rightSideDropped)
            {
                if (droppedPillowsCount == 2)
                {
                    Debug.Log("Changing bedsheet sprite to bedsheet2");
                    ChangeBedsheetSprite("bedsheet2");
                }
                else if (droppedPillowsCount == 3)
                {
                    Debug.Log("Changing bedsheet sprite to bedsheet1");
                    ChangeBedsheetSprite("bedsheet1");
                }
            }
        }
    }

    private void ChangeBedsheetSprite(string spriteName)
    {
        SpriteRenderer bedsheetRenderer = bedsheet.GetComponent<SpriteRenderer>();
        if (bedsheetRenderer != null)
        {
            // Load sprite from Resources/images folder
            Sprite newSprite = Resources.Load<Sprite>("images/" + spriteName);
            if (newSprite != null)
            {
                bedsheetRenderer.sprite = newSprite;
                Debug.Log("Changed bedsheet sprite to: " + spriteName);
            }
            else
            {
                Debug.LogError("Sprite not found in Resources/images: " + spriteName);
            }
        }
        else
        {
            Debug.LogError("SpriteRenderer component not found on bedsheet.");
        }
    }

    private void DisableAllColliders()
    {
        PillowDragAndDrop[] allPillows = FindObjectsOfType<PillowDragAndDrop>();
        foreach (PillowDragAndDrop pillow in allPillows)
        {
            pillow.GetComponent<Collider2D>().enabled = false;
        }
    }
}
