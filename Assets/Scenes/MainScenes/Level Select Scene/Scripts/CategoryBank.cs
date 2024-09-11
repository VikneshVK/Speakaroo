using System.Collections.Generic;
using UnityEngine;

public class CategoryBank : MonoBehaviour
{
    public enum Category
    {
        LearnToSpeak,
        LearnWords,
        FollowingDirections
    }

    public List<string> learnToSpeakButtons = new List<string> { "Speak 1", "Speak 2" };
    public List<string> learnWordsButtons = new List<string> { "Word 1", "Word 2", "Word 3", "Word 4", "Word 5" };
    public List<string> followingDirectionsButtons = new List<string> { "Direction 1", "Direction 2", "Direction 3" };

    // Get buttons for the selected category
    public List<string> GetButtonsForCategory(Category category)
    {
        switch (category)
        {
            case Category.LearnToSpeak:
                return learnToSpeakButtons;
            case Category.LearnWords:
                return learnWordsButtons;
            case Category.FollowingDirections:
                return followingDirectionsButtons;
            default:
                return null;
        }
    }
}
