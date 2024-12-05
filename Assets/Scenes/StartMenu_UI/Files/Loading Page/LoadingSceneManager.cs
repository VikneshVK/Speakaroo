using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingPageManager : MonoBehaviour
{
    public Slider progressBar;

    private void Start()
    {
        // Get the target scene name passed from the previous SceneManager
        string targetScene = PlayerPrefs.GetString("TargetScene", ""); // Default to an empty string if not found

        if (!string.IsNullOrEmpty(targetScene))
        {
            StartCoroutine(LoadSceneAsync(targetScene));
        }
        else
        {
            Debug.LogError("No target scene specified for loading.");
        }
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            if (progressBar != null)
            {
                progressBar.value = Mathf.Clamp01(operation.progress / 0.9f);
            }

            if (operation.progress >= 0.9f)
            {
                if (progressBar != null)
                {
                    progressBar.value = 1f;
                }

                yield return new WaitForSeconds(0.5f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
