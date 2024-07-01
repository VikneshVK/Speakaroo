using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdController : MonoBehaviour
{
    public float speed = 5.0f;
    private Animator animator;
    private bool isFlying = false;
    private Rigidbody2D rb;
    public Transform brushHolder; // Reference to the brush holder
    public Transform brushContainer; // Reference to the final position of the brush

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

        if (brushHolder == null || brushContainer == null)
        {
            Debug.LogError("Brush holder or brush container not assigned in the inspector");
        }
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
        animator.SetBool("knockBrush", true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("brush"))
        {
            StopFlying();  // Stop flying when colliding with a brush tagged object

            // Start the coroutine to trigger the brush animations
            StartCoroutine(TriggerBrushKnockAtMidpoint());
        }
    }

    private IEnumerator TriggerBrushKnockAtMidpoint()
    {
        // Play knockBrush animation on bird
        animator.SetTrigger("knockBrush");

        // Get the duration of the knockBrush animation
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float knockBrushDuration = stateInfo.length;

        // Wait for half of the knockBrush animation duration
        yield return new WaitForSeconds(knockBrushDuration / 2);

        // Perform the tweens
        if (brushHolder != null && brushContainer != null)
        {
            // Rotate the brushHolder to 90 degrees on the z-axis
            LeanTween.rotateZ(brushHolder.gameObject, 85f, 0.5f);

            // Get the brush transform (assuming it's the first child of brushHolder)
            Transform brushTransform = brushHolder.GetChild(0);

            if (brushTransform != null)
            {
                // Animate the brush to fall to the position of the brushContainer
                LeanTween.move(brushTransform.gameObject, brushContainer.position, 1.0f).setEase(LeanTweenType.easeInOutQuad);

                // Rotate the brush during the fall with a final rotation value of 90 degrees on the z-axis
                LeanTween.rotateAround(brushTransform.gameObject, Vector3.forward, 360f, 1.0f).setOnComplete(() =>
                {
                    brushTransform.rotation = Quaternion.Euler(0, 0, 90);
                });
            }
            else
            {
                Debug.LogError("Brush transform not found as child of brushHolder");
            }
        }
        else
        {
            Debug.LogError("Brush holder or brush container not assigned in the inspector");
        }
    }
}
