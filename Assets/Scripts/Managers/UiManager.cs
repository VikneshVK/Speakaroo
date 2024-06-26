using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public Button retryButton;
    public Button closeButton;

    void Start()
    {
        retryButton.onClick.AddListener(RetryAction);
        closeButton.onClick.AddListener(CloseAction);
        retryButton.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
    }

    public void ShowButtons()
    {
        retryButton.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);
    }

    private void RetryAction()
    {
        // Broadcast a message or call a method on other scripts to perform the retry
        BroadcastMessage("PerformRetry"); // Ensure that receiver scripts have a PerformRetry method
    }

    private void CloseAction()
    {
        BroadcastMessage("PerformClose");        
    }
}
