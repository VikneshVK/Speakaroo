using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrisbeeMiniGame : MonoBehaviour, IMiniGame
{
    private List<FrisbeeSpin> frisbees; // Updated to handle FrisbeeSpin objects
    private GameObject stCanvasPrefab; // Reference to ST Canvas
    private string stCanvasTag = "Mask"; // The tag assigned to the ST Canvas in the scene
    private MiniGameController miniGameController; // Reference to the MiniGameController
    public AudioClip audio1;

    private void Start()
    {
        Debug.Log("FrisbeeMiniGame initialized.");

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
        // Get all FrisbeeSpin scripts attached to the frisbees
        frisbees = new List<FrisbeeSpin>(GetComponentsInChildren<FrisbeeSpin>());

        // Log the number of frisbees added at the start of the mini-game
        Debug.Log($"StartMiniGame: Number of frisbees added: {frisbees.Count}");

        // Register to the OnDestroy event of each frisbee
        foreach (var frisbee in frisbees)
        {
            frisbee.OnDestroyEvent += OnFrisbeeDestroyed;
        }
    }

    private void OnFrisbeeDestroyed(FrisbeeSpin frisbee)
    {
        // Remove the frisbee from the list when it is destroyed
        frisbees.Remove(frisbee);

        // Log the current number of remaining frisbees
        Debug.Log($"OnFrisbeeDestroyed: Frisbee destroyed. Remaining frisbees: {frisbees.Count}");

        // If all frisbees are destroyed, end the mini-game
        if (frisbees.Count == 0)
        {
            Debug.Log("All frisbees destroyed, ending mini-game.");
            EndMiniGame();
        }
    }

    public void EndMiniGame()
    {
        // Scale down the mini-game prefab
        LeanTween.scale(gameObject, Vector3.zero, 0.25f).setEase(LeanTweenType.easeInBack).setOnComplete(() =>
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
                miniGameController.MarkMiniGameCompleted("FrisbeeMiniGame");
            }

        });
    }
}
