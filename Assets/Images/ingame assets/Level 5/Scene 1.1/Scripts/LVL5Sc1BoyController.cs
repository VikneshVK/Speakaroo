using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LVL5Sc1BoyController : MonoBehaviour
{

    public GameObject Kiki;
    public GameObject walkRig;
    public GameObject talkRig;
    public Transform stopPosition;
    public Transform stopPosition2;
    public float walkSpeed = 2f;

    private bool isWalking;
    private bool hasReachedStopPosition;
    private bool hasReachedStopPosition2;
    private bool talkComplete;

    private Animator walkAnimator;
    private Animator talkAnimator;
    private Animator kikiAnimator;

    void Start()
    {
        isWalking = false;
        hasReachedStopPosition = false;
        hasReachedStopPosition2 = false;
        talkComplete = false;

        walkAnimator = walkRig.GetComponent<Animator>();
        talkAnimator = talkRig.GetComponent<Animator>();
        kikiAnimator = Kiki.GetComponent<Animator>();

        talkRig.SetActive(true);
        walkRig.SetActive(false);
    }


    void Update()
    {
        if (!isWalking && talkAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            talkAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f && !hasReachedStopPosition)
        {
            talkRig.SetActive(false);
            walkRig.SetActive(true);
            isWalking = true;
        }

        if (isWalking && !hasReachedStopPosition)
        {
            MoveToStopPosition();
        }

        if (!isWalking && kikiAnimator.GetCurrentAnimatorStateInfo(0).IsName("BirdTalk") &&
            kikiAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f && !hasReachedStopPosition2)
        {
            talkRig.SetActive(false);
            walkRig.SetActive(true);
            isWalking = true;
            walkAnimator.SetTrigger("canWalk");
        }

        if (isWalking && !hasReachedStopPosition2)
        {
            MoveToStopPosition2();
        }
    }

    private void MoveToStopPosition()
    {
        Vector3 targetPosition = new Vector3(stopPosition.position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x - stopPosition.position.x) < 0.1f)
        {
            Debug.Log("positionReached");
            hasReachedStopPosition = true;
            isWalking = false;

            talkRig.SetActive(true);
            walkRig.SetActive(false);
            talkAnimator.SetTrigger("CanTalk");
        }
    }

    private void MoveToStopPosition2()
    {
        Vector3 targetPosition = new Vector3(stopPosition2.position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);
        
    }
}
