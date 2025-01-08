using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class birdActions : MonoBehaviour
{
    public Transform birdStopPosition;
    public Transform finalStopPosition;
    public float flyspeed = 2f;
    public GameObject pipe;
    public GameObject objectsToEnable;
    public float helperHandDelay = 5f;
    public TextMeshProUGUI subtitleText;
    public boy_Actions1 jojoController;
    private Animator animator;
    private TapControl tapControl;
    /*private SpriteRenderer spriteRenderer;*/
    private AudioSource kikiAudiosource;

    private bool isFlying = false;
    private bool isIdleCompleted = false;
    private bool collidersEnabled = false;
    private bool hasPlayedAudioClip2 = false;

    private Helper_PointerController helperPointerController;
    private AudioClip audioClip1;
    private AudioClip audioClip2;

    void Start()
    {
        animator = GetComponent<Animator>();
        tapControl = pipe.GetComponent<TapControl>();
        /*spriteRenderer = GetComponent<SpriteRenderer>();*/
        helperPointerController = FindObjectOfType<Helper_PointerController>();
        kikiAudiosource = GetComponent<AudioSource>();
        audioClip1 = Resources.Load<AudioClip>("audio/Lvl3sc2/Mom told us to wash our toys");
        audioClip2 = Resources.Load<AudioClip>("audio/Lvl3sc2/Now show your toys under the water");
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
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.3f)
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
                animator.SetBool("canFly", false);
                kikiAudiosource.clip = audioClip1;
                kikiAudiosource.Play();
                StartCoroutine(RevealTextWordByWord("Mom told us to wash our toys", 0.5f));

            }
        }
    }

    private void CheckAndEnableColliders()
    {
        if (!collidersEnabled && animator.GetCurrentAnimatorStateInfo(0).IsName("Bird Talk") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            Collider2D collider = objectsToEnable.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
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
            /*spriteRenderer.flipX = true;*/
            animator.SetBool("waterPlay", false);
            animator.SetBool("canFly", true);

            Vector3 newtargetPosition = new Vector3(finalStopPosition.position.x, finalStopPosition.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, newtargetPosition, flyspeed * Time.deltaTime);

            if (Mathf.Abs(finalStopPosition.position.y - transform.position.y) <= 0.1f)
            {
                /*spriteRenderer.flipX = false;*/
                isFlying = false;
                animator.SetBool("canFly", false);
                if (!hasPlayedAudioClip2 && audioClip2 != null && jojoController.jojoAudioPlayed)
                {
                    /*animator.SetTrigger("helper");*/
                    kikiAudiosource.clip = audioClip2;
                    kikiAudiosource.Play();
                    StartCoroutine(RevealTextWordByWord("Now show your toys under the water", 0.5f));
                    hasPlayedAudioClip2 = true; // Mark that the second audio has been played
                }
            }
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
        subtitleText.text = "";
    }
}
