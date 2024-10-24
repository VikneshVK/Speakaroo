using UnityEngine;

public class FrisbeeSpin : MonoBehaviour
{
    public delegate void FrisbeeDestroyed(FrisbeeSpin frisbee);
    public event FrisbeeDestroyed OnDestroyEvent;

    public float spinDuration = 1f; // Time it takes for the frisbee to complete one revolution
    public float flyAwayDuration = 1f; // Time it takes for the frisbee to fly away
    public float ellipseRadiusX = 3f; // Radius on the X-axis of the ellipse
    public float ellipseRadiusY = 1.5f; // Radius on the Y-axis of the ellipse
    public float flyAwayForce = 500f; // Force to fly the frisbee away
    public float flyAwayRandomness = 0.5f; // How much randomness to add to the fly-away direction
    public AudioSource spinAudioSource; // Assign the audio source in the inspector

    private bool isSpinning = false;
    private Vector2 lastDirection; // Store the last movement direction

    private void OnMouseDown()
    {
        if (!isSpinning)
        {
            isSpinning = true;

            // Play spin audio
            if (spinAudioSource != null)
            {
                spinAudioSource.Play();
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
                // Once the fly away is complete, destroy the frisbee
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
}
