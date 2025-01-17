using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Lvl4Sc1JojoController : MonoBehaviour
{
    public Transform stopPosition;
    public float walkSpeed = 2f;
    public GameObject Bird;
    public GameObject prefabToSpawn;
    public Transform spawnLocation;
    public GameObject fridge;
    public GameObject[] prefabsToSpawnAfterSpriteChange;
    public Transform[] spawnLocationsForPrefabs;
    public GameObject AudioManager;
    public AudioClip Audioclip1;
    public AudioClip AudioClip2;
    public AudioClip AudioClip3;
    public SubtitleManager subtitleManager;

    [Header("SFX")]
    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;
    private bool walkingSfxPlayed = false;

    private Lvl4Sc1Audiomanger lvl4Sc1Audiomanger;
    private Animator animator;
    private Animator birdAnimator;
    /*private SpriteRenderer spriteRenderer;*/
    private Collider2D fridgeCollider;
    private SpriteRenderer fridgeSpriteRenderer;

    private bool isWalking;
    private bool isIdleCompleted;
    public bool prefabSpawned;
    private bool fridgeColliderEnabled;
    private bool spriteChanged;

   

    void Start()
    {
        animator = GetComponent<Animator>();
        birdAnimator = Bird.GetComponent<Animator>();
        lvl4Sc1Audiomanger = AudioManager.GetComponent<Lvl4Sc1Audiomanger>();
        isWalking = false;
        isIdleCompleted = false;
        prefabSpawned = false;
        fridgeColliderEnabled = false;
        spriteChanged = false;
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
        if (fridge != null)
        {
            fridgeCollider = fridge.GetComponent<Collider2D>();
            fridgeSpriteRenderer = fridge.GetComponent<SpriteRenderer>();

            if (fridgeCollider != null)
            {
                fridgeCollider.enabled = false;
            }
        }
    }

    void Update()
    {
        HandleIdleCompletion();
        HandleWalking();
        CheckAndEnableFridgeCollider();

        // Continuously check and spawn the prefab as long as itemsDropped < 3
        if (KikiController1.itemsDropped < 3)
        {
            CheckAndSpawnPrefab();
        }
    }

    private void HandleIdleCompletion()
    {
        if (!isIdleCompleted && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.1f)
        {
            isIdleCompleted = true;
            /*spriteRenderer.flipX = true;*/
            isWalking = true;
            animator.SetBool("canWalk", true);
        }
    }

    private void HandleWalking()
    {
        if (isWalking)
        {
            WalkToStopPosition();
        }
    }

    private void WalkToStopPosition()
    {
        if (stopPosition != null)
        {
            Vector3 targetPosition = new Vector3(stopPosition.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, walkSpeed * Time.deltaTime);
            if (!walkingSfxPlayed)
            {
                SfxAudioSource.loop = true;
                walkingSfxPlayed = true;
                SfxAudioSource.clip = SfxAudio1;
                SfxAudioSource.Play();
            }
            if (Mathf.Abs(transform.position.x - stopPosition.position.x) <= 0.1f)
            {
                SfxAudioSource.Stop();
                isWalking = false;
                /*spriteRenderer.flipX = false;*/
                animator.SetBool("canWalk", false);
                animator.SetBool("canTalk", true);
                lvl4Sc1Audiomanger.PlayAudio(Audioclip1);
                subtitleManager.DisplaySubtitle("I am very hungry, Let's see, What's inside the fridge", "JoJo", Audioclip1);                
            }
        }
    }

    public void CheckAndSpawnPrefab()
    {
        if (!prefabSpawned && animator.GetCurrentAnimatorStateInfo(0).IsName("Icecream") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            if (prefabToSpawn != null && spawnLocation != null)
            {
                Instantiate(prefabToSpawn, spawnLocation.position, spawnLocation.rotation);
                prefabSpawned = true;
            }
        }
    }

    public void CheckAndEnableFridgeCollider()
    {
        if (!fridgeColliderEnabled && animator.GetCurrentAnimatorStateInfo(0).IsName("Talk") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            if (fridgeCollider != null)
            {
                fridgeCollider.enabled = true; // Enable the fridge collider
                fridgeColliderEnabled = true; // Ensure this happens only once
            }

            animator.SetBool("canTalk", false);

            // Trigger the "Fridge" animation on the bird animator
            if (birdAnimator != null)
            {
                birdAnimator.SetTrigger("fridge");
                lvl4Sc1Audiomanger.PlayAudio(AudioClip3);
                subtitleManager.DisplaySubtitle("Open the fridge", "Kiki", AudioClip3);                
            }
        }
    }


    // Method to handle fridge tapping
    public void OnFridgeTapped()
    {
        if (!spriteChanged && fridgeSpriteRenderer != null)
        {
            
            Sprite newSprite = Resources.Load<Sprite>("Images/LVL 4/fridge_open");
            if (newSprite != null)
            {
                fridgeSpriteRenderer.sprite = newSprite;
                spriteChanged = true;
                fridgeCollider.enabled = false;

                // Get reference to LVL4Sc1HelperController
                LVL4Sc1HelperController helperController = FindObjectOfType<LVL4Sc1HelperController>();

                // Spawn additional prefabs at specified locations and register each one
                for (int i = 0; i < prefabsToSpawnAfterSpriteChange.Length; i++)
                {
                    if (i < spawnLocationsForPrefabs.Length)
                    {
                        GameObject spawnedInteractable = Instantiate(prefabsToSpawnAfterSpriteChange[i],
                                                                    spawnLocationsForPrefabs[i].position,
                                                                    spawnLocationsForPrefabs[i].rotation);

                        // Register the interactable with the helper controller
                        helperController?.RegisterInteractable(spawnedInteractable);
                    }
                }
            }
        }
        // Start the coroutine for delayed trigger
        StartCoroutine(TriggerIceCreamWithDelay());
    }


    private IEnumerator TriggerIceCreamWithDelay()
    {
        yield return new WaitForSeconds(1f); // Wait for 1 second
        animator.SetTrigger("IceCream");
        lvl4Sc1Audiomanger.PlayAudio(AudioClip2);
        subtitleManager.DisplaySubtitle("I want to eat ice cream..!", "JoJo", AudioClip2);       
    }    
}
