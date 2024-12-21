using TMPro;
using UnityEngine;
using System.Collections;

public class ShowerMechanics : MonoBehaviour
{
    public Animator hotTapAnimator, boyAnimator, birdAnimator;
    public ParticleSystem showerParticles;
    public GameObject prefabToSpawn;
    public Transform spawnLocation;
    public GameObject shampooGameObject; // Reference to the shampoo GameObject
    public GameObject HotTap;
    public ShampooController ShampooController;

    [Header("Audio")]
    public Lvl1Sc1AudioManager Audiomanager; // Reference to the audio source
    public AudioClip audio1; // Audio clip for the "Talk sample" animation
    public AudioClip audio2;
    public AudioClip audio3;
    public AudioClip audio4;
    public AudioClip audio5;
    public AudioClip audio6;
    public AudioClip audio7;
    public AudioClip audio8;
    public AudioClip sfxAudio1;
    public TextMeshProUGUI subtitleText;

    [Header("Helper Timer")]
    public Lvl1Sc2HelperFunction helperFunctionScript; // Reference to the helper function script

    public bool hotTapOn = false;
    private bool hasSpawned = false;
    private bool hasTalkStarted = false; // To track if the "Talk sample" animation started
    private bool hasTalkEnded = false; // To track if the "Talk sample" animation ended
    private bool hasGiggleStarted = false;
    private bool hasAskKikiStarted = false;
    private bool hasShowerDoneStarted = false;
    private bool birdanimation = false;
    private bool allowTapInteraction = true;
    private bool colliderEnabled = false;
    private bool handReachingAudioPlayed = false;
    private AudioSource SfxAudioSoure;

    private void Awake()
    {
        SfxAudioSoure = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
    }
    void Update()
    {
        if (allowTapInteraction)
        {
            HandleTapInput();
        }

        CheckAnimationAndSpawnPrefab();
        CheckTalkSampleAnimation();
        CheckGiggleAnimation();
        CheckAskKikiAnimation();
        CheckBirdAniamtion();
        CheckShowerDoneAnimation();
        CheckHandReachingAnimation();
        CheckShampooAnimation();
    }

