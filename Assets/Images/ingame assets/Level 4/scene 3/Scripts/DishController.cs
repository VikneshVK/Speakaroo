using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DishController : MonoBehaviour
{
    public GameObject maskPrefab;
    public ScrubberController scrubberController;
    private bool isSpawning = false;
    private GameObject instantiatedMask;

    private Vector3 originalPosition;  // Store original position manually
    private Vector3 originalScale;     // Store original scale manually
    private bool isScrubbing = false;

    public bool isDishSelected = false;

    private Dictionary<Transform, int> originalSortingOrders = new Dictionary<Transform, int>();
    private Collider2D objectCollider;
    private DishController[] allDishes;
    private DishWashingManager dishWashingManager;

    private void Start()
    {
        // Ensure original position and scale are stored, even if they are zero
        originalPosition = (transform.position == Vector3.zero) ? transform.localPosition : transform.position;
        originalScale = (transform.localScale == Vector3.zero) ? Vector3.one : transform.localScale;

        objectCollider = GetComponent<Collider2D>();
        allDishes = FindObjectsOfType<DishController>();
        dishWashingManager = FindObjectOfType<DishWashingManager>();
        StoreOriginalSortingOrders();
    }

    public void OnMouseDown()
    {
        if (isSpawning || isScrubbing) return;

        DisableOtherColliders();

        // Store the current position as the original position before moving the dish to the center
        originalPosition = transform.position;  // Save current position
        originalScale = transform.localScale;   // Save current scale

        ChangeSortingOrderOfChildren(20);

        // Tween the dish to the center of the viewport and scale it up slightly
        LeanTween.move(gameObject, Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane + 5f)), 0.5f).setOnComplete(() =>
        {
            isDishSelected = true;
        });

        LeanTween.scale(gameObject, originalScale * 2f, 0.5f); // Use the stored original scale
    }


    public void StartScrubbing()
    {
        if (!isScrubbing)
        {
            isScrubbing = true;
            StartCoroutine(SpawnPrefabs());
        }
    }

    private IEnumerator SpawnPrefabs()
    {
        Debug.Log("Spawning prefabs started.");
        isSpawning = true;

        for (float timer = 0; timer < 5f; timer += Time.deltaTime)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                Transform dChild = FindDChild();
                if (dChild != null)
                {
                    Vector3 spawnPos = new Vector3(mousePos.x, mousePos.y, dChild.position.z);
                    if (!AlreadyHasMaskAtPoint(spawnPos, dChild))
                    {
                        instantiatedMask = Instantiate(maskPrefab, spawnPos, Quaternion.identity);
                        instantiatedMask.transform.SetParent(dChild);
                    }
                }
            }

            yield return null;
        }

        isSpawning = false;
        Debug.Log("Spawning prefabs ended.");

        Transform dChildToDestroy = FindDChild();
        if (dChildToDestroy != null)
        {
            StartCoroutine(CleanDishAfterDelay(dChildToDestroy.gameObject));
        }
    }

    private Transform FindDChild()
    {
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("D"))
            {
                return child;
            }
        }
        return null;
    }

    private bool AlreadyHasMaskAtPoint(Vector3 point, Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag("Mask"))
            {
                float distance = Vector3.Distance(child.position, point);
                if (distance < 0.01f)
                    return true;
            }
        }
        return false;
    }

    private IEnumerator CleanDishAfterDelay(GameObject dChild)
    {
        yield return new WaitForSeconds(2f);
        Destroy(dChild);

        // Tween the dish back to its original position and scale
        LeanTween.move(gameObject, originalPosition, 0.5f).setOnComplete(() =>
        {
            transform.localScale = originalScale; // Reset to the original scale
            isDishSelected = false;
            RestoreOriginalSortingOrders();
            objectCollider.enabled = false;
            EnableOtherColliders();

            if (scrubberController != null)
            {
                scrubberController.ResetPosition();
            }

            if (dishWashingManager != null)
            {
                DishWashingManager.DishWashed();
            }

            isScrubbing = false;
        });

        Debug.Log("Dish cleaned and moved back to original position.");
    }

    private void StoreOriginalSortingOrders()
    {
        foreach (Transform child in transform)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                originalSortingOrders[child] = sr.sortingOrder;
            }
        }
    }

    private void ChangeSortingOrderOfChildren(int baseOrder)
    {
        foreach (Transform child in transform)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (child.name.StartsWith("D-"))
                {
                    sr.sortingOrder = baseOrder + 1; // Dirty objects on top
                }
                else if (child.name.StartsWith("C-"))
                {
                    sr.sortingOrder = baseOrder; // Clean objects below dirty
                }
                else
                {
                    sr.sortingOrder = baseOrder; // Default for other children
                }
            }
        }
    }

    private void RestoreOriginalSortingOrders()
    {
        foreach (Transform child in originalSortingOrders.Keys)
        {
            if (child == null)
                continue;

            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = originalSortingOrders[child];
            }
        }
    }

    private void DisableOtherColliders()
    {
        foreach (DishController dish in allDishes)
        {
            if (dish != this)
            {
                dish.GetComponent<Collider2D>().enabled = false;
            }
        }
    }

    private void EnableOtherColliders()
    {
        foreach (DishController dish in allDishes)
        {
            if (dish != this)
            {
                bool hasDChild = false;

                // Check if the dish has a child GameObject with a name starting with "D"
                foreach (Transform child in dish.transform)
                {
                    if (child.name.StartsWith("D"))
                    {
                        hasDChild = true;
                        break;
                    }
                }

                // Enable the collider only if the dish has a child starting with "D"
                if (hasDChild)
                {
                    dish.GetComponent<Collider2D>().enabled = true;
                }
            }
        }
    }

}
