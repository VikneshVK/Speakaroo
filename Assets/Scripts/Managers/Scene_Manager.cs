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
        TransitionAnim.SetTrigger("end");
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(LvlName);
    }
}
