using UnityEngine;
using UnityEngine.UI;

public class CustomButton : MonoBehaviour
{
    public Image buttonImage; // Reference to the Image component of the button
    private string levelToLoad; // The level this button will load

    // Set the image and level
    public void SetupButton(Sprite image, string levelName)
    {
        buttonImage.sprite = image; // Set the custom image
        levelToLoad = levelName;     // Set the level to load on click
    }

    // Load the level when the button is clicked
    public void OnButtonClick()
    {
        // Load the level using the stored level name
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelToLoad);
    }
}
