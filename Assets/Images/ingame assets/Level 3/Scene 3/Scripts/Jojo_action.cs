using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jojo_action : MonoBehaviour
{
    public Transform stopPosition;
    public float walkSpeed = 2f;
    public GameObject Bird;
    public List<GameObject> objectsToEnable; // List to hold objects whose colliders will be enabled


    private Animator animator;
    private Animator birdAnimator;
    private SpriteRenderer spriteRenderer;
    
    private bool isWalking = false;
    private bool isIdleCompleted = false;
    private bool collidersEnabled = false; // To ensure colliders are enabled only once

    void Start()
    {
        animator = GetComponent<Animator>();
        birdAnimator = Bird.GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
    }

    void Update()
    {
        HandleIdleCompletion();
        HandleWalking();
        CheckAndEnableColliders();


    }

    private void HandleIdleCompletion()
    {
        if (!isIdleCompleted && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            isIdleCompleted = true;
            spriteRenderer.flipX = true;
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
            Vector3 targetPosition = new Vector3(stopPosition.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);

            if (Mathf.Abs(transform.position.x - stopPosition.position.x) <= 0.1f)
            {
                isWalking = false;
                spriteRenderer.flipX = false;
                animator.SetBool("canWalk", false);
                animator.SetBool("canTalk", true);
            }
        }
    }

    private void CheckAndEnableColliders()
    {
        if (!collidersEnabled && animator.GetCurrentAnimatorStateInfo(0).IsName("Walk") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            Debug.Log("enable Colliders");
            foreach (GameObject obj in objectsToEnable)
            {
                Collider2D collider = obj.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }
            collidersEnabled = true; // Prevents multiple enabling
        }
    }



}
