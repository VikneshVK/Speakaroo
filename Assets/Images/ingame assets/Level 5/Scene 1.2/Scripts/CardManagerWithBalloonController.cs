using UnityEngine;
using System.Collections;

public class CardManagerWithBalloonController : MonoBehaviour
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
            BalloonController balloonController = card.GetComponent<BalloonController>();

            if (balloonController != null && card.transform.localScale != Vector3.zero)
            {
                yield return new WaitForSeconds(2f);

                if (card.transform.localScale != Vector3.zero)
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
            if (card.transform.localScale == Vector3.zero) yield break;

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

                if (card == null || card.transform.localScale == Vector3.zero || cardCollider == null || !cardCollider.enabled)
                {
                    yield break;
                }
            }
            else
            {
                yield break;
            }
        }

        if (card == card1) isCard1Shaking = false;
        else if (card == card2) isCard2Shaking = false;
    }
}
