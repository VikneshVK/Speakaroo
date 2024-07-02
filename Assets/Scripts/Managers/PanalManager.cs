using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class PanalManager : MonoBehaviour
{
    // Singleton instance
    public static PanalManager Instance { get; private set; }

    public float sessionTime; // need to update it on parental control
    private float timer;
    private bool sessionRunning;

    public GameObject timeoutPanel; // Assign in the Inspector
    public TMP_Text timeoutText; // Assign in the Inspector
    public TMP_InputField answerInputField; // Assign in the Inspector
    public TMP_Text feedbackText; // Assign in the Inspector

    private string correctAnswer;
    private int securityCase; // Changed to int for consistency with the switch case
    private int correctMathAnswer;

    [SerializeField] private GameObject buttonsParent; // For number buttons
    [SerializeField] private Button backspaceButton; // For backspace button

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ensure the gameObject persists across scenes
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
        AssignButtonCallbacks();
    }

    private void Start()
    {
        if (timeoutPanel != null)
        {
            timeoutPanel.SetActive(false); // Ensure the timeout panel is initially hidden
        }

        if (SceneManager.GetActiveScene().name != "Home Screen"
            && SceneManager.GetActiveScene().name != "Minigames Level Select") // Start session if not in HomeScene
        {
            StartGameSession();
        }
    }

    private void Update()
    {
        if (sessionRunning)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                TimerEnded();
            }
        }
    }

    public void LoadHomeScene()
    {
        SceneManager.LoadScene("HomeScene");
        HideTimeoutPanel();
    }

    public void LoadLevelsScene()
    {
        SceneManager.LoadScene("LevelsScene");
        HideTimeoutPanel();
    }

    public void LoadLevelScene(int levelIndex)
    {
        SceneManager.LoadScene(levelIndex);
        HideTimeoutPanel();
        StartGameSession();
    }

    public void MiniGameLevelSelectScene()
    {
        SceneManager.LoadScene("Minigames Level Select");
        HideTimeoutPanel();
    }

    public void StartGameSession()
    {
        if (!sessionRunning && sessionTime != 0)
        {
            timer = sessionTime;
            sessionRunning = true;
        }

        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
        }
    }

    public void LevelCompleted()
    {
        int nextLevelIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextLevelIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextLevelIndex);
        }
        else
        {
            SceneManager.LoadScene("GameCompleteScene"); // Example scene for game completion
        }
        HideTimeoutPanel();
    }

    private void TimerEnded()
    {
        sessionRunning = false;
        if (timeoutPanel != null && timeoutText != null)
        {
            timeoutText.text = "Time's up! Solve this problem to continue:";
            timeoutPanel.SetActive(true);
            GenerateMathProblem();
        }
    }

    private void GenerateMathProblem()
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
                timeoutText.text = $"Time's Up..! Solve this problem to Continue:\n{number1} + {number2} = ?";
                break;
            case 2:
                // Subtraction challenge
                number1 = Random.Range(0, 101);
                number2 = Random.Range(0, number1 + 1);
                correctMathAnswer = number1 - number2;
                timeoutText.text = $"Time's Up..! Solve this problem to Continue:\n{number1} - {number2} = ?";
                break;
            case 3:
                // Year entry challenge
                timeoutText.text = "Enter Your year of Birth:";
                break;
        }
    }

    public void CheckAnswer()
    {
        int userAnswer;
        bool isCorrect = false;

        if (!int.TryParse(answerInputField.text, out userAnswer))
        {
            feedbackText.text = "Invalid input. Please enter a valid number.";
            StartCoroutine(ShowFeedback());
            return;
        }

        switch (securityCase)
        {
            case 1:
            case 2:
                isCorrect = (userAnswer == correctMathAnswer);
                break;
            case 3:
                int currentYear = System.DateTime.Now.Year; //to get system time year
                Debug.Log("Current Year is: " + currentYear);
                int age = currentYear - userAnswer; // to calculate age
                Debug.Log("age is:  " + age);
                isCorrect = (age >= 18);
                break;
        }

        if (isCorrect)
        {
            feedbackText.text = "Correct! Loading next level...";
            LoadNextLevel();
        }
        else
        {
            feedbackText.text = "Try again!";
            ClearAnswerInputField();
        }

        // Show feedback for 2 seconds
        StartCoroutine(ShowFeedback());
    }

    private void ClearAnswerInputField()
    {
        answerInputField.text = "";
    }

    private IEnumerator ShowFeedback()
    {
        feedbackText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        feedbackText.gameObject.SetActive(false);
    }

    private void LoadNextLevel()
    {
        int nextLevelIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextLevelIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextLevelIndex);
        }
        else
        {
            SceneManager.LoadScene("GameCompleteScene"); // Example scene for game completion
        }
        HideTimeoutPanel();
    }

    private void HideTimeoutPanel()
    {
        if (timeoutPanel != null)
        {
            timeoutPanel.SetActive(false);
        }
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
        if (answerInputField.text.Length < 4)
        {
            answerInputField.text += number.ToString();
        }
    }

    private void OnBackspaceButtonClicked()
    {
        if (answerInputField.text.Length > 0)
        {
            answerInputField.text = answerInputField.text.Substring(0, answerInputField.text.Length - 1);
        }
    }
}
