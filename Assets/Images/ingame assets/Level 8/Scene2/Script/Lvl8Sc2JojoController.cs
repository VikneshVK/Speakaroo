using System.Collections;
using TMPro;
using UnityEngine;

public class Lvl8Sc2JojoController : MonoBehaviour
{
    public Transform stopPosition;
    public float walkSpeed = 2f;
    public GameObject Bird;
    public AudioClip dialogueAudio;
    public AudioSource audioSource;
    public SubtitleManager subtitleManager;

    [Header("SFX")]
    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;

    private Animator jojoAnimator;
    private Animator birdAnimator;
    private SpriteRenderer spriteRenderer;
    private bool isWalking = false;
    private bool isIdleCompleted = false;
    private bool walkingaudioPlayed;

    void Start()
    {
        jojoAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        birdAnimator = Bird.GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
        walkingaudioPlayed = false;
    }

    void Update()
    {
        HandleIdleCompletion();
        HandleWalking();
    }

    private void HandleIdleCompletion()
    {
        if (!isIdleCompleted && jojoAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            jojoAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.1f)
        {
            isIdleCompleted = true;
            isWalking = true;
            jojoAnimator.SetBool("CanWalk", true);
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
            spriteRenderer.flipX = true; // Adjust direction if needed
            Vector3 targetPosition = new Vector3(stopPosition.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);
            if (SfxAudioSource != null && !walkingaudioPlayed) 
            {
                walkingaudioPlayed = true;
                SfxAudioSource.loop = true;
                SfxAudioSource.clip = SfxAudio1;
                SfxAudioSource.Play();
                Debug.Log("Audio is playing");
            }
            
            if (Mathf.Abs(transform.position.x - stopPosition.position.x) <= 0.1f)
            {
                isWalking = false;                
                jojoAnimator.SetBool("CanWalk", false);
                SfxAudioSource.Stop();
                StartCoroutine(flipCharacter());
            }
        }
    }

    private IEnumerator flipCharacter()
    {
        yield return new WaitForSeconds(0.5f);
        spriteRenderer.flipX = false; // Reset direction

        // Trigger the "Dialouge1" animation
        jojoAnimator.SetTrigger("Dialouge1");

        // Wait for the "Dialouge1" animation to start
        while (!jojoAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dialouge1"))
        {
            yield return null;
        }

        // Play the dialogue audio
        if (dialogueAudio != null && audioSource != null)
        {
            audioSource.clip = dialogueAudio;
            audioSource.Play();
            subtitleManager.DisplaySubtitle("YAY..! We are at the Lab, Let's mix some Colors", "Kiki", dialogueAudio);
        }
    }
}
