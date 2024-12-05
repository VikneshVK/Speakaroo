using UnityEngine;
using UnityEngine.Purchasing;

public class IAP_Manager : MonoBehaviour, IStoreListener
{
    private static IAP_Manager instance;

    private static IStoreController storeController; // Handles all the products and purchasing
    private static IExtensionProvider storeExtensionProvider; // Handles platform-specific details

    private string YearlySub = "com.littlelearninglab.speakaroo.yearly_subscription";
    private string LifetimeSub = "lifetime";

    public static IAP_Manager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("IAP_Manager instance is null. Ensure it is present in the scene.");
            }
            return instance;
        }
    }

    void Awake()
    {
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

    void Start()
    {
        if (storeController == null)
        {
            InitializePurchasing();
        }

        // Check subscription status on start
        if (IsSubscribed())
        {
            Debug.Log("User has an active subscription.");
            GrantPremiumAccess();
        }
        else
        {
            Debug.Log("User does not have an active subscription.");
        }
    }

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

    private void SaveSubscriptionStatus(bool isSubscribed)
    {
        PlayerPrefs.SetInt("YearlySubscriptionActive", isSubscribed ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool IsSubscribed()
    {
        return PlayerPrefs.GetInt("YearlySubscriptionActive", 0) == 1;
    }

    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            Debug.Log("IAP is not initialized, cannot restore purchases.");
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("Restoring purchases...");
            var appleExtensions = storeExtensionProvider.GetExtension<IAppleExtensions>();
            appleExtensions.RestoreTransactions((result) => {
                Debug.Log($"Restore transactions completed: {result}");
            });
        }
        else
        {
            Debug.Log("Restore purchases is not supported on this platform.");
        }
    }

    public void OnPurchaseCompleted(Product product)
    {
        if (product.definition.id == YearlySub)
        {
            Debug.Log("Purchased yearly subscription!");
            SaveSubscriptionStatus(true);
            GrantPremiumAccess();
        }
        else if(product.definition.id == LifetimeSub)
        {
            Debug.Log("Lifetime subscription Activated!");
            SaveSubscriptionStatus(true);
            GrantPremiumAccess();
        }
    }

    private void GrantPremiumAccess()
    {
        Scene_Manager_Final sceneManager = FindObjectOfType<Scene_Manager_Final>();
        if (sceneManager != null)
        {
            sceneManager.UnlockPremiumAccess();
            Debug.Log("Premium access granted and applied in Scene Manager.");
        }
        else
        {
            Debug.LogWarning("Scene_Manager_Final not found in the scene.");
        }
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"Purchase of {product.definition.id} failed due to {failureReason}");
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;
        Debug.Log("IAP initialized successfully.");
        foreach (var product in controller.products.all)
        {
            Debug.Log($"Product: {product.definition.id}, Type: {product.definition.type}");
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError("IAP Initialization failed: " + error);
    }

   

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (args.purchasedProduct.definition.id == LifetimeSub || args.purchasedProduct.definition.id == YearlySub)
        {
            Debug.Log("Lifetime purchase processed!");
            SaveSubscriptionStatus(true); // Update your logic for lifetime access
        }
        else
        {
            Debug.LogError($"Unrecognized product ID: {args.purchasedProduct.definition.id}. Verify it in Unity and the store.");
        }

        return PurchaseProcessingResult.Complete;
    }


    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"IAP Initialization failed: {error}. Message: {message}");
    }
}
