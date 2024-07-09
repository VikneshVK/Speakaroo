using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Brush_Controller : MonoBehaviour
{
    public GameObject teeth;
    public GameObject foam;
    public Transform initialPosition;
    public SpriteRenderer spriteRenderer;
    public Animator boyAnimator;
    public CameraController cameraController;
    public float hoverDuration = 10f;
    public LayerMask interactionLayers;

    private Camera mainCamera;
    private Animator animator;
    private bool isDragging = false;
    private bool isHovering = false;
    private float hoverTime = 0f;
    private Sprite spriteBrush1;
    private Sprite spriteBrushBack;

    void Start()
    {
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Load sprites from the Resources folder within the Images subdirectory
        spriteBrush1 = Resources.Load<Sprite>("Images/brush 1");
        spriteBrushBack = Resources.Load<Sprite>("Images/brush back");

        // Check if the sprites are loaded correctly
        if (spriteBrush1 != null)
        {
            Debug.Log("Sprite 'brush 1' loaded successfully.");
        }
        else
        {
            Debug.LogError("Failed to load sprite 'brush 1'. Check if the file exists in the Resources/Images folder with the correct name.");
        }

        if (spriteBrushBack != null)
        {
            Debug.Log("Sprite 'brush back' loaded successfully.");
        }
        else
        {
            Debug.LogError("Failed to load sprite 'brush back'. Check if the file exists in the Resources/Images folder with the correct name.");
        }
    }



    void Update()
    {
        if (isDragging)
        {
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mousePosition;

            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero, 0f, interactionLayers);
            CheckRaycastHits(hits);
        }

        if (isHovering)
        {
            hoverTime += Time.deltaTime;
            if (hoverTime >= hoverDuration)
            {
                CompleteBrushing();
            }
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
    }

    void OnMouseUp()
    {
        isDragging = false;
        isHovering = false;
        foam.SetActive(false);
        StartCoroutine(cameraController.ZoomOut());
    }

    void CheckRaycastHits(RaycastHit2D[] hits)
    {
        foreach (var hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("DropPoint"))
            {
                transform.position = hit.collider.transform.position;
                GetComponent<Collider2D>().enabled = false;
                if (!animator.GetBool("paste_On"))
                {
                    animator.SetBool("paste_On", true);
                    teeth.SetActive(true);
                    // Change sprite after dropping on DropPoint and animation played
                    StartCoroutine(ChangeSpriteAfterAnimation(spriteBrush1));
                }
            }
            else if (hit.collider.gameObject.CompareTag("Teeth"))
            {
                if (!isHovering)
                {
                    isHovering = true;
                    StartCoroutine(cameraController.ZoomInOn(teeth.transform));
                    // Change sprite before activating foam
                    spriteRenderer.sprite = spriteBrushBack;

                    foam.SetActive(true);
                    Debug.Log("Sprite changed to brush back on teeth hover.");
                }
            }
        }

        // If dragging ended and no relevant object was found, handle the hover exit logic
        if (!isDragging && !isHovering)
        {
            foam.SetActive(false);
            StartCoroutine(cameraController.ZoomOut());
            GetComponent<Collider2D>().enabled = true; // Re-enable the collider once the dragging stops and no interaction is detected
        }
    }

    IEnumerator ChangeSpriteAfterAnimation(Sprite newSprite)
    {
        yield return null; // Ensure animation starts
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("PasteOnAnimation") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }
        spriteRenderer.sprite = newSprite;
        Debug.Log("Sprite changed to: " + newSprite.name);
        animator.SetBool("paste_On", false);
        GetComponent<Collider2D>().enabled = true;// Reset the boolean
    }

    private void CompleteBrushing()
    {
        StartCoroutine(cameraController.ZoomOut());
        teeth.SetActive(false);
        transform.position = initialPosition.position;
        boyAnimator.SetBool("isBrushed", true);
        GetComponent<Collider2D>().enabled = false;
        CheckBrushingCompletion();
    }
    public void OnAnimationComplete()
    {
        spriteRenderer.sprite = spriteBrush1; // Change to the desired sprite
        Debug.Log("Animation completed and sprite changed.");
    }
    private void CheckBrushingCompletion()
    {
        SceneManager.LoadScene("Level2");
    }
}
