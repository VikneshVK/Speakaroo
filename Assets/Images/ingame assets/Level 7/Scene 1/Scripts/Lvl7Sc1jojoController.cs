using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using Unity.VisualScripting;


public class Lvl7Sc1JojoController : MonoBehaviour
{
    public Transform[] stopPositions; // Array of stop positions
    public Transform[] birdStopPositions;
    public Transform[] speechBubbleSpawnPositions; // Array of speech bubble spawn positions for each stop
    public GameObject[] foodPrefabs;
    public GameObject speechBubblePrefab;
    public Animator jojoAnimator;
    public GameObject birdGameObject;
    public Camera mainCamera; // Reference to the main camera
    public FoodContainerController foodController; // Reference to FoodContainerController script
    public int PrefabToSpawn;
    public GameObject panelToScale1; // Reference to panel 1 (assigned in inspector)
    public bool panel1Complete;
    public GameObject panelToScale2; // Reference to panel 2 (assigned in inspector)
    public bool panel2Complete;
    public GameObject panelToScale3;  // Reference to ST Canvas prefab for PrefabTouchHandler
    public bool panel3Complete;
    private bool isPanelActive = false;
    public float moveSpeed = 2f;
    public float cameraFollowSpeed = 2f; // Speed at which the camera follows Jojo

   
    public Transform foodDropLocation;
    public Transform foodSpawnLocation1;
    public Transform foodSpawnLocation2;
    public Transform foodSpawnLocation3;
    private bool isFood1Spawned;
    private bool isFood2Spawned;
    private bool isFood3Spawned;

    public AudioClip Audio1;
    public AudioClip audio1;
    public AudioClip Audio2;
    public AudioClip Audio3;
    public AudioClip Audio4;
    private AudioSource boyAudioSource;
    public static int currentStopIndex; 
    private Animator kikiAnimator;
    private bool AudioPlayed = false;
    private bool isWalking = false;
    private bool isTalking = false;
    private bool cameraFollowing = false; // Whether the camera should follow Jojo
    private bool hasCompletedTalk = false;
    private bool levelEnded = false;
    private bool finaldialouge = false;
    private bool sfxAudioPlayed = false;
    public bool audioPlaying = false;
    private GameObject spawnedPrefab;
    public Sprite sprite1; // Reference to Sprite 1 (assigned in inspector)
    public Sprite sprite2; // Reference to Sprite 2 (assigned in inspector)
    public Sprite sprite3; // Reference to Sprite 3 (assigned in inspector)
    public Image panelImage; // Reference to the Image component on the panel

    public float cameraXOffset = 0f; // Constant offset between Jojo and the camera on the x-axis
    public TextMeshProUGUI subtitleText;
    public Transform cameraFollowPoint; // New reference to child object of Jojo for camera to follow

    [Header("SFX")]
    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;
    

