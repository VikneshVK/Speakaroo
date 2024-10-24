using TMPro;
using UnityEngine;
using System.Collections;

public class HelpPointerManager : MonoBehaviour
{
    public static HelpPointerManager Instance;
    public static bool IsAnyObjectBeingInteracted = false;

    public GameObject pointer;  // Helper hand pointer
    public GameObject glowPrefab;      // Glow object to be assigned in inspector
    public TextMeshProUGUI subtitleText;  // Reference to the TMP text component
    private AudioSource audioSource;

    // New flag to track if any help is active
    private bool isHelpActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();  // Get the AudioSource component attached to the game object
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnGlowEffect(GameObject targetObject)
    {
        if (isHelpActive || glowPrefab == null) return;

        // Instantiate the glow prefab at the object's position
        GameObject spawnedGlow = Instantiate(glowPrefab, targetObject.transform.position, Quaternion.identity);

        LeanTween.scale(spawnedGlow, new Vector3(8, 8, 8), 0.5f)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnComplete(() =>
            {
                StartCoroutine(WaitAndDestroyGlow(spawnedGlow));
            });

        Debug.Log("Glow prefab spawned for " + targetObject.name);
    }

    private IEnumerator WaitAndDestroyGlow(GameObject glow)
    {
        yield return new WaitForSeconds(1f);  // Wait for 1 second
        LeanTween.scale(glow, Vector3.zero, 0.5f)
                 .setEase(LeanTweenType.easeInOutSine)
                 .setOnComplete(() =>
                 {
                     Destroy(glow);  // Destroy the glow object after scaling down
                 });
    }


    public void CheckAndShowHelpForObject(GameObject reactivatedObject, float delayBeforeHelp)
    {
        if (isHelpActive) return;

        InteractableObject interactable = reactivatedObject.GetComponent<InteractableObject>();
        if (interactable != null && !interactable.isInteracted)
        {
            
            pointer.transform.position = interactable.transform.position;
            pointer.SetActive(true);

            StartCoroutine(HandleHelpPointer(interactable, delayBeforeHelp));

            SpawnGlowEffect(reactivatedObject);  // Spawn glow when help is triggered

            if (subtitleText != null)
            {
                StartCoroutine(RevealTextWordByWord("Put the Toys on the Shelf", 0.5f));
            }

            if (audioSource != null)
            {
                audioSource.Play();
                StartCoroutine(DisableSubtitleAfterAudio());
            }

            isHelpActive = true;
        }
    }

    private IEnumerator HandleHelpPointer(InteractableObject interactable, float delayBeforeHelp)
    {
        while (!interactable.isInteracted)  // Continue tweens until the object is interacted with
        {
            // Fetch the current drop position dynamically
            Vector3 dropPosition = interactable.GetDropLocation();

            // Tween the helper hand to the drop position
            LeanTween.move(pointer, dropPosition, 1f)
                .setEase(LeanTweenType.easeInOutSine)
                .setOnComplete(() =>
                {
                    // After reaching the drop position, teleport back to the object's position
                    pointer.transform.position = interactable.transform.position;
                });

            // Wait for the tween to finish before starting the next tween (1 second duration in this case)
            yield return new WaitForSeconds(1f);
        }

        // Once interacted, stop the pointer if needed
        StopHelpPointer();
    }


    // Coroutine to reveal text word by word
    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";  // Clear the text before starting
        subtitleText.gameObject.SetActive(true);  // Ensure the subtitle text is active

        string[] words = fullText.Split(' ');  // Split the full text into individual words

        // Reveal words one by one
        for (int i = 0; i < words.Length; i++)
        {
            // Instead of appending, build the text up to the current word
            subtitleText.text = string.Join(" ", words, 0, i + 1);  // Show only the words up to the current index
            yield return new WaitForSeconds(delayBetweenWords);  // Wait before revealing the next word
        }
    }

    // Coroutine to disable the subtitle after the audio finishes, with a 1-second delay
    private IEnumerator DisableSubtitleAfterAudio()
    {
        // Wait until the audio is done playing
        while (audioSource != null && audioSource.isPlaying)
        {
            yield return null;
        }

        // Wait an additional 1 second after the audio finishes
        yield return new WaitForSeconds(1f);

        // Disable the subtitle text
        if (subtitleText != null)
        {
            subtitleText.gameObject.SetActive(false);
        }
    }

    public void ClearHelpRequest(InteractableObject interactable)
    {
        if (pointer.activeInHierarchy && pointer.transform.position == interactable.transform.position)
        {
            StopHelpPointer();
        }
    }

    public void StopHelpPointer()
    {
        isHelpActive = false;  // Reset the help active flag
        LeanTween.cancel(pointer);  // Stop any ongoing tween
        pointer.SetActive(false);   // Deactivate the pointer

        if (audioSource != null)
        {
            audioSource.Stop();  // Stop the audio if the helper hand is deactivated
        }

        Debug.Log("Help pointer deactivated");
    }


}