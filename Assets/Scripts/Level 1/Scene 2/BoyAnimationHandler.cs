using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoyAnimationHandler : StateMachineBehaviour
{
    public GameObject prefabToSpawn; // Prefab to instantiate
    public Transform spawnLocation; // Location where the prefab should be spawned

    // Called when the state transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (prefabToSpawn != null && spawnLocation != null)
        {
            Instantiate(prefabToSpawn, spawnLocation.position, spawnLocation.rotation);
        }
        else
        {
            Debug.LogWarning("Prefab to spawn or spawn location not set.");
        }
    }
}
