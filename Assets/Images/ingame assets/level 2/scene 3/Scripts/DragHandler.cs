using UnityEngine;
using System.Collections;

public class DragHandler : MonoBehaviour
{
    public Transform targetPosition;
    public GameObject nextGameObject;
    public Animator birdAnimator;
    public dragManager dragManager;

    private Vector2 originalPosition;
    private bool isDragging = false;
    private Collider2D objectCollider;
    private AnchorGameObject anchor;
    private HelperPointer helperPointer;

    private AudioSource feedbackAudioSource;
    private AudioClip positiveAudio1;
    private AudioClip positiveAudio2;
    private AudioClip negativeAudio;

    private bool isDroppedSuccessfully = false;

    public bool IsDragged => isDragging;

    void Awake()
    {
        // Assign the collider and HelperPointer in Awake to ensure they're ready before OnEnable
        objectCollider = GetComponent<Collider2D>();
        anchor = GetComponent<AnchorGameObject>();
        helperPointer = FindObjectOfType<HelperPointer>();

        if (helperPointer == null)
        {
            Debug.LogError("HelperPointer not found in the scene. Please ensure a HelperPointer script is attached to a GameObject.");
        }
        positiveAudio1 = Resources.Load<AudioClip>("Audio/FeedbackAudio/Audio1");
        positiveAudio2 = Resources.Load<AudioClip>("Audio/FeedbackAudio/Audio2");
        negativeAudio = Resources.Load<AudioClip>("Audio/FeedbackAudio/Audio3");
        GameObject audioObject = GameObject.FindGameObjectWithTag("FeedbackAudio");
        feedbackAudioSource = audioObject.GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        // Ensure the HelperPointer and Collider2D are properly initialized
        if (helperPointer != null && objectCollider.enabled)
        {
            helperPointer.ScheduleHelperHand(this, dragManager);
        }
    }

    void Start()
    {
        objectCollider.enabled = false;  // Initially disable all colliders
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseDown();
        }
        if (Input.GetMouseButton(0) && isDragging)
        {
            OnMouseDrag();
        }
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            OnMouseUp();
        }
    }

    void OnMouseDown()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (objectCollider == Physics2D.OverlapPoint(mousePosition))
        {
            isDragging = true;
            originalPosition = transform.position;
            if (anchor != null)
            {
                anchor.enabled = false;
            }

            // Stop the helper hand when the object is being interacted with
            helperPointer?.StopHelperHand();
        }
    }

    void OnMouseDrag()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector2(mousePosition.x, mousePosition.y);
    }

    void OnMouseUp()
    {
        isDragging = false;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Vector2.Distance(mousePosition, targetPosition.position) < 3f)  // Adjust the threshold as needed
        {
            if (!isDroppedSuccessfully)  // Check if already successfully dropped
            {
                PlayPositiveFeedbackAudio();
                LeanTween.move(gameObject, targetPosition.position, 0.5f).setOnComplete(() =>
                {
                    OnSuccessfulDrop();
                });
            }
        }
        else
        {
            PlayNegativeFeedbackAudio();
            LeanTween.move(gameObject, originalPosition, 0.5f).setOnComplete(OnFailedDrop);
        }
    }

    void OnFailedDrop()
    {
        // Logic for handling a failed drop, rescheduling helper hand
        if (helperPointer != null)
        {
            helperPointer.ScheduleHelperHand(this, dragManager, 10f);  // Reschedule helper hand after 10 seconds
        }

        // Resetting the object's position or any other necessary state reset
        Debug.Log("Drop failed, retry.");
    }

    void OnSuccessfulDrop()
    {
        if (isDroppedSuccessfully) return;  // Prevent duplicate drops
        isDroppedSuccessfully = true;

        Debug.Log("Successful drop confirmed at: " + Time.time);
        objectCollider.enabled = false;  // Disable collider to prevent re-triggering

        // Increment the correct drop count here, which will also handle the audio playback
        dragManager.OnItemDropped();

        if (nextGameObject != null)
        {
            nextGameObject.GetComponent<Collider2D>().enabled = true;

            // Schedule the helper hand for the next object
            var nextDragHandler = nextGameObject.GetComponent<DragHandler>();
            if (nextDragHandler != null && helperPointer != null)
            {
                helperPointer.ScheduleHelperHand(nextDragHandler, dragManager);
            }
        }

        Destroy(targetPosition.gameObject);
    }

    private void PlayPositiveFeedbackAudio()
    {
        if (feedbackAudioSource != null)
        {
            AudioClip[] positiveAudios = new AudioClip[] { positiveAudio1, positiveAudio2 };
            feedbackAudioSource.clip = positiveAudios[Random.Range(0, positiveAudios.Length)];
            feedbackAudioSource.Play();
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
}
