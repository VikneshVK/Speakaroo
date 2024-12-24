using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParentalControlScript : MonoBehaviour
{
    public GameObject PTPanel; // Reference to the PTPanel
    public GameObject SettingsPanel; // Reference to the settings panel
    public TMP_Text Text1, Text2, Text3; // Text components for the code sequence
    public GameObject Keypad; // Reference to the keypad GameObject
    public int MaxRetries = 3; // Maximum retries allowed

    private Dictionary<int, Sprite> defaultSprites = new Dictionary<int, Sprite>();
    private Dictionary<int, Sprite> pressedSprites = new Dictionary<int, Sprite>();
    private List<int> codeSequence = new List<int>();
    private int currentInputIndex = 0; // Tracks the player's progress in the sequence
    private bool isParentalControlPassed = false; // Tracks if the parental control is passed
    private Dictionary<string, int> wordToNumber = new Dictionary<string, int>()
    {
        { "One", 1 },
        { "Two", 2 },
        { "Three", 3 },
        { "Four", 4 },
        { "Five", 5 },
        { "Six", 6 },
        { "Seven", 7 },
        { "Eight", 8 },
        { "Nine", 9 }
    };
    private int retries;

    private enum ButtonContext { None, Settings, Purchase, Downloads }
    private ButtonContext currentContext = ButtonContext.None;

    void Start()
    {
        retries = 0;
        OpenPTPanel();
    }

    void LoadSprites()
    {
        foreach (Transform button in Keypad.transform)
        {
            int number;
            if (int.TryParse(button.name, out number))
            {
                Button buttonComponent = button.GetComponent<Button>();
                defaultSprites[number] = buttonComponent.image.sprite; // Default sprite
                ButtonControlScript buttonScript = button.GetComponent<ButtonControlScript>();
                if (buttonScript != null)
                {
                    pressedSprites[number] = buttonScript.PressedSprite; // Pressed sprite
                }
            }
        }
    }

    void GenerateRandomSequence()
    {
        List<int> availableNumbers = new List<int>(wordToNumber.Values);
        codeSequence.Clear();

        for (int i = 0; i < 3; i++) // Generate 3 unique random numbers
        {
            int randomIndex = UnityEngine.Random.Range(0, availableNumbers.Count);
            codeSequence.Add(availableNumbers[randomIndex]);
            availableNumbers.RemoveAt(randomIndex);
        }
    }

    void AssignTextValues()
    {
        string[] words = new string[wordToNumber.Count];
        wordToNumber.Keys.CopyTo(words, 0);

        Text1.text = words[codeSequence[0] - 1];
        Text2.text = words[codeSequence[1] - 1];
        Text3.text = words[codeSequence[2] - 1];
    }

    public void OnButtonPressed(int number)
    {
        if (currentInputIndex < codeSequence.Count)
        {
            if (number == codeSequence[currentInputIndex]) // Correct input
            {
                ChangeButtonSprite(number, pressedSprites[number]);
                currentInputIndex++;

                if (currentInputIndex == codeSequence.Count) // Sequence completed
                {
                    isParentalControlPassed = true; // Set the boolean to true                   
                    ExecuteContextAction(); // Perform the action based on the button context
                }
            }
            else // Incorrect input
            {
                retries++;
                ShakeKeypad();
                ResetKeypad();

                if (retries >= MaxRetries)
                {
                    PTPanel.SetActive(false); // Disable the panel after max retries
                    retries = 0;
                }
            }
        }
    }

    void ChangeButtonSprite(int number, Sprite sprite)
    {
        Transform buttonTransform = Keypad.transform.Find(number.ToString());
        if (buttonTransform != null)
        {
            buttonTransform.GetComponent<Image>().sprite = sprite;
        }
    }

    void ResetKeypad()
    {
        currentInputIndex = 0; // Reset progress
        foreach (Transform button in Keypad.transform)
        {
            int number;
            if (int.TryParse(button.name, out number))
            {
                button.GetComponent<Image>().sprite = defaultSprites[number];
            }
        }
        /*PTPanel.SetActive(false);*/
    }

    void ShakeKeypad()
    {
        LeanTween.cancel(Keypad);
        Vector3 originalPosition = Keypad.transform.localPosition;
        LeanTween.moveLocalX(Keypad, originalPosition.x + 10f, 0.1f).setEaseShake().setOnComplete(() =>
        {
            Keypad.transform.localPosition = originalPosition;
        });
    }

    void ExecuteContextAction()
    {
        switch (currentContext)
        {
            case ButtonContext.Settings:
                SettingsPanel.SetActive(true);
                break;
            case ButtonContext.Purchase:
                FindObjectOfType<Scene_Manager_Final>().LoadLevel("Purchase page", 1);
                break;
            case ButtonContext.Downloads:
                FindObjectOfType<Scene_Manager_Final>().LoadLevel("Worksheets", 1);
                break;
        }
        currentContext = ButtonContext.None; // Reset context
        isParentalControlPassed = false;
        ResetKeypad();
    }

    void OpenPTPanel()
    {
        LoadSprites();
        GenerateRandomSequence();
        AssignTextValues();
        ResetKeypad();
    }

    public void OnSettingsButtonClicked()
    {
        if (isParentalControlPassed)
        {
            isParentalControlPassed = false;
            SettingsPanel.SetActive(true); // Open the settings panel directly
        }
        else
        {
            currentContext = ButtonContext.Settings;
            OpenPTPanel();
            PTPanel.SetActive(true); // Open the PT panel
        }
    }

    public void OnPurchaseButtonClicked()
    {
        if (isParentalControlPassed)
        {
            isParentalControlPassed = false;
            FindObjectOfType<Scene_Manager_Final>().LoadLevel("Purchase", 1);
        }
        else
        {
            currentContext = ButtonContext.Purchase;
            
            PTPanel.SetActive(true); // Open the PT panel
        }
    }

    public void OnCloseButtonClicked()
    {
        retries = 0;
        ResetKeypad();
        PTPanel.SetActive(false);        

    }

    public void OnDownloadsButtonClicked()
    {
        if (isParentalControlPassed)
        {
            isParentalControlPassed = false;
            FindObjectOfType<Scene_Manager_Final>().LoadLevel("Worksheets", 1);
        }
        else
        {
            currentContext = ButtonContext.Downloads;
            OpenPTPanel();
            PTPanel.SetActive(true); // Open the PT panel
        }
    }
}
