using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JojoController1 : MonoBehaviour
{
    public Transform stopPosition;
    public float walkSpeed = 2f;

    public GameObject bird;
    public GameObject walkRig;
    public GameObject talkRig;

    public bool birdcanTalk;
    private bool isWalking;
    private bool isIdleCompleted;

    private Animator walkRigAnimator;
    private Animator talkRigAnimator;
    private Animator birdAnimator;
    
    private AudioSource audioSource;
    public AudioSource kikiAudiosource;

    void Start()
    {
        isWalking = false;
        isIdleCompleted = false;
        birdcanTalk = false;

    walkRigAnimator = walkRig.GetComponent<Animator>();
        talkRigAnimator = talkRig.GetComponent<Animator>();
        birdAnimator = bird.GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        talkRig.SetActive(true);
        walkRig.SetActive(false);

    }
    void Update()
    {
        HandleIdleCompletion();
        HandleWalking();
        HandleTalkCompletion();
    }

    private void HandleIdleCompletion()
    {
        if (!isIdleCompleted && talkRigAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            talkRigAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            isIdleCompleted = true;
            isWalking = true;
            talkRig.SetActive(false);
            walkRig.SetActive(true);
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
                talkRig.SetActive(true);
                walkRig.SetActive(false);
                isWalking = false;
                talkRigAnimator.SetTrigger("canTalk");
                audioSource.Play();
            }

        }
    }
    private void HandleTalkCompletion()
    {
        if (talkRigAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dialouge 1") &&
            talkRigAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {

            birdcanTalk = true;
            kikiAudiosource.Play();
            birdAnimator.SetBool("canTalk", true);
        }
    }
}
