using UnityEngine;

public class Lvl7Sc2DragManager : MonoBehaviour
{
    public Lvl7Sc2QuestManager questManager;

    // References to the toppings GameObjects
    public GameObject sauceTopping;
    public GameObject cheeseTopping;
    public GameObject mushroomTopping;
    public GameObject pepperoniTopping;

    // Reference to the PizzaDrag script
    public PizzaDrag pizzaDrag;

    private GameObject[] currentToppings;  // Array to store the toppings for this pizza
    private int currentToppingIndex = 0;   // Track the current topping being dropped
    private int totalToppings = 0;         // Total number of toppings for this pizza

    void Start()
    {
        // Initialize the colliders based on the current pizza state
        UpdateColliders();
    }

    // Method to update the enabled collider based on PizzaMade and the current progress
    public void UpdateColliders()
    {
        DisableAllColliders();

        // Determine the sequence of toppings based on PizzaMade value
        if (questManager.PizzaMade == 0)
        {
            currentToppings = new GameObject[] { sauceTopping, cheeseTopping };
            totalToppings = 2;
        }
        else if (questManager.PizzaMade == 1)
        {
            currentToppings = new GameObject[] { sauceTopping, cheeseTopping, mushroomTopping };
            totalToppings = 3;
        }
        else if (questManager.PizzaMade == 2)
        {
            currentToppings = new GameObject[] { sauceTopping, cheeseTopping, mushroomTopping, pepperoniTopping };
            totalToppings = 4;
        }
        questManager.UpdateQuestDisplay();
        EnableNextTopping();
    }

    // Enable the next topping in the sequence
    public void EnableNextTopping()
    {
        if (currentToppingIndex < totalToppings)
        {
            GameObject topping = currentToppings[currentToppingIndex];
            topping.GetComponent<Collider2D>().enabled = true;
            UpdateIconOpacityForTopping(topping);
        }
    }

    // Call this method when a topping is dropped successfully
    public void OnToppingDropped()
    {
        if (currentToppingIndex < totalToppings)
        {
            GameObject droppedTopping = currentToppings[currentToppingIndex];
            droppedTopping.GetComponent<Collider2D>().enabled = false;

            currentToppingIndex++;

            // If the required number of toppings have been dropped, enable the pizza collider
            if (currentToppingIndex == totalToppings)
            {
                // Enable the pizza collider by calling PizzaDrag's method
                pizzaDrag.EnablePizzaCollider();
            }
            else
            {
                // Enable the next topping collider
                EnableNextTopping();
            }
            UpdateIconOpacityForTopping(droppedTopping);
        }
    }

    public void ResetDragManager()
    {
        currentToppingIndex = 0;  // Reset the topping index
        currentToppings = null;   // Reset the toppings array
        totalToppings = 0;        // Reset total toppings count   
        pizzaDrag.pizzaDropped = false;
        pizzaDrag.canTapPizzaImage = false;
        UpdateColliders();        // Reinitialize the colliders for the new pizza
    }

    // Disable all colliders to reset
    public void DisableAllColliders()
    {
        sauceTopping.GetComponent<Collider2D>().enabled = false;
        cheeseTopping.GetComponent<Collider2D>().enabled = false;
        mushroomTopping.GetComponent<Collider2D>().enabled = false;
        pepperoniTopping.GetComponent<Collider2D>().enabled = false;
    }

    private void UpdateIconOpacityForTopping(GameObject topping)
    {
        // Update the icon opacity based on the collider enabled status
        if (topping == sauceTopping)
        {
            questManager.UpdateIconOpacity(questManager.sauceIcon, topping.GetComponent<Collider2D>().enabled);
        }
        else if (topping == cheeseTopping)
        {
            questManager.UpdateIconOpacity(questManager.cheeseIcon, topping.GetComponent<Collider2D>().enabled);
        }
        else if (topping == mushroomTopping)
        {
            questManager.UpdateIconOpacity(questManager.toppingsIcon, topping.GetComponent<Collider2D>().enabled);
        }
        else if (topping == pepperoniTopping)
        {
            questManager.UpdateIconOpacity(questManager.pepperoniIcon, topping.GetComponent<Collider2D>().enabled);
        }
    }

}
