using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public Button[] buttons; // Array of buttons in your scene
    private IAP_Manager iapManager; // Reference to the IAP_Manager
    private string YearlySub = "com.littlelearninglab.speakaroo.yearly_subscription"; // Make sure this matches your IAP_Manager

    void Start()
    {
        iapManager = FindObjectOfType<IAP_Manager>(); // Get a reference to the IAP_Manager in your scene

        // Initially lock buttons if purchase is not completed
        if (!IsSubscriptionActive())
        {
            LockAllButtons();
        }
        else
        {
            UnlockAllButtons();
        }
    }

    // Lock all buttons by enabling lock image and setting the alpha to 50
    public void LockAllButtons()
    {
        foreach (Button button in buttons)
        {
            Image lockImage = button.transform.Find("Lock_Image").GetComponent<Image>();
            if (lockImage != null)
            {
                lockImage.gameObject.SetActive(true);
                Debug.Log("Lock image enabled for button: " + button.name);
            }
            else
            {
                Debug.LogError("LockImage not found for button: " + button.name);
            }

            // Make the button uninteractable
            button.interactable = false;

            SetButtonAlpha(button, 50); // Set button alpha to 50
        }
    }

    public void UnlockAllButtons()
    {
        foreach (Button button in buttons)
        {
            Transform lockImageTransform = button.transform.Find("Lock_Image");

            if (lockImageTransform != null)
            {
                Image lockImage = lockImageTransform.GetComponent<Image>();
                if (lockImage != null)
                {
                    lockImage.gameObject.SetActive(false); // Disable the lock image
                    Debug.Log("Lock image disabled for button: " + button.name);
                }
                else
                {
                    Debug.LogError("No Image component found on Lock_Image for button: " + button.name);
                }
            }
            else
            {
                Debug.LogError("Lock_Image not found for button: " + button.name);
            }
            button.interactable = true;
            SetButtonAlpha(button, 255); // Set button alpha to 255
        }
    }

    // Helper function to set the button alpha value
    private void SetButtonAlpha(Button button, float alpha)
    {
        ColorBlock cb = button.colors;
        Color normalColor = cb.normalColor;
        normalColor.a = alpha / 255f; 
        cb.normalColor = normalColor;
        button.colors = cb; 
    }

    
    private bool IsSubscriptionActive()
    {
        // Implement your logic to check if the user has purchased the subscription
        // This is just a placeholder check
        return PlayerPrefs.GetInt(YearlySub, 0) == 1; // 1 if purchased, 0 if not
    }

    // Call this method from IAP_Manager when purchase is completed
    public void OnPurchaseCompleted()
    {
        PlayerPrefs.SetInt(YearlySub, 1); // Store the subscription status
        UnlockAllButtons(); // Unlock buttons once the purchase is complete
        Debug.Log("ButtonManager OnPurchaseCompleted() called");
    }
}
