using System.Collections;
using UnityEngine;

public class colliderManager : MonoBehaviour
{
    public GameObject card1Front;
    public GameObject card2Front;
    public RetryButton retryButton; // Reference to the RetryButton script

    private void Start()
    {
        // Initialize the colliders: enable for card1 and disable for card2
        card1Front.GetComponent<Collider2D>().enabled = true;
        card2Front.GetComponent<Collider2D>().enabled = false;

        // Subscribe to the OnCard1PlaybackComplete event
        ST_AudioManager.Instance.OnCard1PlaybackComplete += EnableCard2FrontCollider;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event when this object is destroyed
        ST_AudioManager.Instance.OnCard1PlaybackComplete -= EnableCard2FrontCollider;
    }

    private void EnableCard2FrontCollider()
    {
        // Start a coroutine to wait until the Retry button becomes non-interactable
        StartCoroutine(WaitForRetryButtonToDisable());
    }

    private IEnumerator WaitForRetryButtonToDisable()
    {
        // Wait until the Retry button becomes non-interactable
        yield return new WaitUntil(() => !retryButton.retryButton.interactable);

        // Enable the collider for card2Front
        card2Front.GetComponent<Collider2D>().enabled = true;
    }
}