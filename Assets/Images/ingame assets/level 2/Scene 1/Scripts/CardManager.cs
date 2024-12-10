using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CardManager : MonoBehaviour
{
    public GameObject card1;
    public GameObject card2;
    public GameObject maskPrefab;
    public RetryButton retryButton;
    public colliderManager colliderManager;

    private bool isCard1Shaking;
    private bool isCard2Shaking;

    private void Start()
    {
        isCard1Shaking = false;
        isCard2Shaking = false;

        if (ST_AudioManager.Instance != null)
        {
            ST_AudioManager.Instance.OnRetryClicked += OnRetryButtonClicked;
            ST_AudioManager.Instance.OnRecordingPlaybackStart += OnRecordingPlaybackStart;
            ST_AudioManager.Instance.OnRecordingPlaybackEnd += OnRecordingPlaybackEnd; // Updated subscription
        }
        StartCoroutine(MonitorCard(card1));
        StartCoroutine(MonitorCard(card2));
    }


    private void OnDestroy()
    {
        if (ST_AudioManager.Instance != null)
        {
            ST_AudioManager.Instance.OnRetryClicked -= OnRetryButtonClicked;
            ST_AudioManager.Instance.OnRecordingPlaybackStart -= OnRecordingPlaybackStart;
            ST_AudioManager.Instance.OnRecordingPlaybackEnd -= OnRecordingPlaybackEnd;
        }
    }

    private void OnRetryButtonClicked()
    {
        Debug.Log("Retry button clicked, handling collider states.");

        if (retryButton != null)
        {
            // Disable raycast for retry button
            retryButton.retryButton.interactable = false;
            retryButton.retryButton.GetComponent<Image>().raycastTarget = false;
        }

        if (card2 != null)
        {
            Collider2D card2Collider = card2.GetComponent<Collider2D>();
            if (card2Collider != null)
            {
                Debug.Log("Disabling Card 2 Collider in OnRetryButtonClicked.");
                card2Collider.enabled = false; // Disable Card 2 collider
            }
            else
            {
                Debug.LogError("Card 2 Collider is null.");
            }
        }
        else
        {
            Debug.LogError("Card 2 reference is null in CardManager.");
        }

        // Use colliderManager to re-enable Card 2's collider after retry button becomes non-interactable
        if (colliderManager != null)
        {
            Debug.Log("Calling EnableCard2FrontCollider in ColliderManager.");
            colliderManager.EnableCard2FrontCollider();
        }

        // Stop shaking behavior for Card 2
        if (isCard2Shaking)
        {
            Debug.Log("Stopping Card 2 shake coroutine.");
            StopCoroutine(ShakeCard(card2));
            isCard2Shaking = false;
        }
    }


    private void OnRecordingPlaybackStart()
    {
        // Disable Card 2 interaction and shaking during Card 1's playback
        if (card1 == null && card2 != null)
        {
            Collider2D card2Collider = card2.GetComponent<Collider2D>();
            if (card2Collider != null) card2Collider.enabled = false;

            // Stop shaking for Card 2 if it's active
            if (isCard2Shaking)
            {
                StopCoroutine(ShakeCard(card2));
                isCard2Shaking = false;
            }
        }

        // Disable raycast for retry button during playback
        if (retryButton != null)
        {
            retryButton.retryButton.interactable = false;
            retryButton.retryButton.GetComponent<Image>().raycastTarget = false;
        }
    }


    private void OnRecordingPlaybackEnd(int cardNumber)
    {
        if (cardNumber == 1 && card2 != null) // Handle Card 2
        {
            Collider2D card2Collider = card2.GetComponent<Collider2D>();
            if (card2Collider != null) card2Collider.enabled = true;

            StartCoroutine(ShakeCard(card2));
        }
        else if (cardNumber == 2) // Handle completion for Card 2
        {
            Debug.Log("Recording and playback workflow completed for Card 2.");
            // Add any additional logic for Card 2 here, if needed.
        }

        // Re-enable retry button raycast after playback ends
        if (retryButton != null)
        {
            retryButton.retryButton.interactable = true;
            retryButton.retryButton.GetComponent<Image>().raycastTarget = true;
        }
    }


    private IEnumerator MonitorCard(GameObject card)
    {
        while (card != null)
        {
            Collider2D cardCollider = card.GetComponent<Collider2D>();
            if (cardCollider != null && cardCollider.enabled && !IsMaskPrefabSpawned(card))
            {
                yield return new WaitForSeconds(2f);

                if (card != null && cardCollider.enabled && !IsMaskPrefabSpawned(card))
                {
                    if (card == card1 && !isCard1Shaking)
                    {
                        isCard1Shaking = true;
                        StartCoroutine(ShakeCard(card1));
                    }
                    else if (card == card2 && !isCard2Shaking)
                    {
                        yield return new WaitUntil(() => !retryButton.retryButton.interactable);

                        if (!isCard1Shaking)
                        {
                            isCard2Shaking = true;
                            StartCoroutine(ShakeCard(card2));
                        }
                    }
                }
            }
            yield return null;
        }
    }

    private bool IsMaskPrefabSpawned(GameObject card)
    {
        if (card == null) return false; // Ensure card hasn't been destroyed

        foreach (Transform child in card.transform)
        {
            if (child == null) continue; // Check for null child (if the hierarchy changes)

            if (child.name.Contains(maskPrefab.name)) return true;
        }
        return false;
    }

    private IEnumerator ShakeCard(GameObject card)
    {
        while (card != null)
        {
            if (card == null) yield break; // Ensure card hasn't been destroyed

            Collider2D cardCollider = card.GetComponent<Collider2D>();
            if (cardCollider != null && cardCollider.enabled)
            {
                LeanTween.scale(card, card.transform.localScale * 1.2f, 0.5f).setEase(LeanTweenType.easeOutBack)
                    .setOnComplete(() =>
                    {
                        if (card != null && card.GetComponent<Collider2D>() != null && card.GetComponent<Collider2D>().enabled)
                        {
                            LeanTween.scale(card, card.transform.localScale / 1.2f, 0.5f).setEase(LeanTweenType.easeInBack);
                        }
                    });

                yield return new WaitForSeconds(3f);

                // Check if the card is still valid before continuing
                if (card == null || IsMaskPrefabSpawned(card) || cardCollider == null || !cardCollider.enabled)
                {
                    yield break;
                }
            }
            else
            {
                yield break; // Exit the coroutine if the collider is disabled or the card is null
            }
        }

        // Reset shaking state if the card is no longer valid
        if (card == card1) isCard1Shaking = false;
        else if (card == card2) isCard2Shaking = false;
    }
}