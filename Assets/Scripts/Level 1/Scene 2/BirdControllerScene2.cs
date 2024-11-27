using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdControllerScene2 : MonoBehaviour
{
    public float speed = 5.0f;
    private Animator animator;
    private bool isFlying = false;
    private Rigidbody2D rb;
    public Transform shampoo; // Reference to the shampoo
    public Transform shampooContainer; // Reference to the final position of the shampoo
    public Lvl1Sc2HelperFunction helperFunctionScript;
    private SpriteRenderer Sprite;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        Sprite = GetComponent<SpriteRenderer>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on the GameObject");
            return; // Stop further execution to avoid accessing null reference
        }

        rb.isKinematic = true; // Set Rigidbody2D to kinematic because we are moving the bird manually
        rb.gravityScale = 0; // Prevent Rigidbody2D from applying gravity

        if (shampoo == null || shampooContainer == null)
        {
            Debug.LogError("Shampoo or shampoo container not assigned in the inspector");
        }

    }

    void Update()
    {
        if (animator.GetBool("isFlying"))
        {
            if (!isFlying)
            {
                StartFlying();
            }
            rb.velocity = Vector2.left * speed;
        }
        
    }

    public void StartFlying()
    {
        isFlying = true;
        animator.SetBool("isFlying", true);
    }

    public void StopFlying()
    {
        isFlying = false;
        animator.SetBool("isFlying", false);
        rb.velocity = Vector2.zero; // Ensure the bird stops moving
        animator.SetBool("knock", true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Shampoo"))
        {
            StopFlying();  // Stop flying when colliding with a shampoo tagged object

            // Start the coroutine to trigger the shampoo animations
            StartCoroutine(TriggerShampooKnockAtMidpoint());
        }
    }

    private IEnumerator TriggerShampooKnockAtMidpoint()
    {
        // Play knockShampoo animation on bird
        animator.SetTrigger("knock");
        
        // Wait for half of the knockShampoo animation duration
        yield return new WaitForSeconds(GetCurrentAnimationLength() / 2);

        // Perform the tweens
        if (shampoo != null && shampooContainer != null)
        {
            // Animate the shampoo to move to the position of the shampooContainer
            LeanTween.move(shampoo.gameObject, shampooContainer.position, 1.0f)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() =>
                {
                    Sprite.flipX = false;

                    // On tween complete, reference the helper script and start the timer for shampoo
                    if (helperFunctionScript != null)
                    {
                        helperFunctionScript.StartTimer(false); // Start the helper timer for the shampoo
                    }
                    else
                    {
                        Debug.LogError("Helper function script not assigned in the inspector.");
                    }
                });

            // Rotate the shampoo to -30 degrees during the movement
            LeanTween.rotateZ(shampoo.gameObject, 30f, 1.0f);
        }
        else
        {
            Debug.LogError("Shampoo or shampoo container not assigned in the inspector.");
        }
    }


    private float GetCurrentAnimationLength()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.length / (stateInfo.speed != 0 ? stateInfo.speed : 1);
    }
}

