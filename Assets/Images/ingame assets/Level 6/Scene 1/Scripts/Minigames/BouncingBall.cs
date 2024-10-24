using UnityEngine;

public class BouncingBall : MonoBehaviour
{
    public delegate void BallDestroyed(BouncingBall ball);
    public event BallDestroyed OnDestroyEvent;

    public float bounceForce = 500f; // How far the ball should bounce
    public float bounceDuration = 1f; // Time it takes for the ball to bounce away
    public AudioSource bounceAudioSource; // Assign the audio source in the inspector

    private bool isBouncing = false;

    private void OnMouseDown()
    {
        if (!isBouncing)
        {
            isBouncing = true;

            // Play bounce audio
            if (bounceAudioSource != null)
            {
                bounceAudioSource.Play();
            }

            // Generate a random direction for the ball to bounce
            Vector2 randomDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1f)).normalized;

            // Use LeanTween to bounce the ball in a random direction
            LeanTween.move(gameObject, (Vector2)transform.position + randomDirection * bounceForce, bounceDuration)
                .setEase(LeanTweenType.easeInBounce)
                .setOnComplete(() =>
                {
                    // Once the bounce is complete, destroy the ball
                    OnDestroyEvent?.Invoke(this);
                    Destroy(gameObject);
                });
            LeanTween.rotate(gameObject, new Vector3(0, 0, Random.Range(360f, 720f)), bounceDuration)
    .setEase(LeanTweenType.easeOutQuad);
        }
    }
}
