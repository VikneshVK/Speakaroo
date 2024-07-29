using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class ItemDragHandler : MonoBehaviour
{
    public bool isDry = false;
    private Vector3 offset;
    private Transform basketTransform;
    private Transform toyBasketTransform;
    private Transform teddyPosition;
    private Transform dinoPosition;
    private Transform bunnyPosition;
    private Transform selectedTarget;
    private Vector3 originalTargetScale;
    private bool isDragging = false;
    private Vector3 startPosition;
    private Animator boyAnimator;

    public static int clothesOnLine = 6;  // Variable to track the number of cloth and toy objects on the line
    public static int toysDryed = 0;      // Static variable to track the number of dry toys placed in the cloth basket

    public GameObject teddyPositionObject;
    public GameObject dinoPositionObject;
    public GameObject bunnyPositionObject;
    public CloudManager cloudManager;
    public Transform sun;
    public Transform sunTargetPosition;
    public GameObject boy;
    public SpriteRenderer clothBasketSpriteRenderer; // Reference to the sprite renderer of the cloth basket

    private float originalMinSpeed;
    private float originalMaxSpeed;

    private void Start()
    {
        
        basketTransform = GameObject.FindGameObjectWithTag("ClothBasket").transform;
        toyBasketTransform = GameObject.FindGameObjectWithTag("ToyBasket").transform;
        teddyPosition = teddyPositionObject.transform;
        dinoPosition = dinoPositionObject.transform;
        bunnyPosition = bunnyPositionObject.transform;
        startPosition = transform.position;
        boyAnimator = boy.GetComponent<Animator>();
        
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

        if (clothesOnLine == 0 && toysDryed == 3)
        {
            boyAnimator.SetBool("allDryed", true);
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
                // Deactivate the sprite renderer
                GetComponent<SpriteRenderer>().enabled = false;

                // Update the cloth basket's sprite
                UpdateClothBasketSprite();

                DecrementClothesCount();
            }
            else if (gameObject.tag == "Toy")
            {
                if (isDry && selectedTarget == toyBasketTransform)
                {
                    IncrementToysDryed();
                    DecrementClothesCount();
                }
                else if (!isDry && selectedTarget == GetClotheslinePosition(gameObject.name))
                {
                    IncrementClothesCount(); // Increment only if the wet toy is dropped in the correct position
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
                    startCollider = toyBasketTransform.GetComponent<Collider2D>();
                }

                if (startCollider != null)
                {
                    startPosition = startCollider.bounds.center;
                }
            }

            // Reset the position if the drop is incorrect
            transform.position = startPosition;
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

    private Transform DetermineDropTarget()
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

    private bool IsOverlapping(Transform target)
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
        CheckClothesCount();
    }

    private void IncrementClothesCount()
    {
        clothesOnLine++;
        Debug.Log("Clothes on Line increased to: " + clothesOnLine);
        CheckClothesCount();
    }

    private void CheckClothesCount()
    {
        if (clothesOnLine == 6)
        {
            Debug.Log("Starting 10 seconds countdown to dry all items.");
            StartCoroutine(SetAllDry());
        }
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

    IEnumerator SetAllDry()
    {
        // Save original speeds
        originalMinSpeed = cloudManager.minSpeed;
        originalMaxSpeed = cloudManager.maxSpeed;

        // Increase cloud speeds
        cloudManager.minSpeed = 50f;
        cloudManager.maxSpeed = 50f;

        // Update speeds on existing clouds
        var existingClouds = FindObjectsOfType<CloudMovement>();
        foreach (var cloud in existingClouds)
        {
            cloud.speed = Random.Range(cloudManager.minSpeed, cloudManager.maxSpeed);
        }

        // Enable all colliders
        Collider2D[] colliders = FindObjectsOfType<Collider2D>();
        foreach (var collider in colliders)
        {
            collider.enabled = true;
        }

        // Tween the sun's position to the target position over 10 seconds
        LeanTween.move(sun.gameObject, sunTargetPosition.position, 10f);

        // Wait for 10 seconds
        yield return new WaitForSeconds(10);

        // Set all items as dry
        GameObject[] allDraggableObjects = GameObject.FindGameObjectsWithTag("Cloth");
        allDraggableObjects = allDraggableObjects.Concat(GameObject.FindGameObjectsWithTag("Toy")).ToArray();
        foreach (GameObject obj in allDraggableObjects)
        {
            ItemDragHandler handler = obj.GetComponent<ItemDragHandler>();
            if (handler != null)
            {
                handler.isDry = true;
                Debug.Log("Set isDry to true for " + obj.name);
            }
        }

        // Change sprites for specific objects based on their names
        ChangeSpritesAfterDrying();

        // Reset cloud speeds to original values
        cloudManager.minSpeed = originalMinSpeed;
        cloudManager.maxSpeed = originalMaxSpeed;

        // Update speeds on existing clouds again
        foreach (var cloud in existingClouds)
        {
            cloud.speed = Random.Range(cloudManager.minSpeed, cloudManager.maxSpeed);
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
            }
        }
    }
}
