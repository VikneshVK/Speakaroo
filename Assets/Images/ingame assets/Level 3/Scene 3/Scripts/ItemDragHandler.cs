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
    private Animator birdAnimator;
    public List<GameObject> objectsToEnable;

    public static int clothesOnLine ;  // Variable to track the number of cloth and toy objects on the line
    public static int toysDryed ;      // Static variable to track the number of dry toys placed in the cloth basket
    private bool isTransitionComplete = false; // Tracks if the dry-to-wet transition is complete
    private bool tweenstarted = true;

    public Kiki_actions kikiActions;
    public Jojo_action1 jojoActions;
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
    public GameObject Bird;
    public SpriteRenderer clothBasketSpriteRenderer; // Reference to the sprite renderer of the cloth basket
    public AudioSource finalaudio;
    private bool finalaudioplayed;
    private bool firstHalfAudioPlayed;
    private bool firstHalfAnimationPlayed;
    private bool collidersEnabled;
    private float originalMinSpeed;
    private float originalMaxSpeed;
    public AudioSource firstHalfAudio;
    private bool isAudioPlaying = false;
    private bool isCorrectDrop = false;
    public bool HasInteracted { get; private set; } = false;

    public AudioClip feedbackAudio1;
    public AudioClip feedbackAudio2;
    public AudioClip feedbackAudio3;
    public AudioSource feedbackAudioSource;

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
        birdAnimator = Bird.GetComponent<Animator>();
        DisableToyPositionColliders();
        clothesOnLine = 6;
        toysDryed = 0;
        Debug.Log("clothsOnLine" + clothesOnLine);
        Debug.Log("Toys Dryed" + toysDryed);
        finalaudioplayed = false;
        firstHalfAudioPlayed = false;
        firstHalfAnimationPlayed = false;
        collidersEnabled = false;


        feedbackAudio1 = Resources.Load<AudioClip>("Audio/FeedbackAudio/Audio1");
        feedbackAudio2 = Resources.Load<AudioClip>("Audio/FeedbackAudio/Audio2");
        feedbackAudio3 = Resources.Load<AudioClip>("Audio/FeedbackAudio/Audio3");

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
            CheckAndEnableColliders();
        }

        // Check if all toys have been dried
        if (clothesOnLine == 0 && toysDryed == 3)
        {
            boyAnimator.SetBool("allDryed", true);
            if (!finalaudioplayed)
            {
                finalaudio.Play();
                finalaudioplayed = true;
            }
        }

        // After the transition (removing dry clothes and adding wet toys) is complete
        if (clothesOnLine == 6 && isTransitionComplete)
        {
            // Now call AreAllItemsWet() to check if all items are wet
            if (AreAllItemsWet() && tweenstarted && !firstHalfAudioPlayed)
            {
                birdAnimator.SetTrigger("1stHalf");          
                firstHalfAudioPlayed = true;
                firstHalfAudio.Play();
                
            }
        }
        
        if(firstHalfAudioPlayed && birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("1st Half") &&
            birdAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f && !firstHalfAnimationPlayed)
        {
            firstHalfAnimationPlayed = true;
            kikiActions.MoveOffScreen();
            jojoActions.MoveOffScreen();
            StartCoroutine(SetAllDry());
        }
        
    }

    private void OnMouseDown()
    {
        if ((gameObject.tag == "Cloth" && isDry) || (gameObject.tag == "Toy" && gameObject.GetComponent<Collider2D>().enabled))
        {
            isDragging = true;
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            offset = transform.position - mousePosition;
            offset.z = 0;

            selectedTarget = DetermineDropTarget();
            if (selectedTarget != null)
            {
                originalTargetScale = selectedTarget.localScale;
            }
        }
    }


    private void OnMouseUp()
    {
        isDragging = false;
        if (selectedTarget != null)
        {
            selectedTarget.localScale = originalTargetScale;
        }

        isCorrectDrop = false;

        if (IsOverlapping(selectedTarget))
        {
            Collider2D targetCollider = selectedTarget.GetComponent<Collider2D>();
            if (targetCollider != null)
            {
                transform.position = targetCollider.bounds.center;
            }

            GetComponent<Collider2D>().enabled = false;

            HasInteracted = true; // Mark as interacted

            if (gameObject.tag == "Cloth")
            {
                Destroy(gameObject);
                UpdateClothBasketSprite();
                DecrementClothesCount();
                isCorrectDrop = true;
            }
            else if (gameObject.tag == "Toy")
            {
                if (isDry && (selectedTarget == teddyInitialPosition.transform || selectedTarget == dinoInitalPosition.transform || selectedTarget == bunnyInitialPosition.transform))
                {
                    IncrementToysDryed();
                    DecrementClothesCount();
                    DisableToyPositionCollider(gameObject.name);
                    isCorrectDrop = true;
                }
                else if (!isDry && selectedTarget == GetClotheslinePosition(gameObject.name))
                {
                    IncrementClothesCount();
                    isCorrectDrop = true;
                }

                if (clothesOnLine == 6)
                {
                    isTransitionComplete = true;
                }
            }
        }
        else
        {
            StartCoroutine(BlinkInRedThenReset());
        }

        if((clothesOnLine != 6 )|| (clothesOnLine == 0 && toysDryed == 3))
        {
            if (isCorrectDrop)
            {
                // Shuffle between feedback1 and feedback2 for correct drop
                string feedbackTrigger = Random.value > 0.5f ? "feedback1" : "feedback2";
                birdAnimator.SetTrigger(feedbackTrigger);

                if (feedbackTrigger == "feedback1")
                {
                    PlayFeedbackAudio(feedbackAudio1);
                }
                else
                {
                    PlayFeedbackAudio(feedbackAudio2);
                }
            }
            else
            {
                // Trigger feedback3 for incorrect drop
                birdAnimator.SetTrigger("feedback3");
                PlayFeedbackAudio(feedbackAudio3);
            }
        }
        
    }
    private IEnumerator BlinkInRedThenReset()
    {
        // Get the SpriteRenderer component
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on this GameObject!");
            yield break;
        }

        Color originalColor = spriteRenderer.color;

        Debug.Log("Starting blink effect");

        // Blink 2 times
        for (int i = 0; i < 2; i++)
        {
            // Set color to red
            Debug.Log("Blinking red");
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.2f);

            // Set back to original color
            Debug.Log("Blinking back to original color");
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.2f);
        }

        Debug.Log("Blinking complete, resetting position");

        // After blinking, reset the position
        Collider2D startCollider = null;
        if (isDry)
        {
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
            startPosition = gameObject.name switch
            {
                "wet kuma" => teddyResetPosition.transform.position,
                "wet dino" => dinoResetPosition.transform.position,
                "wet bunny" => bunnyResetPosition.transform.position,
                _ => startPosition
            };
        }

        if (startCollider != null)
        {
            startPosition = startCollider.bounds.center;
        }

        // Reset position after blinking
        transform.position = startPosition;
    }



    private void PlayFeedbackAudio(AudioClip audioClip)
    {
        if (feedbackAudioSource != null && audioClip != null)
        {
            feedbackAudioSource.clip = audioClip;
            feedbackAudioSource.Play();
            isAudioPlaying = true;
            Debug.Log("Starting CheckIfAudioFinished coroutine for: " + audioClip.name);
            ResetHelperHandTimer(isCorrectDrop);
        }
        else
        {
            Debug.LogError("AudioSource or AudioClip is missing!");
        }
    }

    

    private void ResetHelperHandTimer(bool isCorrectDrop)
    {
        Debug.Log("helperhand reset is called");
        HelpHandController helperHand = FindObjectOfType<HelpHandController>();
        if (helperHand != null)
        {
            // Log to check interaction status
            Debug.Log("Interacted with: " + gameObject.name + " | Correct drop: " + isCorrectDrop);

            helperHand.OnObjectInteracted(gameObject, isCorrectDrop);
        }
        else
        {
            Debug.LogError("HelpHandController not found.");
        }
    }



    private void CheckAndEnableColliders()
    {
        if (!collidersEnabled)
        {
            foreach (GameObject obj in objectsToEnable)
            {
                ItemDragHandler item = obj.GetComponent<ItemDragHandler>();
                if (item != null)
                {
                    Collider2D collider = obj.GetComponent<Collider2D>();
                    if (collider != null)
                    {
                        collider.enabled = true;
                    }
                }
            }
            collidersEnabled = true;

            // Notify the HelpHandController for Jojo's action
            HelpHandController helperHand = FindObjectOfType<HelpHandController>();
            if (helperHand != null)
            {
                helperHand.StartHelperHandRoutineForJojo(objectsToEnable.ToArray());
            }
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

        if (clothesOnLine < 2)
        {
            spriteName = "laundry-pile";
        }
        else if (clothesOnLine == 2)
        {
            spriteName = "laundry-pile-2";
        }
        else if (clothesOnLine == 4)
        {
            spriteName = "laundry-pile-3";
        }
        else if (clothesOnLine == 6)
        {
            spriteName = "laundry-pile-4";
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
                    return isDry ? teddyInitialPosition.transform : teddyPosition;
                case "wet dino":
                    return isDry ? dinoInitalPosition.transform : dinoPosition;
                case "wet bunny":
                    return isDry ? bunnyInitialPosition.transform : bunnyPosition;
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
