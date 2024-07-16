using UnityEngine;

public class DragAndDropController : MonoBehaviour
{
    public GameObject correctDropZoneObject; // Reference to the drop zone game object
    public GameObject speechBubblePrefab; // Reference to the speech bubble prefab
    public GameObject mechanicsPrefab; // Prefab specific to this game object (bus, whale, or building)
    public Transform speechBubbleContainer;
    public GameObject Boy;
    public Animator boyAnimator;
    public Animator animator; // Animator attached to this game object

    public float blinkDuration = 0.1f; // Duration for each blink
    public int blinkCount = 2; // Number of times the object should blink on incorrect drop
    public float dropOffset = 0.5f; // Allowable distance to count as a correct drop

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isDragging = false;
    private Renderer objectRenderer;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        objectRenderer = GetComponent<Renderer>();
        animator = GetComponent<Animator>();
        boyAnimator = Boy.GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            CheckDrop();
        }

        if (isDragging)
        {
            DragObject();
        }
    }

    void DragObject()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePosition.x, mousePosition.y, originalPosition.z);
    }

    void CheckDrop()
    {
        if (IsCorrectDropZone())
        {
            boyAnimator.SetBool("isRightDrop", true);
            transform.position = correctDropZoneObject.transform.position;
            transform.rotation = correctDropZoneObject.transform.rotation;
            animator.SetTrigger("isRightDrop");
            // Animation Event will call SpawnSpeechBubble at the end of the animation
        }
        else
        {
            boyAnimator.SetBool("isWrongDrop", true);
            StartCoroutine(BlinkRedAndReset());
        }
    }

    // Method to be called by an Animation Event
    public void SpawnSpeechBubble()
    {
        GameObject speechBubble = Instantiate(speechBubblePrefab, speechBubbleContainer.position, Quaternion.identity);
        SpeechBubble bubbleController = speechBubble.GetComponent<SpeechBubble>();
        bubbleController.Setup(mechanicsPrefab, this);
       /* bubbleController.SpawnMechanicsPrefab();*/
    }

    bool IsCorrectDropZone()
    {
        return Vector3.Distance(transform.position, correctDropZoneObject.transform.position) <= dropOffset;
    }

    System.Collections.IEnumerator BlinkRedAndReset()
    {
        Color originalColor = objectRenderer.material.color;
        for (int i = 0; i < blinkCount; i++)
        {
            objectRenderer.material.color = Color.red;
            yield return new WaitForSeconds(blinkDuration);
            objectRenderer.material.color = originalColor;
            yield return new WaitForSeconds(blinkDuration);
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
}
