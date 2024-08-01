using UnityEngine;

public class FridgeInteraction : MonoBehaviour
{
     public GameObject Boy;
     private JojoController jojoController;

    void Start()
    {
        
        jojoController = Boy.GetComponent<JojoController>();
    }

    void OnMouseDown()
    {
        // Ensure the collider is enabled and call OnFridgeTapped on JojoController
        if (jojoController != null && GetComponent<Collider2D>().enabled)
        {
            jojoController.OnFridgeTapped();
        }
    }
}
