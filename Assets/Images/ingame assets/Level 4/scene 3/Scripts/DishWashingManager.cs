using TMPro;
using UnityEngine;
using System.Collections;   

public class DishWashingManager : MonoBehaviour
{
    public GameObject[] dirtyDishes;
    public static int dishesWashed = 0;
    public bool allDishesWashed = false;
    
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
        if (dishesWashed == 3)
        {
            instance.AllDishesAreWashed();
        }
    }

    private void AllDishesAreWashed()
    {
        Debug.Log("All dishes are washed!");

        foreach (GameObject dish in dirtyDishes)
        {
            LeanTween.scale(dish, Vector3.zero, 0.5f).setOnComplete(() =>
            {
                LeanTween.scale(gameObject, Vector3.zero, 0.5f).setOnComplete(() =>
                {
                    DisableAllColliders();
                    allDishesWashed = true; 
                    gameObject.SetActive(false);
                });
            });
        }
    }

    private void DisableAllColliders()
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        Debug.Log("All colliders disabled.");
    }

    
}
