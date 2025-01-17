using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using TMPro;

public class SpriteChangeController : MonoBehaviour
{
    public SpriteRenderer blenderJarSpriteRenderer;
    public List<string> fruitsInBlender = new List<string>();
    public List<GameObject> fruitObjectsInBlender = new List<GameObject>();
    private JuiceController juiceController;
    public JuiceManager juiceManager;
    private bool validationStatus;
    private bool animationProcess;
    public bool blenderInteractable;


    [Header("Bird Settings")]
    public GameObject bird;
    public Transform birdEndPosition;
    public Animator birdAnimator;
    public Vector3 birdInitialPosition;


    [Header("Tween Settings")]
    public LVL4Sc2AudioManager audioManager;
    public SubtitleManager subtitleManager;
    public AudioClip audio1;
    public AudioClip audio2;

    private string subtitle1 = "Turn on the Blender";
    private string subtitle2 = "Not Sure about that";

    void Start()
    {
        blenderJarSpriteRenderer = GameObject.FindGameObjectWithTag("Blender_Jar").GetComponent<SpriteRenderer>();
        juiceController = FindObjectOfType<JuiceController>();
        

        if (bird != null)
        {
            birdInitialPosition = bird.transform.position;
            
        }
        validationStatus = false;
        animationProcess = false;

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
    public void ResetGlassSprites()
    {
        Sprite defaultGlassSprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/glass");
        if (defaultGlassSprite == null)
        {
            Debug.LogWarning("Default glass sprite not found!");
            return;
        }

        // Reset Glass 1
        SpriteRenderer glass1SpriteRenderer = GameObject.FindGameObjectWithTag("Glass1").GetComponent<SpriteRenderer>();
        if (glass1SpriteRenderer != null)
        {
            glass1SpriteRenderer.sprite = defaultGlassSprite;
        }
        else
        {
            Debug.LogWarning("Glass1 SpriteRenderer is missing.");
        }

        // Reset Glass 2
        SpriteRenderer glass2SpriteRenderer = GameObject.FindGameObjectWithTag("Glass2").GetComponent<SpriteRenderer>();
        if (glass2SpriteRenderer != null)
        {
            glass2SpriteRenderer.sprite = defaultGlassSprite;
        }
        else
        {
            Debug.LogWarning("Glass2 SpriteRenderer is missing.");
        }

        Debug.Log("Glass sprites reset to default.");
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

    private void DisableAllFruitColliders()
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
                fruitCollider.enabled = false;
            }
        }

        Debug.Log("All fruit colliders disabled.");
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

