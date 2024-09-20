using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class SpriteChangeController : MonoBehaviour
{
    public SpriteRenderer blenderJarSpriteRenderer;
    public List<string> fruitsInBlender = new List<string>();
    public List<GameObject> fruitObjectsInBlender = new List<GameObject>();
    private JuiceController juiceController;
    public JuiceManager juiceManager;

    [Header("Bird Settings")]
    public GameObject bird;
    public Transform birdEndPosition;
    public Animator birdAnimator;
    public Vector3 birdInitialPosition;

    void Start()
    {
        blenderJarSpriteRenderer = GameObject.FindGameObjectWithTag("Blender_Jar").GetComponent<SpriteRenderer>();
        juiceController = FindObjectOfType<JuiceController>();
        juiceManager = FindObjectOfType<JuiceManager>();

        if (bird != null)
        {
            birdInitialPosition = bird.transform.position;
        }
    }

    public void ActivateBlenderSprite(bool isDragging)
    {
        
        if (fruitsInBlender.Count == 0 && isDragging)
        {
            blenderJarSpriteRenderer.sprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/blender_active");
        }
        else if (fruitsInBlender.Count == 1 && isDragging)
        {
            string fruitTag = fruitsInBlender[0];
            if (fruitTag == "Kiwi")
            {
                blenderJarSpriteRenderer.sprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/kbo");
            }
            else if (fruitTag == "SB")
            {
                blenderJarSpriteRenderer.sprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/sbo");
            }
            else if (fruitTag == "BB")
            {
                blenderJarSpriteRenderer.sprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/bbo");
            }
        }
        else
        {
            Debug.LogWarning("Invalid state for dragging or fruits in blender count.");
        }
    }

    public void ResetBlender()
    {
        Sprite defaultBlenderSprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/default_blender");
        if (defaultBlenderSprite != null)
        {
            blenderJarSpriteRenderer.sprite = defaultBlenderSprite;
        }
        fruitsInBlender.Clear();
        fruitObjectsInBlender.Clear();
        Debug.Log("Blender reset. Fruits in blender cleared.");
        EnableAllFruitColliders();
    }

    void EnableAllFruitColliders()
    {
        GameObject[] fruits = GameObject.FindGameObjectsWithTag("Kiwi")
            .Concat(GameObject.FindGameObjectsWithTag("SB"))
            .Concat(GameObject.FindGameObjectsWithTag("BB"))
            .ToArray();

        foreach (GameObject fruit in fruits)
        {
            Collider2D fruitCollider = fruit.GetComponent<Collider2D>();
            if (fruitCollider != null)
            {
                fruitCollider.enabled = true;
            }
        }

        Debug.Log("All fruit colliders enabled.");
    }

    public void ResetBlenderJarSprite()
    {
        Sprite defaultBlenderJarSprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/default_blender");
        if (defaultBlenderJarSprite != null)
        {
            blenderJarSpriteRenderer.sprite = defaultBlenderJarSprite;
        }
        Debug.Log("Blender jar sprite reset to default.");
    }

    public bool IsOverlappingBlenderJar(Collider2D fruitCollider)
    {
        // Define the area of the blender jar
        GameObject blenderJar = GameObject.FindGameObjectWithTag("Blender_Jar");
        Bounds blenderBounds = blenderJar.GetComponent<SpriteRenderer>().bounds; // Use SpriteRenderer bounds instead of Collider2D bounds

        // Define the corners of the area
        Vector2 pointA = new Vector2(blenderBounds.min.x, blenderBounds.min.y);
        Vector2 pointB = new Vector2(blenderBounds.max.x, blenderBounds.max.y);

        // Check if the fruit's collider overlaps with this area
        Collider2D[] overlappingColliders = Physics2D.OverlapAreaAll(pointA, pointB);
        bool isOverlapping = overlappingColliders.Contains(fruitCollider);

        // Add a debug log to show the result
        Debug.Log("IsOverlappingBlenderJar: " + isOverlapping);

        return isOverlapping;
    }



    public void UpdateBlenderJarSprite(string fruitTag, GameObject fruit)
    {
        Debug.Log($"Updating Blender Jar Sprite for fruit: {fruitTag}");

        if (fruitTag != "Blender_Jar") // Only add fruits that are not Blender_Jar
        {
            // Ensure the fruit is not already in the list
            if (!fruitsInBlender.Contains(fruitTag))
            {
                fruitsInBlender.Add(fruitTag);
                fruitObjectsInBlender.Add(fruit);
                Debug.Log($"Fruit added: {fruitTag}. Total fruits in blender: {fruitsInBlender.Count}");

                // Disable the collider of the dropped fruit
                Collider2D fruitCollider = fruit.GetComponent<Collider2D>();
                if (fruitCollider != null)
                {
                    fruitCollider.enabled = false;
                }
            }
            else
            {
                Debug.Log($"Fruit {fruitTag} already in blender. Skipping.");
            }
        }

        // Load the appropriate sprite
        string spriteName = DetermineSpriteName();
        Debug.Log($"Sprite name determined: {spriteName}");
        Sprite newBlenderJarSprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/" + spriteName);
        if (newBlenderJarSprite != null)
        {
            blenderJarSpriteRenderer.sprite = newBlenderJarSprite;
        }
        else
        {
            Debug.LogError($"Sprite not found for combination: {spriteName}. Resetting blender.");
            ResetBlender();
            return;
        }

        // Validate fruits when appropriate
        if (juiceManager.isKikiJuice && fruitsInBlender.Count == 2)
        {
            Debug.Log("Second time and two fruits in blender. Validating fruits...");
            if (juiceController.ValidateFruit(fruitsInBlender))
            {
                juiceController.EnableBlenderCollider();
                // Start a coroutine to monitor player interaction with the blender
                juiceController.StartBlenderInteractionTimer(); // New method added in JuiceController
            }
            else
            {
                StartBirdTweenSequence();
                Debug.Log("Fruits validation failed. Starting bird tween sequence.");
            }
        }
        else if (!juiceManager.isKikiJuice && fruitsInBlender.Count == 1)
        {
            Debug.Log("First time and one fruit in blender. Validating fruit...");
            if (juiceController.ValidateFruit(fruitsInBlender))
            {
                juiceController.EnableBlenderCollider();
                // Start a coroutine to monitor player interaction with the blender
                juiceController.StartBlenderInteractionTimer(); // New method added in JuiceController
            }
            else
            {
                StartBirdTweenSequence();
                Debug.Log("Fruit validation failed. Starting bird tween sequence.");
            }
        }
    }


    private string DetermineSpriteName()
    {
        Debug.Log("Determining sprite name based on fruits in blender...");
        fruitsInBlender.Sort();
        if (juiceManager.isKikiJuice && fruitsInBlender.Count == 2)
        {
            // For Kiki's juice
            if (fruitsInBlender.Contains("Kiwi") && fruitsInBlender.Contains("SB"))
            {
                return "kiwiSB_blender";
            }
            if (fruitsInBlender.Contains("Kiwi") && fruitsInBlender.Contains("BB"))
            {
                return "bbKiwi_blender";
            }
            if (fruitsInBlender.Contains("SB") && fruitsInBlender.Contains("BB"))
            {
                return "bbSB_blender";
            }
        }
        else if (fruitsInBlender.Count == 1)
        {
            // For both Jojo's and Kiki's single fruit juice
            if (fruitsInBlender.Contains("Kiwi"))
            {
                return "kiwi_blender";
            }
            if (fruitsInBlender.Contains("SB"))
            {
                return "sb_blender";
            }
            if (fruitsInBlender.Contains("BB"))
            {
                return "bb_blender";
            }
        }
        return "default_blender";
    }

    public void UpdateJuiceSprite()
    {
        // Determine the juice sprite based on the fruits in the blender
        string spriteName = DetermineJuiceSpriteName();
        Sprite newJuiceSprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/" + spriteName);
        if (newJuiceSprite != null)
        {
            blenderJarSpriteRenderer.sprite = newJuiceSprite;
        }
    }

    public string GetJuiceSpriteName()
    {
        if (juiceManager.isKikiJuice && fruitsInBlender.Count == 2)
        {
            if (fruitsInBlender.Contains("Kiwi") && fruitsInBlender.Contains("SB"))
            {
                return "kiwiSBJuice_glass";
            }
            if (fruitsInBlender.Contains("Kiwi") && fruitsInBlender.Contains("BB"))
            {
                return "KiwiBBJuice_glass";
            }
            if (fruitsInBlender.Contains("SB") && fruitsInBlender.Contains("BB"))
            {
                return "BBSBJuice_glass";
            }
        }
        else if (!juiceManager.isKikiJuice && fruitsInBlender.Count == 1)
        {
            if (fruitsInBlender.Contains("Kiwi"))
            {
                return "kiwiJuice_glass";
            }
            if (fruitsInBlender.Contains("SB"))
            {
                return "SBJuice_glass";
            }
            if (fruitsInBlender.Contains("BB"))
            {
                return "BBJuice_glass";
            }
        }

        return "default_blender";
    }

    private string DetermineJuiceSpriteName()
    {
        if (juiceManager.isKikiJuice && fruitsInBlender.Count == 2)
        {
            if (fruitsInBlender.Contains("Kiwi") && fruitsInBlender.Contains("SB"))
            {
                return "kiwiSB_juice";
            }
            if (fruitsInBlender.Contains("Kiwi") && fruitsInBlender.Contains("BB"))
            {
                return "KiwiBB_juice";
            }
            if (fruitsInBlender.Contains("SB") && fruitsInBlender.Contains("BB"))
            {
                return "BBSB_juice";
            }
        }
        else if (!juiceManager.isKikiJuice && fruitsInBlender.Count == 1)
        {
            if (fruitsInBlender.Contains("Kiwi"))
            {
                return "kiwi_juice";
            }
            if (fruitsInBlender.Contains("SB"))
            {
                return "SB_juice";
            }
            if (fruitsInBlender.Contains("BB"))
            {
                return "BB_juice";
            }
        }
        return "default_blender";
    }

    private void StartBirdTweenSequence()
    {
        if (bird != null && birdEndPosition != null)
        {
            LeanTween.move(bird, birdEndPosition.position, 1f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
            {
                birdAnimator.SetTrigger("canTalk");

                StartCoroutine(WaitAndTweenBack(bird, birdInitialPosition));
            });
        }
    }

    private IEnumerator WaitAndTweenBack(GameObject bird, Vector3 initialPosition)
    {
        yield return new WaitForSeconds(2f);

        LeanTween.move(bird, initialPosition, 1f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
        {
            ResetBlender();
        });
    }
}
