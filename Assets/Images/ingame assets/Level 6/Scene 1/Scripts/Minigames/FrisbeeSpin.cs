using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class FrisbeeSpin : MonoBehaviour
{
    public delegate void FrisbeeDestroyed(FrisbeeSpin frisbee);
    public event FrisbeeDestroyed OnDestroyEvent;
    public GameObject Boy;
    public float spinDuration = 1f; // Time it takes for the frisbee to complete one revolution
    public float flyAwayDuration = 1f; // Time it takes for the frisbee to fly away
    public float ellipseRadiusX = 3f; // Radius on the X-axis of the ellipse
    public float ellipseRadiusY = 1.5f; // Radius on the Y-axis of the ellipse
    public float flyAwayForce = 500f; // Force to fly the frisbee away
    public float flyAwayRandomness = 0.5f; // How much randomness to add to the fly-away direction
    public AudioSource spinAudioSource; // Assign the audio source in the inspector

    public AudioSource boyAudioSource;
    private bool isSpinning = false;
    private Vector2 lastDirection; // Store the last movement direction
    private static List<FrisbeeSpin> allFrisbees = new List<FrisbeeSpin>(); // List of all frisbee instances

    private void Awake()
    {
        // Add this frisbee to the list of all frisbees
        allFrisbees.Add(this);
    }

    private void OnDestroy()
    {
        // Remove this frisbee from the list when destroyed
        allFrisbees.Remove(this);
    }

    private void OnMouseDown()
    {
        if (!isSpinning)
        {
            isSpinning = true;
            DisableOtherFrisbees(); // Disable interactivity on other frisbees

            Boy.GetComponent<Animator>().SetTrigger("Giggle");

            // Play spin audio
            if (spinAudioSource != null && boyAudioSource != null)
            {
                spinAudioSource.Play();
                boyAudioSource.Play();
            }

            // Store the initial rotation to maintain it throughout
            Quaternion initialRotation = transform.rotation;

            // Spin the frisbee in an elliptical path without rotating on its Z-axis
            Vector3 originalPosition = transform.position;

            // Calculate the elliptical path without rotating the Z-axis
            LeanTween.value(gameObject, 0f, 360f, spinDuration) // Revolve 2 times (720 degrees)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnUpdate((float angle) =>
                {
                    // Move the frisbee along the ellipse path
                    Vector3 newPosition = GetEllipsePosition(originalPosition, ellipseRadiusX, ellipseRadiusY, angle % 360); // Use modulus to loop
                    Vector3 direction = newPosition - transform.position;
                    lastDirection = direction.normalized; // Store the last direction
                    transform.position = newPosition;

                    // Maintain the original rotation
                    transform.rotation = initialRotation;
                })
                .setOnComplete(() =>
                {
                    // After the revolution, immediately start flying away in the last direction
                    FlyAway();
                });
        }
    }

    private void FlyAway()
    {
        // Add randomness to the last direction to create varied flight paths
        Vector2 randomizedDirection = lastDirection + new Vector2(Random.Range(-flyAwayRandomness, flyAwayRandomness), Random.Range(-flyAwayRandomness, flyAwayRandomness)).normalized;
        randomizedDirection.Normalize(); // Normalize to keep consistent direction magnitude

        // Make the frisbee fly away in the randomized direction
        LeanTween.move(gameObject, (Vector2)transform.position + randomizedDirection * flyAwayForce, flyAwayDuration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                // Once the fly away is complete, re-enable other frisbees and destroy this frisbee
                EnableOtherFrisbees();
                OnDestroyEvent?.Invoke(this);
                Destroy(gameObject);
            });
    }

    // Function to calculate a position on the ellipse based on its radii and angle
    private Vector3 GetEllipsePosition(Vector3 center, float radiusX, float radiusY, float angle)
    {
        float radianAngle = angle * Mathf.Deg2Rad;
        float x = center.x + Mathf.Cos(radianAngle) * radiusX;
        float y = center.y + Mathf.Sin(radianAngle) * radiusY;
        return new Vector3(x, y, center.z);
    }

    // Disable interactivity on all other frisbees by removing EventTrigger events
    private void DisableOtherFrisbees()
    {
        foreach (var frisbee in allFrisbees)
        {
            if (frisbee != this)
            {
                frisbee.RemoveDragEvents();
            }
        }
    }

    // Enable interactivity on all other frisbees by adding EventTrigger events back
    private void EnableOtherFrisbees()
    {
        foreach (var frisbee in allFrisbees)
        {
            if (frisbee != this)
            {
                frisbee.AddDragEvents();
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
            // Add PointerDown event to start the spin
            EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDownEntry.callback.AddListener((data) => { OnMouseDown(); });
            eventTrigger.triggers.Add(pointerDownEntry);

            // Add PointerUp event to stop the interaction
            EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUpEntry.callback.AddListener((data) => { /* Optional: Add any OnMouseUp logic here */ });
            eventTrigger.triggers.Add(pointerUpEntry);
        }
    }
}
