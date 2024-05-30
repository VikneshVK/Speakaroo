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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            /*DontDestroyOnLoad(gameObject);*/
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (timeoutPanel != null)
        {
            timeoutPanel.SetActive(false); // Ensure the timeout panel is initially hidden
        }

        StartGameSession();  // dont forget to remove it after the demo
                             // code is set to run automatically when the level is loaded.
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
    }

    public void LoadLevelsScene()
    {
        SceneManager.LoadScene("LevelsScene");
    }

    /*public void LoadSettingsScene()
    {
        SceneManager.LoadScene("SettingsScene");
    }*/

    public void LoadLevelScene(int levelIndex)
    {
        if (timeoutPanel != null)
        {
            timeoutPanel.SetActive(false); // Ensure the timeout panel is hidden before loading a new scene
        }
        SceneManager.LoadScene(levelIndex);
        StartGameSession();
    }

    public void StartGameSession()
    {
        if (!sessionRunning)
        {
            timer = sessionTime;
            sessionRunning = true;
        }
        feedbackText.gameObject.SetActive(false);
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
        string[] operators = { "+", "-", "*", "<", ">" };
        string selectedOperator = operators[Random.Range(0, operators.Length)];

        switch (selectedOperator)
        {
            case "+":
                correctAnswer = (num1 + num2).ToString();
                timeoutText.text += $"\n{num1} + {num2} = ?";
                break;
            case "-":
                correctAnswer = (num1 - num2).ToString();
                timeoutText.text += $"\n{num1} - {num2} = ?";
                break;
            case "*":
                correctAnswer = (num1 * num2).ToString();
                timeoutText.text += $"\n{num1} * {num2} = ?";
                break;
            case "<":
                correctAnswer = num1 < num2 ? "Yes" : "No";
                timeoutText.text += $"\nIs {num1} < {num2}? (Yes or No)";
                break;
            case ">":
                correctAnswer = num1 > num2 ? "Yes" : "No";
                timeoutText.text += $"\nIs {num1} > {num2}? (Yes or No)";
                break;
        }
    }

    public void CheckAnswer()
    {
        if (answerInputField.text.Trim().ToLower() == correctAnswer.ToLower())
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
    }
}
