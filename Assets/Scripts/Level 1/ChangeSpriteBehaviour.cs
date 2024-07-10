using UnityEngine;

public class ChangeSpriteBehaviour : StateMachineBehaviour
{
    public GameObject brushPrefab; 
   

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Transform spawnPos = GameObject.FindWithTag("DropPoint").transform;

        if (brushPrefab != null)
        {
            GameObject instantiatedBrush = GameObject.Instantiate(brushPrefab, spawnPos.position, Quaternion.identity);
            instantiatedBrush.SetActive(true);
            Debug.Log("Brush prefab instantiated and activated.");
        }
        else
        {
            Debug.LogError("Brush prefab is not assigned in the Animator's Inspector.");
        }

       
        GameObject.Destroy(animator.gameObject);
    }
}
