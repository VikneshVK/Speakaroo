using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class birdActions : MonoBehaviour
{
    public Transform birdStopPosition;
    public Transform finalStopPosition;
    public float flyspeed = 2f;
    public GameObject pipe;
    public List<GameObject> objectsToEnable;
    public float helperHandDelay = 5f;

    private Animator animator;
    private TapControl tapControl;
    private SpriteRenderer spriteRenderer;

    private bool isFlying = false;
    private bool isIdleCompleted = false;
    private bool collidersEnabled = false;

    private Helper_PointerController helperPointerController;

    void Start()
    {
        animator = GetComponent<Animator>();
        tapControl = pipe.GetComponent<TapControl>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        helperPointerController = FindObjectOfType<Helper_PointerController>();
    }

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
            foreach (GameObject obj in objectsToEnable)
            {
                Collider2D collider = obj.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }

            collidersEnabled = true;

            // Call the EnableCollidersAndStartTimer method from the Helper_PointerController
            if (helperPointerController != null)
            {
                helperPointerController.EnableCollidersAndStartTimer();
            }
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("spray"))
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
