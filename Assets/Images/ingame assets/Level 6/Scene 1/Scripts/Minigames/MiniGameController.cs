using UnityEngine;
using System.Collections.Generic;

public class MiniGameController : MonoBehaviour
{
    // List to track completed mini-games
    public List<string> completedMiniGames = new List<string>();
    public BeachBoxHandler beachBoxHandler;

    // Variable to track the number of completed mini-games
    private int completedMiniGamesCount = 0;

    // Public property to access completed mini-games count
    public int CompletedMiniGamesCount => completedMiniGamesCount;

    // Method to mark a mini-game as completed
    public void MarkMiniGameCompleted(string miniGameName)
    {
        if (!completedMiniGames.Contains(miniGameName))
        {
            completedMiniGames.Add(miniGameName);
            completedMiniGamesCount++;

            Debug.Log($"Mini-game '{miniGameName}' marked as completed.");
            Debug.Log($"Total completed mini-games: {completedMiniGamesCount}");

            // Set playRestared to true in BeachBoxHandler
            beachBoxHandler.playRestared = true;
        }
    }

    // Method to check if a mini-game has been completed
    public bool IsMiniGameCompleted(string miniGameName)
    {
        bool isCompleted = completedMiniGames.Contains(miniGameName);
        Debug.Log($"Checking if '{miniGameName}' is completed: {isCompleted}");
        return isCompleted;
    }
}
