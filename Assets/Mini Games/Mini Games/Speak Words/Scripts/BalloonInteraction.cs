using System.Collections;
using UnityEngine;

public class BalloonInteraction : MonoBehaviour
{
    private void OnMouseDown()
    {
        // Play the confetti particle effect
        ParticleSystem confetti = GetComponentInChildren<ParticleSystem>();
        if (confetti != null)
        {
            confetti.Play(); // Play the confetti effect
        }

        // Add any other logic for balloon popping here (e.g., scaling down, destroying)
        StartCoroutine(PopBalloonRoutine(gameObject));
    }

    IEnumerator PopBalloonRoutine(GameObject balloon)
    {
        // Scale down animation
        LeanTween.scale(balloon, Vector3.zero, 0.3f).setEaseInBack();

        // Wait for the animation to finish
        yield return new WaitForSeconds(0.3f);

        // Destroy the balloon object
        Destroy(balloon);
    }
}
