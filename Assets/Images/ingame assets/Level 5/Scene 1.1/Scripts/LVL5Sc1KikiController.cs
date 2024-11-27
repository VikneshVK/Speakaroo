using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LVL5Sc1KikiController : MonoBehaviour
{
    public Transform birdStopPosition;

    private Animator animator;
    private Vector3 targetPosition;
    public float flySpeed = 2f;

    private bool isFlying;
    private bool isIdleCompleted;

   /* private bool canTalk;*/
    void Start()
    {
        isFlying = false;
        isIdleCompleted = false;

        animator = GetComponent<Animator>();

    }


    void Update()
    {
        if (isFlying)
        {
            FlyToPosition(targetPosition);
        }
        else
        {
            HandleIdleCompletion();
        }


    }

    private void HandleIdleCompletion()
    {
        if (!isIdleCompleted && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.1f)
        {
            isIdleCompleted = true;
            MoveToStopPosition(); // Start flying to the stop position after idle completes
        }
    }

    public void MoveToStopPosition()
    {
        /*spriteRenderer.flipX = true; // Ensure sprite is facing correct direction*/
        animator.SetBool("CanFly", true);
        targetPosition = birdStopPosition.position;
        isFlying = true;
    }

    private void FlyToPosition(Vector3 targetPosition)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, flySpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
        {
            isFlying = false; // Stop flying
            animator.SetBool("CanFly", false);
            /*canTalk = true;*/
        }
    }
}
