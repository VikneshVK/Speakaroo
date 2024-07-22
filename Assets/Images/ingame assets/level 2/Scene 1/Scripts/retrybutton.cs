using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class retrybutton : MonoBehaviour
{
    public Button retryButton;
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = retryButton.transform.localScale;
        retryButton.transform.localScale = Vector3.zero;

        ST_AudioManager.Instance.OnCard1PlaybackComplete += HandleCard1PlaybackComplete;
        ST_AudioManager.Instance.OnRecordingComplete += HandleRecordingComplete;
        ST_AudioManager.Instance.OnCard2Interaction += HandleCard2Interaction;
    }

    private void OnDestroy()
    {
        ST_AudioManager.Instance.OnCard1PlaybackComplete -= HandleCard1PlaybackComplete;
        ST_AudioManager.Instance.OnRecordingComplete -= HandleRecordingComplete;
        ST_AudioManager.Instance.OnCard2Interaction -= HandleCard2Interaction;
    }

    private void HandleCard1PlaybackComplete()
    {
        HandleRecordingComplete(1);
    }

    private void HandleRecordingComplete(int cardNumber)
    {
        LeanTween.scale(retryButton.gameObject, originalScale, 0.5f).setEase(LeanTweenType.easeOutBack);
    }

    private void HandleCard2Interaction()
    {
        LeanTween.scale(retryButton.gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInBack);
    }
}
