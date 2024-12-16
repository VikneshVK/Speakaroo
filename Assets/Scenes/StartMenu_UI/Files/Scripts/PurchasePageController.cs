using UnityEngine;

public class PurchasePageController : MonoBehaviour
{
    public IAPController iapController;  // Reference to IAPController in the Purchase page

    private void Start()
    {
        // Find the SpeakarooManager that persists across scenes
        SpeakarooManager speakarooManager = FindObjectOfType<SpeakarooManager>();

        if (iapController == null)
        {
            iapController = FindObjectOfType<IAPController>();  // Find IAPController in the scene
        }

        // Check if both SpeakarooManager and IAPController are found
        if (speakarooManager != null && iapController != null)
        {
            // Set the IAPController reference on SpeakarooManager
            speakarooManager.SetIAPControllerReference(iapController);
            Debug.Log("IAPController reference successfully set in SpeakarooManager.");
        }
        else
        {
            Debug.LogError("SpeakarooManager or IAPController is missing.");
        }
    }
}
