using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public Button retryButton;
    public ScratchCardEffect scratchCardEffect; // Reference to the ScratchCardEffect script
    public ScratchCardController scratchCardController;

    void Start()
    {
        retryButton.onClick.AddListener(RetryAction);
    }

   

    public void RetryAction()
    {
        if (scratchCardEffect != null)
        {
            scratchCardEffect.Retry();
        }
        else if(scratchCardController != null)
        {
            scratchCardController.Retry();
        }
        else
        {
            Debug.LogError("ScratchCardEffect reference not set in UiManager.");
        }
    }
}
