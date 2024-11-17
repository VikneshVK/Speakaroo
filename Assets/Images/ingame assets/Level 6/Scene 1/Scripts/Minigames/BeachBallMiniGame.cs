using UnityEngine;
using System.Collections.Generic;

public class BeachBallMiniGame : MonoBehaviour, IMiniGame
{
    private List<BouncingBall> balls;
    private GameObject stCanvasPrefab; // Reference to ST Canvas
    private string stCanvasTag = "Mask"; // The tag assigned to the ST Canvas in the scene
    private MiniGameController miniGameController; // Reference to the MiniGameController
    public AudioClip audio1;
    private void Start()
    {
        Debug.Log("BeachBallMiniGame initialized.");

        // Get reference to the ST Canvas using its tag
        stCanvasPrefab = GameObject.FindWithTag(stCanvasTag);
        if (stCanvasPrefab == null)
        {
            Debug.LogError("ST Canvas with tag 'mask' not found!");
            return;
        }

        // Get reference to the MiniGameController
        miniGameController = FindObjectOfType<MiniGameController>();

        StartMiniGame();
    }

    public void StartMiniGame()
    {
        // Get all BouncingBall scripts attached to the balls
        balls = new List<BouncingBall>(GetComponentsInChildren<BouncingBall>());

        // Log the number of balls added at the start of the mini-game
        Debug.Log($"StartMiniGame: Number of balls added: {balls.Count}");

        // Register to the OnDestroy event of each ball
        foreach (var ball in balls)
        {
            ball.OnDestroyEvent += OnBallDestroyed;
        }
    }

    private void OnBallDestroyed(BouncingBall ball)
    {
        // Remove the ball from the list when it is destroyed
        balls.Remove(ball);

        // Log the current number of remaining balls
        Debug.Log($"OnBallDestroyed: Ball destroyed. Remaining balls: {balls.Count}");

        // If all balls are destroyed, end the mini-game
        if (balls.Count == 0)
        {
            Debug.Log("All balls destroyed, ending mini-game.");
            EndMiniGame();
        }
    }

    public void EndMiniGame()
    {
        // Scale down the mini-game prefab
        LeanTween.scale(gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInBack).setOnComplete(() =>
        {
            // Destroy the mini-game prefab after scaling down
            Debug.Log("Mini-game scaled down, destroying the game object.");
            Destroy(gameObject);

            // Find the player by tag and trigger the "canTalk" parameter in its Animator
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Animator playerAnimator = player.GetComponent<Animator>();
                Lvl6Sc1JojoController jojoController = player.GetComponent<Lvl6Sc1JojoController>();
                jojoController.hasSpawnedPrefab = false;
                if (playerAnimator != null)
                {
                    // Set "canTalk" to true to start the "Talk 0" animation
                    playerAnimator.SetBool("canTalk", true);
                    jojoController.audioSource.clip = audio1;
                    jojoController.audioSource.Play();
                    Debug.Log("Player 'canTalk' set to true.");
                }
            }

            // Deactivate the ST Canvas after the mini-game ends
            if (stCanvasPrefab != null)
            {
                stCanvasPrefab.SetActive(false);
            }

            if (miniGameController != null)
            {
                miniGameController.MarkMiniGameCompleted("BeachBallMiniGame");
            }

        });
    }
}
