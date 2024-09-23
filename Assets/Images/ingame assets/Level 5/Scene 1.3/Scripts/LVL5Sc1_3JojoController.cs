using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LVL5Sc1_3JojoController : MonoBehaviour
{
    public Transform stopPosition;            // Target stop position 1
    public Transform stopPosition2;           // Target stop position 2
    public GameObject prefab1;                // Prefab to spawn after Talk
    public GameObject prefab2;                // Prefab to spawn after Talk3
    public Transform prefabSpawnLocation;     // Location to spawn prefabs
    public float walkSpeed = 2f;              // Speed of Jojo while walking
    private Animator animator;                // Animator component for Jojo
    private bool isWalking;                   // Check if Jojo is walking
    private bool canWalk;                     // Boolean to trigger walk animation
    public bool canTalk;                      // Boolean controlled by another script to trigger Talk2
    public bool canTalk2;                     // Boolean controlled by another script to trigger Talk4
    private bool isWalkingToPosition2;

    private bool creamApplied;                // Check if the cream is applied
    private bool hasSpawnedPrefab1;           // Flag to ensure prefab1 is only spawned once
    private bool hasSpawnedPrefab2;           // Flag to ensure prefab2 is only spawned once
    private float targetXPosition;            // Target x position for Jojo
    private float targetXPosition2;           // Target x position for stopPosition2
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        isWalking = true;
        canWalk = true;
        canTalk = false;
        canTalk2 = false;
        creamApplied = false;
        hasSpawnedPrefab1 = false;
        hasSpawnedPrefab2 = false;
        isWalkingToPosition2 = false;

        // Set the target X positions for walking
        targetXPosition = stopPosition.position.x;
        targetXPosition2 = stopPosition2.position.x;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Immediately start walking
        animator.SetBool("canWalk", true);
    }

    void Update()
    {

        /*AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log("Current State: " + stateInfo);

        if (stateInfo.IsName("Cream Applied"))
        {
            Debug.Log("Currently in Cream Applied state.");
        }*/
        if (canWalk && isWalking)
        {
            if (isWalkingToPosition2)
            {
                WalkToPosition(targetXPosition2);  // Walk to stopPosition2
            }
            else
            {
                WalkToPosition(targetXPosition);   // Walk to stopPosition1
            }
        }
            
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Talk"))
        {
            HandleTalkCompletion(); // Check if Talk animation is completed
        }
        else if (canTalk)
        {
            HandleTalk2Completion();
        }
        else if (creamApplied)
        {
            Debug.Log("Cream Applied is true, calling HandleCreamAppliedCompletion()");
            HandleCreamAppliedCompletion();
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("Talk 3"))
        {
            HandleTalk3Completion();
        }
        else if (canTalk2)
        {
            HandleTalk4Completion();
        }
    }

    private void WalkToPosition(float targetX)
    {
        if (targetX == targetXPosition)
        {
            spriteRenderer.flipX = true;  // Facing left (for stopPosition)
        }
        else
        {
            spriteRenderer.flipX = false; // Facing right (for stopPosition2)
        }
        // Move Jojo toward the target position
        Vector3 currentPosition = transform.position;
        currentPosition.x = Mathf.MoveTowards(currentPosition.x, targetX, walkSpeed * Time.deltaTime);
        transform.position = currentPosition;

        // Check if Jojo reached the target position
        if (Mathf.Abs(currentPosition.x - targetX) <= 0.1f)
        {
            // Reached stop position
            isWalking = false;
            canWalk = false;
            animator.SetBool("canWalk", false); // Stop walking animation
            spriteRenderer.flipX = false;

            // Trigger Talk animation
            if (targetX == targetXPosition)
            {
                animator.SetBool("canTalk", true);
            }
        }
    }

    private void HandleTalkCompletion()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Talk") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
        {
            // Talk animation is complete, transition to idle and spawn prefab1 only once
            if (!hasSpawnedPrefab1)
            {
                animator.SetBool("canTalk", false);
                SpawnPrefab1();
                hasSpawnedPrefab1 = true; // Ensure prefab1 is only spawned once
            }
        }
    }

    private void SpawnPrefab1()
    {
        // Spawn prefab1 after Talk animation completes
        if (prefab1 != null && prefabSpawnLocation != null)
        {
            Instantiate(prefab1, prefabSpawnLocation.position, prefabSpawnLocation.rotation);
        }
        else
        {
            Debug.LogError("Prefab1 or Spawn Location is missing!");
        }
    }

    private void HandleTalk2Completion()
    {
        Debug.Log("cream applied is = " + creamApplied);
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Talk 2") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            // Set creamApplied to true and stop Talk2
            animator.SetBool("canTalk", false);  // End Talk2
            canTalk = false;
            creamApplied = true;
            animator.SetBool("creamApplied", true); // Start CreamApplied state
            Debug.Log("creamApplied is now set to: " + creamApplied);
        }
    }

    private void HandleCreamAppliedCompletion()
    {
        Debug.Log("cream applied animation is called");

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("SunCream") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
        {
            Debug.Log("cream applied animation is about to end");
            // End CreamApplied animation and transition to Talk3
            creamApplied = false;
            animator.SetBool("creamApplied", false);
            animator.SetBool("canTalk", true); // Transition to Talk3

            HandleTalk3Completion();
        }
    }

    private void HandleTalk3Completion()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Talk 3") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            // End Talk3 animation and spawn prefab2 only once
            if (!hasSpawnedPrefab2)
            {
                animator.SetBool("canTalk", false);
                SpawnPrefab2();
                hasSpawnedPrefab2 = true; // Ensure prefab2 is only spawned once
            }
        }
    }

    private void SpawnPrefab2()
    {
        // Spawn prefab2 after Talk3 animation completes
        if (prefab2 != null && prefabSpawnLocation != null)
        {
            Instantiate(prefab2, prefabSpawnLocation.position, prefabSpawnLocation.rotation);
        }
        else
        {
            Debug.LogError("Prefab2 or Spawn Location is missing!");
        }
    }

    private void HandleTalk4Completion()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Talk 4") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            // End Talk4 animation and move Jojo to stopPosition2
            animator.SetBool("canTalk2", false);

            // Start walking to stopPosition2
            canWalk = true;
            isWalking = true;
            animator.SetBool("canWalk", true);  // Start walking animation

            // Move Jojo to stopPosition2
            isWalkingToPosition2 = true;
        }
    }
}
