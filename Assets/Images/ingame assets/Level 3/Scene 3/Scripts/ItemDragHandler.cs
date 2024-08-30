using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class ItemDragHandler : MonoBehaviour
{
    public bool isDry = false;
    private Vector3 offset;
    public Transform basketTransform;
    private Transform toyBasketTransform;
    private Transform teddyPosition;
    private Transform dinoPosition;
    private Transform bunnyPosition;
    private Transform teddyResetPosition;
    private Transform dinoResetPosition;
    private Transform bunnyResetPosition;
    private Transform selectedTarget;
    private Vector3 originalTargetScale;
    private bool isDragging = false;
    private Vector3 startPosition;
    private Animator boyAnimator;

    public static int clothesOnLine = 6;  // Variable to track the number of cloth and toy objects on the line
    public static int toysDryed = 0;      // Static variable to track the number of dry toys placed in the cloth basket
    private bool isTransitionComplete = false; // Tracks if the dry-to-wet transition is complete
    private bool tweenstarted = true;

    public Kiki_actions kikiActions;
    public Jojo_action jojoActions;
    public float timerDuration;
    public GameObject teddyPositionObject;
    public GameObject dinoPositionObject;
    public GameObject bunnyPositionObject;
    public GameObject teddyInitialPosition;
    public GameObject dinoInitalPosition;
    public GameObject bunnyInitialPosition;
    public CloudManager cloudManager;
    public Transform sun;
    public Transform sunTargetPosition;
    public GameObject boy;
    public SpriteRenderer clothBasketSpriteRenderer; // Reference to the sprite renderer of the cloth basket

    private float originalMinSpeed;
    private float originalMaxSpeed;

    private void Awake()
    {
        LeanTween.init(10000);
    }

    private void Start()
    {
        basketTransform = GameObject.FindGameObjectWithTag("ClothBasket").transform;
        toyBasketTransform = GameObject.FindGameObjectWithTag("ToyBasket").transform;
        teddyPosition = teddyPositionObject.transform;
        dinoPosition = dinoPositionObject.transform;
        bunnyPosition = bunnyPositionObject.transform;
        teddyResetPosition = teddyInitialPosition.transform;
        dinoResetPosition = dinoInitalPosition.transform;
        bunnyResetPosition = bunnyInitialPosition.transform;
        startPosition = transform.position;
        boyAnimator = boy.GetComponent<Animator>();
        DisableToyPositionColliders();
    }

    private void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
            Vector3 objectPosition = Camera.main.ScreenToWorldPoint(mousePosition) + offset;
            transform.position = new Vector3(objectPosition.x, objectPosition.y, transform.position.z);

            if (selectedTarget != null)
            {
                // Scale only the selected target to indicate it's being dragged
                selectedTarget.localScale = originalTargetScale * 1.15f;
            }
        }
        else
        {
            // Reset scale when not dragging
            if (selectedTarget != null)
            {
                selectedTarget.localScale = originalTargetScale;
            }
        }

        // Enable toy colliders when dry clothes have been removed (clothesOnLine == 3)
        if (clothesOnLine == 3 && !isTransitionComplete)
        {
            EnableToyPositionColliders();
        }

        // Check if all toys have been dried
        if (clothesOnLine == 0 && toysDryed == 3)
        {
            boyAnimator.SetBool("allDryed", true);
        }

        // After the transition (removing dry clothes and adding wet toys) is complete
        if (clothesOnLine == 6 && isTransitionComplete)
        {
            // Now call AreAllItemsWet() to check if all items are wet
            if (AreAllItemsWet() && tweenstarted)
            {
                kikiActions.MoveOffScreen();
                jojoActions.MoveOffScreen();
                StartCoroutine(SetAllDry()); // Start the SetAllDry coroutine
            }
        }
    }

    private void OnMouseDown()
    {
        if ((gameObject.tag == "Cloth" && isDry) || (gameObject.tag == "Toy" && gameObject.GetComponent<Collider2D>().enabled))
        {
            isDragging = true;
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            offset = transform.position - mousePosition;
            offset.z = 0; // Ensure there's no change in the z position

            selectedTarget = DetermineDropTarget();
            if (selectedTarget != null)
            {
                originalTargetScale = selectedTarget.localScale; // Store the original scale of the selected target
            }
        }

        
    }



    private void OnMouseUp()
    {
        isDragging = false;
        if (selectedTarget != null)
        {
            selectedTarget.localScale = originalTargetScale; // Reset the target scale to its original value
        }

        bool isCorrectDrop = false;

        if (IsOverlapping(selectedTarget))
        {
            Collider2D targetCollider = selectedTarget.GetComponent<Collider2D>();
            if (targetCollider != null)
            {
                // Move the object to the center of the target's collider
                transform.position = targetCollider.bounds.center;
            }

            GetComponent<Collider2D>().enabled = false; // Disable the collider

            if (gameObject.tag == "Cloth")
            {
                // Destroy the cloth game object instead of just disabling the sprite renderer
                Destroy(gameObject);
                UpdateClothBasketSprite();
                DecrementClothesCount();
                isCorrectDrop = true;
            }
            else if (gameObject.tag == "Toy")
            {
                if (isDry && selectedTarget == toyBasketTransform)
                {
                    IncrementToysDryed();
                    DecrementClothesCount();
                    DisableToyPositionCollider(gameObject.name);
                    isCorrectDrop = true;
                }
                else if (!isDry && selectedTarget == GetClotheslinePosition(gameObject.name))
                {
                    IncrementClothesCount(); // Increment only if the wet toy is dropped in the correct position
                    isCorrectDrop = true;
                }

                // Check if the transition is now complete and update the flag
                if (clothesOnLine == 6)
                {
                    isTransitionComplete = true;
                }
            }
        }
        else
        {
            // Set the start position dynamically based on the toy's dry status and name
            if (gameObject.tag == "Toy")
            {
                Collider2D startCollider = null;
                if (isDry)
                {
                    // Set to specific toy positions based on toy name
                    startCollider = gameObject.name switch
                    {
                        "wet kuma" => teddyPosition.GetComponent<Collider2D>(),
                        "wet dino" => dinoPosition.GetComponent<Collider2D>(),
                        "wet bunny" => bunnyPosition.GetComponent<Collider2D>(),
                        _ => null
                    };
                }
                else
                {
                    // Set to specific toy positions based on toy name
                    startPosition = gameObject.name switch
                    {
                        "wet kuma" => teddyResetPosition.transform.position,
                        "wet dino" => dinoResetPosition.transform.position,
                        "wet bunny" => bunnyResetPosition.transform.position,
                        _ => startPosition // Default to original start position if not found
                    };
                }

                if (startCollider != null)
                {
                    startPosition = startCollider.bounds.center;
                }

                transform.position = startPosition;
            }

            // Reset the position if the drop is incorrect
            transform.position = startPosition;
        }

        // Notify the HelperHandController about the interaction result
        HelpHandController helperHand = FindObjectOfType<HelpHandController>();
        if (helperHand != null)
        {
            helperHand.OnObjectInteracted(gameObject, isCorrectDrop);
        }
    }


    private bool AreAllItemsWet()
    {
        GameObject[] allItems = GameObject.FindGameObjectsWithTag("Cloth")
            .Concat(GameObject.FindGameObjectsWithTag("Toy")).ToArray();

        foreach (GameObject item in allItems)
        {
            ItemDragHandler handler = item.GetComponent<ItemDragHandler>();
            if (handler != null && handler.isDry)
            {
                return false; // If any item is dry, return false
            }
        }
        return true; // All items are wet
    }

    private void DisableToyPositionColliders()
    {
        if (teddyPosition != null) teddyPosition.GetComponent<Collider2D>().enabled = false;
        if (dinoPosition != null) dinoPosition.GetComponent<Collider2D>().enabled = false;
        if (bunnyPosition != null) bunnyPosition.GetComponent<Collider2D>().enabled = false;
    }

    private void EnableToyPositionColliders()
    {
        if (teddyPosition != null) teddyPosition.GetComponent<Collider2D>().enabled = true;
        if (dinoPosition != null) dinoPosition.GetComponent<Collider2D>().enabled = true;
        if (bunnyPosition != null) bunnyPosition.GetComponent<Collider2D>().enabled = true;
    }

    private void DisableToyPositionCollider(string toyName)
    {
        switch (toyName)
        {
            case "wet kuma":
                if (teddyPosition != null) teddyPosition.GetComponent<Collider2D>().enabled = false;
                break;
            case "wet dino":
                if (dinoPosition != null) dinoPosition.GetComponent<Collider2D>().enabled = false;
                break;
            case "wet bunny":
                if (bunnyPosition != null) bunnyPosition.GetComponent<Collider2D>().enabled = false;
                break;
        }
    }

    private Transform GetClotheslinePosition(string toyName)
    {
        switch (toyName)
        {
            case "wet kuma":
                return teddyPosition;
            case "wet dino":
                return dinoPosition;
            case "wet bunny":
                return bunnyPosition;
            default:
                return null; // Return null if toy name doesn't match
        }
    }

    private void UpdateClothBasketSprite()
    {
        string spriteName = "";

        if (clothesOnLine <= 2)
        {
            spriteName = "laundry-pile";
        }
        else if (clothesOnLine == 4)
        {
            spriteName = "laundry-pile-2";
        }
        else if (clothesOnLine == 5)
        {
            spriteName = "laundry-pile-3";
        }

        if (!string.IsNullOrEmpty(spriteName))
        {
            string path = $"Images/Lvl 3/Scene 3/{spriteName}";
            Sprite newSprite = Resources.Load<Sprite>(path);
            if (newSprite != null)
            {
                clothBasketSpriteRenderer.sprite = newSprite;
            }
            else
            {
                Debug.LogError($"Sprite not found for path: {path}");
            }
        }
    }

    IEnumerator SetAllDry()
    {
        tweenstarted = false;
        // Wait until both Kiki and Jojo have reached the off-screen position
        yield return new WaitUntil(() => kikiActions.hasReachedOffScreen && jojoActions.hasReachedOffScreen);

        // Start the 5-second timer
        
        float timerDuration = 5f;

        // Increase cloud speeds for fast-forward effect and start sun and sky tweens simultaneously
        cloudManager.minSpeed = 50f;
        cloudManager.maxSpeed = 50f;
        cloudManager.UpdateCloudSpeeds(); // Update the speeds of all active clouds

        // Tween the sun's position
        var sunTween = LeanTween.move(sun.gameObject, sunTargetPosition.position, timerDuration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                Debug.Log("Sun tween completed.");
            });

        Debug.Log("Sun tween started.");

        // Tween the sky color
        SpriteRenderer skyRenderer = GameObject.FindGameObjectWithTag("Sky").GetComponent<SpriteRenderer>();
        Color targetColor = new Color(1.0f, 0.6549f, 0.4353f, 1f);// Yellow-orange color

        var skyTween = LeanTween.value(skyRenderer.gameObject, skyRenderer.color, targetColor, timerDuration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnUpdate((Color val) =>
            {
                skyRenderer.color = val;
            })
            .setOnComplete(() =>
            {
                Debug.Log("Sky color tween completed.");
            });

        Debug.Log("Sky tween started.");

        // Wait for the duration of the timer while the tweens are happening
        yield return new WaitForSeconds(timerDuration);

        // After the timer ends, reset cloud speeds to a slower range (0.25 to 1)
        cloudManager.minSpeed = 0.25f;
        cloudManager.maxSpeed = 1f;
        cloudManager.UpdateCloudSpeeds(); // Update the speeds of all active clouds

        // Set all items as dry and change sprites
        GameObject[] allDraggableObjects = GameObject.FindGameObjectsWithTag("Cloth")
            .Concat(GameObject.FindGameObjectsWithTag("Toy")).ToArray();
        foreach (GameObject obj in allDraggableObjects)
        {
            ItemDragHandler handler = obj.GetComponent<ItemDragHandler>();
            if (handler != null)
            {
                handler.isDry = true;
            }
        }
        ChangeSpritesAfterDrying();

        // Move Kiki and Jojo back to their stop positions
        kikiActions.ReturnToStopPosition();
        jojoActions.ReturnToStopPosition();
    }



    // Function to set all items as dry
    void SetItemsAsDry()
    {
        GameObject[] allDraggableObjects = GameObject.FindGameObjectsWithTag("Cloth");
        allDraggableObjects = allDraggableObjects.Concat(GameObject.FindGameObjectsWithTag("Toy")).ToArray();
        foreach (GameObject obj in allDraggableObjects)
        {
            ItemDragHandler handler = obj.GetComponent<ItemDragHandler>();
            if (handler != null)
            {
                handler.isDry = true;
            }
        }
    }
    private void ChangeSpritesAfterDrying()
    {
        // Names and corresponding new sprite names
        Dictionary<string, string> nameToSpriteMap = new Dictionary<string, string>
    {
        { "wet_socK", "sock-1" },
        { "wet_shirT", "shirt-1" },
        { "wet_shorT", "shorts-1" },
        { "wet kuma", "dry kuma" },
        { "wet dino", "dry dino" },
        { "wet bunny", "dry bunny" }
    };

        foreach (var nameSpritePair in nameToSpriteMap)
        {
            GameObject obj = GameObject.Find(nameSpritePair.Key);
            if (obj != null)
            {
                SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    string path = $"Images/Lvl 3/Scene 3/{nameSpritePair.Value}";
                    Sprite newSprite = Resources.Load<Sprite>(path);
                    if (newSprite != null)
                    {
                        spriteRenderer.sprite = newSprite;
                    }
                    else
                    {
                        Debug.LogError($"Sprite not found for path: {path}");
                    }
                }
                else
                {
                    Debug.LogError($"SpriteRenderer not found on {nameSpritePair.Key}");
                }
            }
            else
            {
                Debug.LogError($"GameObject not found for {nameSpritePair.Key}");
            }
        }
    }


    public Transform DetermineDropTarget()
    {
        if (gameObject.tag == "Cloth")
            return basketTransform;
        else if (gameObject.tag == "Toy")
        {
            switch (gameObject.name)
            {
                case "wet kuma":
                    return isDry ? toyBasketTransform : teddyPosition;
                case "wet dino":
                    return isDry ? toyBasketTransform : dinoPosition;
                case "wet bunny":
                    return isDry ? toyBasketTransform : bunnyPosition;
                default:
                    return null; // Return null if toy name doesn't match
            }
        }
        return null; // Default to null if no conditions are met
    }

    public bool IsOverlapping(Transform target)
    {
        if (target == null) return false;

        Collider2D targetCollider = target.GetComponent<Collider2D>();
        Collider2D currentCollider = GetComponent<Collider2D>();

        if (targetCollider != null && currentCollider != null)
        {
            return currentCollider.bounds.Intersects(targetCollider.bounds);
        }
        return false;
    }

    private void DecrementClothesCount()
    {
        clothesOnLine--;
        Debug.Log("Clothes on Line decreased to: " + clothesOnLine);
    }

    private void IncrementClothesCount()
    {
        clothesOnLine++;
        Debug.Log("Clothes on Line increased to: " + clothesOnLine);
    }

    private static void IncrementToysDryed()
    {
        toysDryed++;
        Debug.Log("Toys dried count increased to: " + toysDryed);
    }

    public static int GetToysDryed()
    {
        return toysDryed;
    }
}
