using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UserProfile
{
    public string Name;
    public string DateOfBirth;
    public Sprite ProfileImage;
    public List<string> Achievements;
    public int LevelsCompleted;
    public int WordsLearned;

    public UserProfile(string name, string dob, Sprite profileImage)
    {
        Name = name;
        DateOfBirth = dob;
        ProfileImage = profileImage;
        Achievements = new List<string>();
        LevelsCompleted = 0;
        WordsLearned = 0;
    }
}
