using UnityEngine;
using System.Collections;
using TMPro;

public class LVL4Sc2HelperController : MonoBehaviour
{
    public static LVL4Sc2HelperController Instance { get; private set; }

    public GameObject helperHandPrefab;
    public GameObject glowPrefab;
    public TweeningController tweeningController;

    private GameObject spawnedHelperHand;
    private GameObject spawnedGlow;
    private Coroutine delayTimerCoroutine;
    public float helperHandDelay;

    public GameObject bird; // Reference to the bird GameObject
    public Transform birdEndPosition; // Target position for bird animation
    public AudioClip cerealAudio;
    public AudioClip milkAudio;
    public AudioClip cherryAudio;
    public LVL4Sc2AudioManager audioManager;

    public SubtitleManager subtitleManager;
    private Animator birdAnimator;
    private Vector3 birdInitialPosition;

   

    private void Start()
    {
        // Initialize bird's initial position and animator
        if (bird != null)
        {
            birdAnimator = bird.GetComponent<Animator>();
            birdInitialPosition = bird.transform.position;
        }
    }

    public void TriggerBirdSequenceForItem(string itemType)
    {
        AudioClip audioClip = null;
        string animationTrigger = "";
        string subtitle = "";

        // Set audio, animation trigger, and subtitles based on itemType
        switch (itemType)
        {
            case "Cereal":
                audioClip = cerealAudio;
                animationTrigger = "Cereal";
                subtitle = "Put the Cereal in the Bowl";
                break;
            case "Milk":
                audioClip = milkAudio;
                animationTrigger = "Milk";
                subtitle = "Pour the Milk in the Bowl";
                break;
            case "Cherry":
                audioClip = cherryAudio;
                animationTrigger = "Cherry";
                subtitle = "Now Add some Berries on the Top";
                break;
        }

        // Start bird tween and play animation, audio, and subtitles
        if (bird != null && birdEndPosition != null && audioClip != null && birdAnimator != null)
        {
            LeanTween.move(bird, birdEndPosition.position, 1f)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() =>
                {
                    birdAnimator.SetTrigger(animationTrigger);
                    PlayAudioAndSubtitle(audioClip, subtitle);

                    // Tween back to original position after the animation and audio complete
                    StartCoroutine(TweenBirdBackAfterDelay(3f)); // Adjust delay as needed for audio duration
                });
        }
    }

    public void StartDelayTimer()
    {
        if (delayTimerCoroutine != null)
        {
            StopCoroutine(delayTimerCoroutine);
        }
        delayTimerCoroutine = StartCoroutine(DelayTimerCoroutine());
    }

    private IEnumerator DelayTimerCoroutine()
    {
        float timer = 0f;
       /* bool glowSpawned = false;*/

        while (timer < helperHandDelay)
        {
            timer += Time.deltaTime;

            /*// At halfway, trigger the glow effect on the next required item
            if (timer >= helperHandDelay / 2 && !glowSpawned)
            {
                glowSpawned = true;
                SpawnGlowOnNextItem();
            }*/

            yield return null;
        }

        // After the full delay, spawn the helper hand
        SpawnHelperHand();
    }

    public void StopAndResetTimer()
    {
        if (delayTimerCoroutine != null)
        {
            StopCoroutine(delayTimerCoroutine);
            DestroySpawnedHelperHand();  // Stop any active helper hand
            DestroySpawnedGlow();  // Destroy any active glow
        }
        
    }

    private void SpawnGlowOnNextItem()
    {
        GameObject nextItem = FindNextRequiredItem();
        if (nextItem != null)
        {
            SpawnGlowEffect(nextItem);
        }
    }

    private void SpawnHelperHand()
    {
        GameObject nextItem = FindNextRequiredItem();
        Transform destination = GameObject.FindGameObjectWithTag("EmptyBowl")?.transform;

        if (nextItem != null && destination != null)
        {
            // Spawn the helper hand to guide the player
            SpawnHelperHand(nextItem, destination);

            // Trigger the bird sequence based on the item type
            if (nextItem.CompareTag("Cereal"))
            {
                TriggerBirdSequenceForItem("Cereal");
            }
            else if (nextItem.CompareTag("Milk"))
            {
                TriggerBirdSequenceForItem("Milk");
            }
            else if (nextItem.CompareTag("Cherry"))
            {
                TriggerBirdSequenceForItem("Cherry");
            }
        }
    }

    private GameObject FindNextRequiredItem()
    {
        // Logic to determine the next required item based on game state
        if (!DraggingController1.isCerealDropped) return GameObject.FindGameObjectWithTag("Cereal");
        if (!DraggingController1.isMilkDropped) return GameObject.FindGameObjectWithTag("Milk");
        if (!DraggingController1.isCherryDropped) return GameObject.FindGameObjectWithTag("Cherry");

        return null;
    }

    public void SpawnGlowEffect(GameObject targetItem)
    {
        if (glowPrefab == null || targetItem == null) return;

        if (spawnedGlow != null)
        {
            Destroy(spawnedGlow);
        }

        spawnedGlow = Instantiate(glowPrefab, targetItem.transform.position, Quaternion.identity);
        LeanTween.scale(spawnedGlow, Vector3.one * 20, 0.5f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
        {
            StartCoroutine(DestroyGlowAfterDelay());
        });
    }

    private IEnumerator DestroyGlowAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        // Check if spawnedGlow still exists before attempting to scale it down
        if (spawnedGlow != null)
        {
            LeanTween.scale(spawnedGlow, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
            {
                Destroy(spawnedGlow);
                spawnedGlow = null;
            });
        }
    }

    public void SpawnHelperHand(GameObject targetItem, Transform destination)
    {
        if (helperHandPrefab == null || destination == null) return;

        DestroySpawnedHelperHand();
        spawnedHelperHand = Instantiate(helperHandPrefab, targetItem.transform.position, Quaternion.identity);
        LeanTween.move(spawnedHelperHand, destination.position, 1f).setLoopClamp();
    }

    public void DestroySpawnedHelperHand()
    {
        if (spawnedHelperHand != null)
        {
            Destroy(spawnedHelperHand);
            spawnedHelperHand = null;
        }
    }
    private void DestroySpawnedGlow()
    {
        if (spawnedGlow != null)
        {
            Destroy(spawnedGlow);
            spawnedGlow = null;
        }
    }

    private void PlayAudioAndSubtitle(AudioClip audioClip, string subtitleTextContent)
    {
        audioManager.PlayAudio(audioClip);
        subtitleManager.DisplaySubtitle(subtitleTextContent, "Kiki", audioClip);
    }

    private IEnumerator TweenBirdBackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Tween back to initial position
        LeanTween.move(bird, birdInitialPosition, 1f)
            .setEase(LeanTweenType.easeInOutQuad);
    }
}
