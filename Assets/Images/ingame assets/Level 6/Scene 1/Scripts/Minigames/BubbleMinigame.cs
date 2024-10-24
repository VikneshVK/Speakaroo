using System.Collections.Generic;
using UnityEngine;

public class BubbleMinigame : MonoBehaviour, IMiniGame
{
    private List<BubbleBurst> bubbles;
    private GameObject stCanvasPrefab; // Reference to ST Canvas
    private string stCanvasTag = "Mask"; // The tag assigned to the ST Canvas in the scene
    private MiniGameController miniGameController; // Reference to the MiniGameController

    private void Start()
    {
        Debug.Log("BubbleMinigame initialized.");

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
        // Get all BubbleBurst scripts attached to the bubbles
        bubbles = new List<BubbleBurst>(GetComponentsInChildren<BubbleBurst>());

        // Log the number of bubbles added at the start of the mini-game
        Debug.Log($"StartMiniGame: Number of bubbles added: {bubbles.Count}");

        // Register to the OnDestroy event of each bubble
        foreach (var bubble in bubbles)
        {
            bubble.OnDestroyEvent += OnBubbleDestroyed;
        }
    }

    private void OnBubbleDestroyed(BubbleBurst bubble)
    {
        // Remove the bubble from the list when it is destroyed
        bubbles.Remove(bubble);

        // Log the current number of remaining bubbles
        Debug.Log($"OnBubbleDestroyed: Bubble popped. Remaining bubbles: {bubbles.Count}");

        // If all bubbles are destroyed, end the mini-game
        if (bubbles.Count == 0)
        {
            Debug.Log("All bubbles popped, ending mini-game.");
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
                miniGameController.MarkMiniGameCompleted("BubbleMinigame");
            }
        });
    }
}
