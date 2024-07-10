using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Manager : MonoBehaviour
{
    public Animator TransitionAnim;

    // Update is called once per frame
    void Update()
    {

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
}
