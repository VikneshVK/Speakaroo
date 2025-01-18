using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DishdragController : MonoBehaviour
{
    public static List<DishdragController> allDishControllers = new List<DishdragController>();    
    public LVL4Sc3HelperHand helperHandManager;
    public LV4DragManager dragManager;
    public float helperHandDelay;
    public bool isDroppedCorrectly = false;
    public AudioSource audio1;
    public SubtitleManager subtitleManager;

    public Sprite newSprite;
    public Animator birdAnimator;

    public Transform glassDropTarget1; // Drop location for dirty-glass1 and dirty-glass2
    public Transform glassDropTarget2;
    public Transform plateDropTarget1; // Drop location for dirty-plate1 and dirty-plate2
    public Transform plateDropTarget2;
    public Transform bowlDropTarget1; // Drop location for dirty-bowl1 and dirty-bowl2
    public Transform bowlDropTarget2;

    private static HashSet<Transform> usedBowlTargets = new HashSet<Transform>();
    private static HashSet<Transform> usedGlassTargets = new HashSet<Transform>();
    private static HashSet<Transform> usedPlateTargets = new HashSet<Transform>();

    public static int dishesArranged = 0;

    private AudioSource SfxAudioSource;
    public AudioSource correctAudioSource;
    public AudioSource wrongAudioSource;
    public AudioClip SfxAudio1;
    public AudioClip SfxAudio2;
    public GameObject glowPrefab;
    private Vector3 startPosition;
    private bool isDragging = false;
    private Coroutine helperHandCoroutine;

    private void Start()
    {
        startPosition = transform.position;
        allDishControllers.Add(this);
        GetComponent<Collider2D>().enabled = false;
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
    }

    private void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseDown();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            OnMouseDrag();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            OnMouseUp();
        }
    }

    private void OnMouseDown()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);
        if (hitCollider != null && hitCollider.gameObject == gameObject)
        {
            isDragging = true;

            // Change the sorting order to 5 when dragging starts
            GetComponent<SpriteRenderer>().sortingOrder = 5;

            if (helperHandManager != null)
            {
                helperHandManager.StopHelperHand();
            }
        }
    }

    private void OnMouseDrag()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePosition.x, mousePosition.y, transform.position.z);
    }

    private void OnMouseUp()
    {
        isDragging = false;

        // Reset the sorting order when dragging stops
        GetComponent<SpriteRenderer>().sortingOrder = 2;

        if (gameObject.name.Contains("Bowl"))
        {
            Transform target = GetAvailableTarget(bowlDropTarget1, bowlDropTarget2, usedBowlTargets);
            if (target != null)
            {
                usedBowlTargets.Add(target); // Mark target as used
                HandleCorrectDrop(target, isBowl: true);
            }
            else
            {
                StartCoroutine(HandleWrongDropSequence());
            }
        }
        else if (gameObject.name.Contains("glass"))
        {
            Transform target = GetAvailableTarget(glassDropTarget1, glassDropTarget2, usedGlassTargets);
            if (target != null)
            {
                usedGlassTargets.Add(target); // Mark target as used
                HandleCorrectDrop(target, isGlass: true);
            }
            else
            {
                StartCoroutine(HandleWrongDropSequence());
            }
        }
        else if (gameObject.name.Contains("plate"))
        {
            Transform target = GetAvailableTarget(plateDropTarget1, plateDropTarget2, usedPlateTargets);
            if (target != null)
            {
                usedPlateTargets.Add(target); // Mark target as used
                HandleCorrectDrop(target, isPlate: true);
            }
            else
            {
                StartCoroutine(HandleWrongDropSequence());
            }
        }
    }
    private void HandleCorrectDrop(Transform target, bool isGlass = false, bool isPlate = false, bool isBowl = false)
    {
        if (helperHandCoroutine != null)
        {
            StopCoroutine(helperHandCoroutine);
        }

        // Clear existing helper hand and its animations/tweens if any
        if (helperHandManager != null)
        {
            helperHandManager.StopHelperHand();
        }

        birdAnimator.SetTrigger("correct");

        if (correctAudioSource != null)
        {
            correctAudioSource.Play();
        }

        if (SfxAudioSource != null)
        {
            SfxAudioSource.PlayOneShot(SfxAudio1);
        }

        OnDropped(true, isGlass, isPlate, isBowl, target);
    }
    private IEnumerator HandleWrongDropSequence()
    {
        // Trigger wrong animation
        birdAnimator.SetTrigger("wrong");

        // Play wrong audio
        if (wrongAudioSource != null)
        {
            wrongAudioSource.Play();
            yield return new WaitForSeconds(wrongAudioSource.clip.length);
        }

        // Call HandleWrongDrop after the wrong animation and audio are complete
        HandleWrongDrop();
    }

    private void HandleWrongDrop()
    {
        ResetPosition();

        birdAnimator.SetTrigger("dishArrange");

        dragManager.audioSource4.Play(); 

        GameObject glow = Instantiate(glowPrefab, transform.position, Quaternion.identity);
        glow.transform.localScale = Vector3.zero;
        LeanTween.scale(glow, Vector3.one * 8f, 0.5f).setOnComplete(() =>
        {
            StartCoroutine(FadeOutAndDestroy(glow, 2f));
        });
    }

    private IEnumerator FadeOutAndDestroy(GameObject glow, float delay)
    {
        yield return new WaitForSeconds(delay);

        SpriteRenderer glowRenderer = glow.GetComponent<SpriteRenderer>();
        Color originalColor = glowRenderer.color;

        // Fade out
        float fadeDuration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            glowRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        Destroy(glow);
    }


    private void OnDropped(bool correctDrop, bool isGlass = false, bool isPlate = false, bool isBowl = false, Transform target = null)
    {
        SpriteRenderer targetSpriteRenderer = null;

        if (correctDrop)
        {
            if (target != null)
            {
                targetSpriteRenderer = target.GetComponent<SpriteRenderer>();
                if (targetSpriteRenderer != null)
                {
                    targetSpriteRenderer.sprite = newSprite;
                    if (isPlate)
                    {
                        targetSpriteRenderer.transform.localScale = Vector3.one * 0.2f;
                    }
                }
            }

            isDroppedCorrectly = true;
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            gameObject.GetComponent<Collider2D>().enabled = false;
            dishesArranged++;
            CheckDishesArranged();

            if (helperHandManager != null)
            {
                helperHandManager.StopHelperHand();
            }

            StartHelperHandCheckForAll();
        }
        else
        {
            StartHelperHandTimer();
        }
    }

    private Transform GetAvailableTarget(Transform target1, Transform target2, HashSet<Transform> usedTargets)
    {
        if (!usedTargets.Contains(target1))
        {
            Debug.Log($"Checking Target1 {target1.name} for {gameObject.name}. Distance: {Vector3.Distance(transform.position, target1.position)}");
            if (Vector3.Distance(transform.position, target1.position) < 1.0f)
            {
                Debug.Log($"Target1 {target1.name} is valid for {gameObject.name}");
                return target1;
            }
        }

        if (!usedTargets.Contains(target2))
        {
            Debug.Log($"Checking Target2 {target2.name} for {gameObject.name}. Distance: {Vector3.Distance(transform.position, target2.position)}");
            if (Vector3.Distance(transform.position, target2.position) < 1.0f)
            {
                Debug.Log($"Target2 {target2.name} is valid for {gameObject.name}");
                return target2;
            }
        }

       
        return null;
    }

    private void CheckDishesArranged()
    {
        if (dishesArranged == 6)
        {
            StartCoroutine(LevelEnd());
        }
    }

    private IEnumerator LevelEnd()
    {
        yield return new WaitForSeconds(2f);
        birdAnimator.SetTrigger("LvlComplete");
        audio1.Play();
        subtitleManager.DisplaySubtitle("WOW! Thankyou friend the kitchen looks so clean.", "Kiki", audio1.clip);
        Debug.Log("Level Completed! All dishes arranged.");
    }

    private void ResetPosition()
    {
        SfxAudioSource.PlayOneShot(SfxAudio2);
        transform.position = startPosition;
    }

    // Start the helper hand delay timer
    public void StartHelperHandTimer()
    {
        if (helperHandManager == null)
        {
            Debug.LogError("Helper Hand Manager is not assigned for " + gameObject.name);
            return;
        }
        if (!isDroppedCorrectly)
        {
            StartCoroutine(TriggerHelperHandWithDelay());
        }
    }
    private IEnumerator TriggerHelperHandWithDelay()
    {
        Debug.Log("TriggerHelperHandWithDelay started for " + gameObject.name);

        // Initial delay before checking if the object is correctly dropped
        yield return new WaitForSeconds(helperHandDelay);

        // Check again if the object is correctly dropped before spawning helper hand
        if (!isDroppedCorrectly)
        {
            Transform target = null;

            if (gameObject.name.Contains("Bowl"))
            {
                target = GetAvailableTarget(bowlDropTarget1, bowlDropTarget2, usedBowlTargets);
                if (target == null && !usedBowlTargets.Contains(bowlDropTarget1))
                {
                    target = bowlDropTarget1; // Fallback to unused target
                }
                else if (target == null && !usedBowlTargets.Contains(bowlDropTarget2))
                {
                    target = bowlDropTarget2; // Fallback to unused target
                }
            }
            else if (gameObject.name.Contains("glass"))
            {
                target = GetAvailableTarget(glassDropTarget1, glassDropTarget2, usedGlassTargets);
                if (target == null && !usedGlassTargets.Contains(glassDropTarget1))
                {
                    target = glassDropTarget1; // Fallback to unused target
                }
                else if (target == null && !usedGlassTargets.Contains(glassDropTarget2))
                {
                    target = glassDropTarget2; // Fallback to unused target
                }
            }
            else if (gameObject.name.Contains("plate"))
            {
                target = GetAvailableTarget(plateDropTarget1, plateDropTarget2, usedPlateTargets);
                if (target == null && !usedPlateTargets.Contains(plateDropTarget1))
                {
                    target = plateDropTarget1; // Fallback to unused target
                }
                else if (target == null && !usedPlateTargets.Contains(plateDropTarget2))
                {
                    target = plateDropTarget2; // Fallback to unused target
                }
            }

            if (target != null)
            {
                Debug.Log("Target found: " + target.name + " for " + gameObject.name);
                helperHandManager.SpawnHelperHand(transform.position, target.position);
            }
            else
            {
                Debug.LogWarning("No valid target found for " + gameObject.name);
            }
        }
    }

    public static void StartHelperHandCheckForAll()
    {
        Debug.Log("checking Dish Controller");
        foreach (var dishController in allDishControllers)
        {
            if (!dishController.isDroppedCorrectly)
            {
                Debug.Log("Dish Controller identified calling helper timer");
                dishController.StartHelperHandTimer();
                break;
            }
        }
    }
}
