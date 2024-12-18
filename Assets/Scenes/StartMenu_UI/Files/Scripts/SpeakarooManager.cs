using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

public class SpeakarooManager : MonoBehaviour
{
    private static SpeakarooManager instance;
    private IAPController iapController; // Reference to IAPController
    private bool isSubscribed;
    private bool hasLifetimeAccess;

    //public GameObject unlockButton;
    //public TextMeshProUGUI receiptDetailText;

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

    private void OnEnable()
    {
        // Validate the receipt every time the app opens
        Debug.Log("Validating receipt on enable...");
        ValidateReceiptOnStartup();
    }

    private void Start()
    {
        int subscriptionStatus = PlayerPrefs.GetInt("SubscriptionActive"); // Default is 0 (not subscribed)
        bool isSubscribed = subscriptionStatus == 1;

        // Print the result to the console
        if (isSubscribed)
        {
            Debug.Log("User is subscribed.");
        }
        else
        {
            Debug.Log("User is not subscribed.");
        }
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

    public void SetSubscriptionStatus(bool status)
    {
        isSubscribed = status;
        PlayerPrefs.SetInt("IsSubscribed", status ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"Subscription status set to: {status}");
    }

    // Load subscription status from PlayerPrefs
    public bool GetSubscriptionStatus()
    {
        if (PlayerPrefs.HasKey("IsSubscribed"))
        {
            isSubscribed = PlayerPrefs.GetInt("IsSubscribed") == 1;
        }
        else
        {
            isSubscribed = false; // Default to not subscribed
        }

        Debug.Log($"Subscription status loaded: {isSubscribed}");
        return isSubscribed;
    }

    public void SetLifetimeStatus(bool status)
    {
        hasLifetimeAccess = status;
        PlayerPrefs.SetInt("HasLifetimeAccess", status ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"Lifetime access status set to: {status}");
    }

    public bool GetLifetimeStatus()
    {
        if (PlayerPrefs.HasKey("HasLifetimeAccess"))
        {
            hasLifetimeAccess = PlayerPrefs.GetInt("HasLifetimeAccess") == 1;
        }
        else
        {
            hasLifetimeAccess = false; // Default to no lifetime access
        }

        Debug.Log($"Lifetime access status loaded: {hasLifetimeAccess}");
        return hasLifetimeAccess;
    }



    // For debug purposes: reset subscription status in SpeakarooManager
    public void resetSubGM()
    {
        PlayerPrefs.SetInt("SubscriptionActive", 0);
        isSubscribed = false;
    }

    // This is the method that will validate the receipt on app startup
    public void ValidateReceiptOnStartup()
    {
        // Check if the receipt exists in PlayerPrefs
        if (PlayerPrefs.HasKey("Has Bill"))
        {
            Debug.Log("Receipt found, validating...");
            string receipt = PlayerPrefs.GetString("Has Bill"); // Get receipt from PlayerPrefs

            bool isValid = iapController.ValidateReceipt(receipt); // Call your IAPController's ValidateReceipt method

            if (isValid)
            {
                // If the receipt is valid, update subscription status
                SetSubscriptionStatus(true);
                //hideUnlockButton();
               
                Debug.Log("Receipt validated successfully.");
            }

            else if(isValid && PlayerPrefs.GetInt("Has LifeBill") == 1)
            {
                SetLifetimeStatus(true);
            }
            else
            {
                // If the receipt is invalid, set subscription status to false
                SetSubscriptionStatus(false);
                SetLifetimeStatus(false);
                Debug.Log("Receipt validation failed.");
            }
        }
        else
        {
            // No receipt found, treat as not subscribed
            SetSubscriptionStatus(false);
            SetLifetimeStatus(false);
            //showUnlockButton();
            Debug.Log("No receipt found.");
        }
    }

}

