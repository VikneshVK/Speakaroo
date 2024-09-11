using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollManager : MonoBehaviour
{
    public GameObject categoryScrollbar; // Scrollbar for the category scroll view
    public GameObject levelSelectScrollbar; // Scrollbar for the level select scroll view
    public Transform categoryContentParent; // Parent object holding category images (categories scroll view content)
    public Transform buttonContentParent; // Parent object holding buttons (level select scroll view content)
    public GameObject buttonPrefab; // Prefab for the buttons in level select

    public List<Sprite> learnToSpeakButtons; // Button images for "Learn to Speak"
    public List<string> learnToSpeakLevels;  // Level names for "Learn to Speak"
    public List<Sprite> learnWordsButtons;   // Button images for "Learn Words"
    public List<string> learnWordsLevels;    // Level names for "Learn Words"
    public List<Sprite> followingDirectionsButtons; // Button images for "Following Directions"
    public List<string> followingDirectionsLevels;  // Level names for "Following Directions"

    private float categoryScrollPos = 0; // Current scroll position for categories
    private float levelScrollPos = 0; // Current scroll position for level select
    private float[] categoryPositions;
    private float[] levelPositions;

    private List<GameObject> buttons = new List<GameObject>(); // Store instantiated buttons

    void Start()
    {
        // Set up positions for the category scroll view
        categoryPositions = new float[categoryContentParent.childCount];
        float categoryDistance = 1f / (categoryPositions.Length - 1f);

        for (int i = 0; i < categoryPositions.Length; i++)
        {
            categoryPositions[i] = categoryDistance * i;
        }
    }

    void Update()
    {
        HandleCategoryScroll(); // Handles snapping for categories
        HandleLevelSelectScroll(); // Handles snapping and interactivity for level select buttons
    }

    // Handle snapping and category selection for the categories scroll view
    void HandleCategoryScroll()
    {
        if (Input.GetMouseButton(0))
        {
            // Capture the current scrollbar value when the user is dragging
            categoryScrollPos = categoryScrollbar.GetComponent<Scrollbar>().value;
        }
        else
        {
            // Snap to the closest position when the user releases the mouse
            for (int i = 0; i < categoryPositions.Length; i++)
            {
                if (categoryScrollPos < categoryPositions[i] + (1f / (categoryPositions.Length - 1f) / 2) && categoryScrollPos > categoryPositions[i] - (1f / (categoryPositions.Length - 1f) / 2))
                {
                    categoryScrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(categoryScrollbar.GetComponent<Scrollbar>().value, categoryPositions[i], 0.1f);

                    // Load the buttons for the selected category
                    LoadButtonsForCategory(i); // i is the index of the selected category
                }
            }
        }

        // Handle the scaling of category images
        for (int i = 0; i < categoryPositions.Length; i++)
        {
            if (categoryScrollPos < categoryPositions[i] + (1f / (categoryPositions.Length - 1f) / 2) && categoryScrollPos > categoryPositions[i] - (1f / (categoryPositions.Length - 1f) / 2))
            {
                // Scale up the selected category
                categoryContentParent.GetChild(i).localScale = Vector2.Lerp(categoryContentParent.GetChild(i).localScale, new Vector2(1f, 1f), 0.1f);

                // Scale down the other categories
                for (int a = 0; a < categoryPositions.Length; a++)
                {
                    if (a != i)
                    {
                        categoryContentParent.GetChild(a).localScale = Vector2.Lerp(categoryContentParent.GetChild(a).localScale, new Vector2(0.8f, 0.8f), 0.1f);
                    }
                }
            }
        }
    }

    // Load the buttons for the selected category
    void LoadButtonsForCategory(int categoryIndex)
    {
        // Clear existing buttons
        foreach (GameObject button in buttons)
        {
            Destroy(button);
        }
        buttons.Clear();

        List<Sprite> buttonImages = null;
        List<string> buttonLevels = null; // Level names to be assigned to each button

        // Determine which category is selected and load the respective button images and levels
        switch (categoryIndex)
        {
            case 0: // Learn to Speak
                buttonImages = learnToSpeakButtons;
                buttonLevels = learnToSpeakLevels;
                break;
            case 1: // Learn Words
                buttonImages = learnWordsButtons;
                buttonLevels = learnWordsLevels;
                break;
            case 2: // Following Directions
                buttonImages = followingDirectionsButtons;
                buttonLevels = followingDirectionsLevels;
                break;
        }

        if (buttonImages != null && buttonLevels != null)
        {
            // Set up positions for the level select scroll view
            levelPositions = new float[buttonImages.Count];
            float levelDistance = 1f / (levelPositions.Length - 1f);
            for (int i = 0; i < levelPositions.Length; i++)
            {
                levelPositions[i] = levelDistance * i;
            }

            // Instantiate buttons dynamically for the selected category
            for (int i = 0; i < buttonImages.Count; i++)
            {
                GameObject newButton = Instantiate(buttonPrefab, buttonContentParent);
                CustomButton customButton = newButton.GetComponent<CustomButton>();

                // Set the image and the level name using your existing SetupButton method
                customButton.SetupButton(buttonImages[i], buttonLevels[i]);

                // Add the button to the list for later interactivity checks
                buttons.Add(newButton);
            }

            // Ensure only the centered button is interactable
            UpdateButtonInteractivity();
        }
    }

    // Handle snapping and button interactivity for the level select scroll view
    void HandleLevelSelectScroll()
    {
        if (Input.GetMouseButton(0))
        {
            // Capture the current scrollbar value when the user is dragging
            levelScrollPos = levelSelectScrollbar.GetComponent<Scrollbar>().value;
        }
        else
        {
            // Snap to the closest position when the user releases the mouse
            for (int i = 0; i < levelPositions.Length; i++)
            {
                if (levelScrollPos < levelPositions[i] + (1f / (levelPositions.Length - 1f) / 2) && levelScrollPos > levelPositions[i] - (1f / (levelPositions.Length - 1f) / 2))
                {
                    levelSelectScrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(levelSelectScrollbar.GetComponent<Scrollbar>().value, levelPositions[i], 0.1f);

                    // Make only the button in the center interactable
                    UpdateButtonInteractivity(i);
                }
            }
        }
    }

    // Make only the center button interactable
    void UpdateButtonInteractivity(int centerIndex = -1)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            Button btn = buttons[i].GetComponent<Button>();
            if (i == centerIndex)
            {
                btn.interactable = true;
            }
            else
            {
                btn.interactable = false;
            }
        }
    }
}
