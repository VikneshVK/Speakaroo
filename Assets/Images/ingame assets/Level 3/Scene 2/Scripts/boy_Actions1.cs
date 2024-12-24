using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class boy_Actions1 : MonoBehaviour
{
    public Transform stopPosition;
    public float walkSpeed = 2f;
    public GameObject Bird;
    public GameObject pipe;
    public GameObject Hose;    
    public AudioSource tubeAudiosource;
    public AudioSource sunAudioSource;
    public TextMeshProUGUI subtitleText;
    public bool jojoAudioPlayed;

    private SpriteRenderer boysprite;
    private Animator boyAnimator;    
    private Animator birdAnimator;
    private TapControl tapControl;
    private AudioSource jojoAudiosource;
    private Collider2D hoseCollider;
    private Collider2D pipeCollider;
    private bool isWalking ;
    private bool isIdleCompleted;
    private bool canTalk;
    private bool audioplayed;
    private bool WaterAnimationPlayed;
    private bool isSubtitleDisplayed;

    // Start is called before the first frame update
    void Start()
    {
        boyAnimator = GetComponent<Animator>();       
        birdAnimator = Bird.GetComponent<Animator>();
        tapControl = pipe.GetComponent<TapControl>();
        jojoAudiosource = GetComponent<AudioSource>();
        hoseCollider = Hose.GetComponent<Collider2D>();
        pipeCollider = pipe.GetComponent<Collider2D>();
        boysprite = GetComponent<SpriteRenderer>();


        hoseCollider.enabled = false;
        isWalking = false;
        isIdleCompleted = false;
        canTalk = false;
        audioplayed = false;
        isSubtitleDisplayed = false;
        jojoAudioPlayed = false;
        WaterAnimationPlayed = false;
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

            if (Mathf.Abs(transform.position.x - stopPosition.position.x) <= 0.1f)
            {
                isWalking = false;
                boyAnimator.SetBool("canWalk", false);
            }
        }
    }
    private void HandleTalk()
    {
        if(!canTalk && birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Bird Talk") &&
            birdAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            canTalk = true;
            boyAnimator.SetTrigger("CanTalk");
            jojoAudiosource.Play();
            StartCoroutine(RevealTextWordByWord("Look, it's a water tap come on. Let's play", 0.5f));
            hoseCollider.enabled = true;
            /*pipeCollider.enabled = true;   */
        }
    }
    private void OnParticleCollision(GameObject other)
    {
        // Check if the colliding particle system is the water particles
        if (other.CompareTag("spray")) // Make sure the particle system object has the tag "WaterParticles"
        {
            
            boyAnimator.SetBool("waterPlay", true);           
            
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
            
            boyAnimator.SetBool("waterPlay", false);
            if (!WaterAnimationPlayed) 
            {
                WaterAnimationPlayed = true;
                boyAnimator.SetTrigger("Play done");
                sunAudioSource.Play();
                StartCoroutine(audioPlayed());
                StartCoroutine(RevealTextWordByWord("HaHa..! That was so Fun..!", 0.5f));
            }
            
        }
    }

    private IEnumerator audioPlayed()
    {
        yield return new WaitForSeconds(2f);
        jojoAudioPlayed = true;
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
