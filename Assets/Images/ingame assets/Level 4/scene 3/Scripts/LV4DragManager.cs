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
    public SubtitleManager subtitleManager;

    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;
    public GameObject glowPrefab;
    public GameObject glowPrefab2;
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
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
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
            if (SfxAudioSource != null)
            {
                SfxAudioSource.loop = false;
                SfxAudioSource.PlayOneShot(SfxAudio1);
            }
            OnDirtyDishesDropped();
            isDragging = false;
        }
    }


    private IEnumerator PlayAnimationsWithAudio()
    {
        dirtyDishes.GetComponent<Collider2D>().enabled = false;
        yield return new WaitUntil(() => birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talk"));

        audioSource1.Play();
        subtitleManager.DisplaySubtitle("Oh no, The dishes are so dirty.", "Kiki", audioSource1.clip);

        yield return new WaitUntil(() => birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talk") == false);

        yield return new WaitForSeconds(0.5f);

        yield return new WaitUntil(() => birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Put Dishes in Sink"));

        audioSource2.Play();
        subtitleManager.DisplaySubtitle("Put the dishes in the sink!", "Kiki", audioSource2.clip);        

        // Spawn the glow prefab, scale it, wait, and destroy
        if (glowPrefab != null)
        {
            GameObject glow = Instantiate(glowPrefab, dirtyDishes.transform.position, Quaternion.identity);

            // Tween scale to 8
            LeanTween.scale(glow, Vector3.one * 14f, 0.5f).setOnComplete(() =>
            {
                StartCoroutine(FadeOutAndDestroyGlow(glow));
            });
        }

        yield return new WaitUntil(() => birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Put Dishes in Sink") == false);

        yield return new WaitForSeconds(0.5f);

        dirtyDishes.GetComponent<Collider2D>().enabled = true;
        helperHandCoroutine = StartCoroutine(HelperHandDelayTimer());
    }

    private IEnumerator FadeOutAndDestroyGlow(GameObject glow)
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds

        SpriteRenderer glowRenderer = glow.GetComponent<SpriteRenderer>();
        if (glowRenderer != null)
        {
            float fadeDuration = 1f;
            float startAlpha = glowRenderer.color.a;

            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                float normalizedTime = t / fadeDuration;
                Color newColor = glowRenderer.color;
                newColor.a = Mathf.Lerp(startAlpha, 0, normalizedTime);
                glowRenderer.color = newColor;
                yield return null;
            }

            Color finalColor = glowRenderer.color;
            finalColor.a = 0;
            glowRenderer.color = finalColor;
        }

        Destroy(glow); // Destroy the glow object
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
            birdAnimator.SetTrigger(canTalkParam);
            StartCoroutine(PlayAnimationsWithAudio());
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
        DebugHierarchy(spawnedObject.transform);
        Transform floor = spawnedObject.transform.Find("floor");
        Transform sink = spawnedObject.transform.Find("sink");
        Transform canvasGameObject = spawnedObject.transform.Find("PrefabCanvas");

        if (canvasGameObject == null)
        {
            Debug.LogError("PrefabCanvas not found in the spawned object hierarchy!");
            return;
        }

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

                        // Spawn glowPrefab2 on each dish
                        foreach (Transform dish in sink)
                        {
                            SpawnAndAnimateGlow(dish);
                        }
                        subtitleManager.DisplaySubtitle("Let's wash the dishes.", "Kiki", imageAudioSource.clip);
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

        Canvas canvas = canvasGameObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas component not found on PrefabCanvas!");
            return;
        }
        if (canvas.renderMode != RenderMode.ScreenSpaceCamera)
        {
            Debug.LogWarning("Canvas Render Mode is not Screen Space - Camera. Forcing it now.");
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
        }

        // Assign Main Camera
        if (Camera.main != null)
        {
            canvas.worldCamera = Camera.main;
            Debug.Log("Main Camera assigned to PrefabCanvas.");
        }
        else
        {
            Debug.LogError("Main Camera not found. Ensure it is tagged as 'MainCamera'.");
        }

        PrefabSpawned = true;
    }

    void SpawnAndAnimateGlow(Transform dish)
    {
        if (glowPrefab2 == null)
        {
            Debug.LogError("Glow prefab is not assigned!");
            return;
        }

        // Instantiate the glowPrefab2 and set it as a child of the dish
        GameObject glow = Instantiate(glowPrefab2, dish.position, Quaternion.identity, dish);
        glow.transform.localScale = Vector3.zero;

        // Tween the scale of the glow
        LeanTween.scale(glow, Vector3.one * 8f, 0.5f).setEase(LeanTweenType.easeOutExpo).setOnComplete(() =>
        {
            StartCoroutine(FadeOutAndDestroy(glow, 2f));
        });
    }


    IEnumerator FadeOutAndDestroy(GameObject obj, float duration)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            float elapsedTime = 0f;
            Color originalColor = sr.color;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        Destroy(obj);
    }


    // Coroutine to wait and then move the bird image back to its original position
    private IEnumerator WaitAndReturnBirdImage(RectTransform birdRectTransform)
    {
        yield return new WaitForSeconds(3f);
        LeanTween.move(birdRectTransform, new Vector2(-250, -140), 0.5f);
    }
    void DebugHierarchy(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Debug.Log($"Child of {parent.name}: {child.name}");
            DebugHierarchy(child); // Recursively print the hierarchy
        }
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
            
        });

        LeanTween.move(dirtyDishes, originalPosition, 0.5f);
        birdAnimator.SetTrigger("dishClean");
        audioSource3.Play();
        subtitleManager.DisplaySubtitle("Yohoo...! so clean.", "Kiki", audioSource3.clip);
        dishWashingManager.allDishesWashed = false;
        StartCoroutine(TriggerDishArrangeWithDelay());
    }

    IEnumerator TriggerDishArrangeWithDelay()
    {
        yield return new WaitForSeconds(3f); // 2-second delay
        birdAnimator.SetTrigger("dishArrange");
        audioSource4.Play();
        subtitleManager.DisplaySubtitle("Let's put the dishes on the rack.", "Kiki", audioSource4.clip);

        foreach (Transform child in dirtyDishes.transform)
        {
            // Spawn glowPrefab at each child's position
            if (glowPrefab != null)
            {
                GameObject glow = Instantiate(glowPrefab, child.position, Quaternion.identity);

                // Tween scale to 8
                LeanTween.scale(glow, Vector3.one * 8f, 0.5f).setOnComplete(() =>
                {
                    StartCoroutine(FadeOutAndDestroyGlow(glow));
                });
            }

            Collider2D childCollider = child.GetComponent<Collider2D>();
            if (childCollider != null)
            {
                childCollider.enabled = true; // Enable the collider
            }
        }

        Debug.Log("Calling HelperHandDelayTimerforchild from TweenDirtyDishesBack.");
        StartCoroutine(HelperHandDelayTimerforchild());
    }


    private IEnumerator HelperHandDelayTimerforchild()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Calling HelperHandDelayTimerforchild from TweenDirtyDishesBack. for all child");
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


