using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class retrybutton : MonoBehaviour
{
    public Button retryButton;
    public GameObject card2; 
    private Image buttonImage; 
    private Image ringImage; 
    private float recordLength;

    private Sprite recordingSprite;
    private Sprite retrySprite;

    private int retryCountCard1 = 0;
    private int retryCountCard2 = 0;
    private const int maxRetries = 2;

    private void Start()
    {
        
        recordingSprite = Resources.Load<Sprite>("Images/STMechanics/RetrySprite");
        retrySprite = Resources.Load<Sprite>("Images/STMechanics/RecordingSprite");

        
        buttonImage = retryButton.GetComponent<Image>();
        ringImage = retryButton.transform.Find("Ring").GetComponent<Image>();
        /*RetrySprite*/



        retryButton.interactable = false;

        // Subscribe to events
        ST_AudioManager.Instance.OnCard1PlaybackComplete += HandleCard1PlaybackComplete;
        ST_AudioManager.Instance.OnRecordingComplete += HandleRecordingComplete;
        ST_AudioManager.Instance.OnCard2Interaction += HandleCard2Interaction;
        ST_AudioManager.Instance.OnRecordingStart += HandleRecordingStart;

        // Add a listener for the Retry button click event
        retryButton.onClick.AddListener(HandleRetryButtonClick);

        // Get the recording length from the AudioManager
        recordLength = ST_AudioManager.Instance.recordLength;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        ST_AudioManager.Instance.OnCard1PlaybackComplete -= HandleCard1PlaybackComplete;
        ST_AudioManager.Instance.OnRecordingComplete -= HandleRecordingComplete;
        ST_AudioManager.Instance.OnCard2Interaction -= HandleCard2Interaction;
        ST_AudioManager.Instance.OnRecordingStart -= HandleRecordingStart;

        // Remove the listener for the Retry button click event
        retryButton.onClick.RemoveListener(HandleRetryButtonClick);
    }

    private void HandleCard1PlaybackComplete()
    {
        HandleRecordingComplete(1);
    }

    private void HandleRecordingComplete(int cardNumber)
    {
        // Change the button image to the retry sprite and enable button interaction
        buttonImage.sprite = retrySprite;

        if (cardNumber == 1 && retryCountCard1 < maxRetries)
        {
            retryButton.interactable = true;
        }
        else if (cardNumber == 2 && retryCountCard2 < maxRetries)
        {
            retryButton.interactable = true;
        }
    }

    private void HandleCard2Interaction()
    {
       
        buttonImage.sprite = retrySprite;

        if (retryCountCard2 < maxRetries)
        {
            retryButton.interactable = true;
        }
    }

    private void HandleRecordingStart()
    {
        // Change the button image to the recording sprite and disable button interaction
        buttonImage.sprite = recordingSprite;
        retryButton.interactable = false;
        
        ringImage.fillAmount = 1f;
        
        LeanTween.value(gameObject, 1f, 0f, recordLength)
            .setOnUpdate((float val) => { ringImage.fillAmount = val; })
            .setEase(LeanTweenType.linear)
            .setOnComplete(() =>
            {                
                HandleRecordingComplete(0);
            });
    }

    private void HandleRetryButtonClick()
    {
        
        if (card2 != null)
        {
            Collider2D card2Collider = card2.GetComponent<Collider2D>();
            if (card2Collider != null)
            {
                card2Collider.enabled = false;
            }
        }

        // Check the current card and update retry count
        string cardTag = ST_AudioManager.Instance.currentCardTag;
        if (cardTag == "Card_1")
        {
            retryCountCard1++;
            Debug.Log($"Retry Button Used for Card 1: {retryCountCard1} times.");
        }
        else if (cardTag == "Card_2")
        {
            retryCountCard2++;
            Debug.Log($"Retry Button Used for Card 2: {retryCountCard2} times.");
        }

        // If the retry count reaches the limit, disable the button
        if ((cardTag == "Card_1" && retryCountCard1 >= maxRetries) ||
            (cardTag == "Card_2" && retryCountCard2 >= maxRetries))
        {
            retryButton.interactable = false;
            Debug.Log($"Retry Button Disabled for {cardTag}");
        }
        else
        {
            // Change the button image to the recording sprite and disable button interaction
            buttonImage.sprite = recordingSprite;
            retryButton.interactable = false;           
            StartCoroutine(WaitForAudioPlaybackAndStartRecording());
        }
    }

    private IEnumerator WaitForAudioPlaybackAndStartRecording()
    {
        string cardTag = ST_AudioManager.Instance.currentCardTag;

        // Play the original audio clip for the current card
        if (cardTag == "Card_1")
        {
            ST_AudioManager.Instance.PlayAudioAfterDestroy("Card_1");
        }
        else if (cardTag == "Card_2")
        {
            ST_AudioManager.Instance.PlayAudioAfterDestroy("Card_2");
        }

       
        yield return new WaitForSeconds(ST_AudioManager.Instance.audioSourceCard1.clip.length);

        
        ST_AudioManager.Instance.TriggerRecordingStart();

        
        ringImage.fillAmount = 1f;

        // Start reducing the ring's fill amount over the recording duration
        LeanTween.value(gameObject, 1f, 0f, recordLength)
            .setOnUpdate((float val) => { ringImage.fillAmount = val; })
            .setEase(LeanTweenType.linear)
            .setOnComplete(() =>
            {                
                HandleRecordingComplete(0);
            });
    }
}
