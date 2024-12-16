using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Scene_Manager_Final : MonoBehaviour
{
    public Animator TransitionAnim;
    
    /*private string[] freeScenes = { "StartMenu", "Purchase page", "Learn Words", "Learn to Speak", "Learn Sentences", "Follow Direction", "Downloads", "Scene 1" };*/
    public List<string> scenesWithLoadingScreen;
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

    public void LoadLevel(string sceneName)
    {
        if (scenesWithLoadingScreen.Contains(sceneName))
        {
            StartCoroutine(LoadSceneWithLoadingScreen(sceneName));
        }
        else
        {
            StartCoroutine(LoadSceneDirectly(sceneName));
        }
    }

    private IEnumerator LoadSceneWithLoadingScreen(string targetScene)
    {
        Debug.Log($"Loading scene with loading screen: {targetScene}");

        if (TransitionAnim != null)
        {
            TransitionAnim.SetTrigger("end");
        }

        yield return new WaitForSeconds(1.5f);

        // Save the target scene name
        PlayerPrefs.SetString("TargetScene", targetScene);

        // Load the Loading Page scene
        SceneManager.LoadScene("Loading Page");
    }


    IEnumerator LoadSceneDirectly(string LvlName)
    {
        Debug.Log("Attempting to load scene: " + LvlName);

        if (TransitionAnim != null)
        {
            TransitionAnim.SetTrigger("end");
        }

        yield return new WaitForSeconds(1.5f);

        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        Debug.Log("Now loading scene.");
        SceneManager.LoadScene(LvlName);
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

    

        private bool HasPremiumAccess()
    {
        return IAP_Manager.Instance.IsSubscribed() || PlayerPrefs.GetInt("IsPremiumUser", 0) == 1;
    }

    public void UnlockPremiumAccess()
    {
        PlayerPrefs.SetInt("IsPremiumUser", 1); 
        PlayerPrefs.Save();
        Debug.Log("Premium access granted.");
    }

    public void RevokePremiumAccess()
    {
        PlayerPrefs.SetInt("IsPremiumUser", 0); 
        PlayerPrefs.Save();
        Debug.Log("Premium access revoked.");
    }

    private void ShowLockedSceneMessage()
    {
        Debug.Log("This scene is locked. Unlock premium to access it.");        
    }

    public int GetLastCompletedLevel()
    {
        return PlayerPrefs.GetInt("LastCompletedLevel", 1); 
    }
}
