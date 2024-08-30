using UnityEngine;

public class DishWashingManager : MonoBehaviour
{
    public GameObject[] dirtyDishes; // Array to hold references to Bowl_1, Glass_1, etc.
    public static int dishesWashed = 0;

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
    }
}
