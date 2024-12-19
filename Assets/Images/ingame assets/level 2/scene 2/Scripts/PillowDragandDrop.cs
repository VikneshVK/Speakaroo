using UnityEngine;
using System.Collections;
using TMPro;
using Unity.VisualScripting;

public class PillowDragAndDrop : MonoBehaviour
{
    public Transform targetPosition;
    public Collider2D nextCollider;
    public GameObject dust;
    public GameObject bedsheet;
    public float offsetValue = 2f;
    public GameObject nextaudiosoucre; // Reference to the HelperHandController
    public GameObject Boy;
    public GameObject Kiki;
    public TextMeshProUGUI subtitleText;
    public static bool canDrag;
    public bool HasInteracted { get; private set; } = false;
    private AudioSource SfxAudioSource;
    
    public AudioClip sfxAudio2;
    public AudioClip sfxAudio3;
    public HelperHandController helperHandController;
    private bool isDragging = false;
    private Animator boyAnimator;
    private Animator kikiAnimator;
    private Vector3 startPosition;
    private Vector3 offset;
    private int originalSortingOrder;
    private SpriteRenderer spriteRenderer;

    public static int droppedPillowsCount;

    private AudioSource feedbackAudioSource;
    private AudioClip positiveAudio1;
    private AudioClip positiveAudio2;
    private AudioClip negativeAudio;

    // New audio clips for pillow types
    private AudioClip audioClipBigPillow;
    private AudioClip audioClipSmallPillow;

    void Start()
    {
        startPosition = transform.position;
        GetComponent<Collider2D>().enabled = false;
        canDrag = false;
        droppedPillowsCount = 0;
        boyAnimator = Boy.GetComponent<Animator>();
        kikiAnimator = Kiki.GetComponent<Animator>();

        if (dust != null)
        {
            dust.SetActive(false);
        }
        else
        {
            Debug.LogError("Dust game object reference not set in the inspector.");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }
        else
        {
            Debug.LogError("SpriteRenderer component not found.");
        }

        // Load audio clips from Resources
        positiveAudio1 = Resources.Load<AudioClip>("Audio/FeedbackAudio/GOOD JOB");
        positiveAudio2 = Resources.Load<AudioClip>("Audio/FeedbackAudio/KEEP GOING");
        negativeAudio = Resources.Load<AudioClip>("Audio/FeedbackAudio/THAT_S NOT RIGHT");

        audioClipBigPillow = Resources.Load<AudioClip>("Audio/Helper Audio/bigpillow");
        audioClipSmallPillow = Resources.Load<AudioClip>("Audio/Helper Audio/smallpillow");
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
        if (audioClipBigPillow == null || audioClipSmallPillow == null)
        {
            Debug.LogError("Big or small pillow audio clips not found in Resources.");
        }

        GameObject audioObject = GameObject.FindGameObjectWithTag("FeedbackAudio");
        if (audioObject != null)
        {
            feedbackAudioSource = audioObject.GetComponent<AudioSource>();
        }
        else
        {
            Debug.LogError("No GameObject with the tag 'FeedbackAudio' found.");
        }

        if (nextaudiosoucre != null)
        {
            AudioSource testAudioSource = nextaudiosoucre.GetComponent<AudioSource>();
            if (testAudioSource == null)
            {
                Debug.LogError("No AudioSource component found on nextaudiosoucre.");
            }
        }
        else
        {
            Debug.LogError("nextaudiosoucre GameObject is not assigned in the inspector.");
        }
    }

