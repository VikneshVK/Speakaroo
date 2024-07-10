using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public Button retryButton;
    public ScratchCardEffect scratchCardEffect; // Reference to the ScratchCardEffect script

    void Start()
    {
        retryButton.onClick.AddListener(RetryAction);
        retryButton.gameObject.SetActive(false);
    }

    public void ShowButtons()
    {
        retryButton.gameObject.SetActive(true);
    }

    private void RetryAction()
    {
        if (scratchCardEffect != null)
        {
            scratchCardEffect.Retry();
        }
        else
        {
            Debug.LogError("ScratchCardEffect reference not set in UiManager.");
        }
    }
}
