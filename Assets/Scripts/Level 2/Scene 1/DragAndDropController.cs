using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DragAndDropController : MonoBehaviour
{
    public GameObject correctDropZoneObject;
    public GameObject speechBubblePrefab;
    public GameObject mechanicsPrefab;
    public GameObject miniBus;
    public GameObject miniWhale;
    public GameObject miniBuilding;
    public GameObject Boy;
    public Transform speechBubbleContainer;

    public float dropOffset;
    public float blinkCount;
    public float blinkDuration;

    private Animator boyAnimator;
    private bool isDragging = false;
    private Vector3 offset;
    private Dictionary<GameObject, bool> interactionStatus = new Dictionary<GameObject, bool>();    
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private SpriteRenderer objectRenderer;

    void Start()
    {
        boyAnimator = Boy.GetComponent<Animator>();
        interactionStatus.Add(miniBus, false);
        interactionStatus.Add(miniWhale, false);
        interactionStatus.Add(miniBuilding, false);
        originalPosition = transform.position;

        // Disable colliders for the start, only enable for the first interactable
        miniWhale.GetComponent<Collider2D>().enabled = false;
        miniBuilding.GetComponent<Collider2D>().enabled = false;
        objectRenderer = GetComponent<SpriteRenderer>();
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
                HelpPointerManager.IsAnyObjectBeingInteracted = true;
                HelpPointerManager.Instance.StopHelpPointer(); // Stop the help pointer
                offset = transform.position - mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            HelpPointerManager.IsAnyObjectBeingInteracted = false;
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
        transform.position = mousePosition + offset;
    }

    void CheckDrop()
    {
        if (IsCorrectDropZone())
        {
            boyAnimator.SetBool("isRightDrop", true);
            transform.position = correctDropZoneObject.transform.position;
            transform.rotation = Quaternion.identity;
            InstantiateSpeechBubble(); // Spawn speech bubble after successful drop

            EnableMiniatureCollider();
            MarkAsInteracted(gameObject);
        }
        else
        {
            ResetObjectPosition();
        }
    }

    void InstantiateSpeechBubble()
    {
        GameObject speechBubble = Instantiate(speechBubblePrefab, speechBubbleContainer.position, Quaternion.identity);
        SpeechBubble bubbleController = speechBubble.GetComponent<SpeechBubble>();
        bubbleController.Setup(mechanicsPrefab, this);
    }

    void EnableMiniatureCollider()
    {
        switch (gameObject.name)
        {
            case "Bus":
                miniBus.GetComponent<Collider2D>().enabled = true;
                break;
            case "Whale":
                miniWhale.GetComponent<Collider2D>().enabled = true;
                break;
            case "Building":
                miniBuilding.GetComponent<Collider2D>().enabled = true;
                break;
        }
    }

    public void MarkAsInteracted(GameObject obj)
    {
        if (interactionStatus.ContainsKey(obj))
        {
            interactionStatus[obj] = true;
        }
    }

    bool IsCorrectDropZone()
    {
        return Vector3.Distance(transform.position, correctDropZoneObject.transform.position) <= dropOffset;
    }

    public Vector3 GetDropLocation()
    {
        return correctDropZoneObject.transform.position;
    }

    void ResetObjectPosition()
    {
        // Reset position if the drop was incorrect
        transform.position = originalPosition;
        boyAnimator.SetBool("isWrongDrop", true);
        StartCoroutine(BlinkRedAndReset());
    }

    IEnumerator BlinkRedAndReset()
    {
        Color originalColor = objectRenderer.material.color;
        for (int i = 0; i < blinkCount; i++)
        {
            objectRenderer.material.color = Color.red;
            yield return new WaitForSeconds(blinkDuration);
            objectRenderer.material.color = originalColor;
            yield return new WaitForSeconds(blinkDuration);
        }
        boyAnimator.SetBool("isWrongDrop", false);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
}
