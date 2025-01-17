using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LVL5Sc12Jojocontroller : MonoBehaviour
{
    public Transform stopPosition;
    public Transform stopPosition2;
    public GameObject prefabToSpawn;
    public Transform prefabToSpawnLocation;  // Location where the general prefab will be spawned
    public GameObject ticketPrefab;          // Ticket prefab to spawn and animate
    public Transform ticketPrefabSpawnLocation; // Location where the ticket prefab will be spawned
    public Transform ticketEndLocation;      // End location for the ticket prefab
    public float walkSpeed = 2f;             // Speed of the character while walking
    public AudioClip Audio1;
    public AudioClip Audio2;
    public AudioClip Audio3;
    public SubtitleManager subtitleManager;

    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;
    public AudioClip SfxAudio2;
    private bool walkingSfxPlayed = false;

    private Animator animator;               // Animator for the character
    private AudioSource boyAudioSource;
    private bool isWalking;                  // Check if the character is walking
    private bool canWalk;                    // Boolean to trigger walk animation
    private bool canTalk;                    // Boolean to trigger talk animation
    public bool canTalk2;                   // Boolean to trigger talk2 animation
    private bool canTalk3;                   // Boolean to trigger talk3 animation
    private bool isWalkingToPosition2;
    private bool ifDialougeComplete;
    private SpriteRenderer boySprite;

    private bool isIdleCompleted;            // Check if the idle animation is completed
    private bool isTalkCompleted;            // Check if the talk animation is completed
    private bool isTalk2Completed;           // Check if the talk2 animation is completed
    private bool ticketSpawned;
    private float targetXPosition;           // Target x position for the character (stopPosition 1)
    private float targetXPosition2;          // Target x position for the character (stopPosition 2)

    void Start()
    {
        animator = GetComponent<Animator>();
        boySprite = GetComponent<SpriteRenderer>();
        boyAudioSource = GetComponent<AudioSource>();
        isWalking = false;
        canWalk = false;
        canTalk = false;
        canTalk2 = false;
        canTalk3 = false;
        isIdleCompleted = false;
        isTalkCompleted = false;
        isWalkingToPosition2 = false;
        ifDialougeComplete = false;
        boySprite.flipX = true;
        ticketSpawned = false;
        // Set the target X positions (only the X axis is considered)
        targetXPosition = stopPosition.position.x;
        targetXPosition2 = stopPosition2.position.x;
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!isIdleCompleted)
        {
            HandleIdleCompletion();
        }
        else if (canWalk && isWalking)
        {
            if (!walkingSfxPlayed)
            {
                SfxAudioSource.loop = true;
                walkingSfxPlayed = true;
                SfxAudioSource.clip = SfxAudio2;
                SfxAudioSource.Play();
            }
            if (!isWalkingToPosition2)
            {
                WalkToPosition(targetXPosition); // Move to stop position 1
            }
            else
            {
                WalkToPosition(targetXPosition2); // Move to stop position 2
            }
        }
        else if (canTalk)
        {
            HandleTalkCompletion();
        }
        else if (canTalk2)
        {
            HandleTalk2Completion();
        }
        else if (canTalk3)
        {
            HandleTalk3Completion();
        }
    }

    private void HandleIdleCompletion()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.1f)
        {
            
            isIdleCompleted = true;
            canWalk = true;
            animator.SetBool("canWalk", true); // Trigger walk animation
            isWalking = true;
        }
    }

    private void WalkToPosition(float targetXPosition)
    {
        // Only update the x-position, keep y and z the same
        Vector3 currentPosition = transform.position;
        currentPosition.x = Mathf.MoveTowards(currentPosition.x, targetXPosition, walkSpeed * Time.deltaTime);
        transform.position = currentPosition;

        if (Mathf.Abs(currentPosition.x - targetXPosition) <= 0.1f)
        {
            SfxAudioSource.Stop();
            walkingSfxPlayed = false;
            isWalking = false;
            canWalk = false;
            animator.SetBool("canWalk", false); // Stop walk animation

            if (!isWalkingToPosition2)
            {
                
                canTalk = true;               
                animator.SetBool("canTalk", true); // Trigger talk animation
                boyAudioSource.clip = Audio1;
                boyAudioSource.Play();
                subtitleManager.DisplaySubtitle("Ask for the tickets.", "JoJo", Audio1);
            }
            else
            {
                // If it's the second walk phase (to stopPosition2), stop further movement
                isWalkingToPosition2 = false;
            }
        }
    }

    private void HandleTalkCompletion()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Talk") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            canTalk = false;
            animator.SetBool("canTalk", false); // End talk animation
            SpawnGeneralPrefab();
        }
    }

    // Function to spawn the general prefab (different from the ticket)
    private void SpawnGeneralPrefab()
    {
        if (prefabToSpawn != null && prefabToSpawnLocation != null)
        {
            // Spawn the general prefab at its specific location
            Instantiate(prefabToSpawn, prefabToSpawnLocation.position, prefabToSpawnLocation.rotation);
        }
        else
        {
            Debug.LogError("General prefab or spawn location is missing!");
        }
    }

    // Function to spawn the ticket prefab
    private void SpawnTicket()
    {
        if (ticketPrefab != null && ticketPrefabSpawnLocation != null)
        {
            GameObject ticket = Instantiate(ticketPrefab, ticketPrefabSpawnLocation.position, ticketPrefabSpawnLocation.rotation);

            LeanTween.scale(ticket, Vector3.one * 0.75f, 0.5f).setOnComplete(() =>
            {
                LeanTween.move(ticket, ticketEndLocation.position, 1f).setOnComplete(() =>
                {
                    LeanTween.scale(ticket, Vector3.zero, 0.5f).setOnComplete(() =>
                    {
                        Destroy(ticket); 
                        animator.SetBool("canTalk2", true); // Trigger Talk2 animation
                        if (!ifDialougeComplete)
                        {
                            boyAudioSource.clip = Audio2;
                            boyAudioSource.Play();
                            subtitleManager.DisplaySubtitle("Yay! we got the Tickets.", "JoJo", Audio1);
                            ifDialougeComplete = true;
                        }
                    });
                });
            });
        }
        else
        {
            Debug.LogError("Ticket prefab or spawn location is missing!");
        }
    }

    private void HandleTalk2Completion()
    {
        if (!ticketSpawned)
        {
            ticketSpawned = true;
            SpawnTicket();
            if (SfxAudioSource != null)
            {
                SfxAudioSource.loop = false;
                SfxAudioSource.PlayOneShot(SfxAudio1);
            }
        }
        

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Talk2") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            canTalk2 = false; // End Talk2 animation
            animator.SetBool("canTalk2", false);

            // Transition to Talk3
            canTalk3 = true;
            animator.SetBool("canTalk3", true); // Trigger Talk3 animation
            boyAudioSource.clip = Audio3;
            boyAudioSource.Play();
            subtitleManager.DisplaySubtitle("Let's go in.", "JoJo", Audio3);
        }
    }

    private void HandleTalk3Completion()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Talk3") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            canTalk3 = false;
            animator.SetBool("canTalk3", false); // End talk3 animation
            canWalk = true; // Allow walking to stopPosition 2
            isWalking = true;
            isWalkingToPosition2 = true; // Now moving to stopPosition2
            animator.SetBool("canWalk", true);
            /*boySprite.flipX = true;*/
        }
    }
}
