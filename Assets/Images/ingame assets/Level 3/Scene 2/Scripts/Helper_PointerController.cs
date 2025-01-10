using System.Collections;
using TMPro;
using UnityEngine;

public class Helper_PointerController : MonoBehaviour
{
    public GameObject helperHandPrefab;
    public GameObject helperHandPrefab2;
    public ParticleSystem waterParticleSystem;
    public float delayBeforeHelp = 2f;
    public float toyDragDelay = 10f;
    public Transform tapPosition;
    public Transform offscreenPosition;
    public TapControl tapControl;
    public SubtitleManager subtitleManager;
    public GameObject glowPrefab;
    private GameObject glowInstance;

    private GameObject helperHandInstance;
    private bool hasSpawnedHelperHand = false;
    private bool shouldRestartTimer = true;

    public GameObject bird;
    private Animator birdAnimator;

    private AudioSource audioSource;
    private AudioClip audioClip1;
    private AudioClip audioClip2;
    private bool hasPlayedAudioClip2 = false;

    private drag_Toys[] toys;

    void Start()
    {
        // Manually assign toys to ensure a specific order
        toys = new drag_Toys[3];
        toys[0] = GameObject.FindWithTag("Teddy").GetComponent<drag_Toys>(); // Teddy
        toys[1] = GameObject.FindWithTag("Dino").GetComponent<drag_Toys>();   // Dino
        toys[2] = GameObject.FindWithTag("Bunny").GetComponent<drag_Toys>();  // Bunny

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
        float halfDelay = delayBeforeHelp / 2f;

        yield return new WaitForSeconds(halfDelay);

        if (tapControl.isFirstTime)
        {
            Vector3 targetPosition = GetColliderCenter(tapControl);
            if (targetPosition == Vector3.zero) yield break;

            PauseAndSpawnGlow(targetPosition);
            while (glowInstance != null)
            {
                yield return null;
            }

            yield return new WaitForSeconds(halfDelay);

            if (!hasSpawnedHelperHand)
            {
                SpawnHelperHand2(offscreenPosition.position, true);
                /*StartHelperHandTweenLoop(targetPosition);*/
            }
        }
    }

    private Vector3 GetColliderCenter(Component obj)
    {
        Collider2D collider = obj.GetComponent<Collider2D>();
        if (collider != null)
        {
            return collider.bounds.center;
        }
        else
        {
            Debug.LogError("Collider not found on the object.");
            return Vector3.zero;
        }
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
                audioSource.Play();
            }
        }
    }

    public void SpawnHelperHand2(Vector3 startPosition, bool isTapPosition)
    {
        if (helperHandInstance == null)
        {
            helperHandInstance = Instantiate(helperHandPrefab2, startPosition, Quaternion.identity);
            hasSpawnedHelperHand = true;
            birdAnimator.SetTrigger("helper");

            if (isTapPosition && audioClip1 != null)
            {
                audioSource.clip = audioClip1;
                subtitleManager.DisplaySubtitle("Friend, open the tap.", "Kiki", audioSource.clip);                
                audioSource.Play();
            }
        }
    }

    private void StartHelperHandTweenLoop(Vector3 targetPosition)
    {
        if (helperHandInstance != null)
        {
            LeanTween.move(helperHandInstance, targetPosition, 1f)
                     .setEase(LeanTweenType.easeInOutQuad)
                     .setOnComplete(() =>
                     {
                         helperHandInstance.transform.position = offscreenPosition.position;
                         StartHelperHandTweenLoop(targetPosition);
                     });
        }
    }

    public void TweenHelperHandToParticlesPosition(Vector3 startPosition)
    {
        if (helperHandInstance != null)
        {
            if (!hasPlayedAudioClip2 && audioClip2 != null)
            {
                audioSource.clip = audioClip2;
                audioSource.Play();
                subtitleManager.DisplaySubtitle("Now show your toys under the water.", "Kiki", audioSource.clip);                
                hasPlayedAudioClip2 = true;
            }

            LeanTween.move(helperHandInstance, waterParticleSystem.transform.position, 1f)
                     .setEase(LeanTweenType.easeInOutQuad)
                     .setOnComplete(() =>
                     {
                         helperHandInstance.transform.position = startPosition;
                         TweenHelperHandToParticlesPosition(startPosition);
                     });
        }
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

    public void ResetAndRestartTimer()
    {
        if (shouldRestartTimer && !hasSpawnedHelperHand)
        {
            StopAllCoroutines();
            StartCoroutine(StartHelperHandSequence());
            ResetHelperHand();
        }
    }

    public void EnableHelperHandForToy(int toyIndex)
    {
        if (toyIndex < 0 || toyIndex >= toys.Length)
        {
            Debug.LogError("Invalid toy index passed to EnableHelperHandForToy.");
            return;
        }

        GameObject currentToy = toys[toyIndex].gameObject;
        Debug.Log("Helper hand enabled for toy: " + currentToy.name);

        StartCoroutine(StartHelperHandForToy(currentToy));
    }

    private IEnumerator StartHelperHandForToy(GameObject toy)
    {
        // Wait for the specified delay before showing the helper hand
        yield return new WaitForSeconds(toyDragDelay);

        drag_Toys toyScript = toy.GetComponent<drag_Toys>();

        // Check if the toy has been interacted with
        if (!toyScript.IsInteracted())
        {            
            SpawnHelperHand(toy.transform.position, false);
            TweenHelperHandToParticlesPosition(toy.transform.position);
        }
    }    
}
