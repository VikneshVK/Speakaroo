using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JojoController1 : MonoBehaviour
{
    public Transform stopPosition;
    public float walkSpeed = 2f;
    public TextMeshProUGUI subtitleText;
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
            boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
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

            if (Mathf.Abs(transform.position.x - stopPosition.position.x) <= 0.1f)
            {

                isWalking = false;
                boyAnimator.SetBool("canWalk", false);
                spriteRenderer.flipX = true;
                boyAnimator.SetBool("canTalk", true);
                audioSource.Play();
                StartCoroutine(RevealTextWordByWord("Mom asked if we can help clean our garden", 0.3f));
            }

        }
    }
    private void HandleTalkCompletion()
    {
        if (boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dialouge 1") &&
            boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            boyAnimator.SetBool("canTalk", false);
            spriteRenderer.flipX = false;
            birdcanTalk = true;
            kikiAudiosource.Play();
            birdAnimator.SetBool("canTalk", true);
            StartCoroutine(RevealTextWordByWord("Yes, Let's sweep the Garden, my friend, and I will help you, Jojo", 0.3f));
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
            StartCoroutine(RevealTextWordByWord("Good job, the garden looks clean now", 0.5f));
        }


    }

    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";  
        subtitleText.gameObject.SetActive(true);  

        string[] words = fullText.Split(' ');  

        // Reveal words one by one
        for (int i = 0; i < words.Length; i++)
        {
            subtitleText.text = string.Join(" ", words, 0, i + 1);  
            yield return new WaitForSeconds(delayBetweenWords);  
        }
        subtitleText.gameObject.SetActive(false);
    }
}