    void Update()
    {
        if (isDragging && canDrag)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mousePosition + offset;
           
            /*// If the player is interacting, stop the helper hand
            helperHandController.StopHelperHand();*/
        }
    }

    void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            offset = transform.position - mousePosition;

            // Change sorting order to 10 while dragging
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 10;
            }
            
            // Stop the helper hand
            helperHandController.StopHelperHand();
        }
    }

    void OnMouseUp()
    {
        if (isDragging && canDrag)
        {
            isDragging = false;

            // Reset sorting order to original
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = originalSortingOrder;
            }

            if (Vector3.Distance(transform.position, targetPosition.position) < offsetValue)
            {
                canDrag = false;
                PillowDragAndDrop[] allPillows = FindObjectsOfType<PillowDragAndDrop>();

                foreach (PillowDragAndDrop pillow in allPillows)
                {
                   pillow.GetComponent<Collider2D>().enabled = false;                    
                }
                LeanTween.move(gameObject, targetPosition.position, 0.5f).setOnComplete(() =>
                {
                    transform.rotation = Quaternion.identity;
                    GetComponent<Collider2D>().enabled = false;
                    HasInteracted = true;

                    ActivateDust();
                    UpdateBedsheetSprite();
                    targetPosition.gameObject.SetActive(false);

                    // Increment the droppedPillowsCount
                    droppedPillowsCount++;
                    Debug.Log("droppedpillows count: " + droppedPillowsCount);
                    // Call the coroutine to handle animation and audio with a delay
                    StartCoroutine(HandlePillowAnimationAndAudio());
                });

                if (droppedPillowsCount < 3) // Only play audio for the first three pillows
                {
                    boyAnimator.SetTrigger("rightDrop");
                    kikiAnimator.SetTrigger("rightDrop");
                    PlayPositiveFeedbackAudio();
                }
               /* else
                {
                    boyAnimator.SetTrigger("rightDrop");
                    kikiAnimator.SetTrigger("rightDrop");
                }*/
            }
            else
            {
                boyAnimator.SetTrigger("wrongDrop");
                kikiAnimator.SetTrigger("wrongDrop");

                if (droppedPillowsCount < 4)
                {
                    PlayNegativeFeedbackAudio();
                }

                // Reset the position if the drop was incorrect
                transform.position = startPosition;
                transform.rotation = Quaternion.identity;

                // Reset the interaction status and reschedule the helper hand after a delay
                HasInteracted = false;
                helperHandController.ScheduleHelperHand(this);

                // If you need a delay, use a coroutine instead of Invoke:
                StartCoroutine(DelayedScheduleHelperHand());
            }
        }
    }

    private IEnumerator HandlePillowAnimationAndAudio()
    {
        yield return new WaitForSeconds(2f);

        if (droppedPillowsCount < 4)
        {
            EnableNextCollider();

            if (nextCollider != null && nextCollider.enabled)
            {
                var nextPillow = nextCollider.GetComponent<PillowDragAndDrop>();

                if (nextPillow != null)
                {
                    AudioSource audioSource = nextaudiosoucre.GetComponent<AudioSource>() ?? feedbackAudioSource;

                    if (audioSource == null)
                    {
                        Debug.LogError("AudioSource is null. Neither nextaudiosoucre nor feedbackAudioSource is assigned correctly.");
                    }
                    else
                    {
                        if (nextPillow.IsBigPillow())
                        {
                            kikiAnimator.SetTrigger("bigPillow");
                            audioSource.PlayOneShot(audioClipBigPillow);
                            StartCoroutine(RevealTextWordByWord("Put the Big Pillow at the Back", 0.5f));
                            Debug.Log("Playing big pillow audio.");
                        }
                        else
                        {
                            kikiAnimator.SetTrigger("smallPillow");
                            audioSource.PlayOneShot(audioClipSmallPillow);
                            StartCoroutine(RevealTextWordByWord("Put the Small Pillow at the front of the big Pillow", 0.3f));
                            Debug.Log("Playing small pillow audio.");
                        }
                    }

                    helperHandController.ScheduleNextPillow(nextPillow);
                }
            }

            yield return new WaitForSeconds(4f);
            canDrag = true;
        }
    }


    private IEnumerator DelayedScheduleHelperHand()
    {
        yield return new WaitForSeconds(10f);
        helperHandController.ScheduleHelperHand(this);
    }

    void OnEnable()
    {
        if (!HasInteracted && GetComponent<Collider2D>().enabled)
        {
            StartCoroutine(StartHelperHandAfterColliderEnabled());
        }
    }

    private IEnumerator StartHelperHandAfterColliderEnabled()
    {
        // Ensure a small delay to allow the collider to be fully enabled
        yield return new WaitForEndOfFrame();

        // Debug log to confirm that the delay timer is starting
        Debug.Log($"Delay timer started for: {gameObject.name} after collider was enabled");

        if (IsBigPillow() || CorrespondingBigPillowInteracted())
        {
            helperHandController.ScheduleHelperHand(this);
        }
    }

    public bool IsBigPillow()
    {
        return gameObject.name.Contains("Big");
    }

    private bool CorrespondingBigPillowInteracted()
    {
        if (gameObject.name.Contains("Small Left"))
        {
            var bigPillowLeft = FindObjectOfType<boyController1>().pillowBigLeft.GetComponent<PillowDragAndDrop>();
            return bigPillowLeft != null && bigPillowLeft.HasInteracted;
        }
        else if (gameObject.name.Contains("Small Right"))
        {
            var bigPillowRight = FindObjectOfType<boyController1>().pillowBigRight.GetComponent<PillowDragAndDrop>();
            return bigPillowRight != null && bigPillowRight.HasInteracted;
        }
        return false;
    }

    private void EnableNextCollider()
    {
        // Disable all other pillow colliders
        DisableOtherPillowColliders();

        if (nextCollider != null)
        {
            nextCollider.enabled = true;            
        }
    }

    private void DisableOtherPillowColliders()
    {
        PillowDragAndDrop[] allPillows = FindObjectsOfType<PillowDragAndDrop>();

        foreach (PillowDragAndDrop pillow in allPillows)
        {
            if (pillow != this && pillow.GetComponent<Collider2D>() != nextCollider)
            {
                pillow.GetComponent<Collider2D>().enabled = false;
            }
        }
    }

    private void ActivateDust()
    {
        if (dust != null)
        {
            dust.SetActive(true);

            Animator dustAnimator = dust.GetComponent<Animator>();
            dustAnimator.enabled = true;
            if (dustAnimator != null)
            {
                StartCoroutine(DeactivateDustAfterAnimation(dustAnimator));
            }
        }
    }

    private IEnumerator DeactivateDustAfterAnimation(Animator animator)
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        dust.SetActive(false);
    }

    private void UpdateBedsheetSprite()
    {
        if (bedsheet != null)
        {
            string spriteName = "";

            switch (droppedPillowsCount)
            {
                case 0:
                    spriteName = "bedsheet3";
                    break;
                case 1:
                    spriteName = "bedsheet2";
                    break;
                case 2:
                    spriteName = "bedsheet1";
                    break;
                case 3:
                    spriteName = "bedsheet";
                    break;
            }

            if (!string.IsNullOrEmpty(spriteName))
            {
                Sprite newSprite = Resources.Load<Sprite>("images/" + spriteName);
                if (newSprite != null)
                {
                    bedsheet.GetComponent<SpriteRenderer>().sprite = newSprite;
                    Debug.Log("Bedsheet sprite updated to: " + spriteName);
                }
                else
                {
                    Debug.LogError("Sprite not found in Resources/images: " + spriteName);
                }
            }
        }
    }
    private IEnumerator PlayPositiveSound()
    {

        yield return new WaitForSeconds(1f);
        if (feedbackAudioSource != null)
        {
            AudioClip[] positiveAudios = new AudioClip[] { positiveAudio1, positiveAudio2 };
            feedbackAudioSource.clip = positiveAudios[Random.Range(0, positiveAudios.Length)];
            feedbackAudioSource.Play();
        } 
    }

    private IEnumerator PlayNegativeSound()
    {
        yield return new WaitForSeconds(1f);
        if (feedbackAudioSource != null)
        {
            feedbackAudioSource.clip = negativeAudio;
            feedbackAudioSource.Play();
        }
    }

    private void PlayPositiveFeedbackAudio()
    {
        SfxAudioSource.PlayOneShot(sfxAudio2);
        StartCoroutine(PlayPositiveSound());
    }

    private void PlayNegativeFeedbackAudio()
    {
        SfxAudioSource.PlayOneShot(sfxAudio3);
        StartCoroutine(PlayNegativeSound());
    
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
