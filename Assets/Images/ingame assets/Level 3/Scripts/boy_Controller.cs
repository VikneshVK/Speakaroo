using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boy_Controlelr : MonoBehaviour
{
    public Transform stopPosition;
    
    public float walkSpeed = 2f;
    public GameObject Bird;

    private Animator animator;
    private Animator birdAnimator;
    private SpriteRenderer spriteRenderer;
    private bool isWalking = false;
    private bool isIdleCompleted = false;
    private bool isWalkCompleted = false;
    void Start()
    {
        animator = GetComponent<Animator>();
        birdAnimator = Bird.GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleIdleCompletion();
        HandleWalking();
        HandleTalkCompletion();
    }

    private void HandleIdleCompletion()
    {
        if (!isIdleCompleted && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            isIdleCompleted = true;
            isWalking = true;
            animator.SetBool("canWalk", true);
            
        }
    }

    private void HandleWalking()
    {
        if (isWalking)
        {
            WalkToStopPosition();
        }
    }

    private void WalkToStopPosition()
    {
        if (stopPosition != null)
        {
            spriteRenderer.flipX = true;
            Vector3 targetPosition = new Vector3(stopPosition.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);

            if (Mathf.Abs(transform.position.x - stopPosition.position.x) <= 0.1f)
            {
                isWalking = false;
                isWalkCompleted = true;
                
                animator.SetBool("canWalk", false);
                spriteRenderer.flipX = false;
                animator.SetBool("canTalk", true);
                
               
            }
        }
    }
    private void HandleTalkCompletion()
    {
        if ( animator.GetCurrentAnimatorStateInfo(0).IsName("Talk") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            
            birdAnimator.SetBool("canTalk", true);
        }
    }
}
