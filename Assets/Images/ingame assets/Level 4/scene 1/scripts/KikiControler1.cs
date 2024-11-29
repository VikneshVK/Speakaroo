using UnityEngine;
using System.Collections;
using TMPro;

public class KikiController1 : MonoBehaviour
{
    public static int itemsDropped = 0;
    public static bool startFlying = false; // New boolean to track if flying should start

    public Transform dropLocation;
    public Transform startPosition;
    public GameObject boy;
    public GameObject itemHolder;
    public GameObject prefabToSpawn;
    public Transform spawnLocation;
    public Transform dropTarget;
    public float flySpeed = 2f;
    public bool isLocked = false;

    private Lvl4Sc1Audiomanger lvl4Sc1Audiomanger;
    public GameObject AudioManager;
    public AudioClip Audio1;
    public AudioClip Audio2;
    public AudioClip Audio3;
    public AudioClip Audio4;
    public AudioClip Audio5;
    public AudioClip Audio6;
    public AudioClip Audio7;
    public AudioClip Audio8;
    public TextMeshProUGUI subtitleText;

    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;
    public AudioClip SfxAudio2;

    private Animator birdAnimator;
    private Animator boyAnimator;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer; // Reference to SpriteRenderer for flipping
    private GameObject currentItem;
    private JojoController jojoController;

    private Vector3 targetPosition;
    private bool isMoving = false;

    private enum BirdState
    {
        Idle,
        FlyingToItem,
        FlyingToPlate,
        ReturningToStart
    }

    private BirdState currentState = BirdState.Idle;

