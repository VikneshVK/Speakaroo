using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

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
    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;

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
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
    }

    public void Update()
    {
        if (beachBoxDestroyed && playRestared)
        {
            playRestared = false;
            CheckCompletedMiniGames(); // Enable buttons immediately
            StartCoroutine(ScaleActiveButtonsAfterPositioning());
        }
    }

    private IEnumerator ScaleActiveButtonsAfterPositioning()
    {
        yield return new WaitForSeconds(0.6f); // Ensure buttons have updated their positions

        // Disable dragging for all active buttons at once
        if (leftButton.gameObject.activeSelf)
        {
            RemoveDragEvents(leftButton);
        }
        if (centerButton.gameObject.activeSelf)
        {
            RemoveDragEvents(centerButton);
        }
        if (rightButton.gameObject.activeSelf)
        {
            RemoveDragEvents(rightButton);
        }

        // Scale up each button and play effect with the updated highlighter position
        if (leftButton.gameObject.activeSelf)
        {
            yield return ScaleChildAndPlayEffect(leftButton);
        }
        if (centerButton.gameObject.activeSelf)
        {
            yield return ScaleChildAndPlayEffect(centerButton);
        }
        if (rightButton.gameObject.activeSelf)
        {
            yield return ScaleChildAndPlayEffect(rightButton);
        }

        // Enable dragging for all active buttons after all effects are complete
        if (leftButton.gameObject.activeSelf)
        {
            AddDragEvents(leftButton);
        }
        if (centerButton.gameObject.activeSelf)
        {
            AddDragEvents(centerButton);
        }
        if (rightButton.gameObject.activeSelf)
        {
            AddDragEvents(rightButton);
        }
    }

    private void RemoveDragEvents(Image button)
    {
        EventTrigger eventTrigger = button.GetComponent<EventTrigger>();
        if (eventTrigger != null)
        {
            // Remove PointerDown and PointerUp entries related to drag
            eventTrigger.triggers.RemoveAll(entry =>
                entry.eventID == EventTriggerType.PointerDown ||
                entry.eventID == EventTriggerType.PointerUp);
        }
    }

    private void AddDragEvents(Image button)
    {
        EventTrigger eventTrigger = button.GetComponent<EventTrigger>();
        if (eventTrigger != null)
        {
            // Add PointerDown event for starting the drag
            EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDownEntry.callback.AddListener((data) => { button.GetComponent<DraggableTextHandler>().OnMouseDown(); });
            eventTrigger.triggers.Add(pointerDownEntry);

            // Add PointerUp event for ending the drag
            EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUpEntry.callback.AddListener((data) => { button.GetComponent<DraggableTextHandler>().OnMouseUp(); });
            eventTrigger.triggers.Add(pointerUpEntry);
        }
    }


    public void OnMouseDown()
    {
        tapCount++;
        ShakeBox();

        if (tapCount >= 3)
        {
            if (SfxAudioSource != null)
            {
                SfxAudioSource.loop = false;
                SfxAudioSource.PlayOneShot(SfxAudio1);
            }
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
        leftButton.gameObject.SetActive(true);
        centerButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(true);

        CheckCompletedMiniGames();

        float duration = 0.5f;
        Vector3 leftPosition = new Vector3(-550, 70, 0);
        Vector3 centerPosition = new Vector3(0, 70, 0);
        Vector3 rightPosition = new Vector3(550, 70, 0);

        LeanTween.moveLocal(leftButton.gameObject, leftPosition, duration).setEase(LeanTweenType.easeOutBack);
        LeanTween.moveLocal(centerButton.gameObject, centerPosition, duration).setEase(LeanTweenType.easeOutBack);
        LeanTween.moveLocal(rightButton.gameObject, rightPosition, duration).setEase(LeanTweenType.easeOutBack)
                 .setOnComplete(() =>
                 {
                     NotifyDraggableHandlers(); // Notify DraggableTextHandlers to update initial positions
                     StartCoroutine(ScaleUpButtonsAndPlayEffects());
                 });
    }

    private IEnumerator ScaleUpButtonsAndPlayEffects()
    {
        yield return new WaitForSeconds(0.6f);
        // Disable dragging for all active buttons at once
        if (leftButton.gameObject.activeSelf)
        {
            RemoveDragEvents(leftButton);
        }
        if (centerButton.gameObject.activeSelf)
        {
            RemoveDragEvents(centerButton);
        }
        if (rightButton.gameObject.activeSelf)
        {
            RemoveDragEvents(rightButton);
        }

        // Scale up each button and play effect
        if (leftButton.gameObject.activeSelf)
        {
            yield return ScaleChildAndPlayEffect(leftButton);
        }
        if (centerButton.gameObject.activeSelf)
        {
            yield return ScaleChildAndPlayEffect(centerButton);
        }
        if (rightButton.gameObject.activeSelf)
        {
            yield return ScaleChildAndPlayEffect(rightButton);
        }

        // Enable dragging for all active buttons after all effects are complete
        if (leftButton.gameObject.activeSelf)
        {
            AddDragEvents(leftButton);
        }
        if (centerButton.gameObject.activeSelf)
        {
            AddDragEvents(centerButton);
        }
        if (rightButton.gameObject.activeSelf)
        {
            AddDragEvents(rightButton);
        }
    }


    private IEnumerator ScaleChildAndPlayEffect(Image button)
    {
        Transform childComponent = button.transform.GetChild(0);
        AudioSource buttonAudioSource = button.GetComponent<AudioSource>();

        if (childComponent != null && highlighter != null)
        {
            // Position the highlighter at the updated button position before scaling
            highlighter.transform.position = button.transform.position;
            highlighter.transform.localScale = Vector3.zero;
            childComponent.localScale = Vector3.zero;

            // Start scaling the button child and highlighter
            LeanTween.scale(childComponent.gameObject, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);
            LeanTween.scale(highlighter.gameObject, Vector3.one * 8f, 0.5f).setEase(LeanTweenType.easeOutBack);

            yield return new WaitForSeconds(0.5f);

            // Play audio if available
            if (buttonAudioSource != null)
            {
                buttonAudioSource.Play();
            }

            // Scale down the highlighter after the animation
            LeanTween.scale(highlighter.gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInBack);

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void CheckCompletedMiniGames()
    {
        if (miniGameController == null) return;
        textComponent2.enabled = true;

        // Determine which mini-games are completed
        bool beachBallCompleted = miniGameController.IsMiniGameCompleted("BeachBallMiniGame");
        bool bubblesCompleted = miniGameController.IsMiniGameCompleted("BubbleMinigame");
        bool frisbeeCompleted = miniGameController.IsMiniGameCompleted("FrisbeeMiniGame");

        // Deactivate buttons for completed games
        leftButton.gameObject.SetActive(!beachBallCompleted);
        centerButton.gameObject.SetActive(!bubblesCompleted);
        rightButton.gameObject.SetActive(!frisbeeCompleted);

        // List of active buttons
        Image[] activeButtons = { leftButton, centerButton, rightButton };
        int activeCount = 0;
        foreach (Image button in activeButtons)
        {
            if (button.gameObject.activeSelf) activeCount++;
        }

        // Adjust positions based on the number of active buttons
        Vector3 leftPosition = new Vector3(-550, 70, 0);
        Vector3 centerPosition = new Vector3(0, 70, 0);
        Vector3 rightPosition = new Vector3(550, 70, 0);

        if (activeCount == 3)
        {
            // No games completed, arrange in left, center, and right
            SetButtonPosition(leftButton, leftPosition);
            SetButtonPosition(centerButton, centerPosition);
            SetButtonPosition(rightButton, rightPosition);
        }
        else if (activeCount == 2)
        {
            // One game completed, arrange remaining in left and right
            if (leftButton.gameObject.activeSelf && centerButton.gameObject.activeSelf)
            {
                SetButtonPosition(leftButton, leftPosition);
                SetButtonPosition(centerButton, rightPosition);
            }
            else if (leftButton.gameObject.activeSelf && rightButton.gameObject.activeSelf)
            {
                SetButtonPosition(leftButton, leftPosition);
                SetButtonPosition(rightButton, rightPosition);
            }
            else if (centerButton.gameObject.activeSelf && rightButton.gameObject.activeSelf)
            {
                SetButtonPosition(centerButton, leftPosition);
                SetButtonPosition(rightButton, rightPosition);
            }
        }
        else if (activeCount == 1)
        {
            // Two games completed, place the remaining button in the center
            if (leftButton.gameObject.activeSelf)
            {
                SetButtonPosition(leftButton, centerPosition);
            }
            else if (centerButton.gameObject.activeSelf)
            {
                SetButtonPosition(centerButton, centerPosition);
            }
            else if (rightButton.gameObject.activeSelf)
            {
                SetButtonPosition(rightButton, centerPosition);
            }
        }
    }

    // Helper method to set button positions with animation
    private void SetButtonPosition(Image button, Vector3 position)
    {
        LeanTween.moveLocal(button.gameObject, position, 0.5f).setEase(LeanTweenType.easeOutBack).setOnComplete(() =>
        {
            NotifyDraggableHandlers();
        });
    }

    public Vector3 GetButtonPosition(string buttonName)
    {
        switch (buttonName)
        {
            case "Beach Ball":
                return leftButton.transform.position;
            case "Bubbles":
                return centerButton.transform.position;
            case "Frisbee":
                return rightButton.transform.position;
            default:
                Debug.LogWarning($"Button with name {buttonName} not found.");
                return Vector3.zero;
        }
    }

    private void NotifyDraggableHandlers()
    {
        // Notify all draggable handlers to update their initial positions
        foreach (DraggableTextHandler handler in FindObjectsOfType<DraggableTextHandler>())
        {
            handler.UpdateInitialPositionFromHandler();
        }
    }
}