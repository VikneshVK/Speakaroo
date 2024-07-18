using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boyController : MonoBehaviour
{
    public Transform stopPosition;
    public float walkSpeed = 2f;

    public GameObject pillowBigRight;
    public GameObject pillowBigLeft;
    public GameObject pillowSmallLeft;
    public GameObject pillowSmallRight;
    public GameObject bedsheet;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isWalking = false;
    private bool hasReachedStopPosition = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (stopPosition == null)
        {
            Debug.LogError("Stop position not set for BoyController.");
        }

        // Initially disable colliders for the interactive objects
        DisableColliders();
    }

    void Update()
    {
        if (!isWalking && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            animator.SetBool("canWalk", true);
            isWalking = true;
        }

        if (isWalking && !hasReachedStopPosition)
        {
            MoveToStopPosition();
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Talk") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.96f)
        {
            EnableColliders();
        }
    }

    private void MoveToStopPosition()
    {
        // Flip the sprite to face right
        spriteRenderer.flipX = true;

        Vector3 targetPosition = new Vector3(stopPosition.position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x - stopPosition.position.x) < 0.1f)
        {
            hasReachedStopPosition = true;
            isWalking = false;
            animator.SetBool("canWalk", false);
            animator.SetBool("canTalk", true);
            spriteRenderer.flipX = false;
        }
    }

    private void DisableColliders()
    {
        if (pillowBigRight != null) pillowBigRight.GetComponent<Collider2D>().enabled = false;
        if (pillowBigLeft != null) pillowBigLeft.GetComponent<Collider2D>().enabled = false;
        if (pillowSmallLeft != null) pillowSmallLeft.GetComponent<Collider2D>().enabled = false;
        if (pillowSmallRight != null) pillowSmallRight.GetComponent<Collider2D>().enabled = false;
        
    }

    private void EnableColliders()
    {
        if (pillowBigRight != null) pillowBigRight.GetComponent<Collider2D>().enabled = true;
        if (pillowBigLeft != null) pillowBigLeft.GetComponent<Collider2D>().enabled = true;
        Debug.Log("Colliders enabled for interactive objects.");
    }
}

