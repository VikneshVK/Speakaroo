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
    public Collider2D brushCollider; // Reference to the brush's collider

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on the GameObject");
            return; // Stop further execution to avoid accessing null reference
        }
        rb.isKinematic = true;
        rb.gravityScale = 0;

        if (brushHolder == null || brushContainer == null || brushCollider == null)
        {
            Debug.LogError("Brush holder, brush container or brush collider not assigned in the inspector");
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
        else
        {
            StopFlying();
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
        rb.velocity = Vector2.zero;
        animator.SetBool("knock", true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("BrushHolder"))
        {
            other.GetComponent<Collider2D>().enabled = false;
            StopFlying();
            StartCoroutine(TriggerBrushKnockAtMidpoint());
        }
    }

    private IEnumerator TriggerBrushKnockAtMidpoint()
    {
        /*animator.SetTrigger("knockBrush");*/
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float knockBrushDuration = stateInfo.length;
        yield return new WaitForSeconds(knockBrushDuration / 2);

        if (brushHolder != null && brushHolder.childCount > 0)
        {
            Transform brushTransform = brushHolder.GetChild(0);
            brushTransform.SetParent(null);

            if (brushContainer != null)
            {
                LeanTween.rotateZ(brushHolder.gameObject, 85f, 0.5f);
                var tween = LeanTween.move(brushTransform.gameObject, brushContainer.position, 1.0f).setEase(LeanTweenType.easeInOutQuad);
                LeanTween.rotateZ(brushTransform.gameObject, 0f, 1.0f);

                // Wait for the tween to complete before activating the collider
                yield return new WaitForSeconds(tween.time);
                brushCollider.enabled = true; // Activate the collider
            }
            else
            {
                Debug.LogError("Brush container not assigned in the inspector.");
            }
        }
    }
}
