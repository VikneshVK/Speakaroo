using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class profileManager : MonoBehaviour
{
    public static profileManager Instance { get; private set; }

    [Header("Profile Panel")]
    public GameObject profilePanel;
    public Transform profileButtonsParent;
    public GameObject profilePrefab;
    public Button createProfileButton;
    public TMP_Text messageText;

    [Header("Create Profile Panel")]
    public GameObject createProfilePanel;
    public TMP_InputField nameInputField;
    public Image profileImage;
    public Button saveProfileButton;
    public Button cancelProfileButton;

    [Header("DOB Manager")]
    public DOBManager dobManager;

    public TextMeshProUGUI welcomeText;

    private List<UserProfile> userProfiles = new List<UserProfile>();
    private UserProfile currentUserProfile;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        LoadProfiles();
        createProfileButton.onClick.AddListener(OpenCreateProfilePanel);
        saveProfileButton.onClick.AddListener(CreateProfile);
        cancelProfileButton.onClick.AddListener(CloseCreateProfilePanel);

        SceneManager.sceneLoaded += OnHomeSceneLoaded;
    }

    private void Start()
    {
        ShowProfilePanel();
    }

    private void ShowProfilePanel()
    {
        profilePanel.SetActive(true);
        createProfilePanel.SetActive(false);

        // Clear existing profile buttons
        foreach (Transform child in profileButtonsParent)
        {
            Destroy(child.gameObject);
        }

        // Instantiate profile buttons
        if (userProfiles.Count > 0)
        {
            messageText.gameObject.SetActive(false);
            foreach (UserProfile profile in userProfiles)
            {
                GameObject profileButton = Instantiate(profilePrefab, profileButtonsParent);
                profileButton.GetComponentInChildren<TextMeshProUGUI>().text = profile.Name;
                profileButton.GetComponentInChildren<Image>().sprite = profile.ProfileImage;
                profileButton.GetComponent<Button>().onClick.AddListener(() => SelectProfile(profile));
            }
        }
        else
        {
            messageText.gameObject.SetActive(true);
            messageText.text = "Create profile to continue";
        }
    }

    private void OpenCreateProfilePanel()
    {
        profilePanel.SetActive(false);
        createProfilePanel.SetActive(true);
    }

    private void CloseCreateProfilePanel()
    {
        profilePanel.SetActive(true);
        createProfilePanel.SetActive(false);
    }

    private void CreateProfile()
    {
        string name = nameInputField.text;
        string dob = dobManager.GetSelectedDate();
        Sprite image = profileImage.sprite;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(dob) || image == null)
        {
            // Display an error message to fill all fields
            return;
        }

        UserProfile newProfile = new UserProfile(name, dob, image);
        userProfiles.Add(newProfile);
        SaveProfiles();
        SelectProfile(newProfile);
    }

    private void SelectProfile(UserProfile profile)
    {
        currentUserProfile = profile;
        HideProfilePanel();
        LoadHomeScene();
    }

    private void LoadHomeScene()
    {
        SceneManager.LoadScene("HomeScene");
    }

    private void OnHomeSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "HomeScene" && currentUserProfile != null)
        {
            welcomeText.text = $"Welcome, {currentUserProfile.Name}";
            // Set the profile image and any other necessary UI elements
        }
    }

    private void SaveProfiles()
    {
        // Implement save logic (e.g., PlayerPrefs, JSON file, etc.)
    }

    private void LoadProfiles()
    {
        // Implement load logic (e.g., PlayerPrefs, JSON file, etc.)
    }

    public UserProfile GetCurrentUserProfile()
    {
        return currentUserProfile;
    }

    private void HideProfilePanel()
    {
        profilePanel.SetActive(false);
    }
}
