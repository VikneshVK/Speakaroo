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
    public TextMeshProUGUI subtitleText;

    private Animator jojoAnimator;
    private Animator birdAnimator;
    private SpriteRenderer spriteRenderer;
    private bool isWalking = false;
    private bool isIdleCompleted = false;

    void Start()
    {
        jojoAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        birdAnimator = Bird.GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
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

            if (Mathf.Abs(transform.position.x - stopPosition.position.x) <= 0.1f)
            {
                isWalking = false;
                jojoAnimator.SetBool("CanWalk", false);
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
            StartCoroutine(RevealTextWordByWord("YAY..! We are at the Lab, Let's mix some Colors", 0.5f));
        }
    }
    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(true);

        string[] words = fullText.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            subtitleText.text = string.Join(" ", words, 0, i + 1);
            yield return new WaitForSeconds(delayBetweenWords);
        }
        subtitleText.text = "";
    }
}
