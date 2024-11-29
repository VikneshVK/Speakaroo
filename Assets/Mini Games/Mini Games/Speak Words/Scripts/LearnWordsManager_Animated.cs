using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LearnWordsManager_Animated : MonoBehaviour
{
    // Reference to your animated GameObjects
    public GameObject[] animatedItems;   // Array of GameObjects with animations
    public Button nextButton;            // Reference to the Next button
    public Button previousButton;        // Reference to the Previous button  

    // Mechanics
    private int currentIndex = 0;
    private bool[] rewardGiven;          // Array to track if reward has been given for each item
    private bool isAnimating = false;    // Track if an animation is currently playing
    public TextMeshProUGUI itemNameText; // Name of the item displayed
    public Image progressBar;            // Reference to your progress bar
    public int totalItems;
    public int finishedItems;

    // congratulations & audios
    private bool isCongratulating;
    public AudioSource congratulationsAudioSource; // Reference to an AudioSource component for playing audio
    public AudioSource audioSource;                // Add this missing AudioSource for general audio clips
    public AudioClip[] CongratulationsaudioClip;   // Correct name of the congratulatory clips array
    public GameObject confettiLeft;
    public GameObject confettiRight;
    public float sequenceDuration;
    public AudioClip[] itemAudioClips;

    // Confetti prefabs
    public GameObject confettiPrefab;              // Add this for the confetti prefab
    public Transform confettiSpawnPoint;           // Add this for the spawn point of the confetti

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the rewardGiven array based on the number of items
        rewardGiven = new bool[animatedItems.Length];

        // Initialize with the first item
        DisplayItem(currentIndex, false);
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the audio is finished and no animation is happening
        if (!audioSource.isPlaying && !isAnimating && !rewardGiven[currentIndex])
        {
            RewardPlayer();
        }
    }

    public void DisplayItem(int index, bool animate = true)
    {
        // Ensure index is within valid bounds
        if (index < 0 || index >= animatedItems.Length)
        {
            Debug.LogError("Index out of bounds: " + index);
            return;
        }

        // Deactivate all items first
        foreach (GameObject item in animatedItems)
        {
            item.SetActive(false);
        }

        // Activate the current item
        animatedItems[index].SetActive(true);

        // Animate Image properties using Animator or LeanTween
        if (animate)
        {
            Animator anim = animatedItems[index].GetComponent<Animator>();
            if (anim != null)
            {
                anim.Play("PlayColor");  // Ensure this state modifies the Image component
                isAnimating = true;
            }
            else
            {
                // Example color fade as fallback:
                Image img = animatedItems[index].GetComponent<Image>();
                LeanTween.color(img.rectTransform, Color.white, 0.5f).setLoopPingPong();
            }
        }

        // Set the item name in UI
        itemNameText.text = animatedItems[index].name;

        // Play the corresponding audio
        audioSource.clip = itemAudioClips[index];
        audioSource.Play();
    }


    public void NextItem()
    {
        if (currentIndex < animatedItems.Length - 1)
        {
            currentIndex++;
            DisplayItem(currentIndex, true);  // Display the next item
        }
    }

    public void PreviousItem()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            DisplayItem(currentIndex, false);  // Display the previous item
        }
    }

    void RewardPlayer()
    {
        if (!rewardGiven[currentIndex])
        {
            rewardGiven[currentIndex] = true;
            finishedItems++;

            // Update the progress bar
            float progress = (float)finishedItems / totalItems;
            ////LeanTween.value(progressBar.gameObject, progressBar.fillAmount, progress, 0.5f).setOnUpdate((float val) =>
            //{
            //    progressBar.fillAmount = val;
            //});

            // Play congratulatory sequence if necessary
            if (finishedItems % 5 == 0 && finishedItems > 0)
            {
                StartCoroutine(PlayCongratulatorySequence());
            }
        }
    }

    private IEnumerator PlayCongratulatorySequence()
    {
        // Disable interactions
        nextButton.interactable = false;
        previousButton.interactable = false;

        // Play random congratulatory audio
        AudioClip randomCongratsClip = CongratulationsaudioClip[Random.Range(0, CongratulationsaudioClip.Length)];
        congratulationsAudioSource.clip = randomCongratsClip;
        congratulationsAudioSource.Play();

        // Spawn confetti effect at a defined position
        Instantiate(confettiPrefab, confettiSpawnPoint.position, Quaternion.identity);

        // Wait until the audio clip finishes playing
        yield return new WaitForSeconds(randomCongratsClip.length);

        // Re-enable interactions after the sequence
        nextButton.interactable = true;
        previousButton.interactable = true;
    }
}
