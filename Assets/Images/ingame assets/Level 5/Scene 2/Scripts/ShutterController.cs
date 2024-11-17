using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShutterController : MonoBehaviour
{
    public AudioClip shutterSound;
    private AudioSource audioSource;

    public Image flashImage;
    public float flashDuration = 0.2f;
    public PhotoQuestManager photoQuestManager;
    public PhotoCameraController photoCameraController;
    public GameObject questImage;
    public Transform centerReference;
    public GameObject cameraUI;
    public LVL5Sc2HelperController helperController;

    private Transform photoChild;
    private Transform animalChild;
    private Collider2D[] colliders;
    private Collider2D[] allAnimalColliders;
    private Vector3 originalScale;
    

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        foreach (Transform child in transform)
        {
            if (child.gameObject.name.EndsWith("- Photo"))
            {
                photoChild = child;
                photoChild.gameObject.SetActive(false);
                originalScale = photoChild.localScale;
            }
            else if (child.gameObject.name.EndsWith("- Animal"))
            {
                animalChild = child;
            }
        }

        colliders = GetComponents<Collider2D>();
        allAnimalColliders = FindObjectsOfType<Collider2D>();
    }

    void Update()
    {
        HandleTouchInput();
    }

    void HandleTouchInput()
    {
        // For mobile touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                touchPosition.z = 0f;

                RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);

                if (hit.collider != null && hit.collider.gameObject == gameObject && !photoQuestManager.isClicked)
                {
                    photoQuestManager.isClicked = true;
                    StartCoroutine(TakePhoto());
                    helperController.DestroyHelperHand();
                    helperController.ResetDelayTimer();
                }
            }
        }

        // For mouse input (useful for testing in the editor)
        else if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;

            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject && !photoQuestManager.isClicked)
            {
                photoQuestManager.isClicked = true;
                StartCoroutine(TakePhoto());
                helperController.DestroyHelperHand();
                helperController.ResetDelayTimer();
            }
        }
    }

    IEnumerator TakePhoto()
    {

        photoCameraController.canMove = false;
        helperController.DestroyDirPrefab();

        // Calculate the target position for the camera based on the tapped animal
        Vector3 targetPosition = new Vector3(transform.position.x, photoCameraController.transform.position.y, photoCameraController.transform.position.z);

        float panDuration = 1f;
        float elapsedTime = 0f;
        Vector3 initialCameraPosition = photoCameraController.transform.position;

        while (elapsedTime < panDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / panDuration);

            photoCameraController.transform.position = new Vector3(
                Mathf.Lerp(initialCameraPosition.x, targetPosition.x, t),
                initialCameraPosition.y,
                initialCameraPosition.z
            );

            yield return null;
        }

        // Enable camera UI after panning
        if (cameraUI != null)
        {
            cameraUI.SetActive(true);
        }

        yield return new WaitForSeconds(1f);

        photoCameraController.SetPanningEnabled(false);

        questImage.SetActive(false);

        foreach (var collider in colliders)
        {
            if (collider != null) // Ensure the collider still exists
            {
                collider.enabled = false;
            }
        }

        foreach (var collider in allAnimalColliders)
        {
            if (collider != null) // Ensure the collider still exists
            {
                collider.enabled = false;
            }
        }

        if (shutterSound != null)
        {
            audioSource.PlayOneShot(shutterSound);
        }

        if (flashImage != null)
        {
            flashImage.color = new Color(1, 1, 1, 1);

            float elapsedTimeFlash = 0f;
            while (elapsedTimeFlash < flashDuration)
            {
                elapsedTimeFlash += Time.deltaTime;
                float alpha = Mathf.Lerp(1, 0, elapsedTimeFlash / flashDuration);
                flashImage.color = new Color(1, 1, 1, alpha);
                yield return null;
            }

            flashImage.color = new Color(1, 1, 1, 0);
        }

        cameraUI.SetActive(false);

        if (photoChild != null)
        {
            photoChild.gameObject.SetActive(true);

            LeanTween.scale(photoChild.gameObject, originalScale * 1.5f, 1f).setEaseOutBack();

            Vector3 photoTargetPosition = centerReference.position;
            photoTargetPosition.z = photoChild.position.z;
            LeanTween.move(photoChild.gameObject, photoTargetPosition, 1.5f).setEaseOutBack();
        }

        yield return new WaitForSeconds(3f);

        bool isCorrectPhoto = ValidateCurrentPhoto();

        photoQuestManager.ValidatePhoto(isCorrectPhoto, gameObject);

        if (isCorrectPhoto)
        {
            // Enable all remaining colliders and destroy the tapped animal
            EnableAllColliders();
           /* Destroy(gameObject);*/
        }
        else
        {
            // Keep the collider of the tapped animal disabled
            EnableOtherColliders(gameObject);
        }
        
    }

    private bool ValidateCurrentPhoto()
    {
        string requiredAnimalName = photoQuestManager.GetCurrentRequiredAnimalName();
        return gameObject.name == requiredAnimalName;  // Check if the current game object's name (the one tapped and holding ShutterController) matches the required animal's name
    }

    // Enable all colliders in the scene
    private void EnableAllColliders()
    {
        foreach (var collider in allAnimalColliders)
        {
            if (collider != null) // Ensure the collider still exists
            {
                collider.enabled = true;                
            }
        }
    }

    // Enable all colliders except the current animal's collider
    private void EnableOtherColliders(GameObject tappedAnimal)
    {
        foreach (var collider in allAnimalColliders)
        {
            if (collider != null && collider.gameObject != tappedAnimal) // Ensure the collider still exists
            {
                collider.enabled = true; // Enable only colliders that are not the tapped animal
                
            }
        }
    }
}
