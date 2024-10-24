using UnityEngine;
using System.Collections;
using TMPro;


public class Lvl7Sc2QuestManager : MonoBehaviour
{
    public GameObject questDisplayPanel;
    public RectTransform bannerImage;
    public TMP_Text questText;
    public Vector2 offScreenPos = new Vector2(-1920, 0); 
    public Vector2 centerScreenPos = new Vector2(-500, 0);
    public GameObject ing_borad;
    public float tweenTime = 0.5f;       

    private int _pizzaMade = 0;


    public int PizzaMade
    {
        get => _pizzaMade;
        set
        {
            if (_pizzaMade != value)
            {
                _pizzaMade = value;
                UpdateQuestDisplay();  // Automatically update the quest display whenever pizzaMade changes
            }
        }
    }

    // References to GameObjects with SpriteRenderers
    public GameObject questDisplay;
    public GameObject sauceIcon;
    public GameObject cheeseIcon;
    public GameObject toppingsIcon;
    public GameObject pepperoniIcon;

    // References to the sprites to be used for the Quest Display
    public Sprite sprite1;
    public Sprite sprite2;
    public Sprite sprite3;

    // References to the topping game objects with colliders
    public GameObject sauceTopping;
    public GameObject cheeseTopping;
    public GameObject toppingsTopping;
    public GameObject pepperoniTopping;

    public Lvl7Sc2DragManager dragManager;


    public void UpdateQuestDisplay()
    {
        ShowQuestBanner();
        switch (PizzaMade)
        {
            case 0:
                questDisplay.GetComponent<SpriteRenderer>().sprite = sprite1;
                sauceIcon.SetActive(true);
                cheeseIcon.SetActive(true);
                toppingsIcon.SetActive(false);
                pepperoniIcon.SetActive(false);

                // Set icon opacity based on collider status
                UpdateIconOpacity(sauceIcon, sauceTopping.GetComponent<Collider2D>().enabled);
                UpdateIconOpacity(cheeseIcon, cheeseTopping.GetComponent<Collider2D>().enabled);
                break;

            case 1:
                questDisplay.GetComponent<SpriteRenderer>().sprite = sprite2;
                sauceIcon.SetActive(true);
                cheeseIcon.SetActive(true);
                toppingsIcon.SetActive(true);
                pepperoniIcon.SetActive(false);

                // Set icon opacity based on collider status
                UpdateIconOpacity(sauceIcon, sauceTopping.GetComponent<Collider2D>().enabled);
                UpdateIconOpacity(cheeseIcon, cheeseTopping.GetComponent<Collider2D>().enabled);
                UpdateIconOpacity(toppingsIcon, toppingsTopping.GetComponent<Collider2D>().enabled);
                break;

            case 2:
                questDisplay.GetComponent<SpriteRenderer>().sprite = sprite3;
                sauceIcon.SetActive(true);
                cheeseIcon.SetActive(true);
                toppingsIcon.SetActive(true);
                pepperoniIcon.SetActive(true);

                // Set icon opacity based on collider status
                UpdateIconOpacity(sauceIcon, sauceTopping.GetComponent<Collider2D>().enabled);
                UpdateIconOpacity(cheeseIcon, cheeseTopping.GetComponent<Collider2D>().enabled);
                UpdateIconOpacity(toppingsIcon, toppingsTopping.GetComponent<Collider2D>().enabled);
                UpdateIconOpacity(pepperoniIcon, pepperoniTopping.GetComponent<Collider2D>().enabled);
                break;

            default:
                Debug.LogWarning("Invalid pizzaMade value.");
                break;
        }
    }

    // Helper method to set the icon opacity using SpriteRenderer
    public void SetIconOpacity(GameObject icon, float opacity)
    {
        Debug.Log("Opacity SchecK");
        SpriteRenderer spriteRenderer = icon.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = opacity;  // Set the alpha channel for opacity
            spriteRenderer.color = color;
        }
    }

    // This method can be called from DragManager to update the opacity when colliders are enabled/disabled
    public void UpdateIconOpacity(GameObject icon, bool isColliderActive)
    {
        if (isColliderActive)
        {
            SetIconOpacity(icon, 1f);  // Set full opacity when collider is active
        }
        else
        {
            SetIconOpacity(icon, 0.5f);  // Set half opacity when collider is inactive
        }
    }

    // This method can be called from other scripts to increment the pizzaMade count
    public void MakePizza()
    {
        if (PizzaMade < 3) // Assuming you want a maximum of 3 pizzas
        {
            PizzaMade++; // Using the property here ensures UpdateQuestDisplay() is called
            ResetToppingsAndIcons();
            dragManager.ResetDragManager();
            UpdateQuestDisplay();

        }
    }

    public void ResetToppingsAndIcons()
    {
        // Enable the toppings icons
        sauceIcon.SetActive(true);
        cheeseIcon.SetActive(true);
        toppingsIcon.SetActive(true);
        pepperoniIcon.SetActive(true);

        // Re-enable the colliders of the topping GameObjects
        sauceTopping.GetComponent<Collider2D>().enabled = true;
        cheeseTopping.GetComponent<Collider2D>().enabled = true;
        toppingsTopping.GetComponent<Collider2D>().enabled = true;
        pepperoniTopping.GetComponent<Collider2D>().enabled = true;

        // You can also reset the position of these objects if needed
        // e.g. sauceTopping.transform.position = originalPosition;
    }

    public void ShowQuestBanner()
    {
        ing_borad.SetActive(false);
        questDisplayPanel.SetActive(true);

        // Reset the banner image to its off-screen start position (outside the viewport)
        bannerImage.anchoredPosition = offScreenPos;

        // Tween the banner to the center of the screen
        LeanTween.move(bannerImage, centerScreenPos, tweenTime).setOnComplete(() =>
        {
            // Set the text based on the pizzaMade value
            string newText = GetQuestTextForPizza(PizzaMade);

            // Update the TextMeshPro text
            questText.text = newText;

            // Scale the text to make it pop up (1.2x bigger and back to normal)
            questText.transform.localScale = Vector3.zero; // Start from scale 0
            LeanTween.scale(questText.gameObject, Vector3.one, tweenTime).setOnComplete(() =>
            {
                // Wait for 2 seconds
                StartCoroutine(HideQuestBannerAfterDelay(2f));
            });
        });
    }

    private IEnumerator HideQuestBannerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        LeanTween.scale(questText.gameObject, Vector3.zero, tweenTime).setOnComplete(() =>
        {
            LeanTween.move(bannerImage, offScreenPos, tweenTime).setOnComplete(() =>
            {
                questDisplayPanel.SetActive(false);
                ing_borad.SetActive(true);
            });
        });
    }


    // Helper method to get the quest text based on the number of pizzas made
    private string GetQuestTextForPizza(int pizzaMade)
    {
        switch (pizzaMade)
        {
            case 0: return "Let's make a Cheese Pizza!";
            case 1: return "Let's make a Mushroom Pizza!";
            case 2: return "Let's make a Pepperoni Pizza!";
            default: return "Let's make a Pizza!";
        }
    }
}
