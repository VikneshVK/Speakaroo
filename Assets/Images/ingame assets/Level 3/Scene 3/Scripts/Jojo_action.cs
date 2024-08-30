using System;
using System.Collections.Generic;
using UnityEngine;

public class Jojo_action : MonoBehaviour
{
    public Transform stopPosition;
    public Transform offScreenPosition;
    public float walkSpeed = 2f;
    public bool hasReachedOffScreen = false; // New boolean to track off-screen position
    public List<GameObject> objectsToEnable;
    public static event Action<GameObject[]> OnCollidersEnabled;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isWalking = false;
    private bool isIdleCompleted = false;
    private bool collidersEnabled = false; // To ensure colliders are enabled only once
    private bool isReturning = false; // Flag for return trip

    private Vector3 targetPosition; // Store the current target position

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isWalking)
        {
            WalkToPosition(targetPosition);
        }
        else if (!isReturning)
        {
            HandleIdleCompletion();
        }
    }

    private void HandleIdleCompletion()
    {
        if (!isIdleCompleted && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            isIdleCompleted = true;
            MoveToStopPosition(); // Start moving to the stop position after idle completes
        }
    }

    public void MoveToStopPosition()
    {
        spriteRenderer.flipX = true; // Ensure sprite is facing correct direction
        animator.SetBool("canWalk", true);
        targetPosition = stopPosition.position;
        isWalking = true;
        isReturning = false;
        
    }

    public void MoveOffScreen()
    {
        spriteRenderer.flipX = false; // Flip X to true when walking off-screen
        animator.SetBool("canWalk", true);
        targetPosition = offScreenPosition.position;
        isWalking = true;
    }

    public void ReturnToStopPosition()
    {
        spriteRenderer.flipX = true; // Flip X to false when returning to stop position
        isReturning = true;
        animator.SetBool("canWalk", true);
        targetPosition = stopPosition.position;
        isWalking = true;
    }

    private void WalkToPosition(Vector3 targetPosition)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
        {
            isWalking = false; // Stop walking
            
            animator.SetBool("canWalk", false);
            animator.SetBool("canTalk", true);
            spriteRenderer.flipX = false;
            if (targetPosition == offScreenPosition.position)
            {
                hasReachedOffScreen = true; // Set the boolean to true when reaching off-screen
            }
            else if (targetPosition == stopPosition.position)
            {
                CheckAndEnableColliders();
            }
        }
    }



    private void CheckAndEnableColliders()
    {
        if (!collidersEnabled)
        {
            foreach (GameObject obj in objectsToEnable)
            {
                ItemDragHandler item = obj.GetComponent<ItemDragHandler>();
                if (item != null /*&& item.isDry*/)
                {
                    Collider2D collider = obj.GetComponent<Collider2D>();
                    if (collider != null)
                    {
                        collider.enabled = true;
                    }
                }
            }
            collidersEnabled = true;

            // Notify the HelpHandController for Jojo's action
            HelpHandController helperHand = FindObjectOfType<HelpHandController>();
            if (helperHand != null)
            {
                helperHand.StartHelperHandRoutineForJojo(objectsToEnable.ToArray());
            }
        }
    }
}
