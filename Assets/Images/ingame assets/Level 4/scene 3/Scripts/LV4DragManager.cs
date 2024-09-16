using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LV4DragManager : MonoBehaviour
{
    public GameObject dirtyDishes; // Reference to the Dirty Dishes GameObject
    public GameObject foam; // Reference to the Foam GameObject
    public GameObject yourPrefab; // The prefab to spawn (also includes DishWashingManager)
    public Vector3 spawnPosition; // Position to spawn the prefab
    public Animator birdAnimator; // Reference to the Bird Animator
    public string canTalkParam = "canTalk"; // Name of the animation parameter
    public LVL4Sc3HelperHand helperHandManager; // Reference to the Helper Hand Manager
    private List<DishdragController> dishControllers = new List<DishdragController>();

    public DishWashingManager dishWashingManager; // Reference to the DishWashingManager
    private Vector3 originalPosition; // Store the original position
    private Vector3 offset; // Declare offset here
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>(); // Store original scales of children
    private bool isDragging = false;
    public bool PrefabSpawned;

    private Coroutine helperHandCoroutine; // Coroutine for helper hand delay
    private int currentIndex = 0;

    void Start()
    {
        birdAnimator.SetTrigger(canTalkParam);
        PrefabSpawned = false;
        originalPosition = dirtyDishes.transform.position;
        

        // Store original scale of dirty dishes and their children
        foreach (Transform child in dirtyDishes.transform)
        {
            originalScales[child] = child.localScale;
        }

        StartCoroutine(WaitForTalkAnimation());
    }

    void Update()
    {
        HandleMouseInput();

        // Check if dishWashingManager is not null and if all dishes are washed
        if (dishWashingManager != null && dishWashingManager.allDishesWashed)
        {
            TweenDirtyDishesBack();
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == dirtyDishes)
            {
                isDragging = true;
                offset = dirtyDishes.transform.position - (Vector3)mousePosition;
            }
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dirtyDishes.transform.position = new Vector3(mousePosition.x, mousePosition.y, dirtyDishes.transform.position.z) + offset;
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            OnDirtyDishesDropped();
            isDragging = false;
        }
    }

    public IEnumerator WaitForTalkAnimation()
    {
        yield return new WaitUntil(() => birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talk"));
        yield return new WaitForSeconds(0.1f); // Adjust this delay as needed

        yield return new WaitUntil(() => birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talk") == false);

        dirtyDishes.GetComponent<Collider2D>().enabled = true;

        // Start the helper hand delay timer
        helperHandCoroutine = StartCoroutine(HelperHandDelayTimer());
    }

    // Helper hand delay timer coroutine
    private IEnumerator HelperHandDelayTimer()
    {
        // Wait for the delay time before spawning the helper hand
        yield return new WaitForSeconds(helperHandManager.helperHandDelay);

        // Spawn and tween the helper hand if OnDirtyDishesDropped() wasn't called
        helperHandManager.SpawnHelperHand(dirtyDishes.transform.position, foam.transform.position);
    }

    void OnDirtyDishesDropped()
    {
        // Stop the helper hand coroutine to prevent spawning
        if (helperHandCoroutine != null)
        {
            StopCoroutine(helperHandCoroutine);
        }

        // If dropped correctly on the foam
        if (IsDroppedOnFoam(dirtyDishes))
        {
            LeanTween.scale(dirtyDishes, Vector3.zero, 0.5f).setOnComplete(() =>
            {
                GameObject spawnedObject = Instantiate(yourPrefab, spawnPosition, Quaternion.identity);

                dishWashingManager = spawnedObject.GetComponent<DishWashingManager>();

                SpawnPrefab(spawnedObject);

                // Destroy the helper hand if it's active
                helperHandManager.StopHelperHand();
            });
        }
        else
        {
            // If dropped incorrectly
            // Reset the position of the dirty dishes
            dirtyDishes.transform.position = originalPosition;

            // Restart the helper hand delay timer
            helperHandCoroutine = StartCoroutine(HelperHandDelayTimer());
        }
    }

    public bool IsDroppedOnFoam(GameObject droppedObject)
    {
        Collider2D foamCollider = foam.GetComponent<Collider2D>();
        Collider2D droppedCollider = droppedObject.GetComponent<Collider2D>();
        return foamCollider.bounds.Intersects(droppedCollider.bounds);
    }

    void SpawnPrefab(GameObject spawnedObject)
    {
        Transform floor = spawnedObject.transform.Find("floor");
        Transform sink = spawnedObject.transform.Find("sink");

        Vector3 floorOriginalScale = floor.localScale;
        Vector3 sinkOriginalScale = sink.localScale;

        floor.localScale = Vector3.zero;
        sink.localScale = Vector3.zero;

        Dictionary<Transform, Vector3> dishOriginalScales = new Dictionary<Transform, Vector3>();

        foreach (Transform dish in sink)
        {
            dishOriginalScales[dish] = dish.localScale;
            dish.localScale = Vector3.zero;
        }

        LeanTween.scale(floor.gameObject, floorOriginalScale, 0.5f).setEase(LeanTweenType.easeOutBounce).setOnComplete(() =>
        {
            LeanTween.scale(sink.gameObject, sinkOriginalScale, 0.5f).setEase(LeanTweenType.easeOutBounce).setOnComplete(() =>
            {
                foreach (Transform dish in dishOriginalScales.Keys)
                {
                    Vector3 dishOriginalScale = dishOriginalScales[dish];
                    LeanTween.scale(dish.gameObject, dishOriginalScale, 0.5f).setEase(LeanTweenType.easeOutBounce);
                }
            });
        });

        PrefabSpawned = true;
    }

    void TweenDirtyDishesBack()
    {
        LeanTween.scale(dirtyDishes, Vector3.one, 0.5f).setOnComplete(() =>
        {
            foreach (Transform child in dirtyDishes.transform)
            {
                // Enable the SpriteRenderer and change the sprite
                SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    string spritePath = "Images/LVL4Sc3/" + child.name;
                    Debug.Log("Attempting to load sprite from path: " + spritePath);

                    sr.sprite = Resources.Load<Sprite>(spritePath);

                    if (sr.sprite == null)
                    {
                        Debug.LogError("Failed to load sprite at path: " + spritePath);
                    }
                }

                // Reset the scale of the child if it was stored in originalScales
                if (originalScales.ContainsKey(child))
                {
                    child.localScale = originalScales[child];
                }

                // Enable the Collider2D component on each child GameObject
                Collider2D childCollider = child.GetComponent<Collider2D>();
                if (childCollider != null)
                {
                    childCollider.enabled = true; // Enable the collider
                }
            }

            // Disable the main collider of dirtyDishes
            Collider2D dirtyDishesCollider = dirtyDishes.GetComponent<Collider2D>();
            if (dirtyDishesCollider != null)
            {
                dirtyDishesCollider.enabled = false;
            }

            // Start the delay timer to check interactions in DishdragController
            StartHelperHandDelayTimer();
        });

        LeanTween.move(dirtyDishes, originalPosition, 0.5f);
        dishWashingManager.allDishesWashed = false;
    }

    // Start the delay timer to check each DishdragController
    private void StartHelperHandDelayTimer()
    {
        if (helperHandCoroutine != null)
        {
            StopCoroutine(helperHandCoroutine);
        }
        helperHandCoroutine = StartCoroutine(HelperHandDelayTimerforchild());
    }

    // Coroutine to control the delay for checking interactions on DishdragController
    private IEnumerator HelperHandDelayTimerforchild()
    {
        // Wait for a moment before allowing DishdragController to handle its own checks
        yield return new WaitForSeconds(1f);

        // Notify DishdragController objects to start their interaction checks
        DishdragController.StartHelperHandCheckForAll();
    }


public void SetAllDishesWashed()
    {
        if (dishWashingManager != null)
        {
            dishWashingManager.allDishesWashed = true;
        }
    }

}
