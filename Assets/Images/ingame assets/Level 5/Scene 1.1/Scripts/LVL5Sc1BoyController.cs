using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LVL5Sc1BoyController : MonoBehaviour
{

    public GameObject Kiki;
    public Transform stopPosition;
    public Transform stopPosition2;
    public float walkSpeed = 2f;
    public SubtitleManager subtitleManager;
    private bool isWalking;
    private bool hasReachedStopPosition;
    private bool hasReachedStopPosition2;
    private bool talkComplete;
    private bool ticketBooth;
    private AudioSource Audio1;

    [Header("SFX")]
    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;
    private bool walkingSfxPlayed = false;

    private Animator boyAnimator;
    private Animator kikiAnimator;

    void Start()
    {
        isWalking = false;
        hasReachedStopPosition = false;
        hasReachedStopPosition2 = false;
        talkComplete = false;
        ticketBooth = false;
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
        boyAnimator = GetComponent<Animator>();
        kikiAnimator = Kiki.GetComponent<Animator>();
        Audio1 = GetComponent<AudioSource>();
    }


    void Update()
    {
        if (!isWalking && boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f && !hasReachedStopPosition)
        {
            boyAnimator.SetBool("canWalk", true);
            isWalking = true;
        }

        if (isWalking && !hasReachedStopPosition)
        {
            if (!walkingSfxPlayed)
            {
                SfxAudioSource.loop = true;
                walkingSfxPlayed = true;
                SfxAudioSource.clip = SfxAudio1;
                SfxAudioSource.Play();
            }
            MoveToStopPosition();
        }

        if (!isWalking && boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("lets go get tickets") &&
            boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f && !hasReachedStopPosition2)
        {

            isWalking = true;
            boyAnimator.SetBool("canWalk2", true);
            ticketBooth = true;
        }

        if (isWalking && !hasReachedStopPosition2 && ticketBooth)
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
            SfxAudioSource.Stop();
            Debug.Log("positionReached");
            boyAnimator.SetBool("canWalk", false);
            hasReachedStopPosition = true;
            isWalking = false;
            walkingSfxPlayed = false;
            boyAnimator.SetTrigger("canTalk");
            Audio1.Play();
            subtitleManager.DisplaySubtitle("YAY, We are at the Zoo. Let's go get the entry tickets", "Kiki", Audio1.clip);
        }
    }

    private void MoveToStopPosition2()
    {
        Vector3 targetPosition = new Vector3(stopPosition2.position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);
        /*LeanTween.scale(gameObject, new Vector3(0.9f, 0.9f, 0.9f), 2.5f);*/

        if (Mathf.Abs(transform.position.x - targetPosition.x) < 0.1f)
        {
            Debug.Log("positionReached");
            boyAnimator.SetBool("canWalk2", false);

        }
    }
}
