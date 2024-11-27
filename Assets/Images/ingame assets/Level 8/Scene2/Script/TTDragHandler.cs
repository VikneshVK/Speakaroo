using UnityEngine;
using System.Collections;

public class TTDragHandler : MonoBehaviour
{
    public Sprite defaultSprite; // Default sprite for reset
    public Sprite resetSprite;
    public Collider2D beakerCollider; // Beaker collider reference
    public string testTubeName; // Unique name for the test tube
    public bool isDraggable = true; // Flag for draggable state
    public bool dragComplete = false;

    public Animator testTubeAnimator;
    public SpriteRenderer spriteRenderer;

    public GameObject pouringPosition1; // Reference to pouring position 1
    public GameObject pouringPosition2; // Reference to pouring position 2

    private Collider2D testTubeCollider;
    private bool isDragging = false;
    private bool isAnimationPlaying = false;
    private Vector3 originalPosition; // Initial position for reset

    void Start()
    {
        originalPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        testTubeCollider = GetComponent<Collider2D>();
        testTubeAnimator = GetComponent<Animator>();

        if (spriteRenderer == null || testTubeCollider == null || testTubeAnimator == null)
        {
            Debug.LogError($"Missing components on {testTubeName}");
        }
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x, mousePosition.y, originalPosition.z);
        }
    }

    private void OnMouseDown()
    {
        if (!isAnimationPlaying && isDraggable)
        {
            isDragging = true;
        }
    }

    private void OnMouseUp()
    {
        if (!isAnimationPlaying)
        {
            isDragging = false;

            if (beakerCollider != null && testTubeCollider.bounds.Intersects(beakerCollider.bounds))
            {
                Transform parentTransform = transform.parent;
                if (parentTransform != null)
                {
                    // Check the parent's name
                    if (parentTransform.name.Contains("Left"))
                    {
                        // Teleport to pouring position 2
                        transform.position = pouringPosition2.transform.position;
                    }
                    else if (parentTransform.name.Contains("right"))
                    {
                        // Teleport to pouring position 1
                        transform.position = pouringPosition1.transform.position;
                    }
                }

                dragComplete = true;
                NotifyQuestManager();
                TriggerFlowAnimation();

                // Disable all test tube colliders
                DisableAllTestTubeColliders();
            }
            else
            {
                ResetPosition();
            }
        }
    }

    private void DisableAllTestTubeColliders()
    {
        TTDragHandler[] allDragHandlers = FindObjectsOfType<TTDragHandler>();
        foreach (TTDragHandler dragHandler in allDragHandlers)
        {
            if (dragHandler != null && dragHandler.testTubeCollider != null)
            {
                dragHandler.testTubeCollider.enabled = false;
            }
        }
    }

    private void TriggerFlowAnimation()
    {
        if (testTubeAnimator != null)
        {
            testTubeAnimator.SetTrigger("flow");
            isAnimationPlaying = true;

            StartCoroutine(ResetAfterAnimation());
        }
    }

    private IEnumerator ResetAfterAnimation()
    {
        yield return new WaitForSeconds(1.5f);

        spriteRenderer.sprite = defaultSprite;
        testTubeAnimator.enabled = false;
        ResetPosition();
        isAnimationPlaying = false;

        // Enable colliders for test tubes that were not dragged and dropped
        EnableRemainingTestTubeColliders();
    }

    private void EnableRemainingTestTubeColliders()
    {
        TTDragHandler[] allDragHandlers = FindObjectsOfType<TTDragHandler>();
        foreach (TTDragHandler dragHandler in allDragHandlers)
        {
            if (dragHandler != null && dragHandler.testTubeCollider != null && !dragHandler.dragComplete)
            {
                dragHandler.testTubeCollider.enabled = true;
            }
        }
    }

    public void ResetPosition()
    {
        transform.position = originalPosition;
    }

    private void NotifyQuestManager()
    {
        Lvl8Sc2QuestManager.Instance.OnTestTubeDropped(gameObject);
        Lvl8Sc2QuestManager.Instance.TestTubeDropped(testTubeName);
        DisableSiblingDragging();
        isDraggable = false;
    }

    private void DisableSiblingDragging()
    {
        Transform parentTransform = transform.parent;

        foreach (Transform child in parentTransform)
        {
            TTDragHandler siblingHandler = child.GetComponent<TTDragHandler>();
            if (siblingHandler != null)
            {
                siblingHandler.isDraggable = false; 
            }
        }
    }

    public void SetOriginalPosition(Vector3 position)
    {
        originalPosition = position;
    }

    public void EnableDragging()
    {
        isDraggable = true;
    }
}
