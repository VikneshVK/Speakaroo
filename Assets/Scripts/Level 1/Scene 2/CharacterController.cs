using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public Animator animator;
    public GameObject prefabToSpawn; // Prefab to instantiate
    public Transform spawnLocation; // Location where the prefab should be spawned

    private void Start()
    {
        AssignAnimationHandlerParameters();
    }

    private void AssignAnimationHandlerParameters()
    {
        BoyAnimationHandler askKikiHandler = animator.GetBehaviour<BoyAnimationHandler>();

        if (askKikiHandler != null)
        {
            askKikiHandler.prefabToSpawn = prefabToSpawn;
            askKikiHandler.spawnLocation = spawnLocation;
        }
        else
        {
            Debug.LogWarning("AskKikiAnimationHandler not found on animator.");
        }
    }
}
