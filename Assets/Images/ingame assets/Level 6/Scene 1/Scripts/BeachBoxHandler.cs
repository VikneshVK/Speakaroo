using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BeachBoxHandler : MonoBehaviour
{
    public Image leftButton;    // Button for BeachBallMiniGame
    public Image centerButton;  // Button for BubbleMiniGame
    public Image rightButton;   // Button for FrisbeeGameplay
    public Image highlighter;   // Single highlighter for all buttons
    public TextMeshProUGUI textComponent;
    public TextMeshProUGUI textComponent2;
    public string updatedText = "What should we play with?";
    public string updatedText2 = "_______________";
    private int tapCount = 0;
    private Image beachboxSprite;
    private RectTransform rectTransform;
    private bool beachBoxDestroyed = false;
    public bool playRestared = false;

    private MiniGameController miniGameController;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        // Initially hide the buttons
        leftButton.gameObject.SetActive(false);
        centerButton.gameObject.SetActive(false);
        rightButton.gameObject.SetActive(false);

        // Get the reference to MiniGameController
        miniGameController = FindObjectOfType<MiniGameController>();
        beachboxSprite = GetComponent<Image>();
    }

    public void Update()
    {
        if (beachBoxDestroyed && playRestared)
        {
            playRestared = false;
            CheckCompletedMiniGames(); // Enable buttons immediately
        }
    }

    public void OnMouseDown()
    {
        tapCount++;
        ShakeBox();

        if (tapCount >= 3)
        {
            DestroyBoxAndShowButtons();
        }
    }

    private void ShakeBox()
    {
        float shakeIntensity = 10f;
        float shakeTime = 0.3f;

        LeanTween.moveX(rectTransform, rectTransform.anchoredPosition.x + shakeIntensity, shakeTime)
                 .setEase(LeanTweenType.easeShake)
                 .setLoopPingPong(1);
    }

    private void DestroyBoxAndShowButtons()
    {
        // Destroy the box GameObject
        beachBoxDestroyed = true;
        beachboxSprite.enabled = false;

        // Enable and position buttons with tweening
        EnableAndTweenButtons();

        // Update the text component
        if (textComponent != null)
        {
            textComponent.text = updatedText;
        }
        if (textComponent2 != null)
        {
            textComponent2.text = updatedText2;
        }
    }

    private void EnableAndTweenButtons()
    {
        // Activate all buttons
        leftButton.gameObject.SetActive(true);
        centerButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(true);

        // Disable buttons based on completed mini-games
        CheckCompletedMiniGames();

        // Tween each button to its respective position
        float duration = 0.5f;
        Vector3 leftPosition = new Vector3(-550, 70, 0);
        Vector3 centerPosition = new Vector3(0, 70, 0);
        Vector3 rightPosition = new Vector3(550, 70, 0);

        LeanTween.moveLocal(leftButton.gameObject, leftPosition, duration).setEase(LeanTweenType.easeOutBack);
        LeanTween.moveLocal(centerButton.gameObject, centerPosition, duration).setEase(LeanTweenType.easeOutBack);
        LeanTween.moveLocal(rightButton.gameObject, rightPosition, duration).setEase(LeanTweenType.easeOutBack)
                 .setOnComplete(() => StartCoroutine(ScaleUpButtonsAndPlayEffects()));
    }

    private IEnumerator ScaleUpButtonsAndPlayEffects()
    {
        // Scale and play effect for left button's child component, wait for 1 second
        yield return ScaleChildAndPlayEffect(leftButton);

        // Scale and play effect for center button's child component, wait for 1 second
        yield return ScaleChildAndPlayEffect(centerButton);

        // Scale and play effect for right button's child component, wait for 1 second
        yield return ScaleChildAndPlayEffect(rightButton);
    }

    private IEnumerator ScaleChildAndPlayEffect(Image button)
    {
        // Get the child component of the button (the text)
        Transform childComponent = button.transform.GetChild(0);
        AudioSource buttonAudioSource = button.GetComponent<AudioSource>();

        if (childComponent != null && highlighter != null)
        {
            // Move the highlighter to the button's position
            highlighter.transform.position = button.transform.position;

            // Set the highlighter's initial scale to 0
            highlighter.transform.localScale = Vector3.zero;

            // Start scaling up the text component
            childComponent.localScale = Vector3.zero;
            LeanTween.scale(childComponent.gameObject, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);

            // At the same time, scale up the highlighter to 8
            LeanTween.scale(highlighter.gameObject, Vector3.one * 8f, 0.5f).setEase(LeanTweenType.easeOutBack);

            // Wait for both the scaling operations to complete
            yield return new WaitForSeconds(0.5f);

            // Play the audio from the button's AudioSource
            if (buttonAudioSource != null)
            {
                buttonAudioSource.Play();
            }

            // Scale down the highlighter after 0.5 seconds
            LeanTween.scale(highlighter.gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInBack);

            // Wait for the highlighter scaling down to complete
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void CheckCompletedMiniGames()
    {
        if (miniGameController == null) return;

        // Deactivate image components based on completed mini-games
        if (miniGameController.IsMiniGameCompleted("BeachBallMiniGame"))
        {
            leftButton.gameObject.SetActive(false); // Deactivate the image component
        }
        if (miniGameController.IsMiniGameCompleted("BubbleMinigame"))
        {
            centerButton.gameObject.SetActive(false); // Deactivate the image component
        }
        if (miniGameController.IsMiniGameCompleted("FrisbeeMiniGame"))
        {
            rightButton.gameObject.SetActive(false); // Deactivate the image component
        }
    }
}
