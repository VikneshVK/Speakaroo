

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class Lvl7Sc1JojoController : MonoBehaviour
{
    public Transform[] stopPositions; // Array of stop positions
    public Transform[] birdStopPositions;
    public Transform[] speechBubbleSpawnPositions; // Array of speech bubble spawn positions for each stop
    public GameObject speechBubblePrefab;
    public Animator jojoAnimator;
    public GameObject birdGameObject;
    public Camera mainCamera; // Reference to the main camera
    public FoodContainerController foodController; // Reference to FoodContainerController script
    public GameObject panelToScale; // Reference to ST Canvas prefab for PrefabTouchHandler
    public float moveSpeed = 2f;
    public float cameraFollowSpeed = 2f; // Speed at which the camera follows Jojo

    public AudioClip Audio1;
    public AudioClip audio1;
    public AudioClip Audio2;
    public AudioClip Audio3;
    public AudioClip Audio4;
    private AudioSource boyAudioSource;
    public static int currentStopIndex = 0; // Current stop position index
    private Animator kikiAnimator;
    private bool AudioPlayed = false;
    private bool isWalking = false;
    private bool isTalking = false;
    private bool cameraFollowing = false; // Whether the camera should follow Jojo
    private bool hasCompletedTalk = false;
    private bool levelEnded = false;
    private bool finaldialouge = false;
    public bool audioPlaying = false;
    private GameObject spawnedPrefab;
    public Sprite sprite1; // Reference to Sprite 1 (assigned in inspector)
    public Sprite sprite2; // Reference to Sprite 2 (assigned in inspector)
    public Sprite sprite3; // Reference to Sprite 3 (assigned in inspector)
    public Image panelImage; // Reference to the Image component on the panel

    public float cameraXOffset = 0f; // Constant offset between Jojo and the camera on the x-axis
    public TextMeshProUGUI subtitleText;
    public Transform cameraFollowPoint; // New reference to child object of Jojo for camera to follow

    public AudioMixer audioMixer;
    private const string musicVolumeParam = "MusicVolume";
    private const string AmbientVolumeParam = "AmbientVolume";
    void Start()
    {
        kikiAnimator = birdGameObject.GetComponent<Animator>();
        boyAudioSource = GetComponent<AudioSource>();
        MoveToNextStopPosition();
    }

    void Update()
    {
        if (isWalking && currentStopIndex < stopPositions.Length)
        {
            MoveCharacter();
        }

        if (cameraFollowing && currentStopIndex > 0) // Only follow the camera if stop position is greater than 1
        {
            FollowCharacterWithCamera();
        }

        if (isTalking)
        {
            AnimatorStateInfo jojoStateInfo = jojoAnimator.GetCurrentAnimatorStateInfo(0);
            if (jojoStateInfo.IsName("Talk") && jojoStateInfo.normalizedTime >= 1.0f) // Jojo's "Talk" animation is complete
            {
                isTalking = false;
                jojoAnimator.SetBool("canTalk", false); // Reset Jojo's "canTalk"

                // Trigger the bird's "Talk" animation
                kikiAnimator.SetBool("canTalk", true);
                if (!AudioPlayed)
                {
                    AudioPlayed = true;
                    boyAudioSource.clip = Audio2;
                    boyAudioSource.Play();
                    StartCoroutine(RevealTextWordByWord("What do you want to Eat?", 0.5f));
                }
            }
        }

        AnimatorStateInfo kikiStateInfo = kikiAnimator.GetCurrentAnimatorStateInfo(0);
        if (kikiStateInfo.IsName("Talk") && kikiStateInfo.normalizedTime >= 1.0f && !hasCompletedTalk) // Bird's "Talk" animation is complete
        {
            kikiAnimator.SetBool("canTalk", false); // Reset Bird's "canTalk"
            OnTalkAnimationComplete(); // Call the method once bird's Talk animation is done
        }

        // Handle "Talk 0" animation completion
        if (jojoAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talk 0") &&
            jojoAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f) // Animation is complete
        {
            jojoAnimator.SetBool("canTalk", false);
            StartCoroutine(DelayMoveToNextStopPosition());
        }

        // Check if the LevelEnd animation is complete and trigger movement to the right
        if (levelEnded && IsAnimationStateComplete("LevelEnd"))
        {
            MoveJojoRightAndEndScene();
        }

        AnimatorStateInfo chewStateInfo = jojoAnimator.GetCurrentAnimatorStateInfo(0);
        if (chewStateInfo.IsName("Chew") && chewStateInfo.normalizedTime >= 0.9f && !audioPlaying) // Animation is complete
        {
            audioPlaying = true;
            jojoAnimator.SetBool("canTalk", true);
            kikiAnimator.SetTrigger("Talk");

            // Play the boy's audio clip if set
            if (audio1 != null && !boyAudioSource.isPlaying) // Avoid replaying if already playing
            {
                
                boyAudioSource.clip = audio1;
                boyAudioSource.Play();
            }
            else if (audio1 == null)
            {
                Debug.LogWarning("Audio1 is not set in the Inspector!");
            }
        }
    }

    private IEnumerator DelayMoveToNextStopPosition()
    {
        yield return new WaitForSeconds(1.5f); // Wait for 1.5 seconds
        if (!levelEnded) // Only move to the next stop if the level has not ended
        {
            MoveToNextStopPosition();
        }
    }





    private void MoveCharacter()
    {
        // Move Jojo towards the current stop position
        Transform jojoTargetStop = stopPositions[currentStopIndex];
        transform.position = Vector3.MoveTowards(transform.position, jojoTargetStop.position, moveSpeed * Time.deltaTime);

        Transform birdTargetStop = birdStopPositions[currentStopIndex];

        if (birdGameObject != null)
        {
            birdGameObject.transform.position = Vector3.MoveTowards(
                birdGameObject.transform.position,
                birdTargetStop.position,
                moveSpeed * Time.deltaTime
            );
        }
        else
        {
            Debug.LogWarning("Bird GameObject not found! Make sure it's tagged as 'Bird'.");
        }

        // Only start following the camera if current stop is not 1
        if (currentStopIndex > 0)
        {
            cameraFollowing = true;
        }

        // Check if Jojo has reached the stop position
        if (Vector3.Distance(transform.position, jojoTargetStop.position) < 0.1f &&
            Vector3.Distance(birdGameObject.transform.position, birdTargetStop.position) < 0.1f)
        {
            isWalking = false;
            jojoAnimator.SetBool("canWalk", false);
            kikiAnimator.SetBool("canFly", false);

            // Stop the camera from following when reaching the stop position
            cameraFollowing = false;

            if (currentStopIndex == 3)
            {
                // Play LevelEnd animation and Audio4 when at the last stop
                jojoAnimator.SetBool("LevelEnd", true);
                if (!boyAudioSource.isPlaying && !finaldialouge)
                {
                    finaldialouge = true;
                    boyAudioSource.clip = Audio4;
                    boyAudioSource.Play();
                    StartCoroutine(RevealTextWordByWord("Look..! It's a Pizza Shop", 0.5f));
                }
                levelEnded = true; // Mark level as ended
                return;
            }
            else if (currentStopIndex == 0)
            {
                // Play Audio1 when at the first stop
                jojoAnimator.SetBool("canTalk", true);
                boyAudioSource.clip = Audio1;
                boyAudioSource.Play();
                StartCoroutine(RevealTextWordByWord("Iam Hungry..! Lets buy some Food", 0.5f));
            }
            else
            {
                // Play Audio3 for all other stops
                jojoAnimator.SetBool("canTalk", true);
                boyAudioSource.clip = Audio3;
                boyAudioSource.Play();
                StartCoroutine(RevealTextWordByWord("I want omething Sweet", 0.5f));
            }

            isTalking = true;
            AudioPlayed = false;
        }
    }

    private void FollowCharacterWithCamera()
    {
        // Follow the child GameObject (cameraFollowPoint) directly without offset
        Vector3 targetCameraPosition = new Vector3(cameraFollowPoint.position.x, mainCamera.transform.position.y, mainCamera.transform.position.z);

        // Move the camera along the x-axis only when Jojo's child object moves
        if (isWalking)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetCameraPosition, cameraFollowSpeed * Time.deltaTime);
        }
    }

    public void MoveToNextStopPosition()
    {
        if (currentStopIndex < stopPositions.Length)
        {
            isWalking = true;
            hasCompletedTalk = false;
            jojoAnimator.SetBool("canWalk", true); // Start walking animation
            kikiAnimator.SetBool("canFly", true);
            Debug.Log($"Moving to the next stop. Current Stop Index: {currentStopIndex}");
        }
    }

    private void OnTalkAnimationComplete()
    {
        /*Debug.Log("OnTalkAnimationComplete called"); // Log to confirm the method is called
        isTalking = false;*/

        if (currentStopIndex < 3)
        {
            SpawnSpeechBubble(currentStopIndex);
        }

        // Advance to the next stop position if there are more stops
        if (currentStopIndex < stopPositions.Length - 1)
        {
            currentStopIndex++;
            hasCompletedTalk = true; // Reset flag for the next stop
            Debug.Log($"Current Stop Index updated to: {currentStopIndex}");
        }
        /*else if (currentStopIndex == stopPositions.Length - 1)
        { 
            Debug.Log("Reached the final stop position. Ending the scene.");
            jojoAnimator.SetBool("LevelEnd", true);
            boyAudioSource.clip = Audio4;
            boyAudioSource.Play();
        }*/
    }

    private void SpawnSpeechBubble(int stopIndex)
    {
        if (speechBubblePrefab != null && stopIndex < speechBubbleSpawnPositions.Length)
        {
            GameObject spawnedBubble = Instantiate(speechBubblePrefab, speechBubbleSpawnPositions[stopIndex].position, Quaternion.identity);

            // Add PrefabTouchHandler and initialize it with the ST Canvas
            var prefabHandler = spawnedBubble.AddComponent<PrefabTouchHandler2>();
            RectTransform panelRectTransform = panelToScale.GetComponent<RectTransform>();
            prefabHandler.Initialize(panelRectTransform);

            // Set up the callback for when the prefab is tapped
            prefabHandler.OnPrefabTapped = () => OnPrefabTapped();
        }
    }

    private void OnPrefabTapped()
    {
        // Handle what happens when the speech bubble is tapped, like activating the ST Canvas
        if (panelToScale != null)
        {
            SetMusicVolume(-80f);
            SetAmbientVolume(-80f);
            ChangePanelSprite();
            panelToScale.SetActive(true);
        }
    }

    private void SetAmbientVolume(float volume)
    {
        if (audioMixer != null)
        {
            bool result = audioMixer.SetFloat(AmbientVolumeParam, volume); // "MusicVolume" should match the exposed parameter name
            if (!result)
            {
                Debug.LogError($"Failed to set MusicVolume to {volume}. Is the parameter exposed?");
            }
        }
        else
        {
            Debug.LogError("AudioMixer is not assigned in the Inspector.");
        }
    }
    private void SetMusicVolume(float volume)
    {
        if (audioMixer != null)
        {
            bool result = audioMixer.SetFloat(musicVolumeParam, volume); // "MusicVolume" should match the exposed parameter name
            if (!result)
            {
                Debug.LogError($"Failed to set MusicVolume to {volume}. Is the parameter exposed?");
            }
        }
        else
        {
            Debug.LogError("AudioMixer is not assigned in the Inspector.");
        }
    }
    private void ChangePanelSprite()
    {
        // Change the sprite based on the stop index
        switch (currentStopIndex)
        {
            case 1:
                panelImage.sprite = sprite1;
                break;
            case 2:
                panelImage.sprite = sprite2;
                break;
            case 3:
                panelImage.sprite = sprite3;
                break;
            default:
                Debug.LogWarning("Invalid stop index or sprite not assigned.");
                break;
        }
    }

    private void MoveJojoRightAndEndScene()
    {
        jojoAnimator.SetBool("canTalk", false);
        jojoAnimator.SetBool("canWalk", true);
        kikiAnimator.SetBool("canFly2", true);
        isWalking = true;

        // Move Jojo and Kiki to the right indefinitely
        transform.position += Vector3.right * moveSpeed * Time.deltaTime;
        if (birdGameObject != null)
        {
            birdGameObject.transform.position += Vector3.right * moveSpeed * Time.deltaTime;
        }

        cameraFollowing = false; // Stop camera movement after the level ends
    }

    // Check if a specific animation state is complete
    private bool IsAnimationStateComplete(string stateName)
    {
        return jojoAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
               jojoAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
    }

    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(true);

        string[] words = fullText.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            subtitleText.text = string.Join(" ", words, 0, i + 1);
            yield return new WaitForSeconds(delayBetweenWords);
        }
        subtitleText.text = "";
    }
}
