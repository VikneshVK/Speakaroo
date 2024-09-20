using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LVL5Sc12Jojocontroller : MonoBehaviour
{
    public Transform stopPosition;       // Target stop position
    public GameObject prefabToSpawn;     // Prefab to spawn after talk animation
    public Transform prefabSpawnLocation; // Location where the prefab will be spawned
    public float walkSpeed = 2f;         // Speed of the character while walking
    private Animator animator;           // Animator for the character
    private bool isWalking;              // Check if the character is walking
    private bool canWalk;                // Boolean to trigger walk animation
    private bool canTalk;                // Boolean to trigger talk animation

    private bool isIdleCompleted;        // Check if the idle animation is completed
    private bool isTalkCompleted;        // Check if the talk animation is completed

    private float targetXPosition;       // Target x position for the character

    void Start()
    {
        animator = GetComponent<Animator>();
        isWalking = false;
        canWalk = false;
        canTalk = false;
        isIdleCompleted = false;
        isTalkCompleted = false;

        // Set the target X position (only the X axis is considered)
        targetXPosition = stopPosition.position.x;
    }

    void Update()
    {
        if (!isIdleCompleted)
        {
            HandleIdleCompletion();
        }
        else if (canWalk && isWalking)
        {
            WalkToPosition(targetXPosition);
        }
        else if (canTalk)
        {
            HandleTalkCompletion();
        }
    }

    private void HandleIdleCompletion()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            isIdleCompleted = true;
            canWalk = true;
            animator.SetBool("canWalk", true); // Trigger walk animation
            isWalking = true;
        }
    }

    private void WalkToPosition(float targetXPosition)
    {
        // Only update the x-position, keep y and z the same
        Vector3 currentPosition = transform.position;
        currentPosition.x = Mathf.MoveTowards(currentPosition.x, targetXPosition, walkSpeed * Time.deltaTime);
        transform.position = currentPosition;

        if (Mathf.Abs(currentPosition.x - targetXPosition) <= 0.1f)
        {
            // Reached the target x position
            isWalking = false;
            canWalk = false;
            animator.SetBool("canWalk", false); // Stop walk animation
            canTalk = true;
            animator.SetBool("canTalk", true); // Trigger talk animation
        }
    }

    private void HandleTalkCompletion()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Talk") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            canTalk = false;
            animator.SetBool("canTalk", false); // End talk animation
            SpawnPrefab();
        }
    }

    private void SpawnPrefab()
    {
        if (prefabToSpawn != null && prefabSpawnLocation != null)
        {
            Instantiate(prefabToSpawn, prefabSpawnLocation.position, prefabSpawnLocation.rotation);
        }
        else
        {
            Debug.LogError("Prefab or spawn location is missing!");
        }
    }
}
