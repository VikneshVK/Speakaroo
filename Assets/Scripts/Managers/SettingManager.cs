using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject generalSettingsPanel;
    [SerializeField] private GameObject socialMediaPanel;
    [SerializeField] private GameObject parentalControlPanel;
    [SerializeField] private GameObject securityPanel;

    [SerializeField] private TMP_Text securityChallengeText;
    [SerializeField] private TMP_InputField securityAnswerInput;
    [SerializeField] private TMP_Text feedbackText;

    [SerializeField] private GameObject buttonsParent;
    [SerializeField] private Button backspaceButton;

    private int _correctMathAnswer;
    private int _securityCase;

    private void Awake()
    {
        settingsPanel.SetActive(false);
        securityPanel.SetActive(false);
        AssignButtonCallbacks();
    }

    private void AssignButtonCallbacks()
    {
        Button[] buttons = buttonsParent.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            int number = i; // Capture the loop variable
            buttons[i].onClick.AddListener(() => OnNumberButtonClicked(number));
        }
        backspaceButton.onClick.AddListener(OnBackspaceButtonClicked);
    }

    private void OnNumberButtonClicked(int number)
    {
        if (securityAnswerInput.text.Length < 4)
        {
            securityAnswerInput.text += number.ToString();
        }
    }

    public void OnBackspaceButtonClicked()
    {
        if (securityAnswerInput.text.Length > 0)
        {
            securityAnswerInput.text = securityAnswerInput.text.Substring(0, securityAnswerInput.text.Length - 1);
        }
    }

    public void OpenSecurityPanel()
    {
        securityPanel.SetActive(true);
        GenerateSecurityChallenge();
    }

    private void GenerateSecurityChallenge()
    {
        _securityCase = Random.Range(1, 4); // 1 for addition, 2 for subtraction, 3 for year entry
        int number1, number2;

        switch (_securityCase)
        {
            case 1:
                // Addition challenge
                number1 = Random.Range(0, 101);
                number2 = Random.Range(0, 101);
                _correctMathAnswer = number1 + number2;
                securityChallengeText.text = $"Solve this problem to access settings:\n{number1} + {number2} = ?";
                break;
            case 2:
                // Subtraction challenge
                number1 = Random.Range(0, 101);
                number2 = Random.Range(0, number1 + 1);
                _correctMathAnswer = number1 - number2;
                securityChallengeText.text = $"Solve this problem to access settings:\n{number1} - {number2} = ?";
                break;
            case 3:
                // Year entry challenge
                securityChallengeText.text = "Enter your year of birth:";
                break;
        }

        // Clear previous input
        /*securityAnswerInput.text = "";*/  // added as a seperate function below
        feedbackText.text = "";
    }

    public void SubmitSecurityAnswer()
    {
        int userAnswer;
        if (!int.TryParse(securityAnswerInput.text, out userAnswer))
        {
            feedbackText.text = "Invalid input. Please enter a valid number.";
            StartCoroutine(ShowFeedback());
            return;
        }

        bool isCorrect = false;

        switch (_securityCase)
        {
            case 1:
            case 2:
                isCorrect = (userAnswer == _correctMathAnswer);
                break;
            case 3:
                int currentYear = System.DateTime.Now.Year;
                int age = currentYear - userAnswer;
                isCorrect = (age >= 18 && userAnswer > 1900 && userAnswer <= currentYear);
                break;
        }

        if (isCorrect)
        {
            securityPanel.SetActive(false);
            OpenSettingsPanel();
        }
        else
        {
            feedbackText.text = "Incorrect answer. Please try again.";
            ClearSecurityAnswerInput();
            StartCoroutine(ShowFeedback());
        }
    }

    private void ClearSecurityAnswerInput()
    {
        securityAnswerInput.text = "";
    }

    private IEnumerator ShowFeedback()
    {
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        feedbackText.gameObject.SetActive(false);
    }

    public void OpenSettingsPanel()
    {
        settingsPanel.SetActive(true);
        ShowGeneralSettings();
    }

    public void ShowGeneralSettings()
    {
        CloseAllPanels();
        generalSettingsPanel.SetActive(true);
    }

    public void ShowSocialMediaPanel()
    {
        CloseAllPanels();
        socialMediaPanel.SetActive(true);
    }

    public void ShowParentalControlPanel()
    {
        CloseAllPanels();
        parentalControlPanel.SetActive(true);
    }

    public void CloseSettingsPanel()
    {
        settingsPanel.SetActive(false);
    }

    private void CloseAllPanels()
    {
        generalSettingsPanel.SetActive(false);
        socialMediaPanel.SetActive(false);
        parentalControlPanel.SetActive(false);
    }
}
