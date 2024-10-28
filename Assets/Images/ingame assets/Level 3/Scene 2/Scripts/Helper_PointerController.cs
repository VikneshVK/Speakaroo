using System.Collections;
using TMPro;
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

    public GameObject glowPrefab;
    public GameObject glowInstance;

    public TextMeshProUGUI subtitleText;

    private GameObject helperHandInstance;
    private bool hasSpawnedHelperHand = false;
    public drag_Toys[] toys = new drag_Toys[3];
    private bool isToyDragging = false;
    private int currentToyIndex = 0;
    private bool shouldRestartTimer = true;

    public GameObject bird;
    private Animator birdAnimator;

    private AudioSource audioSource;
    private AudioClip audioClip1;
    private AudioClip audioClip2;
    private bool hasPlayedAudioClip2 = false;

    void Start()
    {
        toys = FindObjectsOfType<drag_Toys>();
        toys[0] = GameObject.FindWithTag("Teddy").GetComponent<drag_Toys>();
        toys[1] = GameObject.FindWithTag("Dino").GetComponent<drag_Toys>();
        toys[2] = GameObject.FindWithTag("Bunny").GetComponent<drag_Toys>();

        birdAnimator = bird.GetComponent<Animator>();
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
        float halfDelay = delayBeforeHelp / 2f; // Halfway point (5 seconds if delayBeforeHelp is 10)

        yield return new WaitForSeconds(halfDelay);

        Vector3 targetPosition;
        if (tapControl.isFirstTime)
        {
            Collider2D tapCollider = tapControl.GetComponent<Collider2D>();
            if (tapCollider != null)
            {
                targetPosition = tapCollider.bounds.center;
            }
            else
            {
                Debug.LogError("Tap Collider not found on the TapControl object.");
                yield break;
            }
        }
        else
        {
            targetPosition = toys[currentToyIndex].transform.position;
        }

        PauseAndSpawnGlow(targetPosition);
        while (glowInstance != null)
        {
            yield return null;
        }


        yield return new WaitForSeconds(halfDelay);


        if (tapControl.isFirstTime && !hasSpawnedHelperHand)
        {
            SpawnHelperHand(offscreenPosition.position, true);
            StartHelperHandTweenLoop();
        }

        StartCoroutine(CheckToyInteraction());
    }


    public void PauseAndSpawnGlow(Vector3 targetPosition)
    {
        if (glowInstance == null)
        {
            glowInstance = Instantiate(glowPrefab, targetPosition, Quaternion.identity);

            LeanTween.scale(glowInstance, Vector3.one * 8, 1f).setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() =>
                {
                    StartCoroutine(WaitAndScaleDownGlow());
                });
        }
    }

    private IEnumerator WaitAndScaleDownGlow()
    {
        yield return new WaitForSeconds(2f);

        LeanTween.scale(glowInstance, Vector3.zero, 1f).setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                Destroy(glowInstance);
                glowInstance = null;
                shouldRestartTimer = true;
            });
    }

    public void SpawnHelperHand(Vector3 startPosition, bool isTapPosition)
    {
        if (helperHandInstance == null)
        {
            helperHandInstance = Instantiate(helperHandPrefab, startPosition, Quaternion.identity);
            hasSpawnedHelperHand = true;
            birdAnimator.SetTrigger("helper");

            if (isTapPosition && audioClip1 != null)
            {
                audioSource.clip = audioClip1;
                StartCoroutine(RevealTextWordByWord("Friend, open the tap", 0.5f));
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

            if (!hasPlayedAudioClip2 && audioClip2 != null)
            {
                audioSource.clip = audioClip2;
                audioSource.Play();
                StartCoroutine(RevealTextWordByWord("Now show your toys under the water", 0.5f));
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
                if (glowInstance != null)
                {
                    Destroy(glowInstance);
                    glowInstance = null;
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
            hasPlayedAudioClip2 = false;
        }

        if (glowInstance != null)
        {
            Destroy(glowInstance);
            glowInstance = null;
        }
    }

    public void ResetAndRestartTimer(bool isToyInteraction = false)
    {
        if ((shouldRestartTimer && !hasSpawnedHelperHand) || isToyInteraction)
        {
            StopAllCoroutines();
            StartCoroutine(StartHelperHandSequence());
            ResetHelperHand();
        }
    }

    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(true);

        string[] words = fullText.Split(' ');

        // Reveal words one by one
        for (int i = 0; i < words.Length; i++)
        {
            subtitleText.text = string.Join(" ", words, 0, i + 1);
            yield return new WaitForSeconds(delayBetweenWords);
        }
        subtitleText.text = "";
    }
}
