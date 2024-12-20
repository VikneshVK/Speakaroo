using UnityEngine;
using UnityEngine.UI;

public enum ButtonType
{
    Premium,
    Free
}

[System.Serializable]
public class ButtonData
{
    public Button button;         // Reference to the Button component
    public ButtonType buttonType; // Type of the button (Premium or Free)
}

public class ButtonManager : MonoBehaviour
{
    public ButtonData[] buttonDataArray; // Array of ButtonData to hold buttons and their types
    [SerializeField]
    private SpeakarooManager gameManager; // Reference to the SpeakarooGameManager
    public IAPController mController;

    void Start()
    {
        // Get reference to the SpeakarooGameManager
        gameManager = FindObjectOfType<SpeakarooManager>();

        if (gameManager == null)
        {
            Debug.LogError("SpeakarooGameManager not found in the scene.");
            return;
        }

        // Check the subscription status at the start
        CheckSubscriptionAndUpdateButtons();

        // Update the button interactability based on the subscription status
        UpdateButtonInteractability();

    }

    private void OnEnable()
    {
        if (mController == null)
        {
            Debug.LogError("mController is not assigned!");
            return; // Avoid calling methods if mController is not assigned

        }

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<SpeakarooManager>();
        }
        gameManager.ValidateReceiptOnStartup();
        Debug.Log("Reciept validated on Button");
        mController.CheckSubscriptionStatus();
        Debug.Log("Sub validated in Button");
        CheckSubscriptionAndUpdateButtons();
        UpdateButtonInteractability();
        Debug.Log("Checked button");
        Debug.Log("Checked button on enable");
    }
    private void CheckSubscriptionAndUpdateButtons()
    {
        bool isSubscribed = PlayerPrefs.GetInt("SubscriptionActive") == 1;
        bool hasReceipt = PlayerPrefs.GetInt("Has Bill") == 1;

        Debug.Log($"CheckSubscriptionAndUpdateButtons: isSubscribed={isSubscribed}, hasReceipt={hasReceipt}");

        if (isSubscribed && hasReceipt)
        {
            Debug.Log("Subscription or lifetime access detected. Updating buttons.");

            foreach (ButtonData buttonData in buttonDataArray)
            {
                Debug.Log($"Button {buttonData.button.name} type before: {buttonData.buttonType}");

                if (buttonData.buttonType == ButtonType.Premium)
                {
                    buttonData.buttonType = ButtonType.Free; // Set to Free if user has access
                    Debug.Log($"Button {buttonData.button.name} type after: {buttonData.buttonType}");
                }
            }
        }
        else
        {
            Debug.Log("No active subscription or lifetime access. Keeping buttons as Premium.");
        }

        // Reflect the changes in button interactivity
        UpdateButtonInteractability();
    }


    // This method will lock or unlock buttons based on their type and the subscription status
    private void UpdateButtonInteractability()
    {
        foreach (ButtonData buttonData in buttonDataArray)
        {
            if (buttonData.buttonType == ButtonType.Premium)
            {
                if (gameManager.GetSubscriptionStatus())
                {
                    UnlockButton(buttonData.button); // Unlock premium buttons if subscribed
                }
                else
                {
                    LockButton(buttonData.button); // Lock premium buttons if not subscribed
                }
            }
            else if (buttonData.buttonType == ButtonType.Free)
            {
                UnlockButton(buttonData.button); // Free buttons are always unlocked
            }
        }
    }
    // Lock a button (make it non-interactable and adjust its alpha)
    private void LockButton(Button button)
    {
        button.interactable = false; // Disable the button interaction
        SetButtonAlpha(button, 50);   // Lower the opacity to indicate it's locked
    }

    // Unlock a button (make it interactable and reset its alpha)
    private void UnlockButton(Button button)
    {
        button.interactable = true;  // Enable the button interaction
        SetButtonAlpha(button, 255); // Reset opacity to full
    }

    // Helper function to set the button alpha (opacity)
    private void SetButtonAlpha(Button button, float alpha)
    {
        ColorBlock cb = button.colors;
        Color normalColor = cb.normalColor;
        normalColor.a = alpha / 255f; // Normalize alpha to 0-1 range
        cb.normalColor = normalColor;
        button.colors = cb;
    }
}
