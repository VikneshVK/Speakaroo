using UnityEngine;

public class HP_HelperpointerController : MonoBehaviour
{
    public GameObject leaves1;
    public GameObject leaves2;
    public GameObject helperPointerPrefab;
    public Collider2D trashCanCollider; // Reference to the trash can's collider
    public float inactivityTime = 10f; // Time before the pointer spawns, adjustable in Inspector
    public float tweenDuration = 2f; // Duration of the tween to trash can

    private bool isTimerActive = false;
    private bool leaves1Interacted = false;
    private bool leaves2Interacted = false;
    private float leaves1Timer = 0f;
    private float leaves2Timer = 0f;
    private GameObject helperPointerInstance;

    private LeafDragAndDrop leaves1DragScript;
    private LeafDragAndDrop leaves2DragScript;

    private Collider2D leaves1Collider;
    private Collider2D leaves2Collider;

    private SpriteRenderer leaves1SpriteRenderer;
    private SpriteRenderer leaves2SpriteRenderer;

    private AudioSource audioSource; // Reference to the AudioSource component

    void Start()
    {
        // Get references to the LeafDragAndDrop components
        leaves1DragScript = leaves1.GetComponent<LeafDragAndDrop>();
        leaves2DragScript = leaves2.GetComponent<LeafDragAndDrop>();

        // Get references to the colliders of leaves1 and leaves2
        leaves1Collider = leaves1.GetComponent<Collider2D>();
        leaves2Collider = leaves2.GetComponent<Collider2D>();

        // Get references to the SpriteRenderers of leaves1 and leaves2
        leaves1SpriteRenderer = leaves1.GetComponent<SpriteRenderer>();
        leaves2SpriteRenderer = leaves2.GetComponent<SpriteRenderer>();

        // Get the reference to the AudioSource attached to the same GameObject
        audioSource = GetComponent<AudioSource>();

        // Start the inactivity timer
        StartInactivityTimer();
    }

    void Update()
    {
        if (isTimerActive)
        {
            // Check if leaves1's collider is enabled and its SpriteRenderer is enabled before starting the timer
            if (!leaves1Interacted && leaves1Collider.enabled && leaves1SpriteRenderer.enabled)
            {
                leaves1Timer += Time.deltaTime;

                if (leaves1Timer >= inactivityTime && helperPointerInstance == null)
                {
                    SpawnHelperPointer(leaves1.transform.position);
                }
            }

            // Check if leaves2's collider is enabled and its SpriteRenderer is enabled before starting the timer for leaves2
            if (!leaves2Interacted && leaves2Collider.enabled && leaves2SpriteRenderer.enabled && helperPointerInstance == null)
            {
                // Start the timer for leaves2 only after leaves1 has been interacted with
                leaves2Timer += Time.deltaTime;

                if (leaves2Timer >= inactivityTime && helperPointerInstance == null)
                {
                    SpawnHelperPointer(leaves2.transform.position);
                }
            }

            // Check if the leaves are being dragged; if yes, destroy the helper pointer
            if ((leaves1DragScript.dragging || leaves2DragScript.dragging) && helperPointerInstance != null)
            {
                Destroy(helperPointerInstance);
                ResetTimers();
            }
        }
    }

    public void StartInactivityTimer()
    {
        isTimerActive = true;
        leaves1Timer = 0f; // Reset the timer for leaves1
        leaves2Timer = 0f; // Reset the timer for leaves2
    }

    private void SpawnHelperPointer(Vector3 startPosition)
    {
        // Instantiate the helper pointer prefab at the leaves' position
        helperPointerInstance = Instantiate(helperPointerPrefab, startPosition, Quaternion.identity);

        // Play the audio whenever the helper pointer spawns
        if (audioSource != null)
        {
            audioSource.Play();
        }

        // Get the center of the trash can's collider bounds
        Vector3 trashCanCenter = trashCanCollider.bounds.center;

        // Tween the pointer to the trash can's collider bounds center in a loop
        LeanTween.move(helperPointerInstance, trashCanCenter, tweenDuration).setLoopClamp();
    }

    public void StopInactivityTimer()
    {
        isTimerActive = false;
        ResetTimers();
    }

    private void ResetTimers()
    {
        leaves1Timer = 0f;
        leaves2Timer = 0f;
        leaves1Interacted = false;
        leaves2Interacted = false;
    }

    // Call these methods when leaves1 or leaves2 are interacted with
    public void OnLeaf1Interacted()
    {
        leaves1Interacted = true;
        ResetHelperPointerIfDragging();
    }

    public void OnLeaf2Interacted()
    {
        leaves2Interacted = true;
        ResetHelperPointerIfDragging();
    }

    private void ResetHelperPointerIfDragging()
    {
        if (helperPointerInstance != null)
        {
            Destroy(helperPointerInstance);
        }
    }
}