    void Start()
    {
        birdAnimator = GetComponent<Animator>();
        boyAnimator = boy.GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // Initialize the SpriteRenderer
        transform.position = startPosition.position;
        lvl4Sc1Audiomanger = AudioManager.GetComponent<Lvl4Sc1Audiomanger>();
        jojoController = boy.GetComponent<JojoController>();
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!isLocked)
        {
            switch (currentState)
            {
                case BirdState.Idle:
                    CheckStartFlying();
                    break;
                case BirdState.FlyingToItem:
                case BirdState.FlyingToPlate:
                case BirdState.ReturningToStart:
                    MoveToTarget();
                    break;
            }
        }
    }

    private void CheckStartFlying()
    {
        // Check if startFlying is true and bird is in the Idle state
        if (startFlying && birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            UpdateItemReferences();
            currentItem = GetCurrentItem();
            if (currentItem != null)
            {
                birdAnimator.ResetTrigger("GoToRest");
                birdAnimator.SetTrigger("GoToFood");
                currentState = BirdState.FlyingToItem;
                targetPosition = currentItem.transform.position;
                isMoving = true;
                startFlying = false; // Reset the boolean after starting the flight
            }
        }
    }

    private GameObject GetCurrentItem()
    {
        switch (itemsDropped)
        {
            case 0:
                return GameObject.FindGameObjectWithTag("IceCream");
            case 1:
                return GameObject.FindGameObjectWithTag("Cookies");
            case 2:
                return GameObject.FindGameObjectWithTag("Apples");
            default:
                birdAnimator.SetBool("allDone", true);
                return null;
        }
    }

    private void MoveToTarget()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, flySpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            OnReachTarget();
        }
    }

    private void OnReachTarget()
    {
        switch (currentState)
        {
            case BirdState.FlyingToItem:
                // Set item as a child and trigger the animation to fly to plate
                if (currentItem != null)
                {
                    currentItem.transform.SetParent(itemHolder.transform);
                    currentItem.transform.localPosition = Vector3.zero;
                }
                birdAnimator.SetTrigger("GoToPlate");

                // Flip sprite horizontally when going to the plate
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = true;
                }

                currentState = BirdState.FlyingToPlate;
                targetPosition = dropLocation.position;
                isMoving = true;
                break;

            case BirdState.FlyingToPlate:
                // Release item at the plate location, play audio, and trigger return to start
                if (currentItem != null)
                {
                    currentItem.transform.SetParent(null);
                    currentItem.transform.position = dropLocation.position;
                }
                audioSource.Play();
                if (SfxAudioSource != null)
                {
                    SfxAudioSource.loop = false;
                    SfxAudioSource.PlayOneShot(SfxAudio1);
                }
                StartCoroutine(RevealTextWordByWord("Okay, Here you Go..!", 0.5f));
                birdAnimator.SetTrigger("GoToRest");

                currentState = BirdState.ReturningToStart;
                targetPosition = startPosition.position;
                isMoving = true;
                break;

            case BirdState.ReturningToStart:
                birdAnimator.SetTrigger("PositionReached");

                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = false;
                }

                // Trigger animation based on itemsDropped count
                switch (itemsDropped)
                {
                    case 0:
                        boyAnimator.SetTrigger("IceCream");
                        lvl4Sc1Audiomanger.PlayAudio(Audio1);
                        StartCoroutine(RevealTextWordByWord("Can you please feed me IceCream?", 0.5f));
                        break;
                    case 1:
                        boyAnimator.SetTrigger("Cookies");
                        lvl4Sc1Audiomanger.PlayAudio(Audio2);
                        StartCoroutine(RevealTextWordByWord("Can you please feed me Cookies?", 0.5f));
                        break;
                    case 2:
                        boyAnimator.SetTrigger("Apple");
                        lvl4Sc1Audiomanger.PlayAudio(Audio3);
                        StartCoroutine(RevealTextWordByWord("Can you please feed me Apple?", 0.5f));
                        break;
                }

                // Enable item collider after 2-second delay
                StartCoroutine(EnableColliderAfterDelay(currentItem, 2f));

                SetupDragDrop(currentItem);

                currentState = BirdState.Idle;
                ResetPositionReachedAfterDelay();
                break;
        }
    }

    // Coroutine to enable the collider after a delay
    private IEnumerator EnableColliderAfterDelay(GameObject item, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (item != null)
        {
            Collider2D itemCollider = item.GetComponent<Collider2D>();
            if (itemCollider != null)
            {
                itemCollider.enabled = true;
            }
        }
    }


    // Method to set up drag-and-drop functionality
    private void SetupDragDrop(GameObject item)
    {
        if (item != null)
        {
            Draggable draggable = item.GetComponent<Draggable>();
            if (draggable != null)
            {
                draggable.SetDropTarget(dropTarget); // Set the drop target

                draggable.onDrop += (GameObject droppedItem) =>
                {
                    StartCoroutine(HandleDropWithDelay(droppedItem)); // Start coroutine for delayed handling
                };
            }
        }
    }

    private IEnumerator HandleDropWithDelay(GameObject droppedItem)
    {
        switch (itemsDropped)
        {
            case 0:
                boyAnimator.SetTrigger("FeedBack");
                break;
            case 1:
                boyAnimator.SetTrigger("FeedBack1");
                break;
            case 2:
                boyAnimator.SetTrigger("FeedBack2");
                break;
        }
        if (SfxAudioSource != null)
        {
            SfxAudioSource.loop = false;
            SfxAudioSource.PlayOneShot(SfxAudio2);
        }

        StartCoroutine(TweenAndDestroy(droppedItem));

        yield return new WaitForSeconds(2f);

        if (itemsDropped < 2)
        {
            itemsDropped++;
            boyAnimator.SetTrigger("Expression");
            lvl4Sc1Audiomanger.PlayAudio(Audio4);
            StartCoroutine(RevealTextWordByWord("mmm..! Tasty...!", 0.5f));
            StartCoroutine(DelayedAnimationAndSpawn(3.5f));
        }
        else
        {
            itemsDropped++;
            boyAnimator.SetTrigger("SoFull");
            StartCoroutine(DelayedAnimationAndSpawn(1.5f));
        }
    }
        private IEnumerator TweenAndDestroy(GameObject item)
    {
        LeanTween.scale(item, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInOutBack);

        yield return new WaitForSeconds(0.5f);
        Destroy(item);
    }


    private IEnumerator DelayedAnimationAndSpawn(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        switch (itemsDropped)
        {
            case 1:
                boyAnimator.SetTrigger("Cookies");
                lvl4Sc1Audiomanger.PlayAudio(Audio7);
                StartCoroutine(RevealTextWordByWord("Next, I want to eat Cookies", 0.5f));

                yield return new WaitForSeconds(4f);
                SpawnSpeechBubble();
                break;
            case 2:
                boyAnimator.SetTrigger("Apple");
                lvl4Sc1Audiomanger.PlayAudio(Audio8);
                StartCoroutine(RevealTextWordByWord("Next, I want to eat Apple", 0.5f));

                yield return new WaitForSeconds(4f);
                SpawnSpeechBubble();
                break;
            case 3:
                boyAnimator.SetBool("allDone", true);
                lvl4Sc1Audiomanger.PlayAudio(Audio5);
                StartCoroutine(RevealTextWordByWord("mmm...! I'm so Full...!", 0.5f));

                yield return new WaitForSeconds(3f);
                birdAnimator.SetBool("allDone", true);
                lvl4Sc1Audiomanger.PlayAudio(Audio6);
                StartCoroutine(RevealTextWordByWord("Thankyou for Helping Friend..!", 0.5f));
                break;
        }

    }

    // Method to spawn a speech bubble or other prefab
    private void SpawnSpeechBubble()
    {
        if (prefabToSpawn != null)
        {
            Instantiate(prefabToSpawn, spawnLocation.position, spawnLocation.rotation);
        }
    }

    private IEnumerator ResetPositionReachedAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);

        birdAnimator.SetBool("positionReached", false);
        birdAnimator.ResetTrigger("GoToFood");
        birdAnimator.ResetTrigger("GoToPlate");
        birdAnimator.ResetTrigger("GoToRest");
        jojoController.prefabSpawned = false;
    }

    public void UpdateItemReferences()
    {
        currentItem = null; // Reset currentItem
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
