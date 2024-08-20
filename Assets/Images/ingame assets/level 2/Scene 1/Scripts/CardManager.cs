using UnityEngine;
using System.Collections;

public class CardManager : MonoBehaviour
{
    public GameObject card1;
    public GameObject card2;
    public GameObject maskPrefab;
    public RetryButton retryButton;

    private bool isCard1Shaking = false;
    private bool isCard2Shaking = false;

    private void Start()
    {
        ST_AudioManager.Instance.OnRetryClicked += OnRetryButtonClicked;
        ST_AudioManager.Instance.OnRecordingPlaybackStart += OnRecordingPlaybackStart;
        ST_AudioManager.Instance.OnRecordingPlaybackEnd += OnRecordingPlaybackEnd;

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
        if (card1 == null)
        {
            if (card2 != null)
            {
                Collider2D card2Collider = card2.GetComponent<Collider2D>();
                if (card2Collider != null) card2Collider.enabled = false;

                if (isCard2Shaking)
                {
                    StopCoroutine(ShakeCard(card2));
                    isCard2Shaking = false;
                }
            }
        }
    }

    private void OnRecordingPlaybackStart()
    {
        // Disable Card 2 interaction and shaking during Card 1's playback
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
    }

    private void OnRecordingPlaybackEnd()
    {
        // Re-enable Card 2 interaction and shaking after Card 1's playback is done
        if (card2 != null)
        {
            Collider2D card2Collider = card2.GetComponent<Collider2D>();
            if (card2Collider != null) card2Collider.enabled = true;

            StartCoroutine(ShakeCard(card2));
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