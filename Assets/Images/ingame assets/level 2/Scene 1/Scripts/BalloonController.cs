using UnityEngine;
using System.Collections;

public class BalloonController : MonoBehaviour
{
    public float scaleDownDuration = 0.5f; // Duration for scaling down the balloon
    public GameObject confettiEffectPrefab;
    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;

    private void Start()
    {
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
    }
    private void OnMouseDown()
    {
        // Start the scaling down process
        LeanTween.scale(gameObject, Vector3.zero, scaleDownDuration)
                 .setEase(LeanTweenType.easeInBack)
                 .setOnComplete(() =>
                 {
                     if (SfxAudioSource != null)
                     {
                         SfxAudioSource.loop = false;
                         SfxAudioSource.PlayOneShot(SfxAudio1);
                     }
                     OnScaleDownComplete();
                 });
    }

    private void OnScaleDownComplete()
    {
        Transform parentTransform = transform.parent;

        // Instantiate the confetti effect
        Instantiate(confettiEffectPrefab, transform.position, Quaternion.identity);


        // Start a coroutine to play audio after a delay
        StartCoroutine(PlayAudioAfterDelay(parentTransform.tag, 1f));
    }

    private IEnumerator PlayAudioAfterDelay(string tag, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Play the audio for the balloon's tag
        ST_AudioManager.Instance.PlayAudioAfterDestroy(tag);

        // Destroy the balloon gameObject
        Destroy(gameObject);
    }
}
