using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class Jojo_action1 : MonoBehaviour
{
    public Transform stopPosition;
    public Transform offScreenPosition;
    public float walkSpeed = 2f;
    public bool hasReachedOffScreen = false; // New boolean to track off-screen position
    public List<GameObject> objectsToEnable;
    /*public static event Action<GameObject[]> OnCollidersEnabled;*/
    public Lvl3Sc3DragManager dragmanager;
    public GameObject clothStand;
    public GameObject sun;
    public GameObject farm;
    public GameObject glowPrefab;

    private Animator jojoAnimator;
    public Animator kikiAnimator;
    private SpriteRenderer jojoSprite;

    public TextMeshProUGUI subtitleText;

    // AudioSource and AudioClips
    private AudioSource audioSource;
    private AudioClip audioClip1;
    private AudioClip audioClip2;

    private bool isWalking = false;
    private bool positionreached = false;
    private bool audioPlayed = false;
   /* private bool isIdleCompleted = false;*/
    private bool collidersEnabled = false; // To ensure colliders are enabled only once
    private bool isReturning = false; // Flag for return trip
    private bool isTalk2Triggered = false; // Flag to track when canTalk2 is triggered
    private bool subtitleTriggered = false;

    private Vector3 targetPosition; // Store the current target position

    void Start()
    {
        jojoAnimator = GetComponent<Animator>();
        jojoSprite = GetComponent<SpriteRenderer>();

        // Get the AudioSource and load the audio clips from Resources
        audioSource = GetComponent<AudioSource>();
        audioClip1 = Resources.Load<AudioClip>("audio/lvl3sc3/Looks like some clothes are still wet");
        audioClip2 = Resources.Load<AudioClip>("audio/lvl3sc3/_You are right, Kiki. It is evening now the clothes and toys are dry");
    }

    void Update()
    {
        if (isWalking)
        {
            WalkToPosition(targetPosition);
        }
        else if (!isReturning && !positionreached)
        {
            MoveToStopPosition();
        }

        if (dragmanager.levelComplete && !audioPlayed)
        {
            StartCoroutine(HandleLevelCompleteActionsWithDelay()); 
        }
    }

    private IEnumerator HandleLevelCompleteActionsWithDelay()
    {
        yield return new WaitForSeconds(1f); 

        jojoAnimator.SetBool("allDryed", true);
        kikiAnimator.SetTrigger("allDryed");

        AudioSource farmaudio = farm.GetComponent<AudioSource>();
        if (farmaudio != null)
        {
            farmaudio.Play();
        }
        if (!subtitleTriggered) 
        {
            subtitleTriggered = true;
            StartCoroutine(RevealTextWordByWord("Woo Hoo..! the Laundry is done. lets take it in", 0.5f)); 
        }        
        audioPlayed = true;
    }


    public void MoveToStopPosition()
    {
        jojoSprite.flipX = false;
        targetPosition = stopPosition.position;
        isWalking = true;
        jojoAnimator.SetBool("canWalk", true);
        isReturning = false;
    }

    public void MoveOffScreen()
    {
        targetPosition = offScreenPosition.position;
        jojoSprite.flipX = true;
        jojoAnimator.SetBool("canWalk", true);
        isWalking = true;
    }

    public void ReturnToStopPosition()
    {
        jojoSprite.flipX = false;
        isReturning = true;
        targetPosition = stopPosition.position;
        jojoAnimator.SetBool("canWalk", true);
        isWalking = true;
        isTalk2Triggered = true; // Set flag to trigger canTalk2 when returning to stop position
    }

    private void WalkToPosition(Vector3 targetPosition)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
        {
            isWalking = false; // Stop walking
            jojoAnimator.SetBool("canWalk", false);

            if (targetPosition == offScreenPosition.position)
            {
                hasReachedOffScreen = true; // Reached off-screen

            }
            else if (targetPosition == stopPosition.position)
            {


                if (isTalk2Triggered)
                {
                    StartTalking("canTalk2"); // Trigger "canTalk2" if returning to stop position
                    isTalk2Triggered = false; // Reset flag
                }
                else
                {
                    StartTalking("canTalk"); // Regular talking trigger
                    positionreached = true;
                }

            }
        }
    }

    public void StartTalking(string talkTrigger)
    {
        // Switch to talkRig
        jojoAnimator.SetTrigger(talkTrigger); // Trigger the correct talk animation (canTalk or canTalk2)

        // Play the appropriate audio based on the trigger
        if (talkTrigger == "canTalk" && audioClip1 != null)
        {
            audioSource.clip = audioClip1;
            audioSource.Play();
            StartCoroutine(RevealTextWordByWord("Looks like some clothes are still Wet", 0.5f));
            StartCoroutine(TriggerHelperAfterDialogue());
        }
        else if (talkTrigger == "canTalk2" && audioClip2 != null)
        {
            audioSource.clip = audioClip2;
            audioSource.Play();
            StartCoroutine(RevealTextWordByWord("You are right, Kiki. It is evening now the clothes and toys are dry", 0.5f));
            StartCoroutine(TriggerHelper2AfterDialogue());
        }
    }

    private IEnumerator TriggerHelperAfterDialogue()
    {
        yield return new WaitForSeconds(3.5f);

        // Trigger "helper1" on Kiki's animator
        if (kikiAnimator != null)
        {
            kikiAnimator.SetTrigger("helper1");
        }

        // Play audio from the AudioSource on the clothStand GameObject
        AudioSource clothStandAudio = clothStand.GetComponent<AudioSource>();
        if (clothStandAudio != null)
        {
            clothStandAudio.Play();
        }
        StartCoroutine(RevealTextWordByWord("Let's put the dry clothes in the basket", 0.5f));
        CheckAndEnableColliders();
    }

    private IEnumerator TriggerHelper2AfterDialogue()
    {
        yield return new WaitForSeconds(7.5f);

        // Trigger "helper1" on Kiki's animator
        if (kikiAnimator != null)
        {
            kikiAnimator.SetTrigger("helper2");
        }

        // Play audio from the AudioSource on the sun GameObject
        AudioSource sunAudio = sun.GetComponent<AudioSource>();
        if (sunAudio != null)
        {
            sunAudio.Play();
        }

        // Start the coroutine to reveal text and spawn glow prefabs
        StartCoroutine(RevealTextWordByWord("Let's put the toys and the clothes in the basket", 0.5f));

        // After the text reveals, spawn glow prefabs for the handlers
        SpawnGlowForHandlers();
    }

    private void SpawnGlowForHandlers()
    {
        // Get the combined list of clothesHandlers and toysHandlers from dragmanager
        var allHandlers = new List<GameObject>();

        // Add the GameObjects of the handlers, not the components
        foreach (var handler in dragmanager.clothesHandlers)
        {
            allHandlers.Add(handler.gameObject); // Ensure you add the GameObject
        }

        foreach (var handler in dragmanager.toysHandlers)
        {
            allHandlers.Add(handler.gameObject); // Ensure you add the GameObject
        }

        // Now spawn the glow for each handler (which are GameObjects)
        foreach (var handler in allHandlers)
        {
            // Ensure the handler GameObject is valid before spawning the glow
            if (handler != null)
            {
                // Spawn the glow prefab and enable the collider
                StartCoroutine(SpawnGlowAndEnableCollider(handler, handler.GetComponent<Collider2D>()));
            }
        }
    }


    private void CheckAndEnableColliders()
    {
        if (!collidersEnabled)
        {
            int enabledCount = 0;

            foreach (GameObject obj in objectsToEnable)
            {
                if (enabledCount >= 3) break; // Enable only three cloth objects

                ClothesdragHandler clothHandler = obj.GetComponent<ClothesdragHandler>();
                if (clothHandler != null && clothHandler.isDry) // Check if cloth is dry
                {
                    Collider2D collider = obj.GetComponent<Collider2D>();
                    if (collider != null)
                    {
                        StartCoroutine(SpawnGlowAndEnableCollider(obj, collider));
                        enabledCount++;
                    }
                }
            }
            
        }
    }

    private IEnumerator SpawnGlowAndEnableCollider(GameObject targetObject, Collider2D collider)
    {
        if (glowPrefab == null) yield break;

        // Instantiate the glow prefab
        GameObject glow = Instantiate(glowPrefab, targetObject.transform.position, Quaternion.identity);
        glow.transform.localScale = Vector3.zero; // Start with a scale of 0

        // Tween the scale to 8
        float duration = 0.5f;
        Vector3 targetScale = Vector3.one * 8;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            glow.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, elapsedTime / duration);
            yield return null;
        }

        yield return new WaitForSeconds(2f); // Wait for 2 seconds

        // Fade out the glow
        SpriteRenderer glowRenderer = glow.GetComponent<SpriteRenderer>();
        if (glowRenderer != null)
        {
            Color originalColor = glowRenderer.color;
            elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                glowRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1 - (elapsedTime / duration));
                yield return null;
            }
        }

        // Destroy the glow object
        Destroy(glow);

        // Enable the collider
        collider.enabled = true;
        dragmanager.StartHelperTimerForNextObject();
        collidersEnabled = true;
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
