using UnityEngine;

public class AnimationStateLoader : StateMachineBehaviour
{
    public string sceneToLoad;
    public int Category;
    public string booleanToReset; // The name of the boolean to reset
    public bool GoToMainScene; // New public boolean

    private BubbleSpawnManager bubbleSpawnManager;
    private bool isSceneLoaded = false; // Flag to prevent multiple scene loads

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!string.IsNullOrEmpty(booleanToReset))
        {
            Debug.Log("Resetting boolean parameter: " + booleanToReset);
            animator.SetBool(booleanToReset, false);
            bool currentState = animator.GetBool(booleanToReset);
            Debug.Log("Boolean " + booleanToReset + " after reset: " + currentState);
        }
        else
        {
            Debug.LogWarning("booleanToReset is not set in the Animator State.");
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        bubbleSpawnManager = FindObjectOfType<BubbleSpawnManager>();

        if (GoToMainScene)
        {
            if (bubbleSpawnManager != null)
            {
                bubbleSpawnManager.OnBubblesDestroyed += HandleBubblesDestroyed; // Subscribe to the event
                bubbleSpawnManager.StartBubbleSpawning(); // Call the method to start bubble spawning
            }
            else
            {
                Debug.LogError("BubbleSpawnManager not found");
            }
        }
        else
        {
            LoadNextLevelDirectly();
        }
    }

    private void HandleBubblesDestroyed()
    {
        if (isSceneLoaded) return; // Prevent multiple scene loads

        Scene_Manager_Final sceneManager = FindObjectOfType<Scene_Manager_Final>();
        if (sceneManager != null && !string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log("Loading level: " + sceneToLoad);
            sceneManager.LoadLevel(sceneToLoad, Category);
            isSceneLoaded = true; // Set the flag to true
        }
        else
        {
            Debug.LogError("Scene_Manager not found or scene name is empty");
        }

        if (bubbleSpawnManager != null)
        {
            bubbleSpawnManager.OnBubblesDestroyed -= HandleBubblesDestroyed; // Unsubscribe from the event
        }
    }

    private void LoadNextLevelDirectly()
    {
        Scene_Manager_Final sceneManager = FindObjectOfType<Scene_Manager_Final>();
        if (sceneManager != null && !string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log("Loading level directly: " + sceneToLoad);
            sceneManager.LoadLevel(sceneToLoad, Category);
        }
        else
        {
            Debug.LogError("Scene_Manager not found or scene name is empty");
        }
    }
}
