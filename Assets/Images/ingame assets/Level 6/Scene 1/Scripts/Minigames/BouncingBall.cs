using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BouncingBall : MonoBehaviour
{
    public delegate void BallDestroyed(BouncingBall ball);
    public event BallDestroyed OnDestroyEvent;
    public GameObject Boy;
    public float bounceForce = 500f; // How far the ball should bounce
    public float bounceDuration = 2f; // Time it takes for the ball to bounce away
    public AudioSource bounceAudioSource; // Assign the audio source in the inspector
    public AudioSource boyAudioSource;
    private bool isBouncing = false;
    private bool isAudioPlayed = false;
    private static List<BouncingBall> allBalls = new List<BouncingBall>(); // List of all ball instances

    private void Awake()
    {
        // Add this ball to the list of all balls
        allBalls.Add(this);
    }

    private void OnDestroy()
    {
        // Remove this ball from the list when destroyed
        allBalls.Remove(this);
    }

    private void OnMouseDown()
    {
        if (!isBouncing)
        {
            isBouncing = true;
            DisableOtherBalls(); // Disable interactivity on other balls

            Boy.GetComponent<Animator>().SetTrigger("Giggle");

            // Play bounce audio
            if (bounceAudioSource != null && !isAudioPlayed)
            {
                bounceAudioSource.Play();
                boyAudioSource.Play();
                isAudioPlayed = true;
            }

            // Generate a random direction for the ball to bounce
            Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1f)).normalized;

            // Use LeanTween to bounce the ball in a random direction
            LeanTween.move(gameObject, (Vector2)transform.position + randomDirection * bounceForce, bounceDuration)
                .setEase(LeanTweenType.easeInBounce)
                .setOnComplete(() =>
                {
                    // Re-enable other balls and destroy this ball
                    EnableOtherBalls();
                    OnDestroyEvent?.Invoke(this);
                    Destroy(gameObject);
                });

            LeanTween.rotate(gameObject, new Vector3(0, 0, Random.Range(360f, 720f)), bounceDuration)
                .setEase(LeanTweenType.easeOutQuad);
        }
    }

    // Disable interactivity on all other balls by removing EventTrigger events
    private void DisableOtherBalls()
    {
        foreach (var ball in allBalls)
        {
            if (ball != this)
            {
                ball.RemoveDragEvents();
            }
        }
    }

    // Enable interactivity on all other balls by adding EventTrigger events back
    private void EnableOtherBalls()
    {
        foreach (var ball in allBalls)
        {
            if (ball != this)
            {
                ball.AddDragEvents();
            }
        }
    }

    // Remove EventTrigger events to disable dragging
    private void RemoveDragEvents()
    {
        EventTrigger eventTrigger = GetComponent<EventTrigger>();
        if (eventTrigger != null)
        {
            eventTrigger.triggers.RemoveAll(entry =>
                entry.eventID == EventTriggerType.PointerDown ||
                entry.eventID == EventTriggerType.PointerUp);
        }
    }

    // Add EventTrigger events to enable dragging
    private void AddDragEvents()
    {
        EventTrigger eventTrigger = GetComponent<EventTrigger>();
        if (eventTrigger != null)
        {
            // Add PointerDown event to start the bounce
            EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDownEntry.callback.AddListener((data) => { OnMouseDown(); });
            eventTrigger.triggers.Add(pointerDownEntry);

            // Add PointerUp event for optional future use
            EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUpEntry.callback.AddListener((data) => { /* Optional: Add any OnMouseUp logic here */ });
            eventTrigger.triggers.Add(pointerUpEntry);
        }
    }
}
