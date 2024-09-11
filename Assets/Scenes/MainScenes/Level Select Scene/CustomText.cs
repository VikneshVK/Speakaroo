using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomText : MonoBehaviour
{
    // References to the buttons
    public Button button1;
    public Button button2;
    public Button button3;

    // Reference to the TextMeshPro component
    public TMP_Text displayText;

    void Start()
    {
        // Initially deactivate the text
        if (displayText != null)
        {
            displayText.gameObject.SetActive(false);
        }

        // Add listeners to the buttons
        button1.onClick.AddListener(() => UpdateText("Following Directions"));
        button2.onClick.AddListener(() => UpdateText("Learn Speech"));
        button3.onClick.AddListener(() => UpdateText("Learn Words"));
    }

    // Function to update the TMP text and activate it
    void UpdateText(string buttonText)
    {
        if (displayText != null)
        {
            displayText.text = buttonText;
            displayText.gameObject.SetActive(true); // Activate the text on button click
        }
    }
}
