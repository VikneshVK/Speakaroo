using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

public class SpeakarooManager : MonoBehaviour
{
    private static SpeakarooManager instance;
    [SerializeField]
    private IAPController iapController; // Reference to IAPController
    private bool isSubscribed;

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

    //new add
    public void SetIAPControllerReference(IAPController controller)
    {
        if (controller == null)
        {
            controller = FindObjectOfType<IAPController>();
        }

        if (controller != null)
        {
            iapController = controller;
            Debug.Log("IAPController reference set in SpeakarooManager.");
        }
        else
        {
            Debug.LogError("Failed to assign IAPController reference.");
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
            bool isSubscribed = PlayerPrefs.GetInt("SubscriptionActive", 0) == 1;
            Debug.Log($"Subscription Status: {isSubscribed}");
            return isSubscribed;

        }
        else
        {
            isSubscribed = false; // Default to not subscribed
        }

        Debug.Log($"Subscription status loaded: {isSubscribed}");
        return isSubscribed;
    }

    

    // For debug purposes: reset subscription status in SpeakarooManager
    public void resetSubGM()
    {
        PlayerPrefs.SetInt("SubscriptionActive", 0);
        isSubscribed = false;
    }

    //This is the method that will validate the receipt on app startup - backup
    public void ValidateReceiptOnStartup()
    {
        //new addition
        if (iapController == null)
        {
            iapController = FindObjectOfType<IAPController>();
            if (iapController == null)
            {
                Debug.LogError("IAPController not found. Cannot validate receipt.");
                return;
            }
        }

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
                iapController.RefreshButtonStates();

                Debug.Log("Receipt validated successfully.");
            }

            
            else
            {
                // If the receipt is invalid, set subscription status to false
                SetSubscriptionStatus(false);
                PlayerPrefs.SetInt("Has Bill", 0);
                Debug.Log("Receipt validation failed.");
                iapController.RefreshButtonStates();
            }
        }
        else
        {
            // No receipt found, treat as not subscribed
            //showUnlockButton();
            Debug.Log("No receipt found.");
        }
    }



}

