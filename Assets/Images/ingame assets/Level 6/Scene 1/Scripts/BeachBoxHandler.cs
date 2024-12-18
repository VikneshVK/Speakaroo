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
    public Image boxbgGlow;
    public TextMeshProUGUI textComponent;
   
    public string updatedText = "What should we play with?";
    
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
            CheckCompletedMiniGames();
            StartCoroutine(ScaleActiveButtonsAfterPositioning());
        }
    }

    private void triggerRaycastOn()
    {
        leftButton.GetComponent<Image>().raycastTarget = true;
        centerButton.GetComponent<Image>().raycastTarget = true;
        rightButton.GetComponent<Image>().raycastTarget = true;
    }

    private void triggerRaycastOFf()
    {
        leftButton.GetComponent<Image>().raycastTarget = false;
        centerButton.GetComponent<Image>().raycastTarget = false;
        rightButton.GetComponent<Image>().raycastTarget = false;
    }

    private IEnumerator ScaleActiveButtonsAfterPositioning()
    {
        yield return new WaitForSeconds(0.6f);

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
        yield return new WaitForSeconds(0.6f);
        triggerRaycastOn();

        leftButton.GetComponent<Button>().interactable = true;
        centerButton.GetComponent<Button>().interactable = true;
        rightButton.GetComponent<Button>().interactable = true;
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
        beachBoxDestroyed = true;
        beachboxSprite.enabled = false;
        boxbgGlow.enabled = false;
        EnableAndTweenButtons();

        if (textComponent != null)
        {
            textComponent.text = updatedText;
        }
        
    }

    private void EnableAndTweenButtons()
    {
        triggerRaycastOFf();

        leftButton.GetComponent<Button>().interactable = false;
        centerButton.GetComponent<Button>().interactable = false;
        rightButton.GetComponent<Button>().interactable = false;

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
                     NotifyDraggableHandlers();   
                     StartCoroutine(ScaleActiveButtonsAfterPositioning());
                 });
    }

    private IEnumerator ScaleChildAndPlayEffect(Image button)
    {
        Transform childComponent = button.transform.GetChild(0);
        AudioSource buttonAudioSource = button.GetComponent<AudioSource>();

        if (childComponent != null && highlighter != null)
        {
            highlighter.transform.position = button.transform.position;
            highlighter.transform.localScale = Vector3.zero;
            childComponent.localScale = Vector3.zero;

            LeanTween.scale(childComponent.gameObject, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);
            LeanTween.scale(highlighter.gameObject, Vector3.one * 8f, 0.5f).setEase(LeanTweenType.easeOutBack);

            yield return new WaitForSeconds(0.5f);

            if (buttonAudioSource != null)
            {
                buttonAudioSource.Play();
            }

            LeanTween.scale(highlighter.gameObject, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInBack);

            yield return new WaitForSeconds(0.5f);
            
        }
    }

    private void CheckCompletedMiniGames()
    {
        if (miniGameController == null) return;
        

        bool beachBallCompleted = miniGameController.IsMiniGameCompleted("BeachBallMiniGame");
        bool bubblesCompleted = miniGameController.IsMiniGameCompleted("BubbleMinigame");
        bool frisbeeCompleted = miniGameController.IsMiniGameCompleted("FrisbeeMiniGame");

        leftButton.gameObject.SetActive(!beachBallCompleted);
        centerButton.gameObject.SetActive(!bubblesCompleted);
        rightButton.gameObject.SetActive(!frisbeeCompleted);

        Image[] activeButtons = { leftButton, centerButton, rightButton };
        int activeCount = 0;

        foreach (Image button in activeButtons)
        {
            if (button.gameObject.activeSelf) activeCount++;
        }

        Vector3 leftPosition = new Vector3(-550, 70, 0);
        Vector3 centerPosition = new Vector3(0, 70, 0);
        Vector3 rightPosition = new Vector3(550, 70, 0);

        if (activeCount == 3)
        {
            SetButtonPosition(leftButton, leftPosition);
            SetButtonPosition(centerButton, centerPosition);
            SetButtonPosition(rightButton, rightPosition);
        }
        else if (activeCount == 2)
        {
            if (leftButton.gameObject.activeSelf && centerButton.gameObject.activeSelf)
            {
                SetButtonPosition(leftButton, leftPosition);
                SetButtonPosition(centerButton, rightPosition);
            }
            else if (centerButton.gameObject.activeSelf && rightButton.gameObject.activeSelf)
            {
                SetButtonPosition(centerButton, leftPosition);
                SetButtonPosition(rightButton, rightPosition);
            }
        }
        else if (activeCount == 1)
        {
            if (leftButton.gameObject.activeSelf)
            {
                SetButtonPosition(leftButton, centerPosition);
            }
            else if (centerButton.gameObject.activeSelf)
            {
                SetButtonPosition(centerButton, centerPosition);
            }
        }
    }
    public Vector3 GetButtonPosition(string buttonName)
    {
        switch (buttonName)
        {
            case "LeftButton":
                return leftButton.transform.position;
            case "CenterButton":
                return centerButton.transform.position;
            case "RightButton":
                return rightButton.transform.position;
            default:
                Debug.LogWarning("Button name not recognized: " + buttonName);
                return Vector3.zero;
        }
    }


    private void SetButtonPosition(Image button, Vector3 position)
    {
        LeanTween.moveLocal(button.gameObject, position, 0.5f).setEase(LeanTweenType.easeOutBack);
    }

    private void NotifyDraggableHandlers()
    {
        foreach (DraggableTextHandler handler in FindObjectsOfType<DraggableTextHandler>())
        {
            handler.UpdateInitialPositionFromHandler();
        }
    }
}
