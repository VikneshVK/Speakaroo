using System;
using UnityEngine;
using UnityEngine.Purchasing;
using TMPro;
using UnityEngine.Purchasing.Security;
using UnityEngine.UI;

public class IAPController : MonoBehaviour, IStoreListener
{
    private IStoreController storeController;
    private IExtensionProvider storeExtensionProvider;

    private string YearlySub = "lifetime"; // Subscription product
    private string LifetimeSub = "lifetime_purchase"; // Non-consumable product

    //For button states
    [SerializeField]
    private Button yearlySubscriptionButton;
    [SerializeField]
    private Button lifetimeSubscriptionButton;

    public TextMeshProUGUI subscriptionStatusText;
    public TextMeshProUGUI debugText;
    public TextMeshProUGUI renewalCountText;
    public GameObject clearLogsButton;

    private int successfulRenewals = 0;
    private int failedRenewals = 0;
    private string logMessages = "";

    private SpeakarooManager speakarooManager; // Reference to SpeakarooManager

    private void Awake()
    {
        speakarooManager = FindObjectOfType<SpeakarooManager>(); // Find SpeakarooManager in the scene
        InitializePurchasing();
    }

    private void Start()
    {
        InitializeDebugUI();
        LoadRenewalData();
        DisplayUserSubscriptionStatus(); // Ensure this method is called after loading data
        CheckSubscriptionStatus();  // Check subscription status when the app starts
        speakarooManager.ValidateReceiptOnStartup();

        FindButtons(); // Dynamically find buttons
        UpdateButtonStates(); // Set their states based on the purchase status
    }

    private void FindButtons()
    {
        // Find the buttons in the scene by their names or tags
        if (yearlySubscriptionButton == null)
            yearlySubscriptionButton = GameObject.Find("Yearly_Sub")?.GetComponent<Button>();
    }



    private void UpdateButtonStates()
    {
        bool hasSubscription = IsSubscribed();
        //Debug.Log(hasSubscription + "sub status");

        // Disable buttons if the respective product is already purchased
        if (yearlySubscriptionButton != null)
            yearlySubscriptionButton.interactable = !hasSubscription;

        if (lifetimeSubscriptionButton != null)
            lifetimeSubscriptionButton.interactable = !hasSubscription;
    }


    public void RefreshButtonStates()
    {
        FindButtons();
        UpdateButtonStates();
    }

    private void OnEnable()
    {
        speakarooManager.ValidateReceiptOnStartup();
        CheckSubscriptionStatus();
        Debug.Log("Force check");


    }

    private void InitializeDebugUI()
    {
        if (debugText != null) debugText.text = "";
        if (clearLogsButton != null) clearLogsButton.SetActive(true);
    }

    public void InitializePurchasing()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(YearlySub, ProductType.Subscription);
        builder.AddProduct(LifetimeSub, ProductType.NonConsumable);

