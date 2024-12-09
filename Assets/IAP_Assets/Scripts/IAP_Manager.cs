using System;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using TMPro;

public class IAP_Manager : MonoBehaviour, IStoreListener
{
    private static IAP_Manager instance;
    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;

    private string YearlySub = "lifetime"; //sub
    private string LifetimeSub = "lifetime_purchase"; //non consumable

    public TextMeshProUGUI subscriptionStatusText;
    public TextMeshProUGUI debugText; // TextMeshPro component for logs
    public GameObject clearLogsButton; // Button to clear logs

    //renewal count --> for testing
    public TextMeshProUGUI renewalCountText;
    private int successfulRenewals = 0; // Tracks successful renewals
    private int failedRenewals = 0;     // Tracks failed renewals

    private string logMessages = ""; // Stores log messages

    public static IAP_Manager Instance => instance;

    void Awake()
    {
        InitializePurchasing();
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void Start()
    {
        // Initialize UI
        InitializeDebugUI();

        await InitializeUnityServices();


        //revokeSub(); //for testing purposes only
        UpdateSubscriptionStatusUI();
        if (IsSubscribed())
        {
            LogMessage("User has an active subscription.");
            GrantPremiumAccess();
        }
        else
        {
            LogMessage("User does not have an active subscription.");
        }
        UpdateRenewalCountUI();
    }

    private void InitializeDebugUI()
    {
        if (debugText != null)
        {
            debugText.text = ""; // Clear initial text
        }

        if (clearLogsButton != null)
        {
            clearLogsButton.SetActive(true); // Enable the button
        }
    }

    private async System.Threading.Tasks.Task InitializeUnityServices()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
            LogMessage("Unity Gaming Services initialized.");
        }
    }

    private void UpdateSubscriptionStatusUI()
    {
        if (subscriptionStatusText != null)
        {
            subscriptionStatusText.text = IsSubscribed()
                ? "Premium User"
                : "Free User";
        }
    }

    private void ValidateSubscriptionStatus(Product product)
    {
        try
        {
            // Create the validator with your app's identifiers
            var validator = new CrossPlatformValidator(
                GooglePlayTangle.Data(),
                null,
                Application.identifier
            );

            // Validate the receipt
            var result = validator.Validate(product.receipt);

            foreach (var receipt in result)
            {
                if (receipt is GooglePlayReceipt googleReceipt)
                {
                    DateTime expiryDate = googleReceipt.purchaseDate;

                    if (DateTime.UtcNow < expiryDate)
                    {
                        LogMessage($"Subscription is active. Expiry Date: {expiryDate}");
                        SaveSubscriptionStatus(true); // Mark user as premium
                        GrantPremiumAccess();         // Unlock premium features
                    }
                    else
                    {
                        LogMessage("Subscription has expired.");
                        SaveSubscriptionStatus(false); // Revert to free user
                    }
                }
                else
                {
                    LogMessage($"Unhandled receipt type: {receipt.GetType()}");
                }
            }
        }
        catch (IAPSecurityException ex)
        {
            LogMessage($"Receipt validation failed: {ex.Message}");
            SaveSubscriptionStatus(false); // Revert to free user
        }
    }



    public void InitializePurchasing()
    {
        if (IsInitialized()) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(YearlySub, ProductType.Subscription);
        builder.AddProduct(LifetimeSub, ProductType.NonConsumable);

        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized() => storeController != null && storeExtensionProvider != null;

    public void BuyYearlySubscription() => BuyProductID(YearlySub);
    public void BuyLifetimeSubscription() => BuyProductID(LifetimeSub);

    public void BuyProductID(string productId)
    {
        if (!IsInitialized())
        {
            LogMessage("IAP is not initialized. Please ensure the Unity Purchasing system is initialized properly.");
            return;
        }

        Product product = storeController?.products?.WithID(productId);

        if (product == null)
        {
            LogMessage($"Product with ID {productId} not found in the store. Please check your IAP catalog.");
            return;
        }

        if (!product.availableToPurchase)
        {
            LogMessage($"Product {productId} is not available for purchase. Ensure the product is active in the Google Play Console.");
            return;
        }

        LogMessage($"Attempting to purchase product: {product.definition.id}");
        storeController.InitiatePurchase(product);
    }

    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            LogMessage("IAP is not initialized, cannot restore purchases.");
            return;
        }
        LogMessage("Restore purchases initiated.");
    }

    public void OnPurchaseCompleted(Product product)
    {
        LogMessage($"Purchase completed for product: {product.definition.id}");
        ValidateReceipt(product);

        if (product.definition.id == YearlySub || product.definition.id == LifetimeSub)
        {
            SaveSubscriptionStatus(true);
            GrantPremiumAccess();
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        LogMessage($"Purchase failed for product {product.definition.id}: {failureReason}");

        if (product.definition.id == YearlySub && failureReason == PurchaseFailureReason.UserCancelled)
        {
            failedRenewals++;
            LogMessage($"Failed renewal detected. Total failed renewals: {failedRenewals}");
            UpdateRenewalCountUI();
        }
    }

    private void ValidateReceipt(Product product)
    {
        try
        {
            // Initialize the receipt validator for Android (Google Play)
            var validator = new CrossPlatformValidator(
                GooglePlayTangle.Data(),
                null,
                Application.identifier
            );

            // Validate the receipt
            var result = validator.Validate(product.receipt);

            // Process each valid receipt
            foreach (var receipt in result)
            {
                LogMessage($"Receipt is valid. Product: {receipt.productID}, Purchase Date: {receipt.purchaseDate}");
            }
        }
        catch (IAPSecurityException ex)
        {
            // Handle invalid receipts
            LogMessage($"Invalid receipt: {ex.Message}");
        }
    }

    public void SaveSubscriptionStatus(bool isSubscribed)
    {
        PlayerPrefs.SetInt("SubscriptionActive", isSubscribed ? 1 : 0);
        PlayerPrefs.Save();

        // Update the subscription status UI
        UpdateSubscriptionStatusUI();
    }

    public bool IsSubscribed() => PlayerPrefs.GetInt("SubscriptionActive", 0) == 1;

    private void GrantPremiumAccess()
    {
        LogMessage("Premium access granted.");
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {

        storeController = controller;
        storeExtensionProvider = extensions;

        foreach (var product in controller.products.all)
        {
            if (product.hasReceipt)
            {
                ValidateSubscriptionStatus(product); // Validate active subscriptions
                Debug.Log("User validation complete, reciept read success");
            }
        }

        LogMessage("Unity IAP successfully initialized.");
    }
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        LogMessage($"Unity IAP initialization failed: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        LogMessage($"Unity IAP initialization failed: {error}. Message: {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Product product = args.purchasedProduct;
        OnPurchaseCompleted(product);

        if (product.definition.id == YearlySub)
        {
            // Subscription-specific processing
            if (IsRenewal(product))
            {
                successfulRenewals++;
                LogMessage($"Successful renewal detected. Total successful renewals: {successfulRenewals}");
                UpdateRenewalCountUI();
            }
        }

        return PurchaseProcessingResult.Complete;
    }

    public void revokeSub()
    {
        PlayerPrefs.SetInt("SubscriptionActive", 0);
    }

    // Method to log messages to TextMeshPro
    private void LogMessage(string message)
    {
        logMessages += $"{message}\n";
        if (debugText != null)
        {
            debugText.text = logMessages;
        }
        Debug.Log(message);
    }

    // Method to clear logs
    public void ClearLogs()
    {
        logMessages = "";
        if (debugText != null)
        {
            debugText.text = "";
        }
        Debug.Log("Logs cleared.");
    }

    private bool IsRenewal(Product product)
    {
        // Detect whether the purchase is a renewal
        return product.receipt != null && product.definition.type == ProductType.Subscription;
    }

    private void UpdateRenewalCountUI()
    {
        if (renewalCountText != null)
        {
            renewalCountText.text = $"Successful Renewals: {successfulRenewals}\nFailed Renewals: {failedRenewals}";
        }
    }
}
