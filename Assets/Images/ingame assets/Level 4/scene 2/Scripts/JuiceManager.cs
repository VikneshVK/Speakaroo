using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class JuiceManager : MonoBehaviour
{
    public TMP_Text fruitRequirementsText;
    public bool isKikiJuice = false;
    public bool sceneEnded = false;
    public JuiceController juiceController;
    public List<string> requiredFruits = new List<string>();
    public bool isSecondTime = false; // This will be updated based on TweeningController

    private TweeningController tweeningController;

    void Start()
    {
        juiceController = FindObjectOfType<JuiceController>();
        tweeningController = FindObjectOfType<TweeningController>(); // Find the TweeningController in the scene
        fruitRequirementsText.gameObject.SetActive(false); // Disable text at start
        UpdateFruitRequirements(isKikiJuice);
    }

    void Update()
    {
        if (tweeningController.isSecondTime)
        {
            fruitRequirementsText.gameObject.SetActive(true); // Enable text when isSecondTime becomes true
            if (!isSecondTime) // Only update fruit requirements once when isSecondTime becomes true
            {
                UpdateFruitRequirements(isKikiJuice);
                isSecondTime = true; // Set the flag to true to avoid repeated updates
            }
        }
        else
        {
            if (isSecondTime) // Reset when isSecondTime becomes false
            {
                fruitRequirementsText.gameObject.SetActive(false); // Disable text
                isSecondTime = false; // Reset the flag
            }
        }
    }

    public void UpdateFruitRequirements(bool isKikiJuice)
    {
        if (isKikiJuice)
        {
            List<string[]> options = new List<string[]>
            {
                new string[] { "Kiwi", "SB" },
                new string[] { "Kiwi", "BB" },
                new string[] { "SB", "BB" }
            };
            requiredFruits = options[Random.Range(0, options.Count)].ToList();
        }
        else
        {
            List<string> options = new List<string> { "Kiwi", "BB", "SB" };
            requiredFruits = new List<string> { options[Random.Range(0, options.Count)] };
        }

        UpdateFruitRequirementsUI();

        if (isKikiJuice)
        {
            EnableAllFruitColliders();
        }
    }

    void UpdateFruitRequirementsUI()
    {
        fruitRequirementsText.text = "Required Fruits: " + string.Join(", ", requiredFruits);
        Debug.Log("Fruit requirements updated: " + fruitRequirementsText.text);
    }

    void EnableAllFruitColliders()
    {
        GameObject[] fruits = GameObject.FindGameObjectsWithTag("Kiwi")
            .Concat(GameObject.FindGameObjectsWithTag("SB"))
            .Concat(GameObject.FindGameObjectsWithTag("BB"))
            .ToArray();

        foreach (GameObject fruit in fruits)
        {
            Collider2D fruitCollider = fruit.GetComponent<Collider2D>();
            if (fruitCollider != null)
            {
                fruitCollider.enabled = true;
            }
        }

        Debug.Log("All fruit colliders enabled.");
    }
}
