using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boy_Actions1 : MonoBehaviour
{
    public Transform stopPosition;
    public float walkSpeed = 2f;
    public GameObject Bird;
    public GameObject pipe;
    public GameObject Hose;
    public GameObject walkRig;
    public GameObject normalRig;
    public AudioSource tubeAudiosource;

    private Animator walkAnimator;
    private Animator normalAnimator;
    private Animator birdAnimator;
    private TapControl tapControl;
    private AudioSource jojoAudiosource;
    private Collider2D hoseCollider;
    private Collider2D pipeCollider;
    private bool isWalking ;
    private bool isIdleCompleted;
    private bool canTalk;
    private bool audioplayed;

    // Start is called before the first frame update
    void Start()
    {
        normalAnimator = normalRig.GetComponent<Animator>();
        walkAnimator = walkRig.GetComponent<Animator>();
        birdAnimator = Bird.GetComponent<Animator>();
        tapControl = pipe.GetComponent<TapControl>();
        jojoAudiosource = GetComponent<AudioSource>();
        hoseCollider = Hose.GetComponent<Collider2D>();
        pipeCollider = pipe.GetComponent<Collider2D>();

        normalRig.SetActive(true);
        walkRig.SetActive(false);
        hoseCollider.enabled = false;
        isWalking = false;
        isIdleCompleted = false;
        canTalk = false;
        audioplayed = false;
    }

    // Update is called once per frame
    void Update()
    {
        HandleIdleCompletion();
        HandleWalking();
        HandleTalk();
        HandleWaterPlay();
    }

    private void HandleIdleCompletion()
    {
        if (!isIdleCompleted && normalAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            normalAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            normalRig.SetActive(false);
            walkRig.SetActive(true);
            isIdleCompleted = true;
            isWalking = true;
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
                normalRig.SetActive(true);
                walkRig.SetActive(false);
            }
        }
    }
    private void HandleTalk()
    {
        if(!canTalk && birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Bird Talk") &&
            normalAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            canTalk = true;
            normalAnimator.SetTrigger("canTalk");
            jojoAudiosource.Play();
            hoseCollider.enabled = true;
            /*pipeCollider.enabled = true;   */ 
        }
    }
    private void OnParticleCollision(GameObject other)
    {
        // Check if the colliding particle system is the water particles
        if (other.CompareTag("spray")) // Make sure the particle system object has the tag "WaterParticles"
        {
            normalAnimator.SetBool("waterPlay", true);
            if (!audioplayed)
            {
                tubeAudiosource.Play();
                audioplayed = true;
            }

        }

    }
    private void HandleWaterPlay()
    {
        if (tapControl != null && !tapControl.isFirstTime)
        {
            normalAnimator.SetBool("waterPlay", false);
        }
    }
}
