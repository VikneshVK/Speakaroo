using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RetryButton : MonoBehaviour
{
    public Button retryButton;
    public GameObject card2;
    private Image buttonImage;
    private Image ringImage;
    private float recordLength;

    private Sprite playbackSprite;
    private Sprite retrySprite;
    private Sprite defaultSprite;

    private AudioSource audioSource; // AudioSource attached to the STMechanics GameObject
    private AudioClip scratchAudioClip;
    private AudioClip revealAudioClip;

    private int retryCountCard1 = 0;
    private int retryCountCard2 = 0;
    private const int maxRetries = 2;

    private void Start()
    {
        // Load sprites
        playbackSprite = Resources.Load<Sprite>("Images/STMechanics/PlaybackSprite");
        retrySprite = Resources.Load<Sprite>("Images/STMechanics/RetrySprite");
        defaultSprite = Resources.Load<Sprite>("Images/STMechanics/DefaultSprite");
        scratchAudioClip = Resources.Load<AudioClip>("Audio/ScratchAudio");
        revealAudioClip = Resources.Load<AudioClip>("Audio/RevealAudio");

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
        SetAlpha(buttonImage, 20);
        SetAlpha(ringImage, 20);

        retryButton.interactable = false;

        // Subscribe to events
        ST_AudioManager.Instance.OnRecordingComplete += HandleRecordingComplete;
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
            ST_AudioManager.Instance.OnRecordingComplete -= HandleRecordingComplete;
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
        buttonImage.sprite = retrySprite;
        SetAlpha(buttonImage, 255);
        SetAlpha(ringImage, 255);
        retryButton.interactable = false;

        // Start filling the ring during recording
        ringImage.fillAmount = 1f;
        LeanTween.value(gameObject, 1f, 0f, recordLength)
            .setOnUpdate((float val) => { ringImage.fillAmount = val; })
            .setEase(LeanTweenType.linear);
    }

    private void HandleRecordingComplete(int cardNumber)
    {
        // Keep the button image as retry sprite
        buttonImage.sprite = retrySprite;
        retryButton.interactable = true;
    }

    private void HandleRecordingPlaybackStart()
    {
        // Ensure the recordedAudioSource and its clip are valid before proceeding
        if (ST_AudioManager.Instance.recordedAudioSource != null && ST_AudioManager.Instance.recordedAudioSource.clip != null)
        {
            // Change the button image to the playback sprite
            buttonImage.sprite = playbackSprite;

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

    private void HandleRecordingPlaybackEnd()
    {
        StartCoroutine(HandlePostPlaybackActions());
    }

    private IEnumerator HandlePostPlaybackActions()
    {

        retryButton.interactable = true;
        yield return new WaitForSeconds(4f);

        // After 4-second delay
        retryButton.interactable = false;

        if (retryCountCard1 < maxRetries)
        {

            EnableCard2();
        }
        else if (retryCountCard2 < maxRetries)
        {

            ST_AudioManager.Instance.TriggerOnPlaybackComplete();
        }
    }

    private void EnableCard2()
    {

        if (card2 != null)
        {
            Collider2D card2Collider = card2.GetComponent<Collider2D>();
            if (card2Collider != null)
            {
                card2Collider.enabled = true;
            }
        }

        buttonImage.sprite = defaultSprite;
        SetAlpha(buttonImage, 20);
        SetAlpha(ringImage, 20);
    }

    private void HandleCard2Interaction()
    {

        buttonImage.sprite = defaultSprite;
        SetAlpha(buttonImage, 20);
        SetAlpha(ringImage, 20);
        retryButton.interactable = false;
    }

    private void HandleRetryButtonClick()
    {

        StopAllCoroutines();

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

        // Disable the button if maximum retries are reached
        if ((cardTag == "Card_1" && retryCountCard1 >= maxRetries) ||
            (cardTag == "Card_2" && retryCountCard2 >= maxRetries))
        {
            retryButton.interactable = false;
            Debug.Log($"Retry Button Disabled for {cardTag}");
        }
        else
        {
            // Change the button image to the retry sprite and disable interaction
            buttonImage.sprite = retrySprite;
            retryButton.interactable = false;

            // Start the process of waiting for the audio to finish and then starting the recording
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
            yield return new WaitForSeconds(ST_AudioManager.Instance.audioSourceCard1.clip.length);
        }
        else if (cardTag == "Card_2")
        {
            ST_AudioManager.Instance.PlayAudioAfterDestroy("Card_2");
            yield return new WaitForSeconds(ST_AudioManager.Instance.audioSourceCard2.clip.length);
        }


        ringImage.fillAmount = 1f;

        LeanTween.value(gameObject, 1f, 0f, recordLength)
            .setOnUpdate((float val) => { ringImage.fillAmount = val; })
            .setEase(LeanTweenType.linear)
            .setOnComplete(() =>
            {
                HandleRecordingComplete(0);
            });
    }

    private void SetAlpha(Image image, float alphaValue)
    {
        Color color = image.color;
        color.a = alphaValue / 255f;
        image.color = color;
    }
}