        // Corrected condition for Kiki's juice and Jojo's juice
        if (juiceManager.isKikiJuice && fruitsInBlender.Count == 2)
        {
            DisableAllFruitColliders();
            animationProcess = false;
            Debug.Log("Kiki's juice mode: Two fruits in blender. Validating fruits...");

            if (juiceController.ValidateFruit(fruitsInBlender) && !animationProcess)
            {
                validationStatus = true;
                StartBirdTweenSequence("BlenderOn", audio1, subtitle1);
            }
            else
            {
                validationStatus = false;
                StartBirdTweenSequence("Kiki_negativeFeedback", audio2, subtitle2);
                StartCoroutine(CallTriggerBirdAfterDelay(4f));
                Debug.Log("Fruits validation failed for Kiki's juice. Starting bird tween sequence.");
            }
        }
        else if (!juiceManager.isKikiJuice && fruitsInBlender.Count == 1)
        {
            DisableAllFruitColliders();
            animationProcess = false;
            Debug.Log("Jojo's juice mode: One fruit in blender. Validating fruit...");

            if (juiceController.ValidateFruit(fruitsInBlender) && !animationProcess)
            {
                validationStatus = true;
                StartBirdTweenSequence("BlenderOn", audio1, subtitle1);
            }
            else
            {
                /*juiceManager.hasTriggeredMultipleFruitAnimation = false;
                juiceManager.hasTriggeredSingleFruitAnimation = false;*/
                validationStatus = false;
                StartBirdTweenSequence("Kiki_negativeFeedback", audio2, subtitle2);
                StartCoroutine(CallTriggerBirdAfterDelay(4f));
                Debug.Log("Fruits validation failed for Jojo's juice. Starting bird tween sequence.");
            }
        }
    }

    private IEnumerator CallTriggerBirdAfterDelay(float delay)
    {
        Debug.Log($"Waiting for {delay} seconds before triggering bird animation...");
        yield return new WaitForSeconds(delay); // Wait for the tween and audio duration
        juiceManager.hasTriggeredMultipleFruitAnimation = false;
        juiceManager.hasTriggeredSingleFruitAnimation = false;
        yield return StartCoroutine(Triggerbird());
    }
    private IEnumerator Triggerbird()
    {
        Debug.Log("Triggerbird() called");

        if (juiceManager != null)
        {
            Debug.Log("juiceManager is not null. Waiting for StartBirdTweenSequence to complete...");

            // Wait for the StartBirdTweenSequence to finish
            yield return new WaitForSeconds(4f);

            // Avoid race conditions by checking flags again
            if (!juiceManager.hasTriggeredSingleFruitAnimation && !juiceManager.hasTriggeredMultipleFruitAnimation)
            {
                juiceManager.TriggerBirdAnimation();
                Debug.Log("TriggerBirdAnimation() successfully triggered after waiting.");
            }
        }
        else
        {
            Debug.LogError("juiceManager is null. Cannot call TriggerBirdAnimation()");
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

    public string GetGlassSpriteName()
    {
        if (juiceManager.isKikiJuice && fruitsInBlender.Count == 2)
        {
            if (fruitsInBlender.Contains("Kiwi") && fruitsInBlender.Contains("SB"))
            {
                return "StrawKiwi_Tumblr";
            }
            if (fruitsInBlender.Contains("Kiwi") && fruitsInBlender.Contains("BB"))
            {
                return "KiwiBlue_Tumblr";
            }
            if (fruitsInBlender.Contains("SB") && fruitsInBlender.Contains("BB"))
            {
                return "BlueStraw_Tumblr";
            }
        }
        else if (!juiceManager.isKikiJuice && fruitsInBlender.Count == 1)
        {
            if (fruitsInBlender.Contains("Kiwi"))
            {
                return "kiwi_Tumblr";
            }
            if (fruitsInBlender.Contains("SB"))
            {
                return "strawberry_Tumblr";
            }
            if (fruitsInBlender.Contains("BB"))
            {
                return "blueberry_Tumblr";
            }
        }

        return "gls";
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



    private void StartBirdTweenSequence(string animationTrigger, AudioClip audioClip, string subtitleTextContent)
    {
        Debug.Log($"Starting Tween Sequence: AnimationTrigger={animationTrigger}, AudioClip={audioClip.name}");

        RectTransform birdRectTransform = bird.GetComponent<RectTransform>();
        if (birdRectTransform == null)
        {
            Debug.LogError("Bird does not have a RectTransform component!");
            return;
        }

        // Set initial position (off-screen, anchored position)
        birdRectTransform.anchoredPosition = new Vector2(-1300, 21f);

        // Tween bird to the end position
        LeanTween.value(-1300f, -790f, 1f).setEase(LeanTweenType.easeInOutQuad).setOnUpdate((float x) =>
        {
            birdRectTransform.anchoredPosition = new Vector2(x, 21f);
        }).setOnComplete(() =>
        {
            Debug.Log("Tween to end position complete!");
            birdAnimator.SetTrigger(animationTrigger);
            audioManager.PlayAudio(audioClip);

            // Start subtitle display coroutine
            subtitleManager.DisplaySubtitle(subtitleTextContent, "Kiki", audioClip);
            StartCoroutine(WaitAndTweenBack(birdRectTransform));
        });

        
    }



    private IEnumerator WaitAndTweenBack(RectTransform birdRectTransform)
    {
        yield return new WaitForSeconds(2f);

        // Tween bird back to its initial position
        LeanTween.value(-790f, -1300f, 1f).setEase(LeanTweenType.easeInOutQuad).setOnUpdate((float x) =>
        {
            birdRectTransform.anchoredPosition = new Vector2(x, 21f);
            birdInitialPosition = birdRectTransform.anchoredPosition;
        }).setOnComplete(() =>
        {
            if (validationStatus)
            {
                juiceController.EnableBlenderCollider();
                juiceController.StartBlenderInteractionTimer();
            }
            else
            {
                ResetBlender();
                juiceManager.ResetUIImages();
            }            
            animationProcess = true;

            
        });
    }

    public Sprite GetDefaultBlenderSprite()
    {
        return Resources.Load<Sprite>("Images/LVL 4 scene 2/default_blender");
    }

    private IEnumerator WaitAndTweenBack(GameObject bird, Vector3 initialPosition)
    {
        yield return new WaitForSeconds(2f);

        LeanTween.value(-790f, -1300f, 1f).setEase(LeanTweenType.easeInOutQuad).setOnUpdate((float x) =>
        {
            ResetBlender();
        });
    }    
}