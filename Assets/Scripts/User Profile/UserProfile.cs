using UnityEngine;

[System.Serializable]
public class UserProfile
{
    public string Name;
    public string DateOfBirth;
    public int ProfileImageIndex;
    public bool AacNeeded;
    public int LevelsCompleted;
    public int WordsLearned;

    // Default constructor for Firebase serialization
    public UserProfile() { }

    public UserProfile(string name, string dateOfBirth, int profileImageIndex, bool aacNeeded, int levelsCompleted, int wordsLearned)
    {
        Name = name;
        DateOfBirth = dateOfBirth;
        ProfileImageIndex = profileImageIndex;
        AacNeeded = aacNeeded;
        LevelsCompleted = levelsCompleted;
        WordsLearned = wordsLearned;
    }
}