        UnityPurchasing.Initialize(this, builder);
        LogMessage("IAP initialization started.");
    }

    //public bool ValidateReceipt(string receipt)
    //{
    //    try
    //    {
    //        var validator = new CrossPlatformValidator(
    //           GooglePlayTangle.Data(),
    //            null, // No AppleTangle for Android-only builds
    //            Application.identifier
    //        );

    //        var result = validator.Validate(receipt);
    //        foreach (var receiptInfo in result)
    //        {
    //            //speakarooManager.DisplayReceiptDetails(receiptInfo);
    //            LogMessage($"Validated receipt: {receiptInfo.productID}");
    //            PlayerPrefs.SetInt("Has Bill", 1); //1 for has bill
    //            PlayerPrefs.Save();
    //            SaveSubscriptionStatus(true);  // Subscription is active
    //            
    //        }

    //        return true; // Validation succeeded
    //    }
    //    catch (IAPSecurityException ex)
    //    {
    //        LogMessage($"Receipt validation failed: {ex.Message}");
    //        SaveSubscriptionStatus(false); // No subscription or canceled
    //        
    //        PlayerPrefs.SetInt("Has Bill", 0); // No receipt
    //        Debug.Log("Has no bill");
    //        return false; // Validation failed
    //    }
    //}

    public bool ValidateReceipt(string receipt)
    {
        try
        {
            var validator = new CrossPlatformValidator(
                GooglePlayTangle.Data(),
                null, // No AppleTangle for Android-only builds
                Application.identifier
            );

            var result = validator.Validate(receipt);
            foreach (var receiptInfo in result)
            {
                LogMessage($"Validated receipt: {receiptInfo.productID}");


                if (receiptInfo.productID == YearlySub)
                {
                    if (IsSubscribed())
                    {
                        PlayerPrefs.SetInt("Has Bill", 1); // 1 for having a valid subscription receipt
                        PlayerPrefs.Save();
                        SaveSubscriptionStatus(true); // Grant subscription access
                        UpdateButtonStates();
                    }

                }
                else
                {
                    LogMessage($"Unknown product ID: {receiptInfo.productID}");
                }
            }

            return true; // Validation succeeded
        }
        catch (IAPSecurityException ex)
        {
            LogMessage($"Receipt validation failed: {ex.Message}");

            // Do not revert to free if user already has access
            if (!IsSubscribed())
            {
                SaveSubscriptionStatus(false);
                PlayerPrefs.SetInt("Has Bill", 0);
                UpdateButtonStates();
            }

            return false;
        }
    }



    public void CheckSubscriptionStatus()
    {
        if (storeController != null)
        {
            Product subscriptionProduct = storeController.products.WithID(YearlySub);

            if (subscriptionProduct != null && subscriptionProduct.hasReceipt)
            {
                string receipt = subscriptionProduct.receipt;
                ValidateReceipt(receipt);
            }

            // Maintain user status if validation fails but status is already active
            if (!IsSubscribed())
            {
                Debug.Log("No valid subscription or lifetime receipt found. Reverting to free user.");
                SaveSubscriptionStatus(false);
                PlayerPrefs.SetInt("Has Bill", 0);
            }
        }
    }


    public void BuyProductID(string productId)
    {
        if (storeController == null)
        {
            LogMessage("Store controller is not initialized.");
            return;
        }

        Product product = storeController.products.WithID(productId);
        if (product != null && product.availableToPurchase)
        {
            LogMessage($"Initiating purchase for product: {productId}");
            storeController.InitiatePurchase(product);
        }
        else
        {
            LogMessage($"Product {productId} is not available for purchase.");
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;
        LogMessage("IAP initialization successful!");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        LogMessage($"IAP initialization failed: {error}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        LogMessage($"Purchase processing started for: {args.purchasedProduct.definition.id}");

        if (args.purchasedProduct.definition.id == YearlySub)
        {
            LogMessage("Yearly subscription purchased.");
            successfulRenewals++;
            SaveSubscriptionStatus(true);  // Subscription is active after purchase
            Debug.Log("Successfull purchase status has been updated: Premium user");
        }

        UpdateRenewalCountUI();
        SaveRenewalDataToManager();
        DisplayUserSubscriptionStatus();
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        LogMessage($"Purchase failed for {product.definition.id}. Reason: {failureReason}");

        if (product.definition.id == YearlySub)
        {
            // Log the failure but do not revert if the user has an active subscription or lifetime
            if (!IsSubscribed())
            {
                SaveSubscriptionStatus(false);
                Debug.Log("Failed purchase status updated: Free user");
            }
            else
            {
                Debug.Log("Purchase failed, but the user still retains their existing status.");
            }
        }
    }

    public void SaveSubscriptionStatus(bool isSubscribed)
    {
        PlayerPrefs.SetInt("SubscriptionActive", isSubscribed ? 1 : 0);
        PlayerPrefs.Save();
        UpdateSubscriptionStatusUI();
    }


    public void RevertToFreeUser()
    {
        SaveSubscriptionStatus(false);
        successfulRenewals = 0; // Reset renewal count
        failedRenewals = 0;
        SaveRenewalDataToManager();
        UpdateRenewalCountUI();
        DisplayUserSubscriptionStatus();
        LogMessage("User reverted to free status.");
        PlayerPrefs.GetInt("Has Bill", 0);
        PlayerPrefs.Save();
    }

    public bool IsSubscribed() => PlayerPrefs.GetInt("SubscriptionActive", 0) == 1;

    private void UpdateSubscriptionStatusUI()
    {
        if (subscriptionStatusText != null)
        {
            subscriptionStatusText.text = IsSubscribed() ? "Premium User" : "Free User";
        }
    }

    private void UpdateRenewalCountUI()
    {
        if (renewalCountText != null)
        {
            renewalCountText.text = $"Successful Renewals: {successfulRenewals}\nFailed Renewals: {failedRenewals}";
        }
    }

    private void SaveRenewalDataToManager()
    {
        if (speakarooManager != null)
        {
            speakarooManager.SaveRenewalData(successfulRenewals, failedRenewals);
            LogMessage("Renewal data saved to SpeakarooManager.");
        }
        else
        {
            LogMessage("SpeakarooManager not found. Could not save renewal data.");
        }
    }

    private void LoadRenewalData()
    {
        if (speakarooManager != null)
        {
            (successfulRenewals, failedRenewals) = speakarooManager.LoadRenewalData();
            UpdateRenewalCountUI();
            LogMessage("Loaded renewal data from SpeakarooManager.");
        }
        else
        {
            LogMessage("SpeakarooManager not found. Could not load renewal data.");
        }
    }

    private void DisplayUserSubscriptionStatus()
    {
        string status = IsSubscribed() ? "Premium User" : "Free User";
        LogMessage($"User subscription status: {status}");
        UpdateSubscriptionStatusUI();
        UpdateRenewalCountUI();
    }

    private void LogMessage(string message)
    {
        logMessages += $"{message}\n";
        if (debugText != null) debugText.text = logMessages;
        Debug.Log(message);
    }

    public void ClearLogs()
    {
        logMessages = "";
        if (debugText != null) debugText.text = "";
        Debug.Log("Logs cleared.");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        throw new NotImplementedException();
    }

    //Restore Purchase
    public void RestorePurchases()
    {
        if (storeController == null)
        {
            Debug.LogError("Store controller is not initialized. Cannot restore purchases.");
            return;
        }

        Debug.Log("Restoring purchases...");

        bool anyPurchaseRestored = false;

        foreach (var product in storeController.products.all)
        {
            if (product.hasReceipt)
            {
                Debug.Log($"Found receipt for product: {product.definition.id}");
                bool isValid = ValidateReceipt(product.receipt);

                if (isValid)
                {
                    Debug.Log($"Restored valid purchase: {product.definition.id}");
                    SaveSubscriptionStatus(true); // Restore subscription status if valid
                    anyPurchaseRestored = true;
                }

                if (product.definition.id == LifetimeSub && isValid)
                {
                    LogMessage("Restored lifetime purchase.");
                    anyPurchaseRestored = true;
                }

                else
                {
                    Debug.LogWarning($"Invalid or expired receipt for product: {product.definition.id}");
                }
            }
        }

        if (!anyPurchaseRestored)
        {
            Debug.LogWarning("No purchases found to restore.");
            // Optionally notify the user that no purchases were restored.
        }


    }


    //Debug
    private void UpdateLifetimeStatusUI()
    {
        if (subscriptionStatusText != null)
        {

            if (IsSubscribed())
            {
                subscriptionStatusText.text = "Subscription Premium User";
            }
            else
            {
                subscriptionStatusText.text = "Free User";
            }
        }
    }



}
