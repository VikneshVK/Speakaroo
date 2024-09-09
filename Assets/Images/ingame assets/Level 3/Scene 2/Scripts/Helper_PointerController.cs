using System.Collections;
using UnityEngine;

public class Helper_PointerController : MonoBehaviour
{
    public GameObject helperHandPrefab;
    public ParticleSystem waterParticleSystem;
    public float delayBeforeHelp = 2f;
    public float toyDragDelay = 10f;
    public Transform tapPosition;
    public Transform offscreenPosition;
    public TapControl tapControl;

    private GameObject helperHandInstance;
    private bool hasSpawnedHelperHand = false;
    public drag_Toys[] toys = new drag_Toys[3];
    private bool isToyDragging = false;
    private int currentToyIndex = 0;
    private bool shouldRestartTimer = true;

    public GameObject bird;
    private Animator birdAnimator;

    // AudioSource and AudioClip references
    private AudioSource audioSource;
    private AudioClip audioClip1;
    private AudioClip audioClip2;

    // Flag to ensure audioClip2 only plays once
    private bool hasPlayedAudioClip2 = false;

    void Start()
    {
        // Find toy references
        toys = FindObjectsOfType<drag_Toys>();
        toys[0] = GameObject.FindWithTag("Teddy").GetComponent<drag_Toys>();
        toys[1] = GameObject.FindWithTag("Dino").GetComponent<drag_Toys>();
        toys[2] = GameObject.FindWithTag("Bunny").GetComponent<drag_Toys>();

        // Find bird animator
        birdAnimator = bird.GetComponent<Animator>();

        // Load AudioSource and AudioClips from Resources
        audioSource = GetComponent<AudioSource>();
        audioClip1 = Resources.Load<AudioClip>("audio/Lvl3sc2/Friend, open the tap");
        audioClip2 = Resources.Load<AudioClip>("audio/Lvl3sc2/Now show your toys under the water");
    }

    public void EnableCollidersAndStartTimer()
    {
        if (shouldRestartTimer)
        {
            StartCoroutine(StartHelperHandSequence());
        }
    }

    private IEnumerator StartHelperHandSequence()
    {
        yield return new WaitForSeconds(delayBeforeHelp);

        if (tapControl.isFirstTime && !hasSpawnedHelperHand)
        {
            SpawnHelperHand(offscreenPosition.position, true); // true indicates tap position
            StartHelperHandTweenLoop();
        }

        StartCoroutine(CheckToyInteraction());
    }

    // Modify SpawnHelperHand to accept a bool that differentiates tap and toy spawning
    public void SpawnHelperHand(Vector3 startPosition, bool isTapPosition)
    {
        if (helperHandInstance == null)
        {
            helperHandInstance = Instantiate(helperHandPrefab, startPosition, Quaternion.identity);
            hasSpawnedHelperHand = true;
            birdAnimator.SetTrigger("helper");

            // Play audioClip1 if helper hand spawns for tap position
            if (isTapPosition && audioClip1 != null)
            {
                audioSource.clip = audioClip1;
                audioSource.Play();
            }
        }
    }

    private void StartHelperHandTweenLoop()
    {
        if (helperHandInstance != null)
        {
            TweenHelperHandToTapPosition();
        }
    }

    public void TweenHelperHandToTapPosition()
    {
        if (helperHandInstance != null && tapControl != null)
        {
            Collider2D tapCollider = tapControl.GetComponent<Collider2D>();

            if (tapCollider != null)
            {
                Vector3 targetPosition = tapCollider.bounds.center;

                Debug.Log($"Tweening helper hand from {offscreenPosition.position} to {targetPosition}");

                LeanTween.move(helperHandInstance, targetPosition, 1f)
                         .setEase(LeanTweenType.easeInOutQuad)
                         .setOnComplete(() =>
                         {
                             helperHandInstance.transform.position = offscreenPosition.position;
                             TweenHelperHandToTapPosition();
                         });
            }
            else
            {
                Debug.LogError("Tap Collider not found on the TapControl object.");
            }
        }
    }

    public void TweenHelperHandToParticlesPosition(int toyIndex)
    {
        if (helperHandInstance != null && toyIndex >= 0 && toyIndex < toys.Length)
        {
            Debug.Log($"Tweening helper hand for toy index: {toyIndex} - {toys[toyIndex].gameObject.name}");

            GameObject toy = toys[toyIndex].gameObject;
            Vector3 startPosition = toy.transform.position;

            // Play audioClip2 only once when the helper hand is spawned for a toy
            if (!hasPlayedAudioClip2 && audioClip2 != null)
            {
                audioSource.clip = audioClip2;
                audioSource.Play();
                hasPlayedAudioClip2 = true; // Mark that audioClip2 has been played
            }

            LeanTween.move(helperHandInstance, waterParticleSystem.transform.position, 1f)
                     .setEase(LeanTweenType.easeInOutQuad)
                     .setOnComplete(() =>
                     {
                         Debug.Log($"Completed tween for {toy.name}. Resetting helper hand to start position.");

                         helperHandInstance.transform.position = startPosition;
                         TweenHelperHandToParticlesPosition(toyIndex);
                     });
        }
        else
        {
            Debug.LogError($"Helper hand tween error: Invalid toy index {toyIndex} or helper hand instance is null.");
        }
    }

    private IEnumerator CheckToyInteraction()
    {
        while (currentToyIndex < toys.Length)
        {
            yield return new WaitForSeconds(toyDragDelay);

            if (!toys[currentToyIndex].GetComponent<Collider2D>().enabled)
            {
                Debug.Log($"{toys[currentToyIndex].gameObject.name} collider disabled, skipping.");
                currentToyIndex++;
                continue;
            }

            isToyDragging = toys[currentToyIndex].isDragging;

            if (isToyDragging)
            {
                if (hasSpawnedHelperHand)
                {
                    ResetHelperHand();
                }
            }
            else
            {
                if (!toys[currentToyIndex].IsInteracted() && !hasSpawnedHelperHand)
                {
                    SpawnHelperHand(toys[currentToyIndex].transform.position, false); // false for toy
                    TweenHelperHandToParticlesPosition(currentToyIndex);
                }
            }

            currentToyIndex++;
        }

        StopAllCoroutines();
    }

    public void ResetHelperHand()
    {
        StopAllCoroutines();

        if (helperHandInstance != null)
        {
            Destroy(helperHandInstance);
            helperHandInstance = null;
            hasSpawnedHelperHand = false;
            shouldRestartTimer = false;
        }
    }

    public void ResetAndRestartTimer(bool isToyInteraction = false)
    {
        if ((shouldRestartTimer && !hasSpawnedHelperHand) || isToyInteraction)
        {
            StopAllCoroutines();
            StartCoroutine(StartHelperHandSequence());
        }
    }
}
