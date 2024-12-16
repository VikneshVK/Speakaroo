using UnityEngine;

public class ThresholdCollider : MonoBehaviour
{
    public SentenceManager_CameraMove sentenceManager;

    // Trigger the event when another collider enters
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the background has entered the trigger area
        if (other.CompareTag("Sentence_BG")) // Ensure your background has the "Background" tag
        {
            Debug.Log("Collider hit");
            sentenceManager.HandleThresholdTrigger();
        }
    }
}
