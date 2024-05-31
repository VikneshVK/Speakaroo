using System.Collections;
using System.Collections.Generic;
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

    private int correctMathAnswer;
    private int securityCase;

    private void Awake()
    {
        settingsPanel.SetActive(false);
        securityPanel.SetActive(false);
    }

    public void OpenSecurityPanel()
    {
        // Activate the security panel and generate a new security challenge
        securityPanel.SetActive(true);
        GenerateSecurityChallenge();
    }

    private void GenerateSecurityChallenge()
    {
        // Randomly select a security case
        securityCase = Random.Range(1, 4); // 1 for addition, 2 for subtraction, 3 for year entry
        int number1, number2;

        switch (securityCase)
        {
            case 1:
                // Addition challenge
                number1 = Random.Range(0, 101);
                number2 = Random.Range(0, 101);
                correctMathAnswer = number1 + number2;
                securityChallengeText.text = $"Solve this problem to access settings:\n{number1} + {number2} = ?";
                break;
            case 2:
                // Subtraction challenge
                number1 = Random.Range(0, 101);
                number2 = Random.Range(0, 101);
                correctMathAnswer = number1 - number2;
                securityChallengeText.text = $"Solve this problem to access settings:\n{number1} - {number2} = ?";
                break;
            case 3:
                // Year entry challenge
                securityChallengeText.text = "Enter Your year of Birth:";
                break;
        }

        // Clear previous input
        securityAnswerInput.text = "";
        feedbackText.text = "";
    }

    public void SubmitSecurityAnswer()
    {
        int userAnswer;
        bool isCorrect = false;

        switch (securityCase)
        {
            case 1:
            case 2:
                if (int.TryParse(securityAnswerInput.text, out userAnswer) && userAnswer == correctMathAnswer)
                {
                    isCorrect = true;
                }
                break;
            case 3:
                if (int.TryParse(securityAnswerInput.text, out userAnswer) && userAnswer >= 2004 && userAnswer <= 2020)
                {
                    isCorrect = true;
                }
                break;
        }

        if (isCorrect)
        {
            // Correct answer, open settings panel
            securityPanel.SetActive(false);
            OpenSettingsPanel();
        }
        else
        {
            // Incorrect answer, show feedback
            feedbackText.text = " Please try again.";
        }
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
