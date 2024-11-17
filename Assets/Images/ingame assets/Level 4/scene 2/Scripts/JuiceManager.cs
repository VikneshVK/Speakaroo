using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine.UI;

public class JuiceManager : MonoBehaviour
{
    public TMP_Text fruitRequirementsText;
    public bool isKikiJuice = false;
    public bool sceneEnded = false;
    public JuiceController juiceController;
    public List<string> requiredFruits = new List<string>();
    public bool isSecondTime = false;
    public LVL4Sc2HelperHand helperHand;

    public Image Image1; // Reference to Image1 UI element
    public Image Image2; // Reference to Image2 UI element
    public Sprite Sprite1; // Sprite for Kiwi
    public Sprite Sprite2; // Sprite for Strawberry
    public Sprite Sprite3; // Sprite for Blueberry

    private TweeningController tweeningController;
    private Animator birdAnimator;
    private GameObject bird;
    public Transform birdEndPosition; // Set this to the end position for bird tweening

    private Vector3 birdInitialPosition; // Store the initial position of the bird

    private bool hasTriggeredSingleFruitAnimation = false; // Flag for single fruit animation
    private bool hasTriggeredMultipleFruitAnimation = false; // Flag for multiple fruits animation

    public LVL4Sc2AudioManager audioManager;
    public AudioClip Audio1; // For Kiwi
    public AudioClip Audio2; // For SB
    public AudioClip Audio3; // For BB
    public AudioClip Audio4; // For Kiwi + BB
    public AudioClip Audio5; // For Kiwi + SB
    public AudioClip Audio6; // For SB + BB
    public TextMeshProUGUI subtitleText;

    void Start()
    {
        juiceController = FindObjectOfType<JuiceController>();
        tweeningController = FindObjectOfType<TweeningController>(); // Get TweeningController reference
        fruitRequirementsText.gameObject.SetActive(false); // Disable text at start

        Image1.gameObject.SetActive(false);
        Image2.gameObject.SetActive(false);

        // Get bird reference and animator
        bird = GameObject.FindGameObjectWithTag("Bird");
        if (bird != null)
        {
            birdAnimator = bird.GetComponent<Animator>();
            birdInitialPosition = bird.transform.position; // Store initial position
        }

        UpdateFruitRequirements(isKikiJuice);      

    }

    void Update()
    {
        // Check if tweeningController is available and if tween is complete
        if (tweeningController != null && tweeningController.TweenComplete)
        {
            if (isKikiJuice && requiredFruits.Count == 2)
            {
                Image1.gameObject.SetActive(true);
                Image2.gameObject.SetActive(true);
            }
            else if (!isKikiJuice && requiredFruits.Count == 1)
            {
                Image1.gameObject.SetActive(true);
                Image2.gameObject.SetActive(false);
            }

            // Only update requirements once when isSecondTime becomes true
            if (!isSecondTime)
            {
                UpdateFruitRequirements(isKikiJuice);
                isSecondTime = true;
            }

            // Trigger the bird animation only once based on the fruit count
            TriggerBirdAnimation();
        }
        else
        {
            if (isSecondTime)
            {
                Image1.gameObject.SetActive(false);
                Image2.gameObject.SetActive(false);
                isSecondTime = false;
                // Reset flags when tween completes and isSecondTime resets
                hasTriggeredSingleFruitAnimation = false;
                hasTriggeredMultipleFruitAnimation = false;
            }
        }
    }


    public void UpdateFruitRequirements(bool isKikiJuice)
    {
        if (isKikiJuice)
        {
            List<string[]> options = new List<string[]>
            {
                new string[] { "Kiwi", "SB" },
                new string[] { "Kiwi", "BB" },
                new string[] { "SB", "BB" }
            };
            requiredFruits = options[Random.Range(0, options.Count)].ToList();
        }
        else
        {
            List<string> options = new List<string> { "Kiwi", "BB", "SB" };
            requiredFruits = new List<string> { options[Random.Range(0, options.Count)] };
        }

        helperHand.InitializeRequiredFruitStatus();
        UpdateFruitRequirementsUI();

        // Set sprite based on the required fruits
        if (!isKikiJuice && requiredFruits.Count == 1)
        {
            Image1.sprite = GetSpriteForFruit(requiredFruits[0]);
        }
        else if (isKikiJuice && requiredFruits.Count == 2)
        {
            Image1.sprite = GetSpriteForFruit(requiredFruits[0]);
            Image2.sprite = GetSpriteForFruit(requiredFruits[1]);
        }

        if (isKikiJuice)
        {
            EnableAllFruitColliders();
        }
    }
    private Sprite GetSpriteForFruit(string fruit)
    {
        switch (fruit)
        {
            case "Kiwi":
                return Sprite1;
            case "SB":
                return Sprite2;
            case "BB":
                return Sprite3;
            default:
                return null;
        }
    }


