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

    public void EnableCard2FrontCollider()
    {
        // Ensure the retry button is interactable
        if (retryButton.retryButton.interactable)
        {
            Debug.Log("Retry button is interactable. Waiting for player interaction or timeout...");
            // Start the coroutine to wait for 4 seconds for retry button action
            StartCoroutine(WaitForRetryButtonToDisable());
        }
        else
        {
            Debug.LogWarning("Retry button is not interactable, which is unexpected in this flow.");
        }
    }


    private IEnumerator WaitForRetryButtonToDisable()
    {
        // Wait for 4 seconds to give the player a chance to interact with the retry button
        yield return new WaitForSeconds(4f);

        // Check again if the retry button is still interactable
        if (retryButton.retryButton.interactable)
        {
            Debug.Log("Retry button was not clicked in 4 seconds. Enabling Card 2's collider.");

            // Enable Card 2's collider
            if (card2Front != null)
            {
                card2Front.GetComponent<Collider2D>().enabled = true;
                Debug.Log("Card 2 collider enabled.");
            }
            else
            {
                Debug.LogError("Card 2 GameObject is null. Cannot enable collider.");
            }
        }
        else
        {
            Debug.Log("Retry button is no longer interactable after timeout.");
        }
    }

}