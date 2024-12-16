using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Coffee.UIEffects;

public class LearnWordsManager : MonoBehaviour
{
    public Image itemImage;                   // UI Image for the item
    public AudioSource audioSource;          // Audio source for item audio
    public Sprite[] itemSprites;             // Array of sprites for items
    public AudioClip[] itemAudioClips;       // Array of audio clips for items
    public Button nextButton;                // Button to go to next item
    public Button previousButton;            // Button to go to previous item
    public TextMeshProUGUI itemNameText;     // UI Text for item name
    public Image progressBar;                // Progress bar UI
    public int totalItems;                   // Total number of items
    public int finishedItems;                // Number of finished items

    public AudioSource congratulationsAudioSource;
    public AudioClip[] CongratulationsaudioClip;
    public GameObject confettiLeft;
    public GameObject confettiRight;
    public float sequenceDuration;

    private int currentIndex = 0;            // Current index of the item
    private bool[] rewardGiven;              // Tracks if rewards are given
    private Vector2 initialImagePosition;    // Initial anchored position of itemImage
    private Vector2 initialTextPosition;     // Initial anchored position of itemNameText
    private bool isAnimating = false;        // Flag for ongoing animations
    private bool isCongratulating = false;   // Flag for congratulatory sequence

    //Lift effect
    public float liftSpeed;         
    public float liftScaleX;
    public float liftScaleY;
    Vector2 OriginalEffectDistance;
    private bool isImageAnimating = false;  // Flag to track animation state

    void Start()
    {
        //Page lift
        OriginalEffectDistance = itemImage.gameObject.GetComponent<UIShadow>().effectDistance;

        RectTransform itemImageRect = itemImage.GetComponent<RectTransform>();
        RectTransform itemNameTextRect = itemNameText.GetComponent<RectTransform>();

        // Store initial anchored positions for UI elements
        initialImagePosition = itemImageRect.anchoredPosition;
        initialTextPosition = itemNameTextRect.anchoredPosition;

        rewardGiven = new bool[itemSprites.Length];
        totalItems = itemSprites.Length;

        // Add listener for tapping the image
        itemImage.GetComponent<Button>().onClick.AddListener(ReplayAudio);

        DisplayItem(currentIndex, false);
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        itemImage.sprite = itemSprites[currentIndex];
        SetButtonInteractable(false);  // Disable buttons at start
    }

    private void SetButtonInteractable(bool interactable)
    {
        nextButton.interactable = interactable && currentIndex < itemSprites.Length - 1;
        previousButton.interactable = interactable && currentIndex > 0;
    }

    public void DisplayItem(int index, bool animate = true, bool isNext = true)
    {
        if (index < 0 || index >= itemSprites.Length)
        {
            Debug.LogError("Index out of bounds: " + index);
            return;
        }

        SetButtonInteractable(false);  // Disable buttons before starting animation

        RectTransform itemImageRect = itemImage.GetComponent<RectTransform>();
        RectTransform itemNameTextRect = itemNameText.GetComponent<RectTransform>();

        if (animate)
        {
            isAnimating = true;
            float startScale = isNext ? 0.8f : 1.2f;
            float endScale = 1f;

            // Animate the image using anchoredPosition
            LeanTween.value(itemImageRect.anchoredPosition.x, isNext ? -Screen.width : Screen.width, 0.6f)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnUpdate((float val) =>
                {
                    itemImageRect.anchoredPosition = new Vector2(val, initialImagePosition.y);
                })
                .setOnComplete(() =>
                {
                    itemImage.sprite = itemSprites[index];
                    itemImageRect.anchoredPosition = new Vector2(isNext ? Screen.width : -Screen.width, initialImagePosition.y);

                    LeanTween.value(itemImageRect.anchoredPosition.x, initialImagePosition.x, 0.6f)
                        .setEase(LeanTweenType.easeInOutQuad)
                        .setOnUpdate((float val) =>
                        {
                            itemImageRect.anchoredPosition = new Vector2(val, initialImagePosition.y);
                        });
                    LeanTween.scale(itemImage.gameObject, new Vector3(endScale, endScale, 1f), 0.6f)
                        .setEase(LeanTweenType.easeOutBack)
                        .setOnComplete(() =>
                        {
                            isAnimating = false;
                            audioSource.clip = itemAudioClips[index];
                            audioSource.Play();  // Start audio only after animation completes
                            Invoke(nameof(EnableButtonsAfterAudio), audioSource.clip.length);
                        });
                });

            // Animate the text component using anchoredPosition
            LeanTween.value(itemNameTextRect.anchoredPosition.y, -Screen.height / 2, 0.6f)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnUpdate((float val) =>
                {
                    itemNameTextRect.anchoredPosition = new Vector2(initialTextPosition.x, val);
                })
                .setOnComplete(() =>
                {
                    // Update the text content when it's off-screen
                    itemNameText.text = itemSprites[index].name;

                    // Reset text position off-screen before moving it back up
                    itemNameTextRect.anchoredPosition = new Vector2(initialTextPosition.x, -Screen.height / 2);

                    LeanTween.value(itemNameTextRect.anchoredPosition.y, initialTextPosition.y, 0.6f)
                        .setEase(LeanTweenType.easeOutBack)
                        .setOnUpdate((float val) =>
                        {
                            itemNameTextRect.anchoredPosition = new Vector2(initialTextPosition.x, val);
                        });
                });
        }
        else
        {
            // No animation: Set item directly
            itemImage.sprite = itemSprites[index];
            itemImageRect.anchoredPosition = initialImagePosition;
            itemImage.transform.localScale = Vector3.one;

            itemNameText.text = itemSprites[index].name;
            itemNameTextRect.anchoredPosition = initialTextPosition;

            isAnimating = false;
            audioSource.clip = itemAudioClips[index];
            audioSource.Play();
            Invoke(nameof(EnableButtonsAfterAudio), audioSource.clip.length);
        }
    }