    public AudioMixer audioMixer;
    private const string musicVolumeParam = "MusicVolume";
    private const string AmbientVolumeParam = "AmbientVolume";
    void Start()
    {
        PrefabToSpawn = 0;
        kikiAnimator = birdGameObject.GetComponent<Animator>();
        boyAudioSource = GetComponent<AudioSource>();
        MoveToNextStopPosition();
        panel1Complete = false;
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
        currentStopIndex = 0;
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
        if (panel1Complete)
        {
            panel1Complete = false; // Reset the flag
            HandlePanel1Completion();
        }
        if (panel2Complete)
        {
            panel2Complete = false; // Reset the flag
            HandlePanel2Completion();
        }
        if (panel3Complete)
        {
            panel3Complete = false; // Reset the flag
            HandlePanel3Completion();
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

    private void HandlePanel1Completion()
    {
        Transform contentHolderTransform = panelToScale1.transform.Find("Content Holder");
        GameObject contentHolderGameObject = contentHolderTransform.gameObject; // <--- Add this line

        LeanTween.scale(contentHolderGameObject, Vector3.zero, 0.5f)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() =>
            {
                if (BGAudioManager_Final.Instance != null && BGAudioManager_Final.Instance.IsVolumeEnabled())
                {
                    SetMusicVolume(-25f);
                }
                SetAmbientVolume(-10f);
                panelToScale1.SetActive(false);
                int prefabIndex = Mathf.Clamp(PrefabToSpawn - 1, 0, foodPrefabs.Length - 1);
                if (!isFood1Spawned)
                {
                    isFood1Spawned = true;
                    SpawnAndTweenPrefab(prefabIndex, foodSpawnLocation1);
                }

            });
    }

    private void HandlePanel2Completion()
    {
        Transform contentHolderTransform = panelToScale2.transform.Find("Content Holder");
        GameObject contentHolderGameObject = contentHolderTransform.gameObject; // <--- Add this line

        LeanTween.scale(contentHolderGameObject, Vector3.zero, 0.5f)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() =>
            {
                if (BGAudioManager_Final.Instance != null && BGAudioManager_Final.Instance.IsVolumeEnabled())
                {
                    SetMusicVolume(-25f);
                }
                SetAmbientVolume(-10f);
                panelToScale2.SetActive(false);
                int prefabIndex = Mathf.Clamp(PrefabToSpawn - 1, 0, foodPrefabs.Length - 1);
                if (!isFood2Spawned)
                {
                    isFood2Spawned = true;
                    SpawnAndTweenPrefab(prefabIndex, foodSpawnLocation2);
                }

            });
    }
    private void HandlePanel3Completion()
    {
        Transform contentHolderTransform = panelToScale3.transform.Find("Content Holder");
        GameObject contentHolderGameObject = contentHolderTransform.gameObject; // <--- Add this line

        LeanTween.scale(contentHolderGameObject, Vector3.zero, 0.5f)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() =>
            {
                panelToScale3.SetActive(false);
                int prefabIndex = Mathf.Clamp(PrefabToSpawn - 1, 0, foodPrefabs.Length - 1);
                if (!isFood3Spawned)
                {
                    if (BGAudioManager_Final.Instance != null && BGAudioManager_Final.Instance.IsVolumeEnabled())
                    {
                        SetMusicVolume(-25f);
                    }
                    SetAmbientVolume(-10f);
                    isFood3Spawned = true;
                    SpawnAndTweenPrefab(prefabIndex, foodSpawnLocation3);
                }

            });
    }


    private void SpawnAndTweenPrefab(int prefabIndex, Transform position)
    {
        if (prefabIndex < 0 || prefabIndex >= foodPrefabs.Length)
        {
            Debug.LogError("Invalid prefabIndex value.");
            return;
        }

        spawnedPrefab = Instantiate(foodPrefabs[prefabIndex], position.position, Quaternion.identity);
        LeanTween.move(spawnedPrefab, foodDropLocation.position, 1f)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnComplete(() =>
            {
                Destroy(spawnedPrefab);
                jojoAnimator.SetTrigger("Chew");
            });
    }




    private void MoveCharacter()
    {
        // Move Jojo towards the current stop position
        Transform jojoTargetStop = stopPositions[currentStopIndex];
        transform.position = Vector3.MoveTowards(transform.position, jojoTargetStop.position, moveSpeed * Time.deltaTime);
        if (!sfxAudioPlayed)
        {
            sfxAudioPlayed = true;
            SfxAudioSource.loop = true;
            SfxAudioSource.clip = SfxAudio1;
            SfxAudioSource.Play();
        }
        
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
            
            SfxAudioSource.Stop();
            SfxAudioSource.loop = false;
            sfxAudioPlayed = false;
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
                StartCoroutine(RevealTextWordByWord("I am Hungry..! Lets buy some Food", 0.5f));
            }
            else
            {
                // Play Audio3 for all other stops
                jojoAnimator.SetBool("canTalk", true);
                boyAudioSource.clip = Audio3;
                boyAudioSource.Play();
                StartCoroutine(RevealTextWordByWord("I want Something Sweet", 0.5f));
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

        if (currentStopIndex < 3)
        {
            SpawnSpeechBubble(currentStopIndex);
        }

        if (currentStopIndex < stopPositions.Length - 1)
        {
            currentStopIndex++;
            hasCompletedTalk = true; // Reset flag for the next stop
            Debug.Log($"Current Stop Index updated to: {currentStopIndex}");
        }
        
    }

    private void SpawnSpeechBubble(int stopIndex)
    {
        if (speechBubblePrefab != null && stopIndex < speechBubbleSpawnPositions.Length)
        {
            GameObject spawnedBubble = Instantiate(speechBubblePrefab, speechBubbleSpawnPositions[stopIndex].position, Quaternion.identity);

            var prefabHandler = spawnedBubble.AddComponent<PrefabTouchHandler2>();
           /* RectTransform panelRectTransform = panelToScale.GetComponent<RectTransform>();
            prefabHandler.Initialize(panelRectTransform);*/

            prefabHandler.OnPrefabTapped = () => OnPrefabTapped();
        }
    }

    private void OnPrefabTapped()
    {
        // Handle music and ambient volume
        SetMusicVolume(-80f);
        SetAmbientVolume(-80f);

        // Activate the correct panel based on the current stop index
        switch (currentStopIndex)
        {
            case 1:
                EnablePanelAndScaleChildren(panelToScale1, "Open", 2f);
                Debug.Log("Panel 1 Enabled.");
                break;
            case 2:
                EnablePanelAndScaleChildren(panelToScale2, "Open", 2f);
                Debug.Log("Panel 2 Enabled.");
                break;
            case 3:
                EnablePanelAndScaleChildren(panelToScale3, "Open", 2f);
                Debug.Log("Panel 3 Enabled.");
                break;
            default:
                Debug.LogWarning("Invalid stop index. No panel activated.");
                break;
        }

        // Change the panel sprite if applicable
        ChangePanelSprite();
    }

    private void EnablePanelAndScaleChildren(GameObject panelToScale, string animatorTrigger, float delay)
    {
        StartCoroutine(HandlePanelAnimationAndScaling(panelToScale, animatorTrigger, delay));
    }

    private IEnumerator HandlePanelAnimationAndScaling(GameObject panelToScale, string animatorTrigger, float delay)
    {
        if (panelToScale == null)
        {
            Debug.LogWarning("Panel to scale is null. Cannot proceed.");
            yield break;
        }

        panelToScale.SetActive(true);

        Transform contentHolder = panelToScale.transform.Find("Content Holder");
        Animator menuAnimator = null;
        Transform uiElementsHolder = null;

        if (contentHolder != null)
        {
            Transform menuPanel = contentHolder.Find("Menu Panel");
            if (menuPanel != null)
            {
                menuAnimator = menuPanel.GetComponent<Animator>();
                menuAnimator?.SetTrigger(animatorTrigger);
                Debug.Log($"Triggered {animatorTrigger} animation on {menuPanel.name}");
            }

            uiElementsHolder = contentHolder.Find("UIElemtens Holder");
        }

        yield return new WaitForSeconds(delay);

        if (uiElementsHolder == null)
        {
            Debug.LogWarning("UIElements Holder not found.");
            yield break;
        }

        Debug.Log($"Scaling children of {uiElementsHolder.name}, Count: {uiElementsHolder.childCount}");

        float tweenDelay = 0f; // Start delay for all tweens

        for (int i = 0; i < uiElementsHolder.childCount; i++)
        {
            Transform child = uiElementsHolder.GetChild(i);
            GameObject childGameObject = child.gameObject;
            Button button = childGameObject.GetComponent<Button>();

            // Process only non-button objects
            if (button == null || button.name == "RetryButton")
            {
                Debug.Log($"Scaling non-button {child.name}, Initial Scale: {child.localScale}");
                child.localScale = Vector3.zero;

                LeanTween.scale(childGameObject, Vector3.one, 0.2f)
                    .setEase(LeanTweenType.easeOutBack)
                    .setDelay(tweenDelay)
                    .setOnComplete(() =>
                    {
                        AudioSource audio = childGameObject.GetComponent<AudioSource>();
                        if (audio != null)
                        {
                            audio.Play();
                            Debug.Log($"Playing audio on {childGameObject.name}");
                        }
                    });
                tweenDelay += 0.6f; // Increase delay for the next tween
            }
        }

        // Then, tween the button game objects
        for (int i = 0; i < uiElementsHolder.childCount; i++)
        {
            Transform child = uiElementsHolder.GetChild(i);
            GameObject childGameObject = child.gameObject;
            Button button = childGameObject.GetComponent<Button>();

            // Process only button objects
            if (button != null && button.name != "RetryButton")
            {
                Debug.Log($"Scaling button {child.name}, Initial Scale: {child.localScale}");
                child.localScale = Vector3.zero;
                button.interactable = false; // Make button non-interactable before tweening

                LeanTween.scale(childGameObject, Vector3.one, 0.5f)
                    .setEase(LeanTweenType.easeOutBack)
                    .setDelay(tweenDelay)
                    .setOnComplete(() =>
                    {
                        AudioSource audio = childGameObject.GetComponent<AudioSource>();
                        if (audio != null)
                        {
                            audio.Play();
                            Debug.Log($"Playing audio on {childGameObject.name}");
                        }
                        button.interactable = true; // Re-enable button after tweening
                    });
                tweenDelay += 1.5f; // Increase delay for the next tween
            }
        }

        yield return new WaitForSeconds(tweenDelay);

        // Finally, make all buttons interactable after all tweens are complete
        foreach (Transform child in uiElementsHolder)
        {
            Button button = child.GetComponent<Button>();
            if (button != null && button.name != "RetryButton")
            {
                button.interactable = true;
                Debug.Log($"Setting button on {child.name} as interactable.");
            }
        }

        Debug.Log("All children scaled successfully.");
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
        if (BGAudioManager_Final.Instance != null && BGAudioManager_Final.Instance.IsVolumeEnabled())
        {
            if (audioMixer != null)
            {
                bool result = audioMixer.SetFloat(musicVolumeParam, volume);
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
