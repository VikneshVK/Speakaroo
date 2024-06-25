using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class Speech_Bubble_Controller : MonoBehaviour
{
    public GameObject prefab;  // Prefab containing the "BG" object with children.

    void Start()
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is not assigned in the inspector!", this);
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (!collider.enabled)
        {
            collider.enabled = true;
        }
    }

    void OnMouseDown()
    {
        // Start the coroutine to defer changes to the end of the frame
        StartCoroutine(DeferChanges());
    }

    private IEnumerator DeferChanges()
    {
        // Yielding to WaitForEndOfFrame waits until all other Updates, LateUpdates, and rendering are done
        yield return new WaitForEndOfFrame();

        // Perform deferred actions after everything else has been processed
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        if (prefab != null)
        {
            Destroy(gameObject);
            GameObject bgInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            bgInstance.transform.localScale = Vector3.zero;  // Start from scale zero

            if (bgInstance.transform.position == Vector3.zero)
            {
                // Animate to original scale if position is exactly zero
                LeanTween.scale(bgInstance, new Vector3(25, 15, 15), 1f).setEase(LeanTweenType.easeOutBack);
            }

            StartDelayedChildAnimations(bgInstance.transform);
        }
        else
        {
            Debug.LogError("Prefab is not assigned or found!", this);
        }
    }

    private void StartDelayedChildAnimations(Transform bgTransform)
    {
        float initialDelay = 1f;  // Delay to start first child's animation after BG's tween
        float staggerTime = 0.5f; // Time stagger between subsequent animations

        // Animate children: Bubble, Kid, Bird
        AnimateChild(bgTransform, "Bubble", initialDelay);
        AnimateChild(bgTransform, "Kid", initialDelay + staggerTime);
        AnimateChild(bgTransform, "Bird", initialDelay + 2 * staggerTime);

        // Animate Bubble's children: Card_1, Card_2
        Transform bubble = bgTransform.Find("Bubble");
        if (bubble != null)
        {
            AnimateChild(bubble, "Card_1", initialDelay + 3 * staggerTime);
            AnimateChild(bubble, "Card_2", initialDelay + 4 * staggerTime);
            AnimateChild(bubble, "ST_Canvas", initialDelay + 5 * staggerTime);
        }
        else
        {
            Debug.LogError("Bubble child not found in BG prefab!", this);
        }
    }

    private void AnimateChild(Transform parentTransform, string childName, float delay)
    {
        Transform child = parentTransform.Find(childName);
        if (child != null)
        {
            Vector3 originalScale = child.localScale; // Capture the original scale
            child.localScale = Vector3.zero; // Set initial scale to zero
            LeanTween.scale(child.gameObject, originalScale, 0.5f).setEase(LeanTweenType.easeOutBack).setDelay(delay);
        }
        else
        {
            Debug.LogError(childName + " not found in " + parentTransform.name, this);
        }
    }
}
