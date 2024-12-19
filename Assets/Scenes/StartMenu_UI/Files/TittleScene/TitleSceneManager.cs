using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleSceneManager : MonoBehaviour
{
    public Scene_Manager_Final sceneManager;
    public Slider loadingSlider; 
    public float waitAfterLoading = 1.5f; 

    private void Start()
    {
        if (sceneManager == null)
        {
            Debug.LogError("Scene_Manager_Final is not assigned. Please assign it in the Inspector.");
            return;
        }

        if (loadingSlider == null)
        {
            Debug.LogError("Loading slider is not assigned. Please assign it in the Inspector.");
            return;
        }

        StartCoroutine(LoadStartSceneWithProgress());
    }

    private IEnumerator LoadStartSceneWithProgress()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("StartMenu");
        asyncLoad.allowSceneActivation = false; // Prevent automatic scene activation

        // Update the loading slider as the scene loads
        while (!asyncLoad.isDone)
        {
            loadingSlider.value = asyncLoad.progress / 0.9f; // Normalize progress (0.9f is max value before activation)
            if (asyncLoad.progress >= 0.9f)
            {
                break;
            }
            yield return null;
        }

        // Ensure the slider is full before proceeding
        loadingSlider.value = 1f;

        // Wait for additional time before activating the scene
        yield return new WaitForSeconds(waitAfterLoading);

        asyncLoad.allowSceneActivation = true; // Activate the scene
    }
}
