using UnityEngine;

public class SpeakarooManager : MonoBehaviour
{
    private static SpeakarooManager instance;
    private IAPController iapController; // Reference to IAPController

    private void Awake()
    {
        // Ensure there's only one instance of SpeakarooManager
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Destroy duplicate
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    public void SetIAPControllerReference(IAPController controller)
    {
        if (controller != null)
        {
            iapController = controller;
            Debug.Log("IAPController reference set in SpeakarooManager.");
        }
        else
        {
            Debug.LogError("Attempted to set a null IAPController reference in SpeakarooManager.");
        }
    }

    public IAPController GetIAPController()
    {
        return iapController;
    }

    public void SaveRenewalData(int successfulRenewals, int failedRenewals)
    {
        PlayerPrefs.SetInt("SuccessfulRenewals", successfulRenewals);
        PlayerPrefs.SetInt("FailedRenewals", failedRenewals);
        PlayerPrefs.Save();
        Debug.Log($"Renewal data saved: {successfulRenewals} successful, {failedRenewals} failed.");
    }

    public (int successfulRenewals, int failedRenewals) LoadRenewalData()
    {
        int successfulRenewals = PlayerPrefs.GetInt("SuccessfulRenewals", 0);
        int failedRenewals = PlayerPrefs.GetInt("FailedRenewals", 0);
        Debug.Log($"Renewal data loaded: {successfulRenewals} successful, {failedRenewals} failed.");
        return (successfulRenewals, failedRenewals);
    }
}