    private void EnableButtonsAfterAudio()
    {
        SetButtonInteractable(true);
    }

    public void NextItem()
    {
        if (currentIndex < itemSprites.Length - 1)
        {
            currentIndex++;
            DisplayItem(currentIndex, true, true);
        }
    }

    public void PreviousItem()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            DisplayItem(currentIndex, true, false);
        }
    }

    public void ReplayAudio()
    {
        if (audioSource.clip != null && !isAnimating)
        {
            // Start the audio playback
            audioSource.Play();

            // Apply the lift effect
            ApplyLiftEffect();
        }
    }

    private void ApplyLiftEffect()
    {
        // Prevent triggering the effect if it's already animating
        if (isImageAnimating) return;

        // Disable button interaction to prevent spamming
        if (itemImage.GetComponent<Button>() != null)
        {
            itemImage.GetComponent<Button>().interactable = false;
        }

        isImageAnimating = true; // Set flag to indicate animation is in progress

        // Get the UIShadow component on the itemImage
        UIShadow uiShadow = itemImage.GetComponent<UIShadow>();

        if (uiShadow != null)
        {
            // Store the original scale before applying any changes
            Vector3 originalScale = itemImage.transform.localScale;

            // Animate the shadow effect distance to simulate the lift
            LeanTween.value(gameObject, OriginalEffectDistance.x, 7f, liftSpeed)
                .setOnUpdate((float val) =>
                {
                    // Gradually change the shadow's X effect distance to simulate lift
                    uiShadow.effectDistance = new Vector2(val, -val);
                });

            // Set shadow color for the lift effect
            uiShadow.effectColor = new Color(0f, 0f, 0f, 0.3f);

            // Animate the image scale to simulate the lift effect
            LeanTween.scale(itemImage.gameObject, new Vector3(liftScaleX, liftScaleY, 1f), liftSpeed)
                .setEase(LeanTweenType.easeInSine)
                .setOnComplete(() =>
                {
                    // Return directly to the original scale
                    LeanTween.scale(itemImage.gameObject, originalScale, liftSpeed)
                        .setEase(LeanTweenType.easeOutSine)
                        .setOnComplete(() =>
                        {
                            // Reset shadow effect distance
                            LeanTween.value(gameObject, 7f, OriginalEffectDistance.x, 0.3f)
                                .setOnUpdate((float val) =>
                                {
                                    uiShadow.effectDistance = new Vector2(val, -val);
                                })
                                .setOnComplete(() =>
                                {
                                    // Re-enable button interaction and reset animation flag
                                    if (itemImage.GetComponent<Button>() != null)
                                    {
                                        itemImage.GetComponent<Button>().interactable = true;
                                    }
                                    isImageAnimating = false;
                                });
                        });
                });
        }
    }

}
