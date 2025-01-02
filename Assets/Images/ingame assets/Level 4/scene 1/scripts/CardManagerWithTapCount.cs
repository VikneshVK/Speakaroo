using UnityEngine;
using System.Collections;

public class CardManagerWithTapCount : MonoBehaviour
{
    public GameObject card1;
    public GameObject card2;
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
            ST_AudioManager.Instance.OnRecordingPlaybackEnd += OnRecordingPlaybackEnd;
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
            retryButton.retryButton.interactable = false;
            retryButton.retryButton.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
        }

        if (card2 != null)
        {
            Collider2D card2Collider = card2.GetComponent<Collider2D>();
            if (card2Collider != null)
            {
                Debug.Log("Disabling Card 2 Collider in OnRetryButtonClicked.");
                card2Collider.enabled = false;
            }
        }

        if (colliderManager != null)
        {
            Debug.Log("Calling EnableCard2FrontCollider in ColliderManager.");
            colliderManager.EnableCard2FrontCollider();
        }

        if (isCard2Shaking)
        {
            Debug.Log("Stopping Card 2 shake coroutine.");
            StopCoroutine(ShakeCard(card2));
            isCard2Shaking = false;
        }
    }

    private void OnRecordingPlaybackStart()
    {
        if (card1 == null && card2 != null)
        {
            Collider2D card2Collider = card2.GetComponent<Collider2D>();
            if (card2Collider != null) card2Collider.enabled = false;

            if (isCard2Shaking)
            {
                StopCoroutine(ShakeCard(card2));
                isCard2Shaking = false;
            }
        }

        if (retryButton != null)
        {
            retryButton.retryButton.interactable = false;
            retryButton.retryButton.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
        }
    }

    private void OnRecordingPlaybackEnd(int cardNumber)
    {
        if (cardNumber == 1 && card2 != null)
        {
            Collider2D card2Collider = card2.GetComponent<Collider2D>();
            if (card2Collider != null) card2Collider.enabled = true;

            StartCoroutine(ShakeCard(card2));
        }
        else if (cardNumber == 2)
        {
            Debug.Log("Recording and playback workflow completed for Card 2.");
        }

        if (retryButton != null)
        {
            retryButton.retryButton.interactable = true;
            retryButton.retryButton.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
        }
    }

    private IEnumerator MonitorCard(GameObject card)
    {
        while (card != null)
        {
            EggController eggController = card.GetComponent<EggController>();

            if (eggController != null && eggController.tapCount < 3)
            {
                yield return new WaitForSeconds(2f);

                if (eggController.tapCount < 3)
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

    private IEnumerator ShakeCard(GameObject card)
    {
        while (card != null)
        {
            EggController eggController = card.GetComponent<EggController>();
            if (eggController == null || eggController.tapCount >= 3) yield break;

            Collider2D cardCollider = card.GetComponent<Collider2D>();
            if (cardCollider != null && cardCollider.enabled)
            {
                // Disable the collider to prevent interactions during the animation
                cardCollider.enabled = false;

                // Perform the scale up animation
                bool scaleUpComplete = false;
                LeanTween.scale(card, card.transform.localScale * 1.2f, 0.5f).setEase(LeanTweenType.easeOutBack)
                    .setOnComplete(() =>
                    {
                        // Perform the scale down animation
                        LeanTween.scale(card, card.transform.localScale / 1.2f, 0.5f).setEase(LeanTweenType.easeInBack)
                            .setOnComplete(() =>
                            {
                                scaleUpComplete = true;
                            });
                    });

                // Wait for both animations to complete
                yield return new WaitUntil(() => scaleUpComplete);

                // Re-enable the collider after both animations are complete
                if (cardCollider != null)
                {
                    cardCollider.enabled = true;
                }

                // Wait for the remaining time to maintain the 3-second interval
                yield return new WaitForSeconds(3f);

                // Stop if conditions to end the coroutine are met
                if (card == null || eggController.tapCount >= 3 || cardCollider == null || !cardCollider.enabled)
                {
                    yield break;
                }
            }
            else
            {
                yield break;
            }
        }

        // Update the shake state flags when done
        if (card == card1) isCard1Shaking = false;
        else if (card == card2) isCard2Shaking = false;
    }


}
