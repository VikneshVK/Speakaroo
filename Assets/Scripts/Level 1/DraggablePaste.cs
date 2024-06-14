using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class DraggablePaste : MonoBehaviour
{
    public Transform initialPosition;
    public Transform toothbrushContainer;
    public GameObject pasteGelQuad; // A Quad with a material to simulate the gel
    public string targetTag = "Paste"; // Tag to compare
    public float snapDistance = 1f;
    public float zoomInSize = 2f;
    public float zoomDuration = 2f;
    public float cleanDuration = 10f; // Duration of the cleaning process

    private bool isDragging = false;
    private bool isNearToothbrush = false;
    private CameraZoom cameraZoom;
    private int fillLevel = 0;
    private Collider2D pasteCollider;
    private Renderer pasteGelRenderer;
    private Material pasteGelMaterial;
    private bool interactionComplete = false;
    private bool isPasteGelFilled = false;

    void Start()
    {
        // Find references by name
        cameraZoom = Camera.main.GetComponent<CameraZoom>();

        if (initialPosition == null)
        {
            initialPosition = transform;
        }

        pasteCollider = GetComponent<Collider2D>();
        if (pasteCollider == null)
        {
            Debug.LogError("Collider2D component not found on the paste object.");
            return;
        }
        pasteCollider.isTrigger = true; // Ensure the collider is set to trigger

        if (pasteGelQuad != null)
        {
            pasteGelRenderer = pasteGelQuad.GetComponent<Renderer>();
            if (pasteGelRenderer != null)
            {
                pasteGelMaterial = pasteGelRenderer.material;
            }
            else
            {
                Debug.LogError("pasteGelQuad does not have a Renderer component.");
            }

            // Set the initial scale of the paste gel to zero
            SetFillAmount(0f);
            pasteGelQuad.SetActive(false); // Initially hide the paste gel
        }
    }

    void OnMouseDown()
    {
        if (!isDragging && !interactionComplete)
        {
            StartZoomInAndEnableDragging();
        }
    }

    void StartZoomInAndEnableDragging()
    {
        if (cameraZoom != null)
        {
            cameraZoom.ZoomTo(transform, zoomInSize, zoomDuration);
            transform.rotation = Quaternion.Euler(0, 0, 45); // Rotate 45 degrees on the z-axis
            isDragging = true;
        }
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;
            transform.position = mousePosition;

            // Don't update pasteGelQuad position to follow the paste object
            // Keep pasteGelQuad in its initial position

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, snapDistance);
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag(targetTag))
                {
                    Debug.Log("Detected target tag: " + targetTag);
                    isNearToothbrush = true;
                    HandleInteraction();
                    break;
                }
                else
                {
                    isNearToothbrush = false;
                }
            }
        }
    }

    void HandleInteraction()
    {
        if (isNearToothbrush && fillLevel < 4)
        {
            pasteGelQuad.SetActive(true); // Show the paste gel
            fillLevel++;
            UpdatePasteGelFill();
            ResetPastePosition();

            if (fillLevel >= 4)
            {
                interactionComplete = true;
                Invoke("StartZoomOut", 2f); // Delay zoom out by 2 seconds
            }
        }
    }

    void UpdatePasteGelFill()
    {
        // Update the fill amount of the paste gel to simulate filling
        float fillAmount = fillLevel * 0.25f;
        SetFillAmount(fillAmount);
        SetGelScale(fillAmount);
    }

    void SetFillAmount(float amount)
    {
        if (pasteGelMaterial != null)
        {
            pasteGelMaterial.SetFloat("_FillAmount", amount);
        }
    }

    void SetGelScale(float amount)
    {
        Vector3 newScale = pasteGelQuad.transform.localScale;
        newScale.x = amount;
        pasteGelQuad.transform.localScale = newScale;
    }

    void ResetPastePosition()
    {
        transform.position = initialPosition.position;
        transform.rotation = initialPosition.rotation;
        isDragging = false;
    }

    void StartZoomOut()
    {
        cameraZoom.ZoomOut(zoomDuration);
        ResetGel();
    }

    void ResetGel()
    {
        if (fillLevel >= 4 && pasteGelQuad != null)
        {
            pasteGelQuad.transform.SetParent(this.transform);
            isPasteGelFilled = true;
            Debug.Log("Paste gel added to the brush.");
        }

        fillLevel = 0;
        SetFillAmount(1f); // Keep the paste gel fully visible
        SetGelScale(1f); // Keep the paste gel scaled to full
        // pasteGelQuad.SetActive(true); // Keep the paste gel active
    }
}
