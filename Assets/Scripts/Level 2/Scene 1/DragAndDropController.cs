using UnityEngine;

public class DragAndDropController : MonoBehaviour
{
    public GameObject correctDropZoneObject; // Reference to the drop zone game object
    public GameObject speechBubblePrefab; // Reference to the speech bubble prefab
    public GameObject mechanicsPrefab; // Prefab specific to this game object (bus, whale, or building)
    public GameObject Bus;
    public GameObject miniBus; // Miniature bus with collider
    public GameObject Whale;
    public GameObject miniWhale; // Miniature whale with collider
    public GameObject Building;
    public GameObject miniBuilding; // Miniature building with collider
    public GameObject Boy;
    public Transform speechBubbleContainer;
    

    public float dropOffset;
    public float blinkCount;
    public float blinkDuration;

    private Animator boyAnimator;
    private Animator animator; // Animator attached to this game object
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

        // Initially disable colliders
        miniBus.GetComponent<Collider2D>().enabled = false;
        miniWhale.GetComponent<Collider2D>().enabled = false;
        miniBuilding.GetComponent<Collider2D>().enabled = false;
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
            

            // Enable the collider of the corresponding miniature object
            EnableMiniatureCollider();
        }
        else
        {
            boyAnimator.SetBool("isWrongDrop", true);
            StartCoroutine(BlinkRedAndReset());
        }
    }

    public void SpawnSpeechBubble()
    {
        GameObject speechBubble = Instantiate(speechBubblePrefab, speechBubbleContainer.position, Quaternion.identity);
        SpeechBubble bubbleController = speechBubble.GetComponent<SpeechBubble>();
        bubbleController.Setup(mechanicsPrefab, this);
        disableColliders();
    }

    void disableColliders()
    {
        Building.GetComponent<Collider2D>().enabled = false;
        Whale.GetComponent<Collider2D>().enabled = false;
        Bus.GetComponent<Collider2D>().enabled = false;
    }
    

    void EnableMiniatureCollider()
    {
        switch (gameObject.name)
        {
            case "bus":
                miniBus.GetComponent<Collider2D>().enabled = true;
                break;
            case "whale":
                miniWhale.GetComponent<Collider2D>().enabled = true;
                break;
            case "building blocks":
                miniBuilding.GetComponent<Collider2D>().enabled = true;
                break;
        }
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
