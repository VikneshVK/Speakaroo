using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Manager : MonoBehaviour
{
    public Animator TransitionAnim;

    void Start()
    {
        // Check if the LevelSelect scene is loaded
        if (SceneManager.GetActiveScene().name == "LevelSelect")
        {
            ResetGameState();
            ResetAllAnimators();
        }
    }

    public void LoadLevel(string LvlName)
    {
        StartCoroutine(LoadScene(LvlName));
    }

    IEnumerator LoadScene(string LvlName)
    {
        Debug.Log("Attempting to load scene: " + LvlName);
        TransitionAnim.SetTrigger("end");
        yield return new WaitForSeconds(1.5f);  // Ensure this delay aligns with your animation duration
        Debug.Log("Now loading scene.");
        SceneManager.LoadScene(LvlName);
    }

    void ResetGameState()
    {
        Debug.Log("Resetting game state.");
        PlayerPrefs.DeleteAll();  // Example: Clear PlayerPrefs
        // Add any other logic needed to reset your game state here
    }

    void ResetAllAnimators()
    {
        Debug.Log("Resetting all animators and their parameters in the scene.");
        Animator[] animators = FindObjectsOfType<Animator>();
        foreach (Animator animator in animators)
        {
            
            animator.Rebind();  // Reset the animator to its default state
            animator.Update(0f);  // Force the animator to update after resetting
        }
    }

    
}
