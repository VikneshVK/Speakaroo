using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class DraggableBrush : MonoBehaviour
{
    private Transform initialPosition;
    private GameObject teeth; // The teeth game object
    private GameObject particleEffect; // The particle effect game object
    public float snapDistance = 1f;
    public float zoomInSize = 2f;
    public float zoomDuration = 2f;
    public float cleanDuration = 10f; // Duration of the cleaning process

    private bool isDragging = false;
    private bool isNearTeeth = false;
    private CameraZoom cameraZoom;
    private Collider2D brushCollider;
    private SpriteRenderer teethSpriteRenderer;
    private Color originalTeethColor;

    void Start()
    {
        // Find references by name
        teeth = GameObject.Find("teeth");
        if (teeth == null)
        {
            Debug.LogError("Teeth game object not found");
            return;
        }

        particleEffect = GameObject.Find("foam");
        if (particleEffect == null)
        {
            Debug.LogError("Particle effect game object not found");
            return;
        }

        GameObject brushContainer = GameObject.Find("brush Container");
        if (brushContainer == null)
        {
            Debug.LogError("Brush Container game object not found");
            return;
        }
        initialPosition = brushContainer.transform;

        cameraZoom = Camera.main.GetComponent<CameraZoom>();
        if (cameraZoom == null)
        {
            Debug.LogError("CameraZoom component not found on the main camera.");
            return;
        }

        brushCollider = GetComponent<Collider2D>();
        if (brushCollider == null)
        {
            Debug.LogError("Collider2D component not found on the brush.");
            return;
        }
        brushCollider.isTrigger = true; // Ensure the collider is set to trigger

        teethSpriteRenderer = teeth.GetComponent<SpriteRenderer>();
        if (teethSpriteRenderer != null)
        {
            originalTeethColor = teethSpriteRenderer.color;
            Debug.Log("Original teeth color: " + originalTeethColor);
        }
        else
        {
            Debug.LogError("Teeth object does not have a SpriteRenderer component.");
            return;
        }

        particleEffect.SetActive(false); // Initially deactivate the particle effect

        // Set initial position of the brush
        transform.position = initialPosition.position;
        Debug.Log("Brush initialized at position: " + initialPosition.position);
        Debug.Log("Collider2D component initialized successfully.");
    }

    void OnMouseDown()
    {
        if (!isDragging)
        {
            isDragging = true;
            Debug.Log("Brush dragging started.");
        }
    }

    void OnMouseUp()
    {
        if (isDragging)
        {
            isDragging = false;
            Debug.Log("Brush dragging ended.");
            HandleBrushOnTeeth();
        }
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;
            transform.position = mousePosition;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, snapDistance);
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Teeth"))
                {
                    isNearTeeth = true;
                    break;
                }
                else
                {
                    isNearTeeth = false;
                }
            }
        }
    }

    void HandleBrushOnTeeth()
    {
        // Reassign brushCollider to ensure it is properly referenced
        if (brushCollider == null)
        {
            Debug.LogWarning("Reassigning brushCollider in HandleBrushOnTeeth.");
            brushCollider = GetComponent<Collider2D>();
        }

        if (isNearTeeth)
        {
            if (brushCollider == null)
            {
                Debug.LogError("Brush collider is null in HandleBrushOnTeeth after reassignment.");
                return;
            }
            brushCollider.enabled = false; // Deactivate collider to make brush non-interactable
            Debug.Log("Brush collider deactivated.");
            StartCoroutine(CleanTeethRoutine());
        }
        else
        {
            ReturnBrushToInitialPosition();
        }
    }

    IEnumerator CleanTeethRoutine()
    {
        // Zoom in to the teeth
        if (cameraZoom != null)
        {
            cameraZoom.ZoomTo(teeth.transform, zoomInSize, zoomDuration);
            yield return new WaitForSeconds(zoomDuration);
        }

        // Reactivate collider to trigger particle effect
        if (brushCollider != null)
        {
            brushCollider.enabled = true;
            Debug.Log("Brush collider reactivated.");
        }

        // Activate particle effect
        if (particleEffect != null)
        {
            particleEffect.SetActive(true);
            Debug.Log("Particle effect activated.");
        }

        // Wait for the cleaning duration
        yield return new WaitForSeconds(cleanDuration);

        // Deactivate particle effect
        if (particleEffect != null)
        {
            particleEffect.SetActive(false);
            Debug.Log("Particle effect deactivated.");
        }

        // Change the color of the teeth to white
        if (teethSpriteRenderer != null)
        {
            teethSpriteRenderer.color = Color.white;
            Debug.Log("Teeth color changed to white.");
        }

        // Return brush to its initial position
        ReturnBrushToInitialPosition();

        // Zoom out to the original view
        if (cameraZoom != null)
        {
            cameraZoom.ZoomOut(zoomDuration);
            Debug.Log("Zooming out.");
        }
    }

    void ReturnBrushToInitialPosition()
    {
        if (initialPosition != null)
        {
            transform.position = initialPosition.position;
            transform.rotation = initialPosition.rotation;
            Debug.Log("Brush returned to initial position: " + initialPosition.position);
        }
        isDragging = false;
        if (brushCollider != null)
        {
            brushCollider.enabled = true; // Ensure the collider is enabled again
            Debug.Log("Brush collider enabled.");
        }
    }
}
