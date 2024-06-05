using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HomeScreenManager : MonoBehaviour
{
    public Button homeScreenProfileButton;
    public Image homeScreenProfileImage;
    public Sprite[] profileImages; // Ensure this matches the profileImages array in GameManager

    private void Start()
    {
        InitializeHomeScreenProfileButton();
    }

    private void InitializeHomeScreenProfileButton()
    {
        UserProfile currentUserProfile = PersistentDataManager.CurrentUserProfile;

        if (currentUserProfile != null)
        {
            homeScreenProfileImage.sprite = profileImages[currentUserProfile.ProfileImageIndex];
            homeScreenProfileButton.onClick.AddListener(OpenUserCreationScene);
        }
        else
        {
            // Handle the case where no profile is selected
            Debug.LogWarning("No user profile selected.");
        }
    }

    private void OpenUserCreationScene()
    {
        SceneManager.LoadScene("User Creation Scene"); // Replace with your user creation scene name
    }
}
