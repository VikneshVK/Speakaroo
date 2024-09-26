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
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                touchPosition.z = 0f;

                RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);

                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    StartCoroutine(TakePhoto());
                }
            }
        }
    }

    IEnumerator TakePhoto()
    {        
        photoCameraController.SetPanningEnabled(false);
        
        questImage.SetActive(false);
       
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        foreach (var collider in allAnimalColliders)
        {
            collider.enabled = false;
        }

        // Enable camera UI
        if (cameraUI != null)
        {
            cameraUI.SetActive(true);
        }

        yield return new WaitForSeconds(1f);

        if (shutterSound != null)
        {
            audioSource.PlayOneShot(shutterSound);
        }

        if (flashImage != null)
        {
            flashImage.color = new Color(1, 1, 1, 1);

            float elapsedTime = 0f;
            while (elapsedTime < flashDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1, 0, elapsedTime / flashDuration);
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
            
            Vector3 targetPosition = centerReference.position;
            targetPosition.z = photoChild.position.z; 
            LeanTween.move(photoChild.gameObject, targetPosition, 1.5f).setEaseOutBack();
        }
        
        yield return new WaitForSeconds(3f);
        
        bool isCorrectPhoto = ValidateCurrentPhoto();
       
        photoQuestManager.ValidatePhoto(isCorrectPhoto, gameObject);
        
        if (isCorrectPhoto)
        {
            EnableOtherColliders(gameObject); 
        }
        else
        {
            EnableAllColliders(); 
        }
        
        /*questImage.SetActive(true);*/
        
        photoCameraController.SetPanningEnabled(true);
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
            collider.enabled = true;
        }
    }

    // Enable all colliders except the current animal's collider
    private void EnableOtherColliders(GameObject tappedAnimal)
    {
        foreach (var collider in allAnimalColliders)
        {
            if (collider.gameObject != tappedAnimal)
            {
                collider.enabled = true; // Enable only colliders that are not the tapped animal
            }
        }
    }
}
