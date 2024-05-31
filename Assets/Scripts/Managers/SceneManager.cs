using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class sceneManager : MonoBehaviour
{
    // Singleton instance
    public static sceneManager Instance { get; private set; }

    public float sessionTime; // need to update it on parental control
    private float timer;
    private bool sessionRunning;

    public GameObject timeoutPanel; // Assign in the Inspector
    public TMP_Text timeoutText; // Assign in the Inspector
    public TMP_InputField answerInputField; // Assign in the Inspector
    public TMP_Text feedbackText; // Assign in the Inspector

    private string correctAnswer;
    private int securityCase; // Changed to int for consistency with the switch case

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
        if (!sessionRunning &&  sessionTime != 0)
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
        int num1 = Random.Range(0, 101);
        int num2 = Random.Range(0, 101);
        securityCase = Random.Range(1, 4); // 1 for addition, 2 for subtraction, 3 for year entry

        switch (securityCase)
        {
            case 1:
                // Addition challenge
                correctAnswer = (num1 + num2).ToString();
                timeoutText.text = $"Time's up! Solve this problem to continue:\n{num1} + {num2} = ?";
                break;
            case 2:
                // Subtraction challenge
                correctAnswer = (num1 - num2).ToString();
                timeoutText.text = $"Time's up! Solve this problem to continue:\n{num1} - {num2} = ?";
                break;
            case 3:
                // Year entry challenge
                timeoutText.text = "Enter your year of birth:";
                break;
        }

        // Clear previous input
        answerInputField.text = "";
        feedbackText.text = "";
    }

    public void CheckAnswer()
    {
        bool isCorrect = false;

        switch (securityCase)
        {
            case 1:
            case 2:
                if (answerInputField.text.Trim().ToLower() == correctAnswer.ToLower())
                {
                    isCorrect = true;
                }
                break;
            case 3:
                if (int.TryParse(answerInputField.text, out int userAnswer) && userAnswer >= 2004 && userAnswer <= 2020)
                {
                    isCorrect = true;
                }
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
        }

        // Show feedback for 2 seconds
        StartCoroutine(ShowFeedback());
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
}
