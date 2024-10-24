using UnityEngine;
using TMPro; // For TextMeshPro
using UnityEngine.UI; // For Image component
using System.Collections;
using System.Collections.Generic;

public class PhotoQuestManager : MonoBehaviour
{
    public GameObject questImage; // Image to display the black and white image of the quest animal
    public TextMeshProUGUI questText; // TextMeshPro for quest text
    public Animator birdAnimator; // Reference to the bird's animator
    public GameObject photoAlbumPanel; // Reference to the photo album panel
    public GameObject[] albumSlots; // Array for 6 slots in the photo album (each with an Image and TMP text)

    public List<GameObject> animals = new List<GameObject>(); // List of 6 unique animal GameObjects

    public PhotoCameraController photoCameraController;
    public LVL5Sc2HelperController helperController;

    private Dictionary<GameObject, Sprite> photoSprites = new Dictionary<GameObject, Sprite>(); // Cache of animal photo sprites
    public int currentAnimalIndex = 0; // Index of the current animal to take a photo of
    private bool validationSuccess = false; // To track validation result

    void Start()
    {
        // Ensure we have exactly 6 animals in the list
        if (animals.Count != 6)
        {
            Debug.LogError("Please assign exactly 6 animals in the PhotoQuestManager script.");
            return;
        }

        // Cache all the " - Photo" sprites and deactivate them after caching
        CacheAndDeactivateAnimalPhotoSprites();

        // Set up the first animal quest
        SetNextAnimalQuest();
    }

    // Cache the " - Photo" sprites for each animal and deactivate them after caching
    void CacheAndDeactivateAnimalPhotoSprites()
    {
        foreach (GameObject animal in animals)
        {
            // Construct the expected name of the "- Photo" child
            string expectedPhotoName = animal.name + " - Photo";

            Debug.Log("Looking for ' - Photo' under: " + animal.name); // Debugging output

            // Find the "- Photo" child of the current animal
            Transform photoTransform = animal.transform.Find(expectedPhotoName);

            if (photoTransform != null)
            {
                SpriteRenderer photoSpriteRenderer = photoTransform.GetComponent<SpriteRenderer>();
                if (photoSpriteRenderer != null)
                {
                    // Cache the sprite
                    photoSprites[animal] = photoSpriteRenderer.sprite;

                    // Deactivate the "- Photo" GameObject
                    photoTransform.gameObject.SetActive(false);
                    Debug.Log("Successfully cached and deactivated: " + expectedPhotoName);
                }
                else
                {
                    Debug.LogError("No SpriteRenderer found on '" + expectedPhotoName + "'.");
                }
            }
            else
            {
                Debug.LogError("No ' - Photo' object found for " + animal.name);
            }
        }
    }

    public string GetCurrentRequiredAnimalName()
    {
        return animals[currentAnimalIndex].name; // Return the name of the current required animal
    }

    // Sets up the next animal quest by displaying the black and white image and quest text
    void SetNextAnimalQuest()
    {
        if (currentAnimalIndex >= animals.Count)
        {
            Debug.Log("All animals have been photographed!");
            return; // All animals are done
        }

        Image albumSlotImage = albumSlots[currentAnimalIndex].GetComponentInChildren<Image>();
        questImage.GetComponent<Image>().sprite = albumSlotImage.sprite;
        questText.text = "Take a photo of " + animals[currentAnimalIndex].name;
    }

    IEnumerator HandleCorrectPhotoSequence(GameObject tappedAnimal)
    {
        yield return new WaitForSeconds(1f);

        GameObject photoChild = tappedAnimal.transform.Find(tappedAnimal.name + " - Photo")?.gameObject;

        if (photoChild != null)
        {
            photoChild.SetActive(false); // Deactivate the photoChild after animation
        }

        LeanTween.scale(photoAlbumPanel, new Vector3(0.9f, 0.9f, 0.9f), 1.5f).setEaseOutBack();
        yield return new WaitForSeconds(2f);

        GameObject currentAnimal = animals[currentAnimalIndex];
        Sprite photoSprite = photoSprites[currentAnimal];
        string animalName = currentAnimal.name;

        Image albumImage = albumSlots[currentAnimalIndex].GetComponentInChildren<Image>();
        TextMeshProUGUI albumText = albumSlots[currentAnimalIndex].GetComponentInChildren<TextMeshProUGUI>();

        albumImage.sprite = photoSprite;
        albumText.text = animalName;

        yield return new WaitForSeconds(2f);

        LeanTween.scale(photoAlbumPanel, Vector3.zero, 1.5f).setEaseInBack();

        // Destroy the tapped animal and its instances across grounds
        DestroyTappedAnimalAcrossGrounds(tappedAnimal.name);

        // Reactivate all remaining animal colliders
        foreach (GameObject animal in animals)
        {
            if (animal != null && animal != tappedAnimal)
            {
                Collider2D collider = animal.GetComponent<Collider2D>();
                if (collider != null) collider.enabled = true;
            }
        }

        questImage.SetActive(true);
        photoCameraController.canMove = true;
        photoCameraController.SetPanningEnabled(true);

        currentAnimalIndex++;
        SetNextAnimalQuest();

        if (helperController != null)
        {
            helperController.OnValidationSuccess();
        }
    }

    IEnumerator HandleIncorrectPhotoSequence(GameObject tappedAnimal)
    {
        yield return new WaitForSeconds(1f);

        GameObject photoChild = tappedAnimal.transform.Find(tappedAnimal.name + " - Photo")?.gameObject;

        // Deactivate the collider of the tapped animal to prevent further interactions
        foreach (GameObject animal in animals)
        {
            if (animal != null && animal != tappedAnimal)
            {
                Collider2D collider = animal.GetComponent<Collider2D>();
                if (collider != null) collider.enabled = true;
            }
        }

        questImage.SetActive(true);
        photoCameraController.canMove = true;
        photoCameraController.SetPanningEnabled(true);

        if (photoChild != null)
        {
            photoChild.SetActive(false); // Deactivate the photoChild after animation
        }
    }

    void DestroyTappedAnimalAcrossGrounds(string animalName)
    {
        // Get all ground objects from the PhotoCameraController
        GameObject[] grounds = { photoCameraController.ground1, photoCameraController.ground2, photoCameraController.ground3 };

        foreach (GameObject ground in grounds)
        {
            // Find the "Animals" container within each ground
            Transform animalsContainer = ground.transform.Find("Animals");

            if (animalsContainer != null)
            {
                // Search for the specific animal within the "Animals" container
                Transform animalTransform = animalsContainer.Find(animalName);

                if (animalTransform != null)
                {
                    Destroy(animalTransform.gameObject);
                }
            }
        }
    }

    public Vector3 GetCurrentAnimalPosition()
    {
        if (currentAnimalIndex < animals.Count && animals[currentAnimalIndex] != null)
        {
            return animals[currentAnimalIndex].transform.position;
        }
        return Vector3.zero; // Return a default value if index is out of bounds or animal is null
    }

    public void ValidatePhoto(bool isCorrectPhoto, GameObject tappedAnimal)
    {
        if (isCorrectPhoto)
        {
            validationSuccess = true;
            birdAnimator.SetTrigger("rightTalk");
            StartCoroutine(HandleCorrectPhotoSequence(tappedAnimal));
        }
        else
        {
            validationSuccess = false;
            birdAnimator.SetTrigger("wrongTalk");
            StartCoroutine(HandleIncorrectPhotoSequence(tappedAnimal));
        }
    }
}
