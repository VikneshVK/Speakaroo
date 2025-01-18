using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndDropController : MonoBehaviour
{
    public GameObject correctDropZoneObject;
    public GameObject speechBubblePrefab;
    public GameObject mechanicsPrefab;
    public GameObject miniBus;
    public GameObject miniWhale;
    public GameObject miniBuilding;
    public GameObject Bus;
    public GameObject Whale;
    public GameObject Building;
    public GameObject Boy;
    public GameObject BoyCharacter;
    public Transform speechBubbleContainer;
    public GameObject bird;
   /* public HelpPointerManager helpercontroller;*/

    public float dropOffset;
    public float blinkCount;
    public float blinkDuration;

    private Animator boyAnimator;
    private Animator birdAnimator;
    private ParrotController parrotController;
    private Animator animator;
    private InteractableObject interactableObject;
    private bool isDragging = false;
    private Vector3 offset;
    private Dictionary<GameObject, bool> interactionStatus = new Dictionary<GameObject, bool>();
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private SpriteRenderer objectRenderer;
    private BoyController1 parentController;
    private int originalOrderInLayer;

    private AudioSource feedbackAudioSource;
    private AudioClip positiveAudio1;
    private AudioClip positiveAudio2;
    private AudioClip negativeAudio;

    void Start()
    {
        boyAnimator = Boy.GetComponent<Animator>();
        parrotController = bird.GetComponent<ParrotController>();
        interactionStatus.Add(miniBus, false);
        interactionStatus.Add(miniWhale, false);
        interactionStatus.Add(miniBuilding, false);
        originalPosition = transform.position;
        interactableObject = GetComponent<InteractableObject>();

        miniWhale.GetComponent<Collider2D>().enabled = false;
        miniBuilding.GetComponent<Collider2D>().enabled = false;
        objectRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        birdAnimator = bird.GetComponent<Animator>();

        positiveAudio1 = Resources.Load<AudioClip>("Audio/FeedbackAudio/GOOD JOB");
        positiveAudio2 = Resources.Load<AudioClip>("Audio/FeedbackAudio/KEEP GOING");
        negativeAudio = Resources.Load<AudioClip>("Audio/FeedbackAudio/THAT_S NOT RIGHT");

        GameObject audioObject = GameObject.FindGameObjectWithTag("FeedbackAudio");
        if (audioObject != null)
        {
            feedbackAudioSource = audioObject.GetComponent<AudioSource>();
        }
        else
        {
            Debug.LogError("No GameObject with the tag 'FeedbackAudio' found.");
        }
        parentController = BoyCharacter.GetComponent<BoyController1>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                HelpPointerManager.Instance.ResetTimerForObject(gameObject);
                HelpPointerManager.Instance.StopHelpPointer();
                offset = transform.position - mousePosition;
                interactableObject.OnInteract();

                // Store the original order in layer and set it to 10 while dragging
                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                originalOrderInLayer = spriteRenderer.sortingOrder;
                spriteRenderer.sortingOrder = 10;
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            CheckDrop();

            // Reset the order in layer to the original value
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = originalOrderInLayer;
        }

        if (isDragging)
        {
            DragObject();
        }
    }

    void DragObject()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = mousePosition + offset;
    }

    void CheckDrop()
    {
        if (IsCorrectDropZone())
        {
            boyAnimator.SetTrigger("isRightDrop");
            birdAnimator.SetTrigger("RightDrop");
            feedbackAudioSource.clip = positiveAudio1;
            feedbackAudioSource.Play();

            // Start coroutine to add delay before playing audio
            StartCoroutine(PlayAudioWithDelay());

            LeanTween.move(gameObject, correctDropZoneObject.transform.position, 0.5f).setOnComplete(() =>
            {
                transform.rotation = Quaternion.identity;
                animator.SetTrigger("isRightDrop");

                StartCoroutine(InstantiateSpeechBubbleAfterAudio());

                DisableAllColliders();
                HelpPointerManager.Instance.ResetTimerForObject(gameObject);
                HelpPointerManager.Instance.StopHelpPointer();
                correctDropZoneObject.GetComponent<SpriteRenderer>().enabled = false;
                interactableObject.UpdateDropStatus(gameObject.name);
                EnableMiniatureCollider();
                MarkAsInteracted(gameObject);
            });
        }
        else
        {
            DisableAllColliders();
            birdAnimator.SetTrigger("wrongDrop");

            PlayNegativeFeedbackAudio();            
            ResetObjectPosition();
            interactableObject.isInteracted = false;
            HelpPointerManager.Instance.ResetTimerForObject(gameObject); // Reset the timer for this object
            HelpPointerManager.Instance.StopHelpPointer();
            StartCoroutine(HandleNegativeFeedbackDelay());
        }
    }

    private IEnumerator HandleNegativeFeedbackDelay()
    {
        
        yield return new WaitForSeconds(2.5f); // Wait for 2.5 seconds
        birdAnimator.SetTrigger("onceMore");
        parentController.PlayAudioWithSubtitles(3, "Put the Big Toys on the Shelf", "Kiki");
        parrotController.SpawnAndTweenGlowOnInteractableObjects();        
       
        yield return new WaitForSeconds(2f);
        ResetInteractionStatus();
        parrotController.EnableCorrectColliders();
    }


    // Coroutine to add delay before playing audio
    private IEnumerator PlayAudioWithDelay()
    {
        yield return new WaitForSeconds(2f); // Adjust delay time as needed
        parentController.PlayAudioWithSubtitles(1, "Hey Kiki, Its your turn to clean up", "JoJo");
    }


    void ResetInteractionStatus()
    {
        if (interactionStatus.ContainsKey(gameObject))
        {
            interactionStatus[gameObject] = false;
        }
      
        if (interactableObject != null)
        {
            interactableObject.EnableInteractionTracking();           
        }
    }

    private void PlayNegativeFeedbackAudio()
    {
        if (feedbackAudioSource != null)
        {
            feedbackAudioSource.clip = negativeAudio;
            feedbackAudioSource.Play();
        }
    }

    private void DisableAllColliders()
    {
        if (Bus != null && Whale != null && Building != null)
        {
            Bus.GetComponent<Collider2D>().enabled = false;
            Whale.GetComponent<Collider2D>().enabled = false;
            Building.GetComponent<Collider2D>().enabled = false;
        }
    }

    IEnumerator InstantiateSpeechBubbleAfterAudio()
    {
        yield return new WaitUntil(() => !feedbackAudioSource.isPlaying);
        yield return new WaitForSeconds(3.5f);

        InstantiateSpeechBubble();
    }

    void InstantiateSpeechBubble()
    {
        GameObject speechBubble = Instantiate(speechBubblePrefab, speechBubbleContainer.position, Quaternion.identity);
        SpeechBubble bubbleController = speechBubble.GetComponent<SpeechBubble>();
        bubbleController.Setup(mechanicsPrefab, this);
    }

    void EnableMiniatureCollider()
    {
        switch (gameObject.name)
        {
            case "Bus":
                miniBus.GetComponent<Collider2D>().enabled = true;
                break;
            case "Whale":
                miniWhale.GetComponent<Collider2D>().enabled = true;
                break;
            case "Building":
                miniBuilding.GetComponent<Collider2D>().enabled = true;
                break;
        }
    }

    public void MarkAsInteracted(GameObject obj)
    {
        if (interactionStatus.ContainsKey(obj))
        {
            interactionStatus[obj] = true;
        }
    }

    bool IsCorrectDropZone()
    {
        return Vector3.Distance(transform.position, correctDropZoneObject.transform.position) <= dropOffset;
    }

    public Vector3 GetDropLocation2()
    {
        return correctDropZoneObject.transform.position;
    }

    void ResetObjectPosition()
    {
        transform.position = originalPosition;
        boyAnimator.SetTrigger("isWrongDrop");
        StartCoroutine(BlinkRedAndReset());
    }

    IEnumerator BlinkRedAndReset()
    {
        Color originalColor = objectRenderer.material.color;
        for (int i = 0; i < blinkCount; i++)
        {
            objectRenderer.material.color = Color.red;
            yield return new WaitForSeconds(blinkDuration);
            objectRenderer.material.color = originalColor;
            yield return new WaitForSeconds(blinkDuration);
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }    
}
