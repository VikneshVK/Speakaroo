using UnityEngine;

public class AnimationStateLoader : StateMachineBehaviour
{
    public string sceneToLoad;
    public string booleanToReset; // The name of the boolean to reset

    // This method is called when the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!string.IsNullOrEmpty(booleanToReset))
        {
            Debug.Log("Resetting boolean parameter: " + booleanToReset);
            animator.SetBool(booleanToReset, false);

            // Verify that the parameter was actually set to false
            bool currentState = animator.GetBool(booleanToReset);
            Debug.Log("Boolean " + booleanToReset + " after reset: " + currentState);
        }
        else
        {
            Debug.LogWarning("booleanToReset is not set in the Animator State.");
        }
    }

    // This method will be called when the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Scene_Manager_Final sceneManager = FindObjectOfType<Scene_Manager_Final>();
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
