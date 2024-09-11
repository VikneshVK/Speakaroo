using UnityEngine;
using UnityEngine.Purchasing;

public class IAP_Manager : MonoBehaviour, IStoreListener
{
    private static IStoreController storeController; // Handles all the products and purchasing
    private static IExtensionProvider storeExtensionProvider; // Handles platform-specific details

    private string YearlySub = "com.littlelearninglab.speakaroo.yearly_subscription";

    void Start()
    {
        if (storeController == null)
        {
            InitializePurchasing();
        }
    }

    // Initialize the purchasing process
    public void InitializePurchasing()
    {
        if (IsInitialized()) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(YearlySub, ProductType.Subscription);

        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }

    // Handle successful purchases
    public void OnPurchaseCompleted(Product product)
    {
        if (product.definition.id == YearlySub)
        {
            Debug.Log("Purchased yearly subscription!");
            // Additional logic here
        }
    }


    // Handle failed purchases
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"Purchase of {product.definition.id} failed due to {failureReason}");
        // Here you can show a popup or message to the user about the failure
    }

    // Unity IAP callbacks
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;
        Debug.Log("IAP initialized successfully.");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError("IAP Initialization failed: " + error);
        // Optional: Show user-friendly message about the failure
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (args.purchasedProduct.definition.id == YearlySub)
        {
            Debug.Log("Purchase processed: " + args.purchasedProduct.definition.id);

            // Call ButtonManager to unlock the buttons
            ButtonManager buttonManager = FindObjectOfType<ButtonManager>();
            if (buttonManager != null)
            {
                buttonManager.OnPurchaseCompleted();
                Debug.Log("ButtonManager OnPurchaseCompleted() called from IAP_Manager");
            }
            else
            {
                Debug.LogError("ButtonManager not found in the scene!");
            }
        }
        else
        {
            Debug.LogError("Unrecognized product: " + args.purchasedProduct.definition.id);
        }

        return PurchaseProcessingResult.Complete;
    }


    // Use this method if a failure with extra error message is implemented
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"IAP Initialization failed: {error}, Message: {message}");
        // Optional: Show user-friendly message
    }

    public void ResetPurchase()
    {
        
        PlayerPrefs.DeleteKey(YearlySub);
        PlayerPrefs.Save();

        Debug.Log("Purchase has been reset.");

        // Optionally, re-lock the buttons
        ButtonManager buttonManager = FindObjectOfType<ButtonManager>();
        if (buttonManager != null)
        {
            buttonManager.LockAllButtons(); // Re-lock the buttons if needed
            Debug.Log("Buttons have been locked after reset.");
        }
    }
}
