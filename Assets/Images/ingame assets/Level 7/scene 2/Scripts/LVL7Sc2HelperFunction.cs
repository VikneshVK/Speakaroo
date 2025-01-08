using UnityEngine;

public class LVL7Sc2HelperFunction : MonoBehaviour
{
    [Header("Helper Settings")]
    public float helpTimer = 5f; // Timer duration
    public GameObject helperHandPrefab; // Reference to the helper hand prefab
    public Animator kikiAnimator;

    private float timer = 0f;
    private GameObject activeHelperHand;
    private bool hasPlayedAudio = false;

    // Variables to store spawn and end positions
    private Vector3 currentSpawnLocation;
    private Vector3 currentEndLocation;
    private AudioClip audioClip;   // Audio clip for the topping
    private string animationTrigger; // Animation trigger for the topping

    // Public method to start the timer
    public void StartTimer(Vector3 spawnLocation, Vector3 endLocation, AudioClip audioClip, string animationTrigger)
    {
        timer = 0f;
        currentSpawnLocation = spawnLocation;
        currentEndLocation = endLocation;
        this.audioClip = audioClip;            // Store the audio clip
        this.animationTrigger = animationTrigger; // Store the animation trigger
        InvokeRepeating(nameof(UpdateTimer), 0f, Time.deltaTime);
    }

    // Public method to reset the timer and destroy active helper hand
    public void ResetTimer()
    {
        timer = 0f;
        if (activeHelperHand != null)
        {
            Destroy(activeHelperHand);
        }
        CancelInvoke(nameof(UpdateTimer));
        hasPlayedAudio = false;
}

    // Update the timer
    private void UpdateTimer()
    {
        timer += Time.deltaTime;
        if (timer >= helpTimer)
        {
            CancelInvoke(nameof(UpdateTimer));
            SpawnAndTweenHelperHand(currentSpawnLocation, currentEndLocation);
        }
    }

    // Spawn and tween the helper hand
    private void SpawnAndTweenHelperHand(Vector3 spawnLocation, Vector3 endLocation)
    {
        if (helperHandPrefab == null) return;

        if (activeHelperHand == null)
        {
            activeHelperHand = Instantiate(helperHandPrefab, spawnLocation, Quaternion.identity);
        }

        // Play the animation on Kiki's Animator
        if (!string.IsNullOrEmpty(animationTrigger) && kikiAnimator != null && !hasPlayedAudio)
        {
            kikiAnimator.SetTrigger(animationTrigger);  // Trigger the animation for Kiki
        }

        // Play the audio clip only once
        if (audioClip != null && !hasPlayedAudio && activeHelperHand.GetComponent<AudioSource>() != null)
        {
            activeHelperHand.GetComponent<AudioSource>().PlayOneShot(audioClip);  // Play the sound for the helper hand
            hasPlayedAudio = true;  // Set the flag to true to prevent audio from playing again
        }

        // Loop tweening for the helper hand movement
        LeanTween.move(activeHelperHand, endLocation, 1f).setOnComplete(() =>
        {
            activeHelperHand.transform.position = spawnLocation; // Teleport back to start position
            SpawnAndTweenHelperHand(spawnLocation, endLocation); // Repeat the tweening (loop)
        });
    }

}
