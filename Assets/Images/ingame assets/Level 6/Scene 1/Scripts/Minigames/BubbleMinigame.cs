using System.Collections.Generic;
using UnityEngine;

public class BubbleMinigame : MonoBehaviour, IMiniGame
{
    private List<BubbleBurst> bubbles;
    private GameObject stCanvasPrefab; // Reference to ST Canvas
    private string stCanvasTag = "Mask"; // The tag assigned to the ST Canvas in the scene
    private MiniGameController miniGameController; // Reference to the MiniGameController
    public AudioClip audio1;

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
        bubbles = new List<BubbleBurst>(GetComponentsInChildren<BubbleBurst>());

        Debug.Log($"StartMiniGame: Number of bubbles added: {bubbles.Count}");

        foreach (var bubble in bubbles)
        {
            bubble.OnDestroyEvent += OnBubbleDestroyed;
        }
    }

    private void OnBubbleDestroyed(BubbleBurst bubble)
    {
        bubbles.Remove(bubble);

        Debug.Log($"OnBubbleDestroyed: Bubble popped. Remaining bubbles: {bubbles.Count}");

        if (bubbles.Count == 0)
        {
            Debug.Log("All bubbles popped, ending mini-game.");
            EndMiniGame();
        }
    }

    public void EndMiniGame()
    {
        LeanTween.scale(gameObject, Vector3.zero, 0.25f).setEase(LeanTweenType.easeInBack).setOnComplete(() =>
        {
            Debug.Log("Mini-game scaled down, destroying the game object.");
            Destroy(gameObject);

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Animator playerAnimator = player.GetComponent<Animator>();
                Lvl6Sc1JojoController jojoController = player.GetComponent<Lvl6Sc1JojoController>();
                jojoController.hasSpawnedPrefab = false;
                if (playerAnimator != null)
                {
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
                miniGameController.MarkMiniGameCompleted("BubbleMinigame");
            }
        });
    }
}
