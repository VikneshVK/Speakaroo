using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LV4DragManager : MonoBehaviour
{
    public GameObject dirtyDishes;
    public GameObject foam;
    public GameObject yourPrefab;
    public Vector3 spawnPosition;
    public Animator birdAnimator;
    public string canTalkParam = "canTalk";
    public LVL4Sc3HelperHand helperHandManager;
    public AudioSource imageAudioSource;
    private List<DishdragController> dishControllers = new List<DishdragController>();

    private DishWashingManager dishWashingManager;
    public bool tweenComplete;
    private Vector3 originalPosition;
    private Vector3 offset;
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();
    private bool isDragging = false;
    public bool PrefabSpawned;

    private Coroutine helperHandCoroutine;
    /*private int currentIndex = 0;*/

    public AudioSource audioSource1; // Drag Audio 1 here
    public AudioSource audioSource2; // Drag Audio 2 here
    public AudioSource audioSource3;
    public AudioSource audioSource4;    
    public TextMeshProUGUI subtitleText;
    public bool timerRunning;

    void Start()
    {
        birdAnimator.SetTrigger(canTalkParam);
        PrefabSpawned = false;
        originalPosition = dirtyDishes.transform.position;
        tweenComplete = false;

        // Store original scale of dirty dishes and their children
        foreach (Transform child in dirtyDishes.transform)
        {
            originalScales[child] = child.localScale;
        }
        timerRunning = false;

        StartCoroutine(PlayAnimationsWithAudio());
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

    
    private IEnumerator PlayAnimationsWithAudio()
    {
        yield return new WaitUntil(() => birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talk"));

        audioSource1.Play();
        StartCoroutine(RevealTextWordByWord("Oh no, the dishes are so Dirty", 0.5f));

        yield return new WaitUntil(() => birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talk") == false);

        yield return new WaitForSeconds(0.5f);

        /*birdAnimator.SetTrigger("next");*/

        yield return new WaitUntil(() => birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Put Dishes in Sink"));
        
        audioSource2.Play();
        StartCoroutine(RevealTextWordByWord("Please put the dishes in the sink!", 0.5f)); // Modified subtitle for clarity

        yield return new WaitUntil(() => birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Put Dishes in Sink") == false);

        yield return new WaitForSeconds(0.5f);

        dirtyDishes.GetComponent<Collider2D>().enabled = true;
        helperHandCoroutine = StartCoroutine(HelperHandDelayTimer());
    }

    private IEnumerator HelperHandDelayTimer()
    {
        yield return new WaitForSeconds(helperHandManager.helperHandDelay);

        helperHandManager.SpawnHelperHand(dirtyDishes.transform.position, foam.transform.position);
    }

    void OnDirtyDishesDropped()
    {
        if (helperHandCoroutine != null)
        {
            StopCoroutine(helperHandCoroutine);
        }

        if (IsDroppedOnFoam(dirtyDishes))
        {
            LeanTween.scale(dirtyDishes, Vector3.zero, 0.5f).setOnComplete(() =>
            {
                GameObject spawnedObject = Instantiate(yourPrefab, spawnPosition, Quaternion.identity);

                dishWashingManager = spawnedObject.GetComponent<DishWashingManager>();

                SpawnPrefab(spawnedObject);

                helperHandManager.StopHelperHand();
            });
        }
        else
        {
            
            dirtyDishes.transform.position = originalPosition;

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
                    tweenComplete = true;
                }

                // Get reference to the bird image GameObject by tag and Animator component
                GameObject birdImage = GameObject.FindWithTag("Bird");
                if (birdImage != null)
                {
                    RectTransform birdRectTransform = birdImage.GetComponent<RectTransform>();
                    Animator birdAnimator = birdImage.GetComponent<Animator>();

                    // Tween bird image to enter the viewport, trigger "instruction", then return
                    LeanTween.move(birdRectTransform, new Vector2(-250, 250), 0.5f).setOnComplete(() =>
                    {
                        birdAnimator.SetTrigger("instruction");
                        imageAudioSource.Play();
                        StartCoroutine(RevealTextWordByWord("Lets Wash the Dishes", 0.5f));
                        StartCoroutine(WaitAndReturnBirdImage(birdRectTransform));
                        timerRunning = true;
                    });
                }
                else
                {
                    Debug.LogError("Bird image GameObject with tag 'Bird' not found.");
                }
            });
        });

        PrefabSpawned = true;
    }

    // Coroutine to wait and then move the bird image back to its original position
    private IEnumerator WaitAndReturnBirdImage(RectTransform birdRectTransform)
    {
        yield return new WaitForSeconds(3f);
        LeanTween.move(birdRectTransform, new Vector2(-250, -140), 0.5f);
    }


    void TweenDirtyDishesBack()
    {
        LeanTween.scale(dirtyDishes, Vector3.one * 1.25f, 0.5f).setOnComplete(() =>
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

                /*// Enable the Collider2D component on each child GameObject
                Collider2D childCollider = child.GetComponent<Collider2D>();
                if (childCollider != null)
                {
                    childCollider.enabled = true; // Enable the collider
                }*/
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
        birdAnimator.SetTrigger("dishClean");
        audioSource3.Play();
        StartCoroutine(RevealTextWordByWord("Yohoo...! so Clean", 0.5f));
        dishWashingManager.allDishesWashed = false;
        StartCoroutine(TriggerDishArrangeWithDelay());
    }

    IEnumerator TriggerDishArrangeWithDelay()
    {
        yield return new WaitForSeconds(3f); // 2-second delay
        birdAnimator.SetTrigger("dishArrange");
        audioSource4.Play();
        StartCoroutine(RevealTextWordByWord("lets put the dishes on the rack", 0.5f));

        foreach (Transform child in dirtyDishes.transform)
        {            
            Collider2D childCollider = child.GetComponent<Collider2D>();
            if (childCollider != null)
            {
                childCollider.enabled = true; // Enable the collider
            }
        }
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

    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(true);

        string[] words = fullText.Split(' ');

        // Reveal words one by one
        for (int i = 0; i < words.Length; i++)
        {
            subtitleText.text = string.Join(" ", words, 0, i + 1);
            yield return new WaitForSeconds(delayBetweenWords);
        }
        subtitleText.text = "";
    }

}


