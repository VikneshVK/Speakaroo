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

    /*private SpriteRenderer boySpriteRender;*/
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
        // Disable colliders for the start, only enable for the first interactable
        miniWhale.GetComponent<Collider2D>().enabled = false;
        miniBuilding.GetComponent<Collider2D>().enabled = false;
        objectRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        birdAnimator = bird.GetComponent<Animator>();

        // Load audio clips from Resources
        positiveAudio1 = Resources.Load<AudioClip>("Audio/FeedbackAudio/Audio1");
        positiveAudio2 = Resources.Load<AudioClip>("Audio/FeedbackAudio/Audio2");
        negativeAudio = Resources.Load<AudioClip>("Audio/FeedbackAudio/Audio3");

        // Find the audio source with the tag "FeedbackAudio"
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
            /*boySpriteRender.flipX = false;*/
            boyAnimator.SetTrigger("isRightDrop");
            birdAnimator.SetTrigger("RightDrop");
            parentController.PlayAudioByIndex(1);

            // Play a random positive audio clip
            /*PlayPositiveFeedbackAudio();*/

            // Tween to the correct drop zone's position
            LeanTween.move(gameObject, correctDropZoneObject.transform.position, 0.5f).setOnComplete(() =>
            {
                transform.rotation = Quaternion.identity;
                animator.SetTrigger("isRightDrop");

                // Start coroutine to instantiate prefab after audio is done
                StartCoroutine(InstantiateSpeechBubbleAfterAudio());

                // Disable all colliders
                DisableAllColliders();

                // Disable the SpriteRenderer of the correctDropZoneObject
                correctDropZoneObject.GetComponent<SpriteRenderer>().enabled = false;

                EnableMiniatureCollider();
                MarkAsInteracted(gameObject);
                /*boySpriteRender.flipX = true;*/
            });
        }
        else
        {
            /*boySpriteRender.flipX = false;*/
            boyAnimator.SetTrigger("isWrongDrop");
            birdAnimator.SetTrigger("wrongDrop");
            // Play the negative audio clip
            PlayNegativeFeedbackAudio();

            // Reset interaction status
            ResetInteractionStatus();

            ResetObjectPosition();
            
        }
    }

    void ResetInteractionStatus()
    {
        if (interactionStatus.ContainsKey(gameObject))
        {
            interactionStatus[gameObject] = false;
        }

        // Restart the interaction tracking in the InteractableObject script
        InteractableObject interactableObject = GetComponent<InteractableObject>();
        if (interactableObject != null)
        {
            interactableObject.EnableInteractionTracking();
        }
    }


    /*private void PlayPositiveFeedbackAudio()
    {
        if (feedbackAudioSource != null)
        {
            AudioClip[] positiveAudios = new AudioClip[] { positiveAudio1, positiveAudio2 };
            feedbackAudioSource.clip = positiveAudios[Random.Range(0, positiveAudios.Length)];
            feedbackAudioSource.Play();
        }
    }*/

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
        // Wait until the audio has finished playing
        yield return new WaitUntil(() => !feedbackAudioSource.isPlaying);
        yield return new WaitForSeconds(3.5f);

        // Instantiate the speech bubble prefab
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
        // Reset position if the drop was incorrect
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
