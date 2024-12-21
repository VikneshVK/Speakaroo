using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class RetryButton : MonoBehaviour
{
    public Button retryButton;
    public colliderManager colliderManager;
    public GameObject card1;
    private Image buttonImage;
    private Image ringImage;
    private float recordLength;

    private Sprite playbackSprite;
    private Sprite retrySprite;
    private Sprite retrySprite2;
    private Sprite defaultSprite;

    private AudioSource audioSource; // AudioSource attached to the STMechanics GameObject
    private AudioClip scratchAudioClip;
    private AudioClip revealAudioClip;

    private int retryCountCard1 = 0;
    private int retryCountCard2 = 0;
    private const int maxRetries = 1;

    private void Start()
    {
        // Load sprites
        playbackSprite = Resources.Load<Sprite>("Images/STMechanics/speak");
        retrySprite = Resources.Load<Sprite>("Images/STMechanics/speak-2");
        defaultSprite = Resources.Load<Sprite>("Images/STMechanics/speak-2");
        retrySprite2 = Resources.Load<Sprite>("Images/STMechanics/speak-1");
        scratchAudioClip = Resources.Load<AudioClip>("Audio/ScratchAudio");
        revealAudioClip = Resources.Load<AudioClip>("Audio/RevealAudio");
        GameObject stCanvas = GameObject.FindGameObjectWithTag("STCanvas");

        if (stCanvas != null)
        {
            Button[] allButtons = stCanvas.GetComponentsInChildren<Button>(true);

            foreach (Button button in allButtons)
            {
                if (button.name == "RetryButton")
                {
                    retryButton = button;
                    break;
                }
            }

            if (retryButton != null)
            {
                Debug.Log("Retry Button found: " + retryButton.name);
            }
            else
            {
                Debug.LogError("RetryButton not found under STCanvas.");
            }
        }

        buttonImage = retryButton.GetComponent<Image>();
        ringImage = retryButton.transform.Find("Ring").GetComponent<Image>();

        // Initialize AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component missing on STMechanics GameObject.");
        }

        // Set initial sprite to default with alpha 20
        buttonImage.sprite = defaultSprite;
       /* SetAlpha(buttonImage, 20);
        SetAlpha(ringImage, 20);*/

        retryButton.interactable = false;

        // Subscribe to events
        ST_AudioManager.Instance.OnRecordingComplete += HandleRecordingPlaybackEnd;
        ST_AudioManager.Instance.OnCard2Interaction += HandleCard2Interaction;
        ST_AudioManager.Instance.OnRecordingStart += HandleRecordingStart;
        ST_AudioManager.Instance.OnRecordingPlaybackStart += HandleRecordingPlaybackStart;
        ST_AudioManager.Instance.OnRecordingPlaybackEnd += HandleRecordingPlaybackEnd;

        retryButton.onClick.AddListener(HandleRetryButtonClick);

        recordLength = ST_AudioManager.Instance.recordLength;
    }

    private void OnDestroy()
    {
        if (ST_AudioManager.Instance != null)
        {
            // Unsubscribe from events
            ST_AudioManager.Instance.OnRecordingComplete -= HandleRecordingPlaybackEnd;
            ST_AudioManager.Instance.OnCard2Interaction -= HandleCard2Interaction;
            ST_AudioManager.Instance.OnRecordingStart -= HandleRecordingStart;
            ST_AudioManager.Instance.OnRecordingPlaybackStart -= HandleRecordingPlaybackStart;
            ST_AudioManager.Instance.OnRecordingPlaybackEnd -= HandleRecordingPlaybackEnd;
        }

        retryButton.onClick.RemoveListener(HandleRetryButtonClick);
    }

    public void PlayScratchAudio()
    {
        if (scratchAudioClip != null)
        {
            audioSource.clip = scratchAudioClip;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("Scratch audio clip is not assigned.");
        }
    }

    public void PlayRevealAudio()
    {
        if (revealAudioClip != null)
        {
            StartCoroutine(PlayRevealAudioWithDelay());
        }
        else
        {
            Debug.LogError("Reveal audio clip is not assigned.");
        }
    }

    private IEnumerator PlayRevealAudioWithDelay()
    {
        audioSource.clip = revealAudioClip;
        audioSource.Play();
        yield return new WaitForSeconds(2f); // Wait for 2 seconds
        // Add any additional logic after reveal audio here
    }

    public void PlayRevealAudioWithCardAudio(string cardTag)
    {
        StartCoroutine(PlayRevealAndCardAudioCoroutine(cardTag));
    }

    private IEnumerator PlayRevealAndCardAudioCoroutine(string cardTag)
    {
        // Play the reveal audio
        if (revealAudioClip != null)
        {
            audioSource.clip = revealAudioClip;
            audioSource.Play();
            yield return new WaitForSeconds(revealAudioClip.length); // Wait for the reveal audio to finish
        }
        else
        {
            Debug.LogError("Reveal audio clip is not assigned.");
        }

        // Now play the original card audio
        ST_AudioManager.Instance.PlayAudioAfterDestroy(cardTag);
    }

    private void HandleRecordingStart()
    {
        // Change the button image to the retry sprite with full opacity
        buttonImage.sprite = playbackSprite;
        
        retryButton.interactable = false;

        // Start filling the ring during recording
        ringImage.fillAmount = 1f;
        LeanTween.value(gameObject, 1f, 0f, recordLength)
            .setOnUpdate((float val) => { ringImage.fillAmount = val; })
            .setEase(LeanTweenType.linear);
    }

    /*private void HandleRecordingComplete(int cardNumber)
    {
        // Keep the button image as retry sprite
        buttonImage.sprite = retrySprite;
        *//*retryButton.interactable = true;*//*
    }*/

    private void HandleRecordingPlaybackStart()
    {
        // Ensure the recordedAudioSource and its clip are valid before proceeding
        if (ST_AudioManager.Instance.recordedAudioSource != null && ST_AudioManager.Instance.recordedAudioSource.clip != null)
        {
            // Change the button image to the playback sprite
            buttonImage.sprite = defaultSprite;

            // Disable raycasts for the retry button during playback
            retryButton.interactable = false;
            retryButton.GetComponent<Image>().raycastTarget = false;

            // Start filling the ring during playback
            ringImage.fillAmount = 1f;
            LeanTween.value(gameObject, 1f, 0f, ST_AudioManager.Instance.recordedAudioSource.clip.length)
                .setOnUpdate((float val) => { ringImage.fillAmount = val; })
                .setEase(LeanTweenType.linear);

            // Play the recorded audio
            ST_AudioManager.Instance.recordedAudioSource.Play();
        }
        else
        {
            Debug.LogError("Recorded audio source or clip is not set.");
        }
    }


    private void HandleRecordingPlaybackEnd(int cardNumber)
    {
        StopAllCoroutines();
        if (cardNumber == 1)
        {
            StartCoroutine(HandlePostPlaybackActionsForCard1());
        }
        else if (cardNumber == 2)
        {
            StartCoroutine(HandlePostPlaybackActionsForCard2());
        }        
    }


    private IEnumerator HandlePostPlaybackActionsForCard1()
    {
        Debug.Log("Handling retry for Card 1...");
        if (retryCountCard1 >= maxRetries)
        {
            buttonImage.sprite = defaultSprite;
            retryButton.interactable = false; // Disable interaction
            retryButton.GetComponent<Image>().raycastTarget = false; // Disable raycasts.
            
           /* SetAlpha(buttonImage, 20);
            SetAlpha(ringImage, 20);*/

            Debug.Log("Max retries reached for Card 1. Retry button is now disabled.");

            EnableCard2(); // Enable Card 2 for interaction
            yield break;
        }

        if (retryCountCard1 < maxRetries)
        {
            buttonImage.sprite = retrySprite2;
            retryButton.interactable = true;
            retryButton.GetComponent<Image>().raycastTarget = true;
            yield return new WaitForSeconds(4f);
            retryButton.interactable = false;
            retryButton.GetComponent<Image>().raycastTarget = false;          

        }
    }

    private IEnumerator HandlePostPlaybackActionsForCard2()
    {
        Debug.Log("Handling retry for Card 2...");
        if (retryCountCard2 >= maxRetries)
        {
            retryButton.interactable = false; // Disable interaction
            retryButton.GetComponent<Image>().raycastTarget = false;
            Debug.Log("Max retries reached for Card 2. Completing playback.");
            ST_AudioManager.Instance.TriggerOnPlaybackComplete();
            yield break;
        }

        if (retryCountCard2 < maxRetries)
        {
            buttonImage.sprite = retrySprite2;
            retryButton.interactable = true;
            retryButton.GetComponent<Image>().raycastTarget = true;
            yield return new WaitForSeconds(4f);
            retryButton.interactable = false;
            retryButton.GetComponent<Image>().raycastTarget = false;
            ST_AudioManager.Instance.TriggerOnPlaybackComplete();
        }
    }




    private void EnableCard2()
    {
        Debug.Log("Enabling Card 2...");
        if (colliderManager != null && colliderManager.card2Front != null)
        {
            colliderManager.card2Front.GetComponent<Collider2D>().enabled = true;
        }

        // Update active card state
        card1 = null; // Mark Card 1 as completed
        retryCountCard2 = 0; // Reset retry count for Card 2

        // Reset button appearance for Card 2
        buttonImage.sprite = defaultSprite;
       /* SetAlpha(buttonImage, 20);
        SetAlpha(ringImage, 20);*/

        Debug.Log("Card 2 enabled. Ready for interaction.");
    }


    private void HandleCard2Interaction()
    {

        buttonImage.sprite = defaultSprite;
       /* SetAlpha(buttonImage, 20);
        SetAlpha(ringImage, 20);*/
        retryButton.interactable = false;
    }

    public void HandleRetryButtonClick()
    {
        string cardTag = ST_AudioManager.Instance.currentCardTag;
        if (cardTag == "Card_1")
        {
            retryCountCard1++;
            Debug.Log($"Retry Button Used for Card 1: {retryCountCard1} times.");

            if (retryCountCard1 >= maxRetries)
            {
                retryButton.interactable = false;
                EnableCard2();
                return;
            }
        }
        else if (cardTag == "Card_2")
        {
            retryCountCard2++;
            Debug.Log($"Retry Button Used for Card 2: {retryCountCard2} times.");

            if (retryCountCard2 >= maxRetries)
            {
                retryButton.interactable = false;
                ST_AudioManager.Instance.TriggerOnPlaybackComplete();
                return;
            }
        }

        // Disable raycasts during retry
        retryButton.interactable = false;
        retryButton.GetComponent<Image>().raycastTarget = false;

        // Default behavior for retries below the max count
        StartCoroutine(WaitForAudioPlaybackAndStartRecording());
    }



    private IEnumerator WaitForAudioPlaybackAndStartRecording()
    {
        string cardTag = ST_AudioManager.Instance.currentCardTag;

        // Play the original audio clip for the current card
        if (cardTag == "Card_1")
        {
            ST_AudioManager.Instance.PlayAudioAfterDestroy("Card_1");
            yield return new WaitForSeconds(ST_AudioManager.Instance.audioSourceCard1.clip.length);
        }
        else if (cardTag == "Card_2")
        {
            ST_AudioManager.Instance.PlayAudioAfterDestroy("Card_2");
            yield return new WaitForSeconds(ST_AudioManager.Instance.audioSourceCard2.clip.length);
        }

        // Enable raycasts after retry operation
        retryButton.interactable = true;
        retryButton.GetComponent<Image>().raycastTarget = true;

        ringImage.fillAmount = 1f;

        LeanTween.value(gameObject, 1f, 0f, recordLength)
            .setOnUpdate((float val) => { ringImage.fillAmount = val; })
            .setEase(LeanTweenType.linear)
            .setOnComplete(() =>
            {
                HandleRecordingPlaybackEnd(0);
            });
    }


   /* private void SetAlpha(Image image, float alphaValue)
    {
        Color color = image.color;
        color.a = alphaValue / 255f;
        image.color = color;
    }*/
}