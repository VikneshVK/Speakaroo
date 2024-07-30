using UnityEngine;

public class BalloonController : MonoBehaviour
{
    public float scaleDownDuration = 0.5f; // Duration for scaling down the balloon
    public GameObject confettiEffectPrefab;
    private void OnMouseDown()
    {
        // Start the scaling down process
        LeanTween.scale(gameObject, Vector3.zero, scaleDownDuration)
                 .setEase(LeanTweenType.easeInBack)
                 .setOnComplete(OnScaleDownComplete);
    }

    private void OnScaleDownComplete()
    {
        Transform parentTransform = transform.parent;
        Instantiate(confettiEffectPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject); // Destroy the balloon gameObject

        // Play the audio for the balloon's tag
        ST_AudioManager.Instance.PlayAudioAfterDestroy(parentTransform.tag);
    }
}
