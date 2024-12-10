using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class BubbleBurst : MonoBehaviour
{
    public delegate void BubbleDestroyed(BubbleBurst bubble);
    public event BubbleDestroyed OnDestroyEvent;
    public TextMeshProUGUI subtitleText1;
    public TextMeshProUGUI subtitleText2;
    public float floatDistance = 0.5f; // Maximum distance the bubble can float in any direction
    public float floatDuration = 2f; // Time it takes for one float cycle
    public Animator bubbleAnimator; // Reference to the Animator component
    public GameObject boy;
    public AudioSource boyAudioSource; // Reference to the boy's AudioSource (assign via Inspector)
    private AudioSource bubbleAudioSource; // The AudioSource on the same GameObject
    private AudioClip[] popClips; // Array to store pop1 and pop2 clips

    private bool isPopped = false;
    private Vector3 originalLocalPosition; // Store the original local position of the bubble
    private static List<BubbleBurst> allBubbles = new List<BubbleBurst>(); // List to keep track of all bubbles

    private void Awake()
    {
        // Add this bubble to the list of all bubbles
        allBubbles.Add(this);
    }

    private void OnDestroy()
    {
        // Remove this bubble from the list when destroyed
        allBubbles.Remove(this);
    }

    private void Start()
    {
        // Store the initial local position to maintain relative positioning
        originalLocalPosition = transform.localPosition;

        // Load pop1 and pop2 audio clips from Resources
        popClips = new AudioClip[]
        {
            Resources.Load<AudioClip>("Audio/Lvl6Sc1/pop1"),
            Resources.Load<AudioClip>("Audio/Lvl6Sc1/pop2")
        };

        // Get the AudioSource component on this GameObject
        bubbleAudioSource = GetComponent<AudioSource>();

        // Start floating effect
        StartFloating();
    }

    private void StartFloating()
    {
        // Make the bubble float in a random direction
        Vector3 localPosition = transform.localPosition;
        FloatInRandomDirection(localPosition);
    }

    private void FloatInRandomDirection(Vector3 originalPosition)
    {
        // Calculate a random target position within a circle defined by floatDistance
        Vector3 randomTarget = originalPosition + new Vector3(
            Random.Range(-floatDistance, floatDistance),
            Random.Range(-floatDistance, floatDistance),
            0);

        // Move the bubble to the random target position and back to original
        LeanTween.moveLocal(gameObject, randomTarget, floatDuration / 2)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnComplete(() =>
            {
                // Once the bubble reaches the target, move it back to a new random direction
                FloatInRandomDirection(originalPosition);
            });
    }

    private void OnMouseDown()
    {
        if (!isPopped)
        {
            isPopped = true;

            // Stop the floating effect
            LeanTween.cancel(gameObject);

            // Disable interactivity on other bubbles
            DisableOtherBubbles();

            // Play bubble pop sound (shuffle between pop1 and pop2)
            if (bubbleAudioSource != null && popClips.Length > 0)
            {
                AudioClip selectedClip = popClips[Random.Range(0, popClips.Length)];
                bubbleAudioSource.PlayOneShot(selectedClip);
            }

            // Play the boy's audio clip
            if (boyAudioSource != null)
            {
                boyAudioSource.Play();
                StartCoroutine(RevealTextWordByWord("Pop..!", 0.25f));
            }

            // Trigger the "Pop" animation
            bubbleAnimator.SetTrigger("Pop");
            boy.GetComponent<Animator>().SetTrigger("Giggle");
        }
    }

    // This method should be called by an animation event when the pop animation finishes
    public void OnPopAnimationComplete()
    {
        // Notify the minigame that this bubble is destroyed
        OnDestroyEvent?.Invoke(this);

        // Re-enable interactivity on other bubbles
        EnableOtherBubbles();

        // Destroy the bubble
        Destroy(gameObject);
    }

    // Disable interactivity on all other bubbles by disabling their colliders
    private void DisableOtherBubbles()
    {
        foreach (var bubble in allBubbles)
        {
            if (bubble != this)
            {
                Collider2D collider = bubble.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }
    }

    // Enable interactivity on all other bubbles by enabling their colliders
    private void EnableOtherBubbles()
    {
        foreach (var bubble in allBubbles)
        {
            if (bubble != this)
            {
                Collider2D collider = bubble.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }
        }
    }

    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText1.text = "";
        subtitleText2.text = "";
        subtitleText1.gameObject.SetActive(true);
        subtitleText2.gameObject.SetActive(true);

        string[] words = fullText.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            subtitleText1.text = string.Join(" ", words, 0, i + 1);
            subtitleText2.text = string.Join(" ", words, 0, i + 1);
            yield return new WaitForSeconds(delayBetweenWords);
        }
        subtitleText1.text = "";
        subtitleText2.text = "";
    }
}
