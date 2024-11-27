using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class FoodContainerController : MonoBehaviour
{
    public Image foodContainerImage; // Reference to the food container image component
    public Image[] imagesToTween; // 3 images to tween (assign in inspector)
    public TextMeshProUGUI textComponent1; // Text Component 1 to enable after third click
    public TextMeshProUGUI textComponent2; // Text Component 2 for "______"
    public float shakeDuration = 0.5f; // Duration for shaking
    public float tweenDuration = 0.5f; // Duration for tweening scale and position
    public float padding = 50f; // Padding for the left and right images
    public Canvas canvas; // Reference to the Canvas component for screen space calculations

    public AudioClip[] audioClips; // Array to hold audio clips for each sprite
    private AudioSource audioSource;

    public int clickCount = 0;
    public bool clicked = false;
    public RectTransform highlighter;

    private int currentStopIndex;

    void Start()
    {
        foreach (Image image in imagesToTween)
        {
            // Set initial scale of both image and its child TMP text to 0
            image.transform.localScale = Vector3.zero;
            image.GetComponentInChildren<TextMeshProUGUI>().transform.localScale = Vector3.zero;
        }
        textComponent1.gameObject.SetActive(false); // Disable the text at the start
        textComponent2.gameObject.SetActive(false); // Disable the text at the start
        highlighter.localScale = Vector3.zero;
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        currentStopIndex = Lvl7Sc1JojoController.currentStopIndex;
        LoadImagesForStopPosition(currentStopIndex);
    }

    void HandleClick()
    {
        clickCount++;

        // Shake the food container when clicked
        LeanTween.scale(foodContainerImage.rectTransform, foodContainerImage.rectTransform.localScale * 1.1f, shakeDuration / 2).setEasePunch();

        if (clickCount == 2)
        {
            // Prepare images for tweening
            PrepareImagesForTweening();
        }

        if (clickCount == 3)
        {
            SetImagesInteractable(false);
            TweenImages();

            // Set text for textComponent1 and textComponent2 after the third click
            textComponent1.text = "What you want to eat?";
            textComponent1.gameObject.SetActive(true);
            textComponent2.text = "______";
            textComponent2.gameObject.SetActive(true);

            // Disable the food container image after the third click
            foodContainerImage.enabled = false;

            // Start a coroutine with a delay before calling PlaySequentialTweenCoroutine
            StartCoroutine(DelayedStartCoroutine());
        }
    }

    private void PrepareImagesForTweening()
    {
        // Loop through the images and their child TMP objects
        for (int i = 0; i < imagesToTween.Length; i++)
        {
            // Enable the image GameObject if it's disabled
            if (!imagesToTween[i].gameObject.activeSelf)
            {
                imagesToTween[i].gameObject.SetActive(true);
            }

            // Reset the scale of the image to 0 to prepare for tweening
            imagesToTween[i].transform.localScale = Vector3.zero;

            // Check if the image has a child TextMeshPro component
            TextMeshProUGUI childText = imagesToTween[i].GetComponentInChildren<TextMeshProUGUI>(true); // Use (true) to find even inactive components
            if (childText != null)
            {
                // Activate the child text temporarily if it's disabled
                bool wasTextInactive = !childText.gameObject.activeSelf;
                if (wasTextInactive)
                {
                    childText.gameObject.SetActive(true);
                }

                // Set the scale of the child text to 0 to prepare for tweening
                childText.transform.localScale = Vector3.zero;

                // If the text was inactive before, deactivate it again
                if (wasTextInactive)
                {
                    childText.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogError($"No TextMeshPro component found on the child of image at index {i}");
            }
        }
    }



    // Coroutine to add a 1-second delay before starting PlaySequentialTweenCoroutine
    private IEnumerator DelayedStartCoroutine()
    {
        yield return new WaitForSeconds(1f); // Delay for 1 second
        StartCoroutine(PlaySequentialTweenCoroutine()); // Call the main coroutine
    }


    private void LoadImagesForStopPosition(int stopIndex)
    {
        string leftSpritePath = "", centerSpritePath = "", rightSpritePath = "";
        int audioIndexOffset = 0;

        switch (stopIndex)
        {
            case 1:
                leftSpritePath = "Images/LVL7Sc1/Sprite1";
                centerSpritePath = "Images/LVL7Sc1/Sprite2";
                rightSpritePath = "Images/LVL7Sc1/Sprite3";
                audioIndexOffset = 0; // AudioClips[0-2]
                break;
            case 2:
                leftSpritePath = "Images/LVL7Sc1/Sprite4";
                centerSpritePath = "Images/LVL7Sc1/Sprite5";
                rightSpritePath = "Images/LVL7Sc1/Sprite6";
                audioIndexOffset = 3; // AudioClips[3-5]
                break;
            case 3:
                leftSpritePath = "Images/LVL7Sc1/Sprite7";
                centerSpritePath = "Images/LVL7Sc1/Sprite8";
                rightSpritePath = "Images/LVL7Sc1/Sprite9";
                audioIndexOffset = 6; // AudioClips[6-8]
                break;
            default:
                Debug.Log("No images to load for stop position 4.");
                return;
        }

        Sprite leftSprite = Resources.Load<Sprite>(leftSpritePath);
        Sprite centerSprite = Resources.Load<Sprite>(centerSpritePath);
        Sprite rightSprite = Resources.Load<Sprite>(rightSpritePath);

        if (leftSprite != null && centerSprite != null && rightSprite != null)
        {
            LoadImages(leftSprite, centerSprite, rightSprite);

            // Assign audio clips to images
            imagesToTween[0].GetComponent<AudioSource>().clip = audioClips[audioIndexOffset];
            imagesToTween[1].GetComponent<AudioSource>().clip = audioClips[audioIndexOffset + 1];
            imagesToTween[2].GetComponent<AudioSource>().clip = audioClips[audioIndexOffset + 2];
        }
        else
        {
            Debug.LogError("One or more sprites failed to load.");
        }

        // Load dynamic text for child TMP
        LoadDynamicText(stopIndex);
    }


    public void LoadImages(Sprite leftImage, Sprite centerImage, Sprite rightImage)
    {
        imagesToTween[0].sprite = leftImage;
        imagesToTween[1].sprite = centerImage;
        imagesToTween[2].sprite = rightImage;

        foreach (Image image in imagesToTween)
        {
            image.transform.localScale = Vector3.zero; // Start with zero scale
        }
    }

    // Load dynamic text into child TMP based on the stop index (hardcoded text)
    private void LoadDynamicText(int stopIndex)
    {
        switch (stopIndex)
        {
            case 1:
                SetTextForImages("French Fries", "Noodles", "Nuggets");
                break;
            case 2:
                SetTextForImages("Cup Cake", "Donut", "Popsicle");
                break;
            case 3:
                SetTextForImages("Grape", "Blue Berry", "Mango");
                break;
            default:
                Debug.LogWarning("Invalid currentStopIndex!");
                break;
        }
    }

    private void SetTextForImages(string leftText, string centerText, string rightText)
    {
        imagesToTween[0].GetComponentInChildren<TextMeshProUGUI>().text = leftText;
        imagesToTween[1].GetComponentInChildren<TextMeshProUGUI>().text = centerText;
        imagesToTween[2].GetComponentInChildren<TextMeshProUGUI>().text = rightText;
    }

    private void TweenImages()
    {
        Vector3[] positions = CalculateImagePositions();

        for (int i = 0; i < imagesToTween.Length; i++)
        {
            if (imagesToTween[i] == null)
            {
                Debug.LogError($"Image at index {i} is not assigned in the Inspector.");
                continue;
            }

            RectTransform imageRect = imagesToTween[i].rectTransform;
            LeanTween.scale(imageRect, Vector3.one, tweenDuration).setEaseOutBounce();
            LeanTween.move(imageRect, positions[i], tweenDuration).setEaseOutQuad();
            /*// Adding the PointerClick listener via EventTrigger
            EventTrigger trigger = imagesToTween[i].gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            int index = i; // Capture index for closure
            entry.callback.AddListener((eventData) => OnImageClicked(index));
            trigger.triggers.Add(entry);*/
        }
    }

    private Vector3[] CalculateImagePositions()
    {
        Vector3[] positions = new Vector3[3];
        if (canvas == null)
        {
            Debug.LogError("Canvas is not assigned. Cannot calculate image positions.");
            return positions; // Return an empty positions array to avoid further errors
        }

        float canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
        float canvasHeight = canvas.GetComponent<RectTransform>().rect.height;

        positions[0] = new Vector3(-canvasWidth / 2 + padding, 0, 0); // Left with padding
        positions[1] = new Vector3(0, 0, 0); // Center
        positions[2] = new Vector3(canvasWidth / 2 - padding, 0, 0); // Right with padding

        return positions;
    }

   /* private void OnImageClicked(int index)
    {
        // Disable all other images' raycast target to prevent interaction while tweening
        SetImagesInteractable(false);

        TextMeshProUGUI childText = imagesToTween[index].GetComponentInChildren<TextMeshProUGUI>();
        AudioSource audioSource = childText.GetComponent<AudioSource>();
        audioSource.Play();

        *//*// Tween text scale to 1
        LeanTween.scale(childText.rectTransform, Vector3.one, 0.5f).setEaseOutBack().setOnComplete(() =>
        {
            // Play audio attached to the text
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
            }

            // Wait for 1 second and then scale the text back to 0
            StartCoroutine(ScaleTextBackToZero(childText, 1f, index));
        });*//*
    }*/

    private IEnumerator ScaleTextBackToZero(TextMeshProUGUI text, float delay, int index)
    {
        yield return new WaitForSeconds(delay);

        // Tween text scale back to 0
        LeanTween.scale(text.rectTransform, Vector3.zero, 0.5f).setEaseInBack().setOnComplete(() =>
        {
            clicked = true;
            SetImagesInteractable(true);
        });
    }


    private IEnumerator PlaySequentialTweenCoroutine()
    {
        for (int i = 0; i < imagesToTween.Length; i++)
        {
            // Get the child text of the current image
            TextMeshProUGUI childText = imagesToTween[i].GetComponentInChildren<TextMeshProUGUI>();

            // Get the AudioSource of the current image
            AudioSource imageAudioSource = imagesToTween[i].GetComponent<AudioSource>();
            if (imageAudioSource == null || imageAudioSource.clip == null)
            {
                Debug.LogWarning($"Audio clip for image {i} is missing or not assigned.");
            }

            // Move the highlighter to the position of the current image
            Vector3 targetPosition = imagesToTween[i].rectTransform.position;
            highlighter.position = targetPosition;

            // Tween the child text scale and highlighter scale
            LeanTween.scale(childText.rectTransform, Vector3.one, 0.5f).setEaseOutBack();
            LeanTween.scale(highlighter, Vector3.one * 8, 0.5f).setEaseOutBack();

            // Wait for the tween animation to complete
            yield return new WaitForSeconds(0.5f);

            // Play the audio clip for the current image
            if (imageAudioSource != null && imageAudioSource.clip != null)
            {
                imageAudioSource.Play();
            }

            // Wait for the audio clip duration or a fallback delay
            float waitTime = imageAudioSource != null && imageAudioSource.clip != null
                ? imageAudioSource.clip.length
                : 1f; // Fallback to 1 second if no clip
            yield return new WaitForSeconds(waitTime);

            // Tween the highlighter scale back to zero
            LeanTween.scale(highlighter, Vector3.zero, 0.5f).setEaseInBack();

            // Wait for the tween animation to complete
            yield return new WaitForSeconds(0.5f);

            // Re-enable interactivity for all images after the sequence
            SetImagesInteractable(true);
        }
    }


    private void SetImagesInteractable(bool interactable)
    {
        for (int i = 0; i < imagesToTween.Length; i++)
        {
            imagesToTween[i].raycastTarget = interactable;
        }
    }


    public void ResetClickCount()
    {
        clickCount = 0;
        Debug.Log("Click count reset to zero.");
    }
}
