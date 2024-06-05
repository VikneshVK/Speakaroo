using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ProfileMessageButton : MonoBehaviour
{
    public Image userImage;
    public TextMeshProUGUI userNameText;
    public Button profileButton;
    private string sceneName;

    private void Start()
    {
        profileButton.onClick.AddListener(OnProfileButtonClick);
    }

    public void SetProfileData(Sprite userProfileImage, string userName)
    {
        userImage.sprite = userProfileImage;
        userNameText.text = userName;
    }

    public void SetSceneName(string sceneName)
    {
        this.sceneName = sceneName;
    }

    private void OnProfileButtonClick()
    {
        SceneManager.LoadScene(sceneName);
    }
}
