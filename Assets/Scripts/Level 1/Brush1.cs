using UnityEngine;
using System.Collections;

public class Brush1 : MonoBehaviour
{
    private Camera mainCamera;
    private GameObject teeth;
    private GameObject foam;
    private ParticleSystem foamParticles;
    private GameObject Boy;
    private Sprite brushBack;
    public float zoomSize = 5f;
    public float zoomDuration = 2f;
    public float hoverDuration = 5f;

    private Vector3 originalCameraPosition;
    private bool isDraggable = false;
    private float hoverTimer = 0f;
    private bool isHovering = false;

    private SpriteRenderer brushSpriteRenderer;
    private Sprite originalSprite;
    private Sprite brushBackSprite;

    private float originalOrthographicSize;

    void Start()
    {
        mainCamera = GameObject.FindWithTag("MainCamera")?.GetComponent<Camera>();
        originalOrthographicSize = mainCamera.orthographicSize;
        teeth = GameObject.FindWithTag("Teeth");
        foam = GameObject.FindWithTag("Foam");
        Boy = GameObject.FindWithTag("Player");
        foamParticles = foam?.GetComponentInChildren<ParticleSystem>();
        brushSpriteRenderer = GetComponent<SpriteRenderer>();
        originalSprite = brushSpriteRenderer.sprite;
        brushBackSprite = Resources.Load<Sprite>("Images/brush back");

        GameObject brushingPlace = GameObject.FindWithTag("Brushing Place");

        if (mainCamera == null || teeth == null || foam == null || foamParticles == null || brushSpriteRenderer == null || brushBackSprite == null || brushingPlace == null)
        {
            Debug.LogError("Initialization Error: One or more essential components are missing.");
            return;
        }

        originalCameraPosition = mainCamera.transform.position;
        StartCoroutine(ZoomAndMove(brushingPlace.transform.position));
    }

    IEnumerator ZoomAndMove(Vector3 targetPosition)
    {
        // This line smoothly changes the orthographic size to zoom in, creating a zoom effect.
        LeanTween.value(gameObject, mainCamera.orthographicSize, zoomSize, zoomDuration)
            .setOnUpdate((float val) => mainCamera.orthographicSize = val)
            .setEase(LeanTweenType.easeInOutQuad); // Optional: Makes the zoom smoother

        // Update to smoothly move the camera along the Y-axis from its current position to a new position.
        Vector3 newCameraPosition = new Vector3(0, -2, -10);
        LeanTween.move(mainCamera.gameObject, newCameraPosition, zoomDuration)
            .setEase(LeanTweenType.easeInOutQuad); // Optional: Makes the movement smoother

        // Move the brush to the target position
        LeanTween.move(gameObject, targetPosition, zoomDuration)
            .setEase(LeanTweenType.easeInOutQuad) // Optional: Makes the movement smoother
            .setOnComplete(() => isDraggable = true);

        yield return new WaitForSeconds(zoomDuration);
    }


    void Update()
    {
        if (isDraggable)
        {
            DragBrush();
        }
    }

    private void DragBrush()
    {
        if (Input.GetMouseButton(0) && GetComponent<Collider2D>().enabled)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; // Ensure the z position is not altered.
            transform.position = mousePos;

            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
            bool currentlyHovering = false;
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject == teeth)
                {
                    currentlyHovering = true;
                    if (!isHovering)
                    {
                        isHovering = true;
                        foamParticles.Play();
                        brushSpriteRenderer.sprite = brushBackSprite;
                        transform.rotation = Quaternion.Euler(0, 0, 90); // Rotate -90 degrees on z-axis
                        StartCoroutine(HandleHover());
                    }
                }
            }
            if (!currentlyHovering && isHovering)
            {
                isHovering = false;
                foamParticles.Stop();
                brushSpriteRenderer.sprite = originalSprite;
                transform.rotation = Quaternion.identity; // Reset rotation
                StopCoroutine(HandleHover());
            }
        }
    }



    IEnumerator HandleHover()
    {
        while (isHovering)
        {
            hoverTimer += Time.deltaTime;
            if (hoverTimer >= hoverDuration)
            {
                // Disable the collider to stop dragging
                Collider2D collider = GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
                else
                {
                    Debug.LogError("Collider component missing on brush object.");
                }

                ZoomOutAndDestroy();
                

                break;
            }
            yield return null;
        }
    }


    private void ZoomOutAndDestroy()
    {
        Animator boyAnimator = Boy.GetComponent<Animator>();
        if (boyAnimator != null)
        {
            boyAnimator.SetBool("isBrushed", true);
        }
        else
        {
            Debug.LogError("Animator component missing on Boy object.");
        }

        Destroy(teeth);
        Destroy(foamParticles);
        gameObject.SetActive(false);    

        // Tween to change the orthographic size of the camera back to its original size
        LeanTween.value(gameObject, mainCamera.orthographicSize, originalOrthographicSize, zoomDuration)
            .setOnUpdate((float val) => mainCamera.orthographicSize = val);

        // Tween to move the camera back to its original position
        Vector3 originalCameraPosition = new Vector3(0, 0, -10);
        LeanTween.move(mainCamera.gameObject, originalCameraPosition, zoomDuration).setOnComplete(() => {
            Destroy(gameObject); // Optionally, destroy the brush object here
        });

    }




}
