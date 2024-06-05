using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject profilePanel;
    public Transform profileButtonsParent;
    public GameObject profilePrefab;
    public Button createProfileButton;
    public TextMeshProUGUI messageText;
    public Button clearUsersButton;
    public GameObject createProfilePanel;
    public TMP_InputField nameInputField;
    public Sprite[] profileImages;
    public Button saveProfileButton;
    public Button cancelProfileButton;
    public Toggle aacToggle;
    public DOBManager dobManager;

    private List<UserProfile> userProfiles = new List<UserProfile>();
    private string profilesFilePath;
    private UserProfile currentUserProfile;

    private void Start()
    {
        profilesFilePath = Path.Combine(Application.persistentDataPath, "userProfiles.json");
        LoadUserProfiles();
        DisplayUserProfiles();

        createProfileButton.onClick.AddListener(OpenCreateProfilePanel);
        saveProfileButton.onClick.AddListener(CreateProfile);
        clearUsersButton.onClick.AddListener(ClearUsers);
        createProfilePanel.SetActive(false);

        clearUsersButton.gameObject.SetActive(userProfiles.Count > 0);
    }

    private void LoadUserProfiles()
    {
        if (File.Exists(profilesFilePath))
        {
            string json = File.ReadAllText(profilesFilePath);
            userProfiles = JsonConvert.DeserializeObject<List<UserProfile>>(json);
        }
    }

    private void SaveUserProfiles()
    {
        string json = JsonConvert.SerializeObject(userProfiles, Formatting.Indented);
        File.WriteAllText(profilesFilePath, json);
    }

    private void CreateProfile()
    {
        string name = nameInputField.text.Trim();
        string dob = dobManager.GetSelectedDate();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(dob))
        {
            DisplayMessage("Please fill in all fields.", true);
            return;
        }

        if (IsProfileCreated(name))
        {
            DisplayMessage("A profile with this name already exists.", true);
            return;
        }

        int randomIndex = Random.Range(0, profileImages.Length);

        UserProfile newProfile = new UserProfile
        {
            Name = name,
            DateOfBirth = dob,
            ProfileImageIndex = randomIndex,
            AacNeeded = aacToggle.isOn,
            LevelsCompleted = 0,
            WordsLearned = 0
        };

        userProfiles.Add(newProfile);
        SaveUserProfiles();

        createProfilePanel.SetActive(false);
        DisplayUserProfiles();
    }

    private void DisplayUserProfiles()
    {
        foreach (Transform child in profileButtonsParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var userProfile in userProfiles)
        {
            GameObject profileButton = Instantiate(profilePrefab, profileButtonsParent);
            profileButton.GetComponentInChildren<TextMeshProUGUI>().text = userProfile.Name;
            profileButton.GetComponent<Image>().sprite = profileImages[userProfile.ProfileImageIndex];
            profileButton.GetComponent<Button>().onClick.AddListener(() => SelectProfile(userProfile));
        }

        bool hasProfiles = userProfiles.Count > 0;
        clearUsersButton.gameObject.SetActive(hasProfiles);
        messageText.gameObject.SetActive(!hasProfiles);
        messageText.text = hasProfiles ? "" : "No profiles found";
    }

    private void ClearUsers()
    {
        userProfiles.Clear();
        SaveUserProfiles();
        DisplayUserProfiles();
    }

    private bool IsProfileCreated(string userName)
    {
        return userProfiles.Exists(profile => profile.Name.Equals(userName, System.StringComparison.OrdinalIgnoreCase));
    }

    private void OpenCreateProfilePanel()
    {
        createProfilePanel.SetActive(true);
        nameInputField.text = "";
        dobManager.ClearDateSelection();
        aacToggle.isOn = false;
        DisplayMessage("", false);
    }

    private void SelectProfile(UserProfile userProfile)
    {
        currentUserProfile = userProfile;
        Debug.Log("Selected profile: " + userProfile.Name);

        // Store the current user profile in PersistentDataManager
        PersistentDataManager.CurrentUserProfile = userProfile;

        // Optionally load the home screen scene
        SceneManager.LoadScene("Home Screen"); // Replace with your home screen scene name
    }

    private void UpdateProfileProgress(int levelsCompleted, int wordsLearned)
    {
        if (currentUserProfile != null)
        {
            currentUserProfile.LevelsCompleted = levelsCompleted;
            currentUserProfile.WordsLearned = wordsLearned;
            SaveUserProfiles();
        }
    }

    private void DisplayMessage(string message, bool isError)
    {
        messageText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        messageText.text = message;
        messageText.color = isError ? Color.red : Color.black;
    }
}


