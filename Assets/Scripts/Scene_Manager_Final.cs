using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Scene_Manager_Final : MonoBehaviour
{
    public Animator TransitionAnim;
    public int currentSceneCategory;
    /*private string[] freeScenes = { "StartMenu", "Purchase page", "Learn Words", "Learn to Speak", "Learn Sentences", "Follow Direction", "Downloads", "Scene 1" };*/
    public List<string> scenesWithLoadingScreen;
    private BGAudioManager_Final bgAudioManager;
    /*private static Scene_Manager_Final instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Make this persistent
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }*/
    void Start()
    {
        bgAudioManager = FindObjectOfType<BGAudioManager_Final>();
        SetNativeRefreshRate();
        if (SceneManager.GetActiveScene().name == "StartMenu")
        {
            ResetGameState();
            ResetAllAnimators();
        }
    }

    private void SetNativeRefreshRate()
    {
        float refreshRate = (float)Screen.currentResolution.refreshRateRatio.value;

        Application.targetFrameRate = Mathf.RoundToInt(refreshRate);

        Debug.Log($"Screen Refresh Rate: {refreshRate} Hz. Target Frame Rate set to: {Application.targetFrameRate}");
    }

    /* public void LoadLevel(string LvlName)
     {
         StartCoroutine(LoadScene(LvlName));
         *//* if (IsSceneAccessible(LvlName))
          {
              StartCoroutine(LoadScene(LvlName));
          }
          else
          {
              Debug.Log("Access Denied: This scene is locked.");
              ShowLockedSceneMessage();
          }*//*
     }*/

    public void LoadLevel(string sceneName, int sceneCategory)
    {
        if (scenesWithLoadingScreen.Contains(sceneName))
        {
            StartCoroutine(LoadSceneWithLoadingScreen(sceneName, sceneCategory));
        }
        else
        {
            StartCoroutine(LoadSceneDirectly(sceneName, sceneCategory));
        }
    }



    private IEnumerator LoadSceneDirectly(string sceneName, int targetSceneCategory)
    {
        Debug.Log("Attempting to load scene: " + sceneName);

        if (TransitionAnim != null)
        {
            TransitionAnim.SetTrigger("end");
        }

        // Change audio if categories differ
        if (bgAudioManager != null && currentSceneCategory != targetSceneCategory)
        {
            bgAudioManager.PlayAudioForCategory(targetSceneCategory);
        }

        yield return new WaitForSeconds(1.5f);

        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        Debug.Log("Now loading scene asynchronously.");

        // Load the scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Optionally, block activation until desired
        asyncLoad.allowSceneActivation = false;

        // Wait until the scene is loaded
        while (!asyncLoad.isDone)
        {
            // Check if the load is nearly complete
            if (asyncLoad.progress >= 0.9f && !asyncLoad.allowSceneActivation)
            {
                // Optionally, trigger activation at the right moment
                Debug.Log("Scene almost loaded. Activating...");
                asyncLoad.allowSceneActivation = true;
            }
            yield return null; // Wait for the next frame
        }

        yield return new WaitForEndOfFrame();

        ResetAllAnimators();
    }






    private IEnumerator LoadSceneWithLoadingScreen(string targetScene, int targetSceneCategory)
    {
        Debug.Log($"Loading scene with loading screen: {targetScene}");

        if (TransitionAnim != null)
        {
            TransitionAnim.SetTrigger("end");
        }

        // Change audio if categories differ
        if (bgAudioManager != null && currentSceneCategory != targetSceneCategory)
        {
            bgAudioManager.PlayAudioForCategory(targetSceneCategory);
        }

        yield return new WaitForSeconds(1.5f);

        // Save the target scene name
        PlayerPrefs.SetString("TargetScene", targetScene);

        // Load the Loading Page scene
        SceneManager.LoadScene("Loading Page");
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == targetScene);

        ResetAllAnimators();
    }

    private void ResetGameState()
    {
        Debug.Log("Resetting game state.");
        PlayerPrefs.DeleteKey("LastCompletedLevel");        
    }

    private void ResetAllAnimators()
    {
        Debug.Log("Resetting all animators and their parameters in the scene.");
        Animator[] animators = FindObjectsOfType<Animator>();
        foreach (Animator animator in animators)
        {
            animator.Rebind();  
            animator.Update(0f);  
        }
    }

    /*private bool IsSceneAccessible(string sceneName)
    {
        return HasPremiumAccess() || System.Array.Exists(freeScenes, scene => scene == sceneName);
    }*/

    

    //private bool HasPremiumAccess()
    //{
    //    return IAPController.Instance.IsSubscribed() || PlayerPrefs.GetInt("IsPremiumUser", 0) == 1;
    //}

    //public void UnlockPremiumAccess()
    //{
    //    PlayerPrefs.SetInt("IsPremiumUser", 1); 
    //    PlayerPrefs.Save();
    //    Debug.Log("Premium access granted.");
    //}

    //public void RevokePremiumAccess()
    //{
    //    PlayerPrefs.SetInt("IsPremiumUser", 0); 
    //    PlayerPrefs.Save();
    //    Debug.Log("Premium access revoked.");
    //}

    //private void ShowLockedSceneMessage()
    //{
    //    Debug.Log("This scene is locked. Unlock premium to access it.");        
    //}

    public int GetLastCompletedLevel()
    {
        return PlayerPrefs.GetInt("LastCompletedLevel", 1); 
    }
}
