using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fridgeController : MonoBehaviour
{
    public GameObject Boy;
    private JojoController jojoController;

    void Start()
    {

        jojoController = Boy.GetComponent<JojoController>();
    }

    void OnMouseDown()
    {
        
        if (jojoController != null && GetComponent<Collider2D>().enabled)
        {
            jojoController.OnFridgeTapped();
        }
    }

}
