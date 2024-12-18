using System;
using UnityEngine;
using UnityEngine.Purchasing;
using TMPro;
using UnityEngine.Purchasing.Security;

public class IAPController : MonoBehaviour, IStoreListener
{
    private IStoreController storeController;
    private IExtensionProvider storeExtensionProvider;

    private string YearlySub = "lifetime"; // Subscription product
    private string LifetimeSub = "lifetime_purchase"; // Non-consumable product

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
    }

    private void OnEnable()
    {
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

                // Check which product is validated
                if (receiptInfo.productID == LifetimeSub)
                {
                    PlayerPrefs.SetInt("Has LifeBill", 1); // 1 for having a valid lifetime receipt
                    PlayerPrefs.Save();
                    SaveLifetimeStatus(true); // Grant lifetime access
                }
                else if (receiptInfo.productID == YearlySub)
                {
                    PlayerPrefs.SetInt("Has Bill", 1); // 1 for having a valid subscription receipt
                    PlayerPrefs.Save();
                    SaveSubscriptionStatus(true); // Grant subscription access
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

            // Clear both subscription and lifetime statuses on failure
            SaveSubscriptionStatus(false);
            SaveLifetimeStatus(false);

            PlayerPrefs.SetInt("Has Bill", 0); // No valid receipt
            PlayerPrefs.Save();

            PlayerPrefs.SetInt("Has LifeBill", 0);

            Debug.Log("Has no bill");
            return false; // Validation failed
        }
    }



    public void CheckSubscriptionStatus()
    {
        // Validate the subscription status each time the user enters the app or relevant scene
        if (storeController != null)
        {
            Product subscriptionProduct = storeController.products.WithID(YearlySub);
            Product lifetimeProduct = storeController.products.WithID(LifetimeSub);
            if (subscriptionProduct != null && subscriptionProduct.hasReceipt)
            {
                string receipt = subscriptionProduct.receipt;
                bool isSubscriptionValid = ValidateReceipt(receipt);

                if (isSubscriptionValid)
                {
                    Debug.Log("Subscription is valid.");
                    SaveSubscriptionStatus(true);  // Subscription is still valid
                }
                else
                {
                    Debug.Log("Subscription is canceled or expired.");
                    //for testing in editor, uncomment here
                    SaveSubscriptionStatus(false);  // Subscription is canceled or expired
                }
            }

            if (lifetimeProduct != null && lifetimeProduct.hasReceipt)
            {
                string lifetime = lifetimeProduct.receipt;
                bool isSubscriptionValid = ValidateReceipt(lifetime);
                if (isSubscriptionValid)
                {
                    SaveLifetimeStatus(true);
                }


                else
                {
                    Debug.Log("No receipt found, subscription may not be active.");
                    SaveLifetimeStatus(false);
                    //SaveSubscriptionStatus(false);  // No receipt found
                }
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
        else if (args.purchasedProduct.definition.id == LifetimeSub)
        {
            LogMessage("Lifetime subscription purchased.");
            SaveLifetimeStatus(true);  // Lifetime access granted
            Debug.Log("Lifetime purchase status updated: Premium user");
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
            failedRenewals++;
            UpdateRenewalCountUI();
            SaveSubscriptionStatus(false);
            SaveLifetimeStatus(false);
            Debug.Log("Failed purchase status has been updated: Free user");
            SaveRenewalDataToManager();

        }
    }

    public void SaveSubscriptionStatus(bool isSubscribed)
    {
        PlayerPrefs.SetInt("SubscriptionActive", isSubscribed ? 1 : 0);
        PlayerPrefs.Save();
        UpdateSubscriptionStatusUI();
    }

    public void SaveLifetimeStatus(bool hasLifetime)
    {
        PlayerPrefs.SetInt("LifetimeAccess", hasLifetime ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool HasLifetimeAccess() => PlayerPrefs.GetInt("LifetimeAccess", 0) == 1;


    public void RevertToFreeUser()
    {
        SaveSubscriptionStatus(false);
        successfulRenewals = 0; // Reset renewal count
        failedRenewals = 0;
        SaveRenewalDataToManager();
        UpdateRenewalCountUI();
        DisplayUserSubscriptionStatus();
        LogMessage("User reverted to free status.");
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
                    SaveLifetimeStatus(true); // Explicitly save lifetime access
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
            if (HasLifetimeAccess())
            {
                subscriptionStatusText.text = "Lifetime Premium User";
            }
            else if (IsSubscribed())
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
