using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoyActions : MonoBehaviour
{
    public Transform stopPosition;
    public float walkSpeed = 2f;
    public GameObject Bird;
    public GameObject pipe;

    private Animator animator;
    private Animator birdAnimator;
    private TapControl tapControl;
    private bool isWalking = false;
    private bool isIdleCompleted = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        birdAnimator = Bird.GetComponent<Animator>();
        tapControl = pipe.GetComponent<TapControl>();
    }

    void Update()
    {
        HandleIdleCompletion();
        HandleWalking();
        HandleWaterPlay();
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
            Vector3 targetPosition = new Vector3(stopPosition.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);

            if (Mathf.Abs(transform.position.x - stopPosition.position.x) <= 0.1f)
            {
                isWalking = false;
                animator.SetBool("canWalk", false);
            }
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        // Check if the colliding particle system is the water particles
        if (other.CompareTag("spray")) // Make sure the particle system object has the tag "WaterParticles"
        {
            animator.SetBool("waterPlay", true);
        }        
    }
    private void HandleWaterPlay()
    {
        if (tapControl != null && !tapControl.isFirstTime)
        {
            animator.SetBool("waterPlay", false);
        }
    }
}
