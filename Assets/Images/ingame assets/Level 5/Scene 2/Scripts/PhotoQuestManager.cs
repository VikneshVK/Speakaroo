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

    private Dictionary<GameObject, Sprite> photoSprites = new Dictionary<GameObject, Sprite>(); // Cache of animal photo sprites
    private int currentAnimalIndex = 0; // Index of the current animal to take a photo of
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

        // Assuming each animal has a black and white image attached to it
        GameObject currentAnimal = animals[currentAnimalIndex];
        questImage.GetComponent<Image>().sprite = photoSprites[currentAnimal]; // Set the cached black and white sprite
        questText.text = "Take a photo of " + currentAnimal.name; // Update quest text
    }

    // Coroutine to handle the correct photo sequence
    IEnumerator HandleCorrectPhotoSequence(GameObject tappedAnimal)
    {
        /* // Wait until the bird's "right talk" animation is finished
         yield return new WaitUntil(() => birdAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);*/
        yield return new WaitForSeconds(1f);
        // Find the "- Photo" child of the tapped animal
        GameObject photoChild = tappedAnimal.transform.Find(tappedAnimal.name + " - Photo").gameObject;

        if (photoChild != null)
        {
            photoChild.SetActive(false); // Deactivate the photoChild after animation
        }

        // Now open the photo album panel
        LeanTween.scale(photoAlbumPanel, new Vector3(0.9f, 0.9f, 0.9f), 1.5f).setEaseOutBack();

        // Wait for the album panel tween to complete
        yield return new WaitForSeconds(2f);

        // Update the album slot with the correct animal's photo and text
        GameObject currentAnimal = animals[currentAnimalIndex];
        Sprite photoSprite = photoSprites[currentAnimal];
        string animalName = currentAnimal.name;

        // Find the corresponding album slot
        Image albumImage = albumSlots[currentAnimalIndex].GetComponentInChildren<Image>(); // Get the Image component in the album slot
        TextMeshProUGUI albumText = albumSlots[currentAnimalIndex].GetComponentInChildren<TextMeshProUGUI>(); // Get the TMP text component in the album slot

        // Update the image and text
        albumImage.sprite = photoSprite; // Set the photo sprite
        albumText.text = animalName; // Set the animal's name

        // Wait for a bit before scaling back down
        yield return new WaitForSeconds(2f);

        // Tween the photo album panel's scale down to 0
        LeanTween.scale(photoAlbumPanel, Vector3.zero, 1.5f).setEaseInBack();

        questImage.SetActive(true);

        // Move on to the next animal
        currentAnimalIndex++;
        SetNextAnimalQuest();
    }

    // Coroutine to handle the incorrect photo sequence
    IEnumerator HandleIncorrectPhotoSequence(GameObject tappedAnimal)
    {
        // Wait until the bird's "wrong talk" animation is finished
        yield return new WaitUntil(() => birdAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);

        // Find the "- Photo" child of the tapped animal, and deactivate it
        GameObject photoChild = tappedAnimal.transform.Find(tappedAnimal.name + " - Photo").gameObject;

        if (photoChild != null)
        {
            photoChild.SetActive(false); // Deactivate the photoChild after animation
        }
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
            StartCoroutine(HandleIncorrectPhotoSequence(tappedAnimal)); // Pass the tapped animal here
        }
    }
}