using System;
using UnityEngine;
using UnityEngine.Purchasing;
using TMPro;

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
            SaveSubscriptionStatus(true);
        }
        else if (args.purchasedProduct.definition.id == LifetimeSub)
        {
            LogMessage("Lifetime subscription purchased.");
            SaveSubscriptionStatus(true);
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
            SaveRenewalDataToManager();
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
}
