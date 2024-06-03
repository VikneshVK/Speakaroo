using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance { get; private set; }

    public GameObject profilePanel; // Assign in the Inspector
    public GameObject profilePrefab; // Assign in the Inspector
    public TMP_InputField nameInputField; // Assign in the Inspector
    public TMP_InputField dobInputField; // Assign in the Inspector
    public Image profileImage; // Assign in the Inspector
    public Button createProfileButton; // Assign in the Inspector

    public TextMeshProUGUI welcomeText; // Assign in the Inspector

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
        createProfileButton.onClick.AddListener(CreateProfile);
        SceneManager.sceneLoaded += OnHomeSceneLoaded;
    }

    private void Start()
    {
        ShowProfilePanel();
    }

    private void ShowProfilePanel()
    {
        profilePanel.SetActive(true);

        // Clear existing profile buttons
        foreach (Transform child in profilePanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Instantiate profile buttons
        foreach (UserProfile profile in userProfiles)
        {
            GameObject profileButton = Instantiate(profilePrefab, profilePanel.transform);
            profileButton.GetComponentInChildren<TextMeshProUGUI>().text = profile.Name;
            profileButton.GetComponentInChildren<Image>().sprite = profile.ProfileImage;
            profileButton.GetComponent<Button>().onClick.AddListener(() => SelectProfile(profile));
        }
    }

    private void HideProfilePanel()
    {
        profilePanel.SetActive(false);
    }

    private void CreateProfile()
    {
        string name = nameInputField.text;
        string dob = dobInputField.text;
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
        if (scene.name == "HomeScene")
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
}

