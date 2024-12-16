using UnityEngine;
using System.Collections;

public class TitleSceneManager : MonoBehaviour
{
    public Scene_Manager_Final sceneManager; // Reference to the Scene_Manager_Final script
    public float waitTime = 3f; // Duration to wait before transitioning

    private void Start()
    {
        if (sceneManager == null)
        {
            Debug.LogError("Scene_Manager_Final is not assigned. Please assign it in the Inspector.");
            return;
        }

        StartCoroutine(TransitionToLoadingPage());
    }

    private IEnumerator TransitionToLoadingPage()
    {
        yield return new WaitForSeconds(waitTime);

        // Save the target scene ("StartMenu") in PlayerPrefs
        PlayerPrefs.SetString("TargetScene", "StartMenu");

        // Use Scene_Manager_Final to load the Loading Page
        sceneManager.LoadLevel("Loading Page");
    }
}
