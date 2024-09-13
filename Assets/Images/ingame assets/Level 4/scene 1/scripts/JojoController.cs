using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JojoController : MonoBehaviour
{
    public Transform stopPosition;
    public float walkSpeed = 2f;
    public GameObject Bird;
    public GameObject prefabToSpawn; // Reference to the prefab to spawn
    public Transform spawnLocation; // Reference to the spawn location for the initial prefab
    public GameObject fridge; // Reference to the fridge GameObject
    public GameObject[] prefabsToSpawnAfterSpriteChange; // Prefabs to spawn after changing the sprite
    public Transform[] spawnLocationsForPrefabs; // Spawn locations for the three prefabs

    private Animator animator;
    private Animator birdAnimator;
    private SpriteRenderer spriteRenderer;
    private Collider2D fridgeCollider;
    private SpriteRenderer fridgeSpriteRenderer;

    private bool isWalking = false;
    private bool isIdleCompleted = false;
    public bool prefabSpawned = false; // To ensure the initial prefab is spawned only once
    private bool fridgeColliderEnabled = false; // To ensure the fridge collider is enabled only once
    private bool spriteChanged = false; // To track if the fridge sprite has been changed

    void Start()
    {
        animator = GetComponent<Animator>();
        birdAnimator = Bird.GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Get references to the fridge's components
        if (fridge != null)
        {
            fridgeCollider = fridge.GetComponent<Collider2D>();
            fridgeSpriteRenderer = fridge.GetComponent<SpriteRenderer>();

            if (fridgeCollider != null)
            {
                fridgeCollider.enabled = false; // Ensure the fridge collider is initially disabled
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
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            isIdleCompleted = true;
            spriteRenderer.flipX = true;
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

            if (Mathf.Abs(transform.position.x - stopPosition.position.x) <= 0.1f)
            {
                isWalking = false;
                spriteRenderer.flipX = false;
                animator.SetBool("canWalk", false);
                animator.SetBool("canTalk", true);
            }
        }
    }

    public void CheckAndSpawnPrefab()
    {
        if (!prefabSpawned && animator.GetCurrentAnimatorStateInfo(0).IsName("Talk 2") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
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
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            if (fridgeCollider != null)
            {
                fridgeCollider.enabled = true; // Enable the fridge collider
                fridgeColliderEnabled = true; // Ensure this happens only once
            }
        }
    }

    // Method to handle fridge tapping
    public void OnFridgeTapped()
    {
        if (!spriteChanged && fridgeSpriteRenderer != null)
        {
            // Load and set the new sprite
            Sprite newSprite = Resources.Load<Sprite>("Images/LVL 4/fridge_open");
            if (newSprite != null)
            {
                fridgeSpriteRenderer.sprite = newSprite;
                spriteChanged = true;
                fridgeCollider.enabled = false;

                // Spawn additional prefabs at specified locations
                for (int i = 0; i < prefabsToSpawnAfterSpriteChange.Length; i++)
                {
                    if (i < spawnLocationsForPrefabs.Length)
                    {
                        Instantiate(prefabsToSpawnAfterSpriteChange[i], spawnLocationsForPrefabs[i].position, spawnLocationsForPrefabs[i].rotation);
                    }
                }
            }
        }
        animator.SetTrigger("talk2");
    }
}
