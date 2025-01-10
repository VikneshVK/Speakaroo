using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JojoController1 : MonoBehaviour
{
    public Transform stopPosition;
    public float walkSpeed = 2f;
    public SubtitleManager subtitleManager;
    public GameObject bird;
    public AudioClip lvlCompleteaudio;
    /*public GameObject walkRig;
    public GameObject talkRig;*/

    public bool birdcanTalk;
    private bool isWalking;
    private bool lvlCompletefollowups;
    private bool isIdleCompleted;
    private SpriteRenderer spriteRenderer;
    private Animator boyAnimator;
    private Animator birdAnimator;

    private AudioSource audioSource;
    public AudioSource kikiAudiosource;
    private Bird_Controller birdcontroller;

    [Header("SFX")]
    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;
    private bool walkingSfxPlayed = false;

    void Start()
    {
        isWalking = false;
        isIdleCompleted = false;
        birdcanTalk = false;
        lvlCompletefollowups = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        boyAnimator = GetComponent<Animator>();
        birdAnimator = bird.GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        birdcontroller = bird.GetComponent<Bird_Controller>();
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
    }
    void Update()
    {
        HandleIdleCompletion();
        HandleWalking();
        HandleTalkCompletion();
        EndTalk();
    }

    private void HandleIdleCompletion()
    {
        if (!isIdleCompleted && boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.1f)
        {
            isIdleCompleted = true;
            isWalking = true;
            boyAnimator.SetBool("canWalk", true);
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
            if (!walkingSfxPlayed)
            {
                SfxAudioSource.loop = true;
                walkingSfxPlayed = true;
                SfxAudioSource.clip = SfxAudio1;
                SfxAudioSource.Play();
            }
            if (Mathf.Abs(transform.position.x - stopPosition.position.x) <= 0.1f)
            {
                SfxAudioSource.Stop();
                isWalking = false;
                boyAnimator.SetBool("canWalk", false);
                
                boyAnimator.SetBool("canTalk", true);
                audioSource.Play();
                subtitleManager.DisplaySubtitle("Mom asked if we can Sweep the dry leaves in the Garden", "JoJo", audioSource.clip);
            }

        }
    }
    private void HandleTalkCompletion()
    {
        if (boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dialouge 1") &&
            boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            boyAnimator.SetBool("canTalk", false);
            
            birdcanTalk = true;
            kikiAudiosource.Play();
            birdAnimator.SetBool("canTalk", true);
            subtitleManager.DisplaySubtitle("Yes, Let's sweep the Garden, my friend, and I will help you, Jojo", "Kiki", kikiAudiosource.clip);
        }
    }

    private void EndTalk()
    {
        if (birdcontroller.LevelComplete && !lvlCompletefollowups)
        {
            lvlCompletefollowups = true;
            boyAnimator.SetTrigger("LevelComplete");
            audioSource.Stop(); // Stop any ongoing audio
            audioSource.clip = lvlCompleteaudio; // Set level complete audio clip
            audioSource.Play();
            subtitleManager.DisplaySubtitle("Good job, the garden looks clean now", "Jojo", audioSource.clip);
        }

    }
    
}
