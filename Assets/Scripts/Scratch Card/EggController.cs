using UnityEngine;
using System.Collections;

public class EggController : MonoBehaviour
{
    private Animator animator;
    private Collider2D boxCollider;
    public int tapCount = 0;
    private Vector3 originalWordScale;
    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;

    public GameObject word; // Reference to the word GameObject

    void Start()
    {
        // Get references to the Animator and Collider components
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<Collider2D>();
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
        // Store the original scale of the word GameObject and set it to 0
        if (word != null)
        {
            originalWordScale = word.transform.localScale;
            word.transform.localScale = Vector3.zero;
        }
    }

    void OnMouseDown()
    {
        // Increment tap count and handle animations based on the current count
        tapCount++;
        HandleTap();
    }

    void HandleTap()
    {
        // Disable the collider while the animation is playing
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        // Trigger the appropriate animation based on the tap count
        switch (tapCount)
        {
            case 1:
                animator.SetTrigger("isTapped1");
                SfxAudioSource.PlayOneShot(SfxAudio1);
                StartCoroutine(WaitForAnimation("egg"));
                break;
            case 2:
                animator.SetTrigger("isTapped2");
                SfxAudioSource.PlayOneShot(SfxAudio1);
                StartCoroutine(WaitForAnimation("egg2"));
                break;
            case 3:
                animator.SetTrigger("isTapped3");
                SfxAudioSource.PlayOneShot(SfxAudio1);
                StartCoroutine(WaitForAnimation("egg3", true));
                break;
            default:
                return; // If tap count exceeds 3, do nothing further
        }
    }

    IEnumerator WaitForAnimation(string animationName, bool destroyAfter = false)
    {
        // Wait until the animation is in the specified state
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            yield return null;
        }

        // Wait until the animation has finished
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        if (destroyAfter)
        {
            if (word != null)
            {
                LeanTween.scale(word, originalWordScale, 0.5f).setEase(LeanTweenType.easeOutBack)
                    .setOnComplete(() =>
                    {
                        ST_AudioManager.Instance.PlayAudioAfterDestroy(transform.parent.tag);
                        Destroy(gameObject);
                    });
            }
            else
            {
                ST_AudioManager.Instance.PlayAudioAfterDestroy(transform.parent.tag);
                Destroy(gameObject);
            }
        }
        else
        {
            // Re-enable the collider after the animation is complete
            if (boxCollider != null)
            {
                boxCollider.enabled = true;
            }
        }
    }
}
