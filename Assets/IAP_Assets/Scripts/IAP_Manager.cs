using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAP_Manager : MonoBehaviour, IStoreListener
{
    private static IAP_Manager instance;

    private static IStoreController storeController; // Handles all the products and purchasing
    private static IExtensionProvider storeExtensionProvider; // Handles platform-specific details

    private string YearlySub = "com.littlelearninglab.speakaroo.yearly_subscription";

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
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;
        Debug.Log("IAP initialized successfully.");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError("IAP Initialization failed: " + error);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (args.purchasedProduct.definition.id == YearlySub)
        {
            Debug.Log("Purchase processed: " + args.purchasedProduct.definition.id);

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

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"IAP Initialization failed: {error}, Message: {message}");
    }     

}
