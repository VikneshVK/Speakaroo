using System.Collections;
using UnityEngine;

public class DishController : MonoBehaviour
{
    public GameObject maskPrefab; // The mask prefab to spawn when scrubbing
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

        // Tween the dish to the center of the viewport and scale it to 120%
        LeanTween.move(gameObject, Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane + 5f)), 0.5f);
        LeanTween.scale(gameObject, originalScale * 1.2f, 0.5f);
    }

    public void StartScrubbing()
    {
        if (!isScrubbing)
        {
            isScrubbing = true;
            StartCoroutine(SpawnPrefabs(transform));
        }
    }

    private IEnumerator SpawnPrefabs(Transform dishTransform)
    {
        Debug.Log("Spawning prefabs started.");
        isSpawning = true;

        // Simulate scrubbing for a set amount of time (e.g., 5 seconds)
        for (float timer = 0; timer < 5f; timer += Time.deltaTime)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null && hit.collider.transform == dishTransform)
            {
                Vector3 spawnPos = new Vector3(hit.point.x, hit.point.y, hit.collider.transform.position.z);
                if (!AlreadyHasMaskAtPoint(spawnPos, hit.collider.transform))
                {
                    instantiatedMask = Instantiate(maskPrefab, spawnPos, Quaternion.identity);
                    instantiatedMask.transform.SetParent(hit.collider.transform);
                }
            }

            yield return null;
        }

        
        isSpawning = false;
        Debug.Log("Spawning prefabs ended.");

        
        SpriteRenderer sr = dishTransform.GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        // Destroy the D-prefixed child object after a delay
        Transform dChild = FindDChild();
        if (dChild != null)
        {
            StartCoroutine(CleanDishAfterDelay(dChild.gameObject));
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
            if (!string.IsNullOrEmpty(child.tag) && child.CompareTag("Mask"))
            {
                float distance = Vector3.Distance(child.position, point);
                if (distance < 0.01f) // Adjust this threshold as necessary
                    return true;
            }
        }
        return false;
    }

    private IEnumerator CleanDishAfterDelay(GameObject dChild)
    {
        yield return new WaitForSeconds(2f);
        Destroy(dChild);

        // Inform the manager that this dish has been cleaned
        DishWashingManager.DishWashed();

        // Tween the dish back to its original position and scale
        LeanTween.move(gameObject, originalPosition, 0.5f).setOnComplete(() =>
        {
            transform.localScale = originalScale;
            isScrubbing = false;
        });
    }
}
