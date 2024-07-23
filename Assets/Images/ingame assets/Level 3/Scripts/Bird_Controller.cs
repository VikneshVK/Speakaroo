using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird_Controller : MonoBehaviour
{
    public Transform birdStopPosition;
    public float flyspeed = 2f;
    public GameObject Boy;   
    public List<GameObject> objectsToEnable; // List to hold objects whose colliders will be enabled

    private Animator animator;
    private Animator boyAnimator;
    private bool isFlying = false;
    private bool isIdleCompleted = false;
    private bool isFlyingCompleted = false;
    private bool collidersEnabled = false; // To ensure colliders are enabled only once

    void Start()
    {
        animator = GetComponent<Animator>();
        boyAnimator = Boy.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleIdleCompletion();
        WalkToStopPosition();
        CheckAndEnableColliders();
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
            Vector3 targetPosition = new Vector3(birdStopPosition.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, flyspeed * Time.deltaTime);

            if (Mathf.Abs(birdStopPosition.position.x - transform.position.x) <= 0.1f)
            {
                isFlying = false;
                isFlyingCompleted = true;
                animator.SetBool("canFly", false);
            }
        }
    }

    private void CheckAndEnableColliders()
    {
        if (!collidersEnabled && animator.GetCurrentAnimatorStateInfo(0).IsName("Talk") &&
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
