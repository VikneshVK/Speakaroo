using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdController : MonoBehaviour
{
    public float speed = 5.0f;
    private Animator animator;
    private bool isFlying = false;
    private Rigidbody2D rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on the GameObject");
            return; // Stop further execution to avoid accessing null reference
        }
        rb.isKinematic = true; // Set Rigidbody2D to kinematic because we are moving the bird manually
        rb.gravityScale = 0; // Prevent Rigidbody2D from applying gravity
    }

    void Update()
    {
        if (animator.GetBool("IsFlying"))
        {
            if (!isFlying)
            {
                StartFlying();
            }
            rb.velocity = Vector2.left * speed;
        }
        else
        {
            StopFlying();
        }
    }

    public void StartFlying()
    {
        isFlying = true;
        animator.SetBool("IsFlying", true);
    }

    public void StopFlying()
    {
        isFlying = false;
        animator.SetBool("IsFlying", false);
        rb.velocity = Vector2.zero; // Ensure the bird stops moving
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("brush"))
        {
            StopFlying();  // Stop flying when colliding with a brush tagged object

            // Get the Animator from the "brush" GameObject
            Animator brushAnimator = other.GetComponent<Animator>();
            Rigidbody2D rb2 = other.GetComponent<Rigidbody2D>();

            if (brushAnimator != null)
            {
                brushAnimator.SetTrigger("brushKnock"); // Trigger the animation
            }
            else
            {
                Debug.LogError("Animator not found on the brush GameObject");
            }
        }
    }
}
