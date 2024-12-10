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

    public float dropOffset;
    public float blinkCount;
    public float blinkDuration;

    private Animator boyAnimator;
    private Animator birdAnimator;
    private Animator animator;
    private InteractableObject interactableObject;
    private bool isDragging = false;
    private Vector3 offset;
    private Dictionary<GameObject, bool> interactionStatus = new Dictionary<GameObject, bool>();
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private SpriteRenderer objectRenderer;
    private BoyController1 parentController;

    private AudioSource feedbackAudioSource;
    private AudioClip positiveAudio1;
    private AudioClip positiveAudio2;
    private AudioClip negativeAudio;

    void Start()
    {
        boyAnimator = Boy.GetComponent<Animator>();
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
                HelpPointerManager.IsAnyObjectBeingInteracted = true;
                HelpPointerManager.Instance.StopHelpPointer(); // Stop the help pointer
                offset = transform.position - mousePosition;
                interactableObject.OnInteract();
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            HelpPointerManager.IsAnyObjectBeingInteracted = false;
            CheckDrop();
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

                correctDropZoneObject.GetComponent<SpriteRenderer>().enabled = false;

                EnableMiniatureCollider();
                MarkAsInteracted(gameObject);
            });
        }
        else
        {
            birdAnimator.SetTrigger("wrongDrop");

            PlayNegativeFeedbackAudio();

            ResetInteractionStatus();

            ResetObjectPosition();
        }
    }

    // Coroutine to add delay before playing audio
    private IEnumerator PlayAudioWithDelay()
    {
        yield return new WaitForSeconds(2f); // Adjust delay time as needed
        parentController.PlayAudioByIndex(1);
    }


    void ResetInteractionStatus()
    {
        if (interactionStatus.ContainsKey(gameObject))
        {
            interactionStatus[gameObject] = false;
        }

        InteractableObject interactableObject = GetComponent<InteractableObject>();
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
