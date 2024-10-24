
using UnityEngine;
using UnityEngine.UI;

public class Lvl7Sc1JojoController : MonoBehaviour
{
    public Transform[] stopPositions; // Array of stop positions
    public Transform[] speechBubbleSpawnPositions; // Array of speech bubble spawn positions for each stop
    public GameObject speechBubblePrefab;
    public Animator jojoAnimator;
    public Camera mainCamera; // Reference to the main camera
    public FoodContainerController foodController; // Reference to FoodContainerController script
    public GameObject panelToScale; // Reference to ST Canvas prefab for PrefabTouchHandler
    public float moveSpeed = 2f;
    public float cameraFollowSpeed = 2f; // Speed at which the camera follows Jojo

    public static int currentStopIndex = 0; // Current stop position index
    private bool isWalking = false;
    private bool isTalking = false;
    private bool cameraFollowing = false; // Whether the camera should follow Jojo
    private GameObject spawnedPrefab;
    public Sprite sprite1; // Reference to Sprite 1 (assigned in inspector)
    public Sprite sprite2; // Reference to Sprite 2 (assigned in inspector)
    public Sprite sprite3; // Reference to Sprite 3 (assigned in inspector)
    public Image panelImage; // Reference to the Image component on the panel

    public float cameraXOffset = 0f; // Constant offset between Jojo and the camera on the x-axis

    public Transform cameraFollowPoint; // New reference to child object of Jojo for camera to follow

    void Start()
    {
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

        if (isTalking && IsAnimationStateComplete("Talk"))
        {
            OnTalkAnimationComplete();
        }

        AnimatorStateInfo stateInfo = jojoAnimator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Talk 0") && stateInfo.normalizedTime >= 1.0f) // Animation is complete
        {
            // Set canTalk to false and move Jojo to the next stop
            jojoAnimator.SetBool("canTalk", false);
            MoveToNextStopPosition();
        }
    }

    private void MoveCharacter()
    {
        // Move Jojo towards the current stop position
        Transform targetStop = stopPositions[currentStopIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetStop.position, moveSpeed * Time.deltaTime);

        // Only start following the camera if current stop is not 1
        if (currentStopIndex > 0)
        {
            cameraFollowing = true;
        }

        // Check if Jojo has reached the stop position
        if (Vector3.Distance(transform.position, targetStop.position) < 0.1f)
        {
            isWalking = false;
            jojoAnimator.SetBool("canWalk", false);

            // Stop the camera from following when reaching the stop position
            cameraFollowing = false;
            jojoAnimator.SetBool("canTalk", true);
            isTalking = true;
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
            jojoAnimator.SetBool("canWalk", true); // Start walking animation
        }
    }

    private void OnTalkAnimationComplete()
    {
        isTalking = false;
        jojoAnimator.SetBool("canTalk", false);

        if (currentStopIndex < 3)
        {
            SpawnSpeechBubble(currentStopIndex);
        }

        // Advance to the next stop position if there are more stops
        if (currentStopIndex < stopPositions.Length - 1)
        {
            currentStopIndex++;
        }
        else if (currentStopIndex == stopPositions.Length - 1)
        {
            MoveJojoRightAndEndScene();
        }
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
            ChangePanelSprite();
            panelToScale.SetActive(true);
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
        // Make Jojo move to the right without camera following
        isWalking = true;
        cameraFollowing = false; // Stop camera movement
    }

    // Check if a specific animation state is complete
    private bool IsAnimationStateComplete(string stateName)
    {
        return jojoAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
               jojoAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
    }
}
