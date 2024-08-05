using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JuiceController : MonoBehaviour
{
    public TMP_Text fruitRequirementsText;
    private List<string> requiredFruits = new List<string>();
    private SpriteChangeController spriteChangeController;

    void Start()
    {
        spriteChangeController = FindObjectOfType<SpriteChangeController>();
        spriteChangeController.OnKikisJuiceChanged += UpdateFruitRequirementsBasedOnKikisJuice;  // Subscribe to the event
        UpdateFruitRequirementsUI();
    }

    private void UpdateFruitRequirementsBasedOnKikisJuice(bool isKikisJuice)
    {
        requiredFruits.Clear();
        if (isKikisJuice)
        {
            requiredFruits.Add("Kiwi");
            requiredFruits.Add("Blueberry");
        }
        else
        {
            requiredFruits.Add("Strawberry");
        }
        UpdateFruitRequirementsUI();
    }

    private void UpdateFruitRequirementsUI()
    {
        fruitRequirementsText.text = "Required Fruits: " + string.Join(", ", requiredFruits);
        Debug.Log("Fruit requirements updated: " + fruitRequirementsText.text);
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (spriteChangeController)
        {
            spriteChangeController.OnKikisJuiceChanged -= UpdateFruitRequirementsBasedOnKikisJuice;
        }
    }
}
