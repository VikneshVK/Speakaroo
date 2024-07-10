using UnityEngine;

public class AnimationStateLoader : StateMachineBehaviour
{
    
    public string sceneToLoad;

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Scene_Manager sceneManager = FindObjectOfType<Scene_Manager>();
        if (sceneManager != null && !string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log("Loading level: " + sceneToLoad);
            sceneManager.LoadLevel(sceneToLoad);
        }
        else
        {
            Debug.LogError("Scene_Manager not found or scene name is empty");
        }
    }

}
