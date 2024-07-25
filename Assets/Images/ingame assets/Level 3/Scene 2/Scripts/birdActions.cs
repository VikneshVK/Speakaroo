using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class birdActions : MonoBehaviour
{
    public Transform birdStopPosition;
    public Transform finalStopPosition;
    public float flyspeed = 2f;
    public GameObject pipe;
    public List<GameObject> objectsToEnable; // List to hold objects whose colliders will be enabled

    private Animator animator;
    private TapControl tapControl;
    private SpriteRenderer spriteRenderer;

    private bool isFlying = false;
    private bool isIdleCompleted = false;    
    private bool collidersEnabled = false; // To ensure colliders are enabled only once

    void Start()
    {
        animator = GetComponent<Animator>();
        tapControl = pipe.GetComponent<TapControl>();
        spriteRenderer = GetComponent<SpriteRenderer>();    
    }

    // Update is called once per frame
    void Update()
    {
        HandleIdleCompletion();
        WalkToStopPosition();
        CheckAndEnableColliders();
        HandleWaterPlay();
    }

    private void HandleIdleCompletion()
    {
        if (!isIdleCompleted && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            isIdleCompleted = true;
            isFlying = true;
            animator.SetBool("canFly", true);
        }
    }

    private void WalkToStopPosition()
    {
        if (birdStopPosition != null && isFlying)
        {
            Vector3 targetPosition = new Vector3(birdStopPosition.position.x, birdStopPosition.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, flyspeed * Time.deltaTime);

            if (Mathf.Abs(birdStopPosition.position.x - transform.position.x) <= 0.1f)
            {
                isFlying = false;                
                animator.SetBool("canFly", false);
            }
        }
    }

    private void CheckAndEnableColliders()
    {
        if (!collidersEnabled && animator.GetCurrentAnimatorStateInfo(0).IsName("Bird Talk") &&
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
            spriteRenderer.flipX = true;
            animator.SetBool("waterPlay", false);
            animator.SetBool("canFly", true);

            Vector3 newtargetPosition = new Vector3(finalStopPosition.position.x, finalStopPosition.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, newtargetPosition, flyspeed * Time.deltaTime);

            if (Mathf.Abs(finalStopPosition.position.y - transform.position.y) <= 0.1f)
            {
                spriteRenderer.flipX = false;
                isFlying = false;
                animator.SetBool("canFly", false);
            }
        }
    }
}