    void HandleTapInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag("Hot Tap"))
            {
                helperFunctionScript.ResetTimer();
                HotTap.GetComponent<Collider2D>().enabled = false;
                ToggleTap();
            }
        }
    }

    public void DisableTapInteraction()
    {
        allowTapInteraction = false; // Disable tap interaction
    }
    void ToggleTap()
    {
        hotTapOn = !hotTapOn;
        hotTapAnimator.SetTrigger(hotTapOn ? "TapOn" : "TapOff");
        boyAnimator.SetBool("IsNormal", hotTapOn);

        if (hotTapOn)
        {
            showerParticles.Play(); // Turn on shower
            if (SfxAudioSoure != null)
            {
                SfxAudioSoure.clip = sfxAudio1;
                SfxAudioSoure.loop = true;
                SfxAudioSoure.Play();
            }

            // Disable the collider when the "Tap On" animation starts playing
            HotTap.GetComponent<Collider2D>().enabled = false;

            // Start coroutine to trigger "close tap" after 2 seconds
            StartCoroutine(TriggerCloseTapAfterDelay());
        }
        else
        {
            // Disable collider immediately when "Tap Off" animation starts playing
            HotTap.GetComponent<Collider2D>().enabled = false;

            showerParticles.Stop(); // Turn off shower
            if (SfxAudioSoure != null)
            {
                SfxAudioSoure.loop = false;
                SfxAudioSoure.Stop();
            }

            // Do not enable the collider until the "Tap Off" animation finishes
        }
    }


   

    private IEnumerator TriggerCloseTapAfterDelay()
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds after tap is turned on

        // Trigger "close tap" animation
        if (birdAnimator != null)
        {
            birdAnimator.SetTrigger("close tap");
            Audiomanager.PlayAudio(audio6);
            StartCoroutine(RevealTextWordByWord("Close the Tap", 0.5f));
        }

        // Wait until bird animation is done
        yield return new WaitForSeconds(1.5f);

        // Re-enable the collider after the "close tap" animation finishes
        HotTap.GetComponent<Collider2D>().enabled = true;
    }


    void CheckHandReachingAnimation()
    {
        AnimatorStateInfo stateInfo = boyAnimator.GetCurrentAnimatorStateInfo(0);

        // Check if "Hand Reaching" animation has started
        if (stateInfo.IsName("hand reaching") && stateInfo.normalizedTime < 0.1f && !handReachingAudioPlayed)
        {
            handReachingAudioPlayed = true;
            Audiomanager.PlayAudio(audio7);
        }
    }
    void CheckAnimationAndSpawnPrefab()
    {
        AnimatorStateInfo stateInfo = boyAnimator.GetCurrentAnimatorStateInfo(0);
        Collider2D hotCollider = HotTap.GetComponent<Collider2D>();

        // Enable tap collider when "Idle 1" animation finishes
        if (stateInfo.IsName("Idle 1") && stateInfo.normalizedTime >= 1.0f && !colliderEnabled)
        {
            colliderEnabled = true;
            hotCollider.enabled = true;
        }

        // Spawn prefab when "Ask Kiki" animation finishes
        if (stateInfo.IsName("Ask Kiki") && stateInfo.normalizedTime >= 0.95f && !hasSpawned)
        {
            SpawnPrefab();
            hasSpawned = true;
        }
    }

    void CheckTalkSampleAnimation()
    {
        AnimatorStateInfo stateInfo = boyAnimator.GetCurrentAnimatorStateInfo(0);

        // Check if the "Talk sample" animation has just started
        if (stateInfo.IsName("Talk sample") && stateInfo.normalizedTime < 0.1f && !hasTalkStarted)
        {
            hasTalkStarted = true;
            Audiomanager.PlayAudio(audio1);
            StartCoroutine(RevealTextWordByWord("Let's take a Shower", 0.5f));
        }

        // Check if the "Talk sample" animation has just ended
        if (stateInfo.IsName("Talk sample") && stateInfo.normalizedTime >= 1.0f && !hasTalkEnded)
        {
            hasTalkEnded = true;
            birdAnimator.SetTrigger("open tap");
            Audiomanager.PlayAudio(audio5);
            StartCoroutine(RevealTextWordByWord("Open the Tap", 0.5f));
            StartHelperTimerForTap();
        }

        // Reset flags when animation changes
        if (!stateInfo.IsName("Talk sample"))
        {
            hasTalkStarted = false;
            hasTalkEnded = false;
        }
    }
    void CheckAskKikiAnimation()
    {
        AnimatorStateInfo stateInfo = boyAnimator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Ask Kiki") && stateInfo.normalizedTime < 0.1f && !hasAskKikiStarted)
        {
            hasAskKikiStarted = true;
            Audiomanager.PlayAudio(audio2);
            StartCoroutine(RevealTextWordByWord("Uh..! I can't reach it, Lets ask Kiki", 0.5f));
        }
    }

    void CheckBirdAniamtion()
    {
        AnimatorStateInfo stateInfo = birdAnimator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("birdKnock") && stateInfo.normalizedTime < 0.1f && !birdanimation)
        {
            birdanimation = true;
            Audiomanager.PlayAudio(audio4);
            StartCoroutine(RevealTextWordByWord("Here you go...! JoJo ", 0.5f));
        }

        if (stateInfo.IsName("birdKnock") && stateInfo.normalizedTime >= 0.9f && birdanimation)
        {
            birdanimation = false;
            StartCoroutine(TriggerShampooAfterDelay());
        }

        CheckShampooAnimation(); // Check for completion of "shampoo" animation
    }

    private IEnumerator TriggerShampooAfterDelay()
    {
        yield return new WaitForSeconds(2f);

        // Trigger "shampoo" parameter on the bird animator
        if (birdAnimator != null)
        {
            birdAnimator.SetTrigger("shampoo");
            Audiomanager.PlayAudio(audio8);
            StartCoroutine(RevealTextWordByWord("Put the Shampoo", 0.5f));
        }
    }

    private void CheckShampooAnimation()
    {
        AnimatorStateInfo stateInfo = birdAnimator.GetCurrentAnimatorStateInfo(0);

        // Check if "shampoo" animation is complete
        if (stateInfo.IsName("shampoo") && stateInfo.normalizedTime >= 0.9f)
        {
            ShampooController.isDraggable = true;
        }
    }

    void CheckGiggleAnimation()
    {
        AnimatorStateInfo stateInfo = boyAnimator.GetCurrentAnimatorStateInfo(0);

        // Check if "Giggle1" or "Giggle" animation has just started
        if ((stateInfo.IsName("Giggle1") || stateInfo.IsName("Giggle")) && stateInfo.normalizedTime < 0.1f && !hasGiggleStarted)
        {
            hasGiggleStarted = true;
            Debug.Log("Giggle animation started. hasGiggleStarted set to true.");
            StartHelperTimerForTap();
        }

        // Reset flag when animation changes
        if (!stateInfo.IsName("Giggle1") && !stateInfo.IsName("Giggle"))
        {
            if (hasGiggleStarted)
            {
                Debug.Log("Giggle animation ended. hasGiggleStarted reset to false.");
            }
            hasGiggleStarted = false;
        }
    }

    void CheckShowerDoneAnimation()
    {
        AnimatorStateInfo stateInfo = boyAnimator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Shower Done") && stateInfo.normalizedTime < 0.5f && !hasShowerDoneStarted)
        {
            hasShowerDoneStarted = true;
            Audiomanager.PlayAudio(audio3);
            StartCoroutine(RevealTextWordByWord("All Clean..! Let's get Dressed", 0.5f));
        }
    }


    void StartHelperTimerForTap()
    {
        if (helperFunctionScript != null)
        {
            helperFunctionScript.StartTimer(true); // Start the helper timer for the tap
        }
    }

    void SpawnPrefab()
    {
        HotTap.GetComponent<Collider2D>().enabled = false;
        Instantiate(prefabToSpawn, spawnLocation.position, Quaternion.identity);
        EnableShampooCollider(); // Enable the shampoo's collider after spawning the prefab
    }

    void EnableShampooCollider()
    {
        Collider2D shampooCollider = shampooGameObject.GetComponent<Collider2D>();
        if (shampooCollider != null)
        {
            shampooCollider.enabled = true; // Enable the collider
        }
    }

    void OnDisable()
    {
        hasSpawned = false; // Reset the spawning flag
    }

    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(true);

        string[] words = fullText.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            subtitleText.text = string.Join(" ", words, 0, i + 1);
            yield return new WaitForSeconds(delayBetweenWords);
        }
        subtitleText.text = "";
    }
}
