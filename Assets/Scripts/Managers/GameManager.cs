using Firebase;
using Firebase.Database;
using Firebase.Extensions;
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

    private DatabaseReference dbReference;
    private bool isFirebaseInitialized = false;

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

        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                isFirebaseInitialized = true;
                Debug.Log("Firebase initialized successfully. dbReference is set.");

                // Fetch profiles from Firebase
                FetchUserProfilesFromFirebase();

                // Sync profiles when Firebase is initialized and internet is available
                SyncUserProfilesWithRealtimeDatabase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });

        // Sync profiles with Realtime Database every 5 minutes
        InvokeRepeating("SyncUserProfilesWithRealtimeDatabase", 300, 300);
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

        // Sync with Realtime Database when online
        SyncUserProfilesWithRealtimeDatabase();
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

    private void SyncUserProfilesWithRealtimeDatabase()
    {
        if (!isFirebaseInitialized || dbReference == null)
        {
            Debug.LogError("Database reference is not initialized.");
            return;
        }

        if (IsConnectedToInternet())
        {
            Debug.Log("Internet connection detected. Syncing profiles with Realtime Database.");

            foreach (var userProfile in userProfiles)
            {
                string json = JsonUtility.ToJson(userProfile);
                Debug.Log("Syncing profile: " + userProfile.Name); // Add debug log before the sync
                dbReference.Child("userProfiles").Child(userProfile.Name).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("User profile synced with Realtime Database: " + userProfile.Name);
                    }
                    else
                    {
                        Debug.LogError("Error syncing user profile with Realtime Database: " + task.Exception);
                    }
                });
            }
        }
        else
        {
            Debug.Log("No internet connection. Profiles will be synced when the device is online.");
        }
    }

    private void FetchUserProfilesFromFirebase()
    {
        if (!isFirebaseInitialized || dbReference == null)
        {
            Debug.LogError("Database reference is not initialized.");
            return;
        }

        dbReference.Child("userProfiles").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                userProfiles.Clear();
                foreach (DataSnapshot profileSnapshot in snapshot.Children)
                {
                    string json = profileSnapshot.GetRawJsonValue();
                    UserProfile userProfile = JsonUtility.FromJson<UserProfile>(json);
                    userProfiles.Add(userProfile);
                }
                SaveUserProfiles(); // Save fetched profiles locally
                DisplayUserProfiles(); // Update the UI
                Debug.Log("User profiles fetched from Firebase.");
            }
            else
            {
                Debug.LogError("Error fetching user profiles from Firebase: " + task.Exception);
            }
        });
    }

    private bool IsConnectedToInternet()
    {
        bool isConnected = Application.internetReachability != NetworkReachability.NotReachable;
        Debug.Log("Internet connectivity status: " + isConnected);
        return isConnected;
    }

    private void DisplayMessage(string message, bool isError)
    {
        messageText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        messageText.text = message;
        messageText.color = isError ? Color.red : Color.black;
    }
}