    void UpdateFruitRequirementsUI()
    {
        fruitRequirementsText.text = "Required Fruits: " + string.Join(", ", requiredFruits);
        Debug.Log("Fruit requirements updated: " + fruitRequirementsText.text);
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

    private void TriggerBirdAnimation()
    {
        if (birdAnimator == null || birdEndPosition == null) return;

        RectTransform birdRectTransform = bird.GetComponent<RectTransform>();
        if (birdRectTransform == null) return;

        // Play animation, audio, and subtitles once for requiredFruits.Count == 1
        if (requiredFruits.Count == 1 && !hasTriggeredSingleFruitAnimation)
        {
            hasTriggeredSingleFruitAnimation = true; // Prevent repeated calls

            LeanTween.value(-1300f, -790f, 1f).setEase(LeanTweenType.easeInOutQuad).setOnUpdate((float x) =>
            {
                birdRectTransform.anchoredPosition = new Vector2(x, 320f);
            }).setOnComplete(() =>
            {
                PlayBirdAnimation(requiredFruits[0]);
                StartCoroutine(ReturnBirdToInitialPosition(birdRectTransform));
            });
        }
        // Play animation, audio, and subtitles once for requiredFruits.Count == 2
        else if (requiredFruits.Count == 2 && !hasTriggeredMultipleFruitAnimation)
        {
            hasTriggeredMultipleFruitAnimation = true; // Prevent repeated calls

            LeanTween.value(-1300f, -790f, 1f).setEase(LeanTweenType.easeInOutQuad).setOnUpdate((float x) =>
            {
                birdRectTransform.anchoredPosition = new Vector2(x, 320f);
            }).setOnComplete(() =>
            {
                if (requiredFruits.Contains("Kiwi") && requiredFruits.Contains("SB"))
                {
                    PlayBirdAnimation("KiwiStrawberry");
                }
                else if (requiredFruits.Contains("Kiwi") && requiredFruits.Contains("BB"))
                {
                    PlayBirdAnimation("KiwiBlueBerry");
                }
                else if (requiredFruits.Contains("SB") && requiredFruits.Contains("BB"))
                {
                    PlayBirdAnimation("StrawberryBlueberry");
                }

                StartCoroutine(ReturnBirdToInitialPosition(birdRectTransform));
            });
        }
    }

    private void PlayBirdAnimation(string fruitTag)
    {
        switch (fruitTag)
        {
            case "Kiwi":
                birdAnimator.SetTrigger("Kiwi");
                audioManager.PlayAudio(Audio1);
                StartCoroutine(RevealTextWordByWord("Put the Kiwis in the Blender", 0.5f));
                break;
            case "SB":
                birdAnimator.SetTrigger("Strawberry");
                audioManager.PlayAudio(Audio2);
                StartCoroutine(RevealTextWordByWord("Put the Strawberries in the Blender", 0.5f));
                break;
            case "BB":
                birdAnimator.SetTrigger("BlueBerry");
                audioManager.PlayAudio(Audio3);
                StartCoroutine(RevealTextWordByWord("Put the Blueberry in the Blender", 0.5f));
                break;
            case "KiwiStrawberry":
                birdAnimator.SetTrigger("KiwiStrawberry");
                audioManager.PlayAudio(Audio5);
                StartCoroutine(RevealTextWordByWord("Put the Strawberries and the Kiwis in the Blender", 0.5f));
                break;
            case "KiwiBlueBerry":
                birdAnimator.SetTrigger("KiwiBlueBerry");
                audioManager.PlayAudio(Audio4);
                StartCoroutine(RevealTextWordByWord("Put the Kiwis and the Blueberries in the Blender", 0.5f));
                break;
            case "StrawberryBlueberry":
                birdAnimator.SetTrigger("StrawberryBlueberry");
                audioManager.PlayAudio(Audio6);
                StartCoroutine(RevealTextWordByWord("Put the Blueberry and the Strawberries in the Blender", 0.5f));
                break;
        }
    }


    private IEnumerator ReturnBirdToInitialPosition(RectTransform birdRectTransform)
    {
        yield return new WaitForSeconds(3f); // Wait for 3 seconds after animation trigger

        // Move bird back to its initial position
        LeanTween.value(-790f, -1300f, 1f).setEase(LeanTweenType.easeInOutQuad).setOnUpdate((float x) =>
        {
            birdRectTransform.anchoredPosition = new Vector2(x, 320f);
        });
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