using UnityEngine;

public class DressController : StateMachineBehaviour
{
    // You can use the OnStateExit method to trigger actions right after the animation state ends
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject[] dresses = { GameObject.FindWithTag("SummerDress"), GameObject.FindWithTag("WinterDress"), GameObject.FindWithTag("SchoolDress") };
        foreach (GameObject dress in dresses)
        {
            Collider2D collider = dress.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
    }
}
