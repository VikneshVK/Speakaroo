using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LearnWordsManager : MonoBehaviour
{
    public Image itemImage;
    public AudioSource audioSource;
    public Sprite[] itemSprites;
    public AudioClip[] itemAudioClips;
    public Button nextButton;
    public Button previousButton;
    public TextMeshProUGUI itemNameText;
    public Image progressBar;
    public int totalItems;
    public int finishedItems;

    public AudioSource congratulationsAudioSource;
    public AudioClip[] CongratulationsaudioClip;
    public GameObject confettiLeft;
    public GameObject confettiRight;
    public float sequenceDuration;

    private int currentIndex = 0;
    private bool[] rewardGiven;
    private Vector3 initialImagePosition;
    private Vector3 initialTextPosition;
    private bool isAnimating = false;
    private bool isCongratulating = false;

    void Start()
    {
        rewardGiven = new bool[itemSprites.Length];
        initialImagePosition = itemImage.transform.position;
        initialTextPosition = itemNameText.transform.position;
        totalItems = itemSprites.Length;
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
        Debug.Log($"Buttons set to interactable: {interactable}");
    }

    public void DisplayItem(int index, bool animate = true, bool isNext = true)
    {
        if (index < 0 || index >= itemSprites.Length)
        {
            Debug.LogError("Index out of bounds: " + index);
            return;
        }

        SetButtonInteractable(false);  // Disable buttons before starting animation

        if (animate)
        {
            isAnimating = true;
            float startScale = isNext ? 0.8f : 1.2f;
            float endScale = 1f;

            // Animate the image
            LeanTween.moveX(itemImage.gameObject, isNext ? -Screen.width : Screen.width, 0.6f)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() =>
                {
                    itemImage.sprite = itemSprites[index];
                    itemImage.transform.position = new Vector3(isNext ? Screen.width : -Screen.width, initialImagePosition.y, initialImagePosition.z);
                    itemImage.transform.localScale = new Vector3(startScale, startScale, 1f);

                    LeanTween.moveX(itemImage.gameObject, initialImagePosition.x, 0.6f)
                        .setEase(LeanTweenType.easeInOutQuad);
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

            // Animate the text component
            LeanTween.moveY(itemNameText.gameObject, -Screen.height / 2, 0.6f)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() =>
                {
                    // Update the text content when it's off-screen
                    itemNameText.text = itemSprites[index].name;

                    // Reset text position off-screen before moving it back up
                    itemNameText.transform.position = new Vector3(initialTextPosition.x, -Screen.height / 2, initialTextPosition.z);

                    LeanTween.moveY(itemNameText.gameObject, initialTextPosition.y, 0.6f)
                        .setEase(LeanTweenType.easeOutBack);
                });
        }
        else
        {
            // No animation: Set item directly
            itemImage.sprite = itemSprites[index];
            itemImage.transform.position = initialImagePosition;
            itemImage.transform.localScale = Vector3.one;

            itemNameText.text = itemSprites[index].name;
            itemNameText.transform.position = initialTextPosition;

            isAnimating = false;
            audioSource.clip = itemAudioClips[index];
            audioSource.Play();
            Invoke(nameof(EnableButtonsAfterAudio), audioSource.clip.length);
        }
    }


    private void EnableButtonsAfterAudio()
    {
        SetButtonInteractable(true);
        Debug.Log("Buttons re-enabled after audio.");
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

    void RewardPlayer()
    {
        if (!rewardGiven[currentIndex])
        {
            rewardGiven[currentIndex] = true;
            finishedItems++;

            float progress = (float)finishedItems / totalItems;
            LeanTween.value(progressBar.gameObject, progressBar.fillAmount, progress, 0.5f)
                .setOnUpdate((float val) =>
                {
                    progressBar.fillAmount = val;
                });

            if (finishedItems % 5 == 0 && finishedItems > 0)
            {
                StartCoroutine(PlayCongratulatorySequence());
            }
        }
    }

    private IEnumerator PlayCongratulatorySequence()
    {
        isCongratulating = true;

        int randomIndex = Random.Range(0, CongratulationsaudioClip.Length);
        congratulationsAudioSource.clip = CongratulationsaudioClip[randomIndex];
        congratulationsAudioSource.Play();

        confettiLeft.GetComponent<ParticleSystem>().Play();
        confettiRight.GetComponent<ParticleSystem>().Play();

        yield return new WaitForSeconds(congratulationsAudioSource.clip.length);

        confettiLeft.GetComponent<ParticleSystem>().Stop();
        confettiRight.GetComponent<ParticleSystem>().Stop();

        yield return new WaitForSeconds(3f);
        isCongratulating = false;
    }
}
