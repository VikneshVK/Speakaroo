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

    private DishWashingManager dishWashingManager; // Reference to the DishWashingManager
    private Vector3 originalPosition; // Store the original position
    private Vector3 offset; // Declare offset here
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>(); // Store original scales of children
    private bool isDragging = false;

    void Start()
    {
        birdAnimator.SetTrigger(canTalkParam);
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

    IEnumerator WaitForTalkAnimation()
    {
        yield return new WaitUntil(() => birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talk") == false);
        dirtyDishes.GetComponent<Collider2D>().enabled = true;
    }

    void OnDirtyDishesDropped()
    {
        if (IsDroppedOnFoam(dirtyDishes))
        {
            LeanTween.scale(dirtyDishes, Vector3.zero, 0.5f).setOnComplete(() =>
            {
                // After scaling, instantiate the yourPrefab (which includes DishWashingManager)
                GameObject spawnedObject = Instantiate(yourPrefab, spawnPosition, Quaternion.identity);

                // Get the DishWashingManager from the prefab
                dishWashingManager = spawnedObject.GetComponent<DishWashingManager>();

                // Call the function to spawn the other visual elements
                SpawnPrefab(spawnedObject);
            });
        }
    }

    bool IsDroppedOnFoam(GameObject droppedObject)
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
    }

    void TweenDirtyDishesBack()
    {
        LeanTween.move(dirtyDishes, originalPosition, 0.5f).setOnComplete(() =>
        {
            // Change the sprites of the children and reset their scale
            foreach (Transform child in dirtyDishes.transform)
            {
                // Construct the sprite path based on the child name
                string spritePath = "Images/LVL4Sc3/" + child.name;  // Adjusted path based on Resources folder
                Debug.Log("Attempting to load sprite at path: " + spritePath); // Debug log for sprite path

                // Load the new sprite for the child
                Sprite newSprite = Resources.Load<Sprite>(spritePath);
                if (newSprite == null)
                {
                    Debug.LogError("Failed to load sprite at path: " + spritePath); // Log error if sprite loading fails
                }

                SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = newSprite; // Assign the sprite if successfully loaded
                }

                // Reset the scale to its original scale
                if (originalScales.ContainsKey(child))
                {
                    child.localScale = originalScales[child];
                }
            }
        });

        // Reset allDishesWashed to false in the DishWashingManager after tweening is complete
        dishWashingManager.allDishesWashed = false;
    }


    public void SetAllDishesWashed()
    {
        if (dishWashingManager != null)
        {
            dishWashingManager.allDishesWashed = true;
        }
    }
}
