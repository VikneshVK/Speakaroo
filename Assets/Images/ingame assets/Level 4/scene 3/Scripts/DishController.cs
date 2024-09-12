using System.Collections;
using UnityEngine;

public class DishController : MonoBehaviour
{
    public GameObject maskPrefab; // The prefab to spawn during scrubbing
    private bool isSpawning = false;
    private GameObject instantiatedMask;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private bool isScrubbing = false;

    private void Start()
    {
        // Store the original position and scale for resetting later
        originalPosition = transform.position;
        originalScale = transform.localScale;
    }

    public void OnMouseDown()
    {
        if (isSpawning || isScrubbing) return;

        // Tween the dish to the center of the viewport and scale it up slightly
        LeanTween.move(gameObject, Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane + 5f)), 0.5f);
        LeanTween.scale(gameObject, originalScale * 1.2f, 0.5f);
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

        // Simulate scrubbing for a set amount of time (5 seconds)
        for (float timer = 0; timer < 5f; timer += Time.deltaTime)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                // Find the child with the "D" prefix
                Transform dChild = FindDChild();
                if (dChild != null)
                {
                    Vector3 spawnPos = new Vector3(mousePos.x, mousePos.y, dChild.position.z);
                    if (!AlreadyHasMaskAtPoint(spawnPos, dChild))
                    {
                        instantiatedMask = Instantiate(maskPrefab, spawnPos, Quaternion.identity);
                        instantiatedMask.transform.SetParent(dChild); // Attach the mask to the D-prefixed child
                    }
                }
            }

            yield return null;
        }

        isSpawning = false;
        Debug.Log("Spawning prefabs ended.");

        // Destroy the D-prefixed child object after scrubbing is complete
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
                if (distance < 0.01f) // Adjust the threshold if necessary
                    return true;
            }
        }
        return false;
    }

    private IEnumerator CleanDishAfterDelay(GameObject dChild)
    {
        yield return new WaitForSeconds(2f); // Delay before cleaning
        Destroy(dChild); // Destroy the D-prefixed child

        // Tween the dish back to its original position and scale
        LeanTween.move(gameObject, originalPosition, 0.5f).setOnComplete(() =>
        {
            transform.localScale = originalScale; // Reset the scale
            isScrubbing = false; // Allow interaction again
        });

        Debug.Log("Dish cleaned and moved back to original position.");
    }
}
