using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Lvl8Sc2KikiController : MonoBehaviour
{
    public Transform targetPosition;
    public float flySpeed = 3f;
    public Animator jojoAnimator; // Reference to Jojo's Animator
    public AudioClip kikiDialogueAudio; // Audio for Kiki Dialogue
    public AudioClip beakerAudio; // Audio for Beaker
    public AudioSource audioSource;
    public GameObject beaker; // Beaker GameObject
    public SubtitleManager subtitleManager;
    public Lvl8SSc2HelperController helperController;

    private Animator kikiAnimator;
    private SpriteRenderer spriteRenderer;
    private bool isFlying = false;
    private bool isIdleCompleted = false;
    

    void Start()
    {
        kikiAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();        
    }

    void Update()
    {
        HandleIdleCompletion();
        HandleFlying();
    }

    private void HandleIdleCompletion()
    {
        if (!isIdleCompleted && kikiAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            kikiAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.1f)
        {
            isIdleCompleted = true;
            isFlying = true;
            kikiAnimator.SetBool("CanFly", true);
        }
    }

    private void HandleFlying()
    {
        if (isFlying)
        {
            FlyToTargetPosition();
        }
    }

    private void FlyToTargetPosition()
    {
        if (targetPosition != null)
        {
            spriteRenderer.flipX = false; // Adjust direction if needed
            Vector3 destination = new Vector3(targetPosition.position.x, targetPosition.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, destination, flySpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition.position) <= 0.1f)
            {
                isFlying = false;
                kikiAnimator.SetBool("CanFly", false); // Transition back to Idle

                StartCoroutine(WatchJojoDialogue());
            }
        }
    }

    private IEnumerator WatchJojoDialogue()
    {
        while (!jojoAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dialouge1"))
        {
            yield return null;
        }

        while (jojoAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dialouge1") &&
               jojoAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        kikiAnimator.SetTrigger("Beaker");

        StartCoroutine(WatchBeakerAnimation());
    }

   

    private IEnumerator WatchBeakerAnimation()
    {
        while (!kikiAnimator.GetCurrentAnimatorStateInfo(0).IsName("Beaker"))
        {
            yield return null;
        }

        if (beakerAudio != null && audioSource != null)
        {
            audioSource.clip = beakerAudio;
            audioSource.Play();
            subtitleManager.DisplaySubtitle("Please place the Beaker on the Stand", "Kiki", beakerAudio);
        }

        while (kikiAnimator.GetCurrentAnimatorStateInfo(0).IsName("Beaker") &&
               kikiAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        if (beaker != null)
        {
            helperController.EnableTestTubeCollider(beaker);
            helperController.SpawnGlow(beaker);
        }
    }
}
