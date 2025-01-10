using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class DishController : MonoBehaviour
{
    public GameObject maskPrefab;
    public ScrubberController scrubberController;
    public TextMeshProUGUI subtitleText;
    public GameObject glowPrefab;
    public GameObject blackoutPanel;

    private bool isSpawning = false;
    private bool isTweening = false; // Flag to ensure the tween occurs only once
    private GameObject instantiatedMask;
    private GameObject instantiatedGlow;
    public Image jojoBanner;
    public float fadeDuration = 0.5f;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private bool isScrubbing = false;

    public bool isDishSelected = false;
    public bool dishCleaned;
    public bool scrubbertimer;

    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;

    private Dictionary<Transform, int> originalSortingOrders = new Dictionary<Transform, int>();
    private Collider2D objectCollider;
    private DishController[] allDishes;
    private DishWashingManager dishWashingManager;
    private LV4DragManager LV4DragManager;
    private AudioSource audioSource;
    private Coroutine subtitleCoroutine;

    private float timer = 0f;
    public bool helperActive = false;
    private void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        originalPosition = (transform.position == Vector3.zero) ? transform.localPosition : transform.position;
        originalScale = (transform.localScale == Vector3.zero) ? Vector3.one : transform.localScale;

        objectCollider = GetComponent<Collider2D>();
        dishWashingManager = FindObjectOfType<DishWashingManager>();
        LV4DragManager = FindObjectOfType<LV4DragManager>();
        dishCleaned = false;
        scrubbertimer = false;
        StoreOriginalSortingOrders();
        allDishes = FindObjectsOfType<DishController>();
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
        if (jojoBanner != null) jojoBanner.color = new Color(1, 1, 1, 0);
    }

    private void Update()
    {
        if (LV4DragManager.timerRunning)
        {
            timer += Time.deltaTime;

            if (timer >= 7f)
            {
                if (!helperActive)
                {
                    Debug.Log($"{gameObject.name} Update: Spawning glow for helperActive = false");
                    StartCoroutine(SpawnGlowEffect());
                }
                timer = 0f;
            }
        }
    }

    public void OnMouseDown()
    {
        if (isSpawning || isScrubbing || isTweening) return; // Check if already tweening or scrubbing

        isTweening = true; // Set tweening flag to true

        Debug.Log($"{gameObject.name} OnMouseDown: Mouse clicked");

        DisableOtherColliders();
        originalPosition = transform.position;
        originalScale = transform.localScale;

        ChangeSortingOrderOfChildren(20);

        LeanTween.move(gameObject, Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane + 5f)), 0.5f);
        LeanTween.scale(gameObject, originalScale * 2f, 0.5f).setOnComplete(() =>
        {
            LeanTween.scale(blackoutPanel, Vector2.one, 0.5f);
            AnimateBirdInstruction();
        });

        isDishSelected = true;
        helperActive = true;
        LV4DragManager.timerRunning = false;
        Debug.Log($"{gameObject.name} OnMouseDown: Dish selected, timerRunning set to false");
    }


    private void AnimateBirdInstruction()
    {
        Debug.Log($"{gameObject.name} AnimateBirdInstruction: Called");

        GameObject birdImage = GameObject.FindWithTag("Bird");
        if (birdImage != null)
        {
            RectTransform birdRectTransform = birdImage.GetComponent<RectTransform>();
            Animator birdAnimator = birdImage.GetComponent<Animator>();

            string subtitleText = "";
            if (gameObject.name == "Bowl_1")
            {
                subtitleText = "Scrub Scrub Scrub the Bowl";
            }
            else if (gameObject.name == "Plate_1")
            {
                subtitleText = "Scrub Scrub Scrub the Plate";
            }
            else if (gameObject.name == "Glass_1")
            {
                subtitleText = "Scrub Scrub Scrub the cup";
            }

            if (subtitleCoroutine != null)
            {
                StopCoroutine(subtitleCoroutine);
            }

            LeanTween.move(birdRectTransform, new Vector2(-250, 250), 0.5f).setOnComplete(() =>
            {
                birdAnimator.SetTrigger("instruction");

                if (audioSource != null)
                {
                    audioSource.Play();
                }
                else
                {
                    Debug.LogError("AudioSource is not assigned or missing in the Inspector.");
                }

                subtitleCoroutine = StartCoroutine(ShowSubtitle(subtitleText, "Kiki", audioSource.clip));
                StartCoroutine(WaitAndReturnBirdImage(birdRectTransform));

                Debug.Log("AnimateBirdInstruction: timerRunning set to true globally");
            });
        }
        else
        {
            Debug.LogError("Bird image GameObject with tag 'Bird' not found.");
        }
    }

    private IEnumerator WaitAndReturnBirdImage(RectTransform birdRectTransform)
    {
        yield return new WaitForSeconds(3f);
        LeanTween.move(birdRectTransform, new Vector2(-250, -200), 0.5f).setOnComplete(()=>
        {
            scrubberController.gameObject.GetComponent<Collider2D>().enabled = true;
            scrubbertimer = true;
        });
    }
    private IEnumerator SpawnGlowEffect()
    {
        if (!helperActive && glowPrefab != null)
        {
            Debug.Log($"{gameObject.name} SpawnGlowEffect: Spawning glow as helperActive is false");

            instantiatedGlow = Instantiate(glowPrefab, transform.position, Quaternion.identity);
            instantiatedGlow.transform.SetParent(transform);
            LeanTween.scale(instantiatedGlow, new Vector3(10f, 7f, 7f), 0.5f);
            yield return new WaitForSeconds(2f);
            LeanTween.scale(instantiatedGlow, Vector3.zero, 0.5f).setOnComplete(() => Destroy(instantiatedGlow));
        }
        else
        {
            Debug.Log($"{gameObject.name} SpawnGlowEffect: helperActive is true or glowPrefab is null, not spawning glow");
        }
    }

    public void StartScrubbing()
    {
        if (!isScrubbing)
        {
            if (SfxAudioSource != null)
            {
                SfxAudioSource.loop = false;
                SfxAudioSource.PlayOneShot(SfxAudio1);
            }
            isScrubbing = true;
            StartCoroutine(SpawnPrefabs());
        }
    }
    private IEnumerator SpawnPrefabs()
    {
        isSpawning = true;

        for (float timer = 0; timer < 3f; timer += Time.deltaTime)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                Transform dChild = FindDChild();
                if (dChild != null)
                {
                    Vector3 spawnPos = new Vector3(mousePos.x, mousePos.y, dChild.position.z);
                    if (!AlreadyHasMaskAtPoint(spawnPos, dChild))
                    {
                        instantiatedMask = Instantiate(maskPrefab, spawnPos, Quaternion.identity);
                        instantiatedMask.transform.SetParent(dChild);
                    }
                }
            }

            yield return null;
        }

        isSpawning = false;

        Transform dChildToDestroy = FindDChild();
        if (dChildToDestroy != null)
        {
            StartCoroutine(CleanDishAfterDelay(dChildToDestroy.gameObject));
        }
    }

    private Transform FindDChild()
    {
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("D"))
            {
                return child;
            }
        }
        return null;
    }

    private bool AlreadyHasMaskAtPoint(Vector3 point, Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag("Mask"))
            {
                float distance = Vector3.Distance(child.position, point);
                if (distance < 0.01f)
                    return true;
            }
        }
        return false;
    }

    private IEnumerator CleanDishAfterDelay(GameObject dChild)
    {
        yield return new WaitForSeconds(2f);
        Destroy(dChild);

        LeanTween.move(gameObject, originalPosition, 0.5f).setOnComplete(() =>
        {
            LeanTween.scale(blackoutPanel, Vector2.zero, 0.5f);
            transform.localScale = originalScale;
            isDishSelected = false;
            isTweening = false; // Reset the tweening flag after cleaning is complete
            RestoreOriginalSortingOrders();
            objectCollider.enabled = false;
            EnableOtherColliders();
            StartCoroutine(SpawnGlowEffect());
            if (scrubberController != null)
            {
                scrubberController.ResetPosition();
            }

            if (dishWashingManager != null)
            {
                DishWashingManager.DishWashed();
            }
            scrubberController.gameObject.GetComponent<Collider2D>().enabled = false;
            isScrubbing = false;

            timer = 0f;
            LV4DragManager.timerRunning = true;
        });
        scrubbertimer = false;  
        scrubberController.scrubberTimerStarted = false; 
        dishCleaned = true;
    }

    private void StoreOriginalSortingOrders()
    {
        foreach (Transform child in transform)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                originalSortingOrders[child] = sr.sortingOrder;
            }
        }
    }

    private void ChangeSortingOrderOfChildren(int baseOrder)
    {
        foreach (Transform child in transform)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (child.name.StartsWith("D-"))
                {
                    sr.sortingOrder = baseOrder + 1; // Dirty objects on top
                }
                else if (child.name.StartsWith("C-"))
                {
                    sr.sortingOrder = baseOrder; // Clean objects below dirty
                }
                else
                {
                    sr.sortingOrder = baseOrder; // Default for other children
                }
            }
        }
    }

    private void RestoreOriginalSortingOrders()
    {
        foreach (Transform child in originalSortingOrders.Keys)
        {
            if (child == null)
                continue;

            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = originalSortingOrders[child];
            }
        }
    }

    private void DisableOtherColliders()
    {
        foreach (DishController dish in allDishes)
        {
            if (dish != this)
            {
                dish.GetComponent<Collider2D>().enabled = false;
            }
        }
    }

    private void EnableOtherColliders()
    {
        foreach (DishController dish in allDishes)
        {
            if (dish != this)
            {
                bool hasDChild = false;

                // Check if the dish has a child GameObject with a name starting with "D"
                foreach (Transform child in dish.transform)
                {
                    if (child.name.StartsWith("D"))
                    {
                        hasDChild = true;
                        break;
                    }
                }

                // Enable the collider only if the dish has a child starting with "D"
                if (hasDChild)
                {
                    dish.GetComponent<Collider2D>().enabled = true;
                }
            }
        }
    }
    private IEnumerator ShowSubtitle(string fullText, string dialogueType, AudioClip audioClip)
    {
        subtitleText.gameObject.SetActive(true);
        Image activeBanner = null;

        // Determine which banner to fade in based on the dialogue type
        if (dialogueType == "Kiki")
        {
            activeBanner = jojoBanner;
        }

        if (activeBanner != null)
        {
            // Fade in the banner
            for (float t = 0; t <= fadeDuration; t += Time.deltaTime)
            {
                float alpha = t / fadeDuration;
                activeBanner.color = new Color(1, 1, 1, alpha);
                yield return null;
            }
            activeBanner.color = new Color(1, 1, 1, 1);
        }

        // Calculate display time for subtitles
        float totalDuration = audioClip != null ? audioClip.length : 3f; // Default to 3 seconds if no audio
        float adjustedDuration = Mathf.Max(0, totalDuration - 1f); // Subtitles should end 1 second before the audio
        string[] sentences = SplitTextIntoSentences(fullText);
        float durationPerSentence = adjustedDuration / sentences.Length;

        foreach (string sentence in sentences)
        {
            subtitleText.text = sentence;

            // Wait for the calculated duration per sentence
            yield return new WaitForSeconds(durationPerSentence);
        }

        // Hide the subtitle text
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(false);

        // Fade out the banner
        if (activeBanner != null)
        {
            for (float t = 0; t <= fadeDuration; t += Time.deltaTime)
            {
                float alpha = 1 - (t / fadeDuration);
                activeBanner.color = new Color(1, 1, 1, alpha);
                yield return null;
            }
            activeBanner.color = new Color(1, 1, 1, 0);
        }
    }
    private string[] SplitTextIntoSentences(string text)
    {
        // Split the text by sentences or character limit
        const int maxCharsPerSentence = 60; // Adjust based on your UI
        if (text.Length <= maxCharsPerSentence)
        {
            return new string[] { text };
        }

        // Break the text into smaller parts
        string[] words = text.Split(' ');
        string currentSentence = "";
        List<string> sentences = new List<string>();

        foreach (string word in words)
        {
            if (currentSentence.Length + word.Length + 1 <= maxCharsPerSentence)
            {
                currentSentence += (currentSentence.Length > 0 ? " " : "") + word;
            }
            else
            {
                sentences.Add(currentSentence);
                currentSentence = word;
            }
        }

        if (!string.IsNullOrEmpty(currentSentence))
        {
            sentences.Add(currentSentence);
        }

        return sentences.ToArray();
    }
}
