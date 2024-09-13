using UnityEngine;

public class DishWashingManager : MonoBehaviour
{
    public GameObject[] dirtyDishes; // Array to hold references to Bowl_1, Glass_1, etc.
    public static int dishesWashed = 0;
    public bool allDishesWashed = false; // Public flag to indicate that all dishes are washed

    private static DishWashingManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void DishWashed()
    {
        dishesWashed++;
        if (dishesWashed == 6)
        {
            instance.AllDishesAreWashed();
        }
    }

    private void AllDishesAreWashed()
    {
        Debug.Log("All dishes are washed!");

        // Tween the scale of child game objects to 0, then scale the parent to 0
        foreach (GameObject dish in dirtyDishes)
        {
            // Tween each child (dish) to scale 0 over 0.5 seconds
            LeanTween.scale(dish, Vector3.zero, 0.5f).setOnComplete(() =>
            {
                // Once all children are scaled, scale the parent object itself
                LeanTween.scale(gameObject, Vector3.zero, 0.5f).setOnComplete(() =>
                {
                    DisableAllColliders(); // Disable colliders after scaling
                    allDishesWashed = true; // Set flag to true
                });
            });
        }
    }

    // Disable all colliders in the scene or related to this manager
    private void DisableAllColliders()
    {
        // Disable all colliders on the manager's attached GameObject
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        Debug.Log("All colliders disabled.");
    }
}
