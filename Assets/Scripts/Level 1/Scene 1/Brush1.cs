using UnityEngine;
using System.Collections;

public class Brush1 : MonoBehaviour
{
    private Camera mainCamera;
    private GameObject teeth;
    private GameObject foam;
    private ParticleSystem foamParticles;
    private GameObject Boy;
    private Animator boyAnimator;
    public float zoomSize = 5f;
    public float zoomDuration = 2f;
    public float hoverDuration ;
    public AudioClip sfxaudio1;

    private Vector3 originalCameraPosition;
    private bool isDraggable = false;
    private float hoverTimer = 0f;
    private bool isHovering = false;

    private SpriteRenderer brushSpriteRenderer;
    private Sprite originalSprite;
    private Sprite brushBackSprite;

    private float originalOrthographicSize;
    private AudioSource SfxAudioSource;

    private Vector3 dragOffset;

    void Start()
    {
        mainCamera = GameObject.FindWithTag("MainCamera")?.GetComponent<Camera>();
        originalOrthographicSize = mainCamera.orthographicSize;
        teeth = GameObject.FindWithTag("Teeth");
        foam = GameObject.FindWithTag("Foam");
        Boy = GameObject.FindWithTag("Player");
        boyAnimator = Boy.GetComponent<Animator>();
        foamParticles = foam?.GetComponentInChildren<ParticleSystem>();
        brushSpriteRenderer = GetComponent<SpriteRenderer>();
        originalSprite = brushSpriteRenderer.sprite;
        brushBackSprite = Resources.Load<Sprite>("Images/brush-back");
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();

        GameObject brushingPlace = GameObject.FindWithTag("Brushing Place");

        if (mainCamera == null || teeth == null || foam == null || foamParticles == null || brushSpriteRenderer == null || brushBackSprite == null || brushingPlace == null)
        {
            Debug.LogError("Initialization Error: One or more essential components are missing.");
            return;
        }

        originalCameraPosition = mainCamera.transform.position;
       

        // Calculate the target position for the brush
        Vector3 brushTargetPosition = brushingPlace.transform.position;

        StartCoroutine(ZoomAndMove(brushTargetPosition));
    }


    IEnumerator ZoomAndMove(Vector3 brushTargetPosition)
    {
        LeanTween.value(gameObject, mainCamera.orthographicSize, zoomSize, zoomDuration)
            .setOnUpdate((float val) => mainCamera.orthographicSize = val)
            .setEase(LeanTweenType.easeInOutQuad);

        LeanTween.move(mainCamera.gameObject, new Vector3(teeth.transform.position.x, teeth.transform.position.y, mainCamera.transform.position.z), zoomDuration)
            .setEase(LeanTweenType.easeInOutQuad);

        LeanTween.move(gameObject, brushTargetPosition, zoomDuration)
            .setEase(LeanTweenType.easeInOutQuad)
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
        if (Input.GetMouseButtonDown(0) && GetComponent<Collider2D>().enabled)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; 
            dragOffset = transform.position - mousePos;
        }

        if (Input.GetMouseButton(0) && GetComponent<Collider2D>().enabled)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; 
            Vector3 targetPosition = mousePos + dragOffset;

            float cameraHeight = mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * mainCamera.aspect;

            float horizontalLimit = cameraWidth * 0.5f;  // Full width of the viewport
            float verticalLimit = 0f;  // Center of the viewport is the upper limit
            float lowerLimit = -cameraHeight * 0.5f; // Bottom of the viewport

            float leftLimit = -horizontalLimit * 0.5f;
            float rightLimit = horizontalLimit * 0.5f;

            targetPosition.x = Mathf.Clamp(targetPosition.x, leftLimit, rightLimit);
            targetPosition.y = Mathf.Clamp(targetPosition.y, lowerLimit, verticalLimit);

            transform.position = targetPosition;

            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.zero);
            bool currentlyHovering = false;
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject == teeth)
                {
                    currentlyHovering = true;
                    boyAnimator.SetTrigger("shakeHead");
                    if (!isHovering)
                    {
                        isHovering = true;
                        foamParticles.Play();
                        if(SfxAudioSource != null)
                        {
                            SfxAudioSource.clip = sfxaudio1;
                            SfxAudioSource.loop = true;
                            SfxAudioSource.Play();
                        }
                        brushSpriteRenderer.sprite = brushBackSprite;
                        StartCoroutine(HandleHover());
                    }
                }
            }
            if (!currentlyHovering && isHovering)
            {
                isHovering = false;
                foamParticles.Stop();
                if (SfxAudioSource != null)
                {                    
                    SfxAudioSource.Pause();
                }
                brushSpriteRenderer.sprite = originalSprite;
                transform.rotation = Quaternion.identity; // Reset rotation
                StopCoroutine(HandleHover());
            }

            // Ensure the foam particles continue to play while hovering
            if (currentlyHovering)
            {
                if (!foamParticles.isPlaying)
                {
                    foamParticles.Play();
                    if (SfxAudioSource != null)
                    {
                        SfxAudioSource.clip = sfxaudio1;
                        SfxAudioSource.loop = true;
                        SfxAudioSource.Play();
                    }
                }
            }
            else
            {
                if (foamParticles.isPlaying)
                {
                    foamParticles.Stop();
                    if (SfxAudioSource != null)
                    {
                        SfxAudioSource.Pause();
                    }
                }
            }
        }
        else
        {
            if (foamParticles.isPlaying)
            {
                foamParticles.Stop();
                if (SfxAudioSource != null)
                {
                    SfxAudioSource.Pause();
                }
            }
        }
    }



    IEnumerator HandleHover()
    {
        while (isHovering && Input.GetMouseButton(0))
        {
            hoverTimer += Time.deltaTime;
            Debug.Log("Hover Timer: " + hoverTimer); // Debug log to show hover time
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
        SfxAudioSource.loop = false;

        // Tween to change the orthographic size of the camera back to its original size
        LeanTween.value(gameObject, mainCamera.orthographicSize, originalOrthographicSize, zoomDuration)
            .setOnUpdate((float val) => mainCamera.orthographicSize = val);

        // Tween to move the camera back to its original position
        LeanTween.move(mainCamera.gameObject, originalCameraPosition, zoomDuration).setOnComplete(() =>
        {
            // Add a 2-second delay before destroying the game object
            LeanTween.delayedCall(2f, () =>
            {
                Destroy(gameObject); // Optionally, destroy the brush object here
            });
        });
    }

}
