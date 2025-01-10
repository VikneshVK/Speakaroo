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

    public GameObject groundL;
    public GameObject groundR;

    public AudioClip Dialouge1;
    public AudioClip FinalAudio;
    public AudioClip audio1;
    public AudioClip audio2;
    public AudioClip audio3;
    public AudioClip audio4;
    public AudioClip audio5;
    public AudioClip audio6;
    public AudioClip rightAudio;
    public AudioClip wrongAudio;
    public SubtitleManager subtitleManager;

    public bool isClicked = false;

    private Dictionary<GameObject, Sprite> photoSprites = new Dictionary<GameObject, Sprite>(); // Cache of animal photo sprites
    public int currentAnimalIndex = 0; // Index of the current animal to take a photo of
    private bool validationSuccess = false; // To track validation result

    void Start()
    {
        if (animals.Count != 6)
        {            
            return;
        }

        CacheAndDeactivateAnimalPhotoSprites();

        StartCoroutine(PlayIntroAndSetNextQuest());
    }

    IEnumerator PlayIntroAndSetNextQuest()
    {
        yield return new WaitForSeconds(1f);

        birdAnimator.SetTrigger("Intro");
        GetComponent<AudioSource>().PlayOneShot(Dialouge1);
        subtitleManager.DisplaySubtitle("Help us take Pictures of the Animals", "Kiki", Dialouge1);

        yield return new WaitForSeconds(3f);

        SetNextAnimalQuest();
    }



    void CacheAndDeactivateAnimalPhotoSprites()
    {
        foreach (GameObject animal in animals)
        {

            string expectedPhotoName = animal.name + " - Photo";           

            Transform photoTransform = animal.transform.Find(expectedPhotoName);

            if (photoTransform != null)
            {
                SpriteRenderer photoSpriteRenderer = photoTransform.GetComponent<SpriteRenderer>();
                if (photoSpriteRenderer != null)
                {

                    photoSprites[animal] = photoSpriteRenderer.sprite;

                    photoTransform.gameObject.SetActive(false);                    
                }
                
            }            
        }
    }

    public string GetCurrentRequiredAnimalName()
    {
        return animals[currentAnimalIndex].name;
    }

    void SetNextAnimalQuest()
    {
        if (currentAnimalIndex >= animals.Count)
        {
            Debug.Log("All animals have been photographed!");
            return; // All animals are done
        }

        // Get the current animal and its name
        GameObject currentAnimal = animals[currentAnimalIndex];
        string animalName = currentAnimal.name;

        // Set quest image and text
        Image albumSlotImage = albumSlots[currentAnimalIndex].GetComponentInChildren<Image>();
        questImage.GetComponent<Image>().enabled = true;
        questImage.GetComponent<Image>().sprite = albumSlotImage.sprite;
        questText.text = "Take a photo of " + animalName;

        // Trigger bird animations and play audio based on animal name
        switch (animalName)
        {
            case "Hippo":
                birdAnimator.SetTrigger("Hippo");
                GetComponent<AudioSource>().PlayOneShot(audio1);
                subtitleManager.DisplaySubtitle("Find the hippo.", "Kiki", audio1);
                break;
            case "Croc":
                birdAnimator.SetTrigger("Croc");
                GetComponent<AudioSource>().PlayOneShot(audio2);
                subtitleManager.DisplaySubtitle("Find the crocodile.", "Kiki", audio2);
                break;
            case "Lion":
                birdAnimator.SetTrigger("Lion");
                GetComponent<AudioSource>().PlayOneShot(audio3);
                subtitleManager.DisplaySubtitle("Find the lion.", "Kiki", audio3);
                break;
            case "Monkey":
                birdAnimator.SetTrigger("Monkey");
                GetComponent<AudioSource>().PlayOneShot(audio4);
                subtitleManager.DisplaySubtitle("Find the monkey.", "Kiki", audio4);
                break;
            case "Panda":
                birdAnimator.SetTrigger("Panda");
                GetComponent<AudioSource>().PlayOneShot(audio5);
                subtitleManager.DisplaySubtitle("Find the panda.", "Kiki", audio5);
                break;
            case "Tiger":
                birdAnimator.SetTrigger("Tiger");
                GetComponent<AudioSource>().PlayOneShot(audio6);
                subtitleManager.DisplaySubtitle("Find the tiger.", "Kiki", audio6);
                break;
            default:
                Debug.LogWarning("No animation or audio set for " + animalName);
                break;
        }
    }


    /*IEnumerator HandleCorrectPhotoSequence(GameObject tappedAnimal)
    {
        yield return new WaitForSeconds(1f);

        GameObject photoChild = tappedAnimal.transform.Find(tappedAnimal.name + " - Photo")?.gameObject;

        if (photoChild != null)
        {
            photoChild.SetActive(false); // Deactivate the photoChild after animation
        }

        LeanTween.scale(photoAlbumPanel, new Vector3(1f, 1f, 1f), 1.5f).setEaseOutBack();
        yield return new WaitForSeconds(2f);

        // New code starts here
        Transform animationPanel = photoAlbumPanel.transform.Find("Animation Panel");
        if (animationPanel != null)
        {
            Animator animationPanelAnimator = animationPanel.GetComponent<Animator>();
            if (animationPanelAnimator != null)
            {
                animationPanelAnimator.SetBool("album open", true); // Turn "album open" parameter to true
            }
        }

        yield return new WaitForSeconds(1f); // Wait for 1 second

        // Tween the scale of all previous index children to 0.6
        for (int i = 0; i <= currentAnimalIndex; i++)
        {
            Transform child = animationPanel.GetChild(i);
            if (child != null)
            {
                LeanTween.scale(child.gameObject, new Vector3(0.6f, 0.6f, 0.6f), 1f).setEaseInOutSine();
            }
        }

        // Update album slot with correct photo and text
        GameObject currentAnimal = animals[currentAnimalIndex];
        Sprite photoSprite = photoSprites[currentAnimal];
        string animalName = currentAnimal.name;

        Image albumImage = albumSlots[currentAnimalIndex].GetComponentInChildren<Image>();
        TextMeshProUGUI albumText = albumSlots[currentAnimalIndex].GetComponentInChildren<TextMeshProUGUI>();

        albumImage.sprite = photoSprite;
        albumText.text = animalName;

        yield return new WaitForSeconds(3f); // Wait for 2 seconds to show updated album slot

        // Tween scales of all previous index children to zero
        for (int i = 0; i < currentAnimalIndex; i++)
        {
            Transform child = animationPanel.GetChild(i);
            if (child != null)
            {
                LeanTween.scale(child.gameObject, Vector3.zero, 1f).setEaseInOutSine();
            }
        }

        // Scale current index child to zero (if needed)
        Transform currentIndexChild = animationPanel.GetChild(currentAnimalIndex);
        if (currentIndexChild != null)
        {
            LeanTween.scale(currentIndexChild.gameObject, Vector3.zero, 1f).setEaseInOutSine();
        }
        yield return new WaitForSeconds(1f);
        if (animationPanel != null)
        {
            Animator animationPanelAnimator = animationPanel.GetComponent<Animator>();
            if (animationPanelAnimator != null)
            {
                animationPanelAnimator.SetBool("album open", false); // Turn "album open" parameter to true
            }
        }
        yield return new WaitForSeconds(1f);
        // Scale photoAlbumPanel down to zero size
        LeanTween.scale(photoAlbumPanel, Vector3.zero, 1.5f).setEaseInBack();

        DestroyTappedAnimalAcrossGrounds(tappedAnimal.name);

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

        yield return new WaitForSeconds(1.5f);

        if (currentAnimalIndex >= animals.Count)
        {
            birdAnimator.SetTrigger("FinalDialouge");
            GetComponent<AudioSource>().PlayOneShot(FinalAudio);
            StartCoroutine(RevealTextWordByWord("You are a good photographer", 0.5f));
        }
        else
        {
            SetNextAnimalQuest();
        }

        isClicked = false;

        if (helperController != null)
        {
            helperController.OnValidationSuccess();
        }
    }*/

    IEnumerator HandleCorrectPhotoSequence(GameObject tappedAnimal)
    {
        yield return new WaitForSeconds(1f);

        GameObject photoChild = tappedAnimal.transform.Find(tappedAnimal.name + " - Photo")?.gameObject;

        if (photoChild != null)
        {
            photoChild.SetActive(false); // Deactivate the photoChild after animation
        }

        LeanTween.scale(photoAlbumPanel, new Vector3(1f, 1f, 1f), 1.5f).setEaseOutBack();
        yield return new WaitForSeconds(2f);

        // New code starts here
        Transform animationPanel = photoAlbumPanel.transform.Find("Animation Panel");
        if (animationPanel != null)
        {
            Animator animationPanelAnimator = animationPanel.GetComponent<Animator>();
            if (animationPanelAnimator != null)
            {
                animationPanelAnimator.SetBool("album open", true); // Turn "album open" parameter to true
            }
        }

        yield return new WaitForSeconds(1f); // Wait for 1 second

        // Lean tween the scale of Animation Panel children to 0.6
        foreach (Transform child in animationPanel)
        {
            LeanTween.scale(child.gameObject, new Vector3(0.6f, 0.6f, 0.6f), 1f).setEaseInOutSine();
        }

        // Update album slot with correct photo and text
        GameObject currentAnimal = animals[currentAnimalIndex];
        Sprite photoSprite = photoSprites[currentAnimal];
        string animalName = currentAnimal.name;

        Image albumImage = albumSlots[currentAnimalIndex].GetComponentInChildren<Image>();
        TextMeshProUGUI albumText = albumSlots[currentAnimalIndex].GetComponentInChildren<TextMeshProUGUI>();

        albumImage.sprite = photoSprite;
        albumText.text = animalName;

        yield return new WaitForSeconds(5f); // Wait for 2 seconds to show updated album slot

        // Tween scales of Animation Panel children to zero
        foreach (Transform child in animationPanel)
        {
            LeanTween.scale(child.gameObject, Vector3.zero, 1f).setEaseInOutSine();
        }
        yield return new WaitForSeconds(1f);
        // Turn "album open" parameter to false
        if (animationPanel != null)
        {
            Animator animationPanelAnimator = animationPanel.GetComponent<Animator>();
            if (animationPanelAnimator != null)
            {
                animationPanelAnimator.SetBool("album open", false); // Turn "album open" parameter to true
            }
        }

        // Scale photoAlbumPanel down to zero size
        LeanTween.scale(photoAlbumPanel, Vector3.zero, 1.5f).setEaseInBack();

        DestroyTappedAnimalAcrossGrounds(tappedAnimal.name);

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

        yield return new WaitForSeconds(1.5f);

        if (currentAnimalIndex >= animals.Count)
        {
            birdAnimator.SetTrigger("FinalDialouge");
            GetComponent<AudioSource>().PlayOneShot(FinalAudio);
            subtitleManager.DisplaySubtitle("You are a good photographer.", "Kiki", FinalAudio);
        }
        else
        {
            SetNextAnimalQuest();
        }

        isClicked = false;

        if (helperController != null)
        {
            helperController.OnValidationSuccess();
        }
    }

    IEnumerator HandleIncorrectPhotoSequence(GameObject tappedAnimal)
    {
        yield return new WaitForSeconds(1f);

        GameObject photoChild = tappedAnimal.transform.Find(tappedAnimal.name + " - Photo")?.gameObject;

        foreach (GameObject animal in animals)
        {
            if (animal != null && animal != tappedAnimal)
            {
                Collider2D collider = animal.GetComponent<Collider2D>();
                if (collider != null) collider.enabled = true;
            }
        }
        isClicked = false;
        questImage.SetActive(true);
        photoCameraController.canMove = true;
        photoCameraController.SetPanningEnabled(true);

        if (photoChild != null)
        {
            photoChild.SetActive(false); // Deactivate the photoChild after animation
        }
        if (helperController != null)
        {
            helperController.OnValidationFailed();
        }
    }

    void DestroyTappedAnimalAcrossGrounds(string animalName)
    {
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

    public List<Vector3> GetAnimalPositionsInAllGrounds(int animalIndex)
    {
        List<Vector3> positions = new List<Vector3>();

        // Main Ground - find the animal in the Animals container
        if (animalIndex < animals.Count)
        {
            GameObject animal = animals[animalIndex];
            if (animal != null)
            {
                positions.Add(animal.transform.position);
                
            }
        }

        // Ground L - look within the Animals container
        Transform animalInGroundL = groundL.transform.Find("Animals/" + animals[animalIndex].name);
        if (animalInGroundL != null)
        {
            positions.Add(animalInGroundL.position);
            
        }

        // Ground R - look within the Animals container
        Transform animalInGroundR = groundR.transform.Find("Animals/" + animals[animalIndex].name);
        if (animalInGroundR != null)
        {
            positions.Add(animalInGroundR.position);            
        }

        return positions;
    }


    public void ValidatePhoto(bool isCorrectPhoto, GameObject tappedAnimal)
    {
        if (isCorrectPhoto)
        {
            validationSuccess = true;
            birdAnimator.SetTrigger("rightTalk");
            GetComponent<AudioSource>().PlayOneShot(rightAudio);
            StartCoroutine(HandleCorrectPhotoSequence(tappedAnimal));
        }
        else
        {
            validationSuccess = false;
            birdAnimator.SetTrigger("wrongTalk");
            GetComponent<AudioSource>().PlayOneShot(wrongAudio);
            StartCoroutine(HandleIncorrectPhotoSequence(tappedAnimal));
        }
    }
}
