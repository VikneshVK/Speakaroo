using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fridgeController : MonoBehaviour
{
    public GameObject Boy;
    private Lvl4Sc1JojoController jojoController;
    private LVL4Sc1HelperController helperController;

    void Start()
    {
        GameObject helperHandObject = GameObject.FindGameObjectWithTag("HelperHand");
        if (helperHandObject != null)
        {
            helperController = helperHandObject.GetComponent<LVL4Sc1HelperController>();
        }
        else
        {
            Debug.Log("helperhand not found");
        }
        jojoController = Boy.GetComponent<Lvl4Sc1JojoController>();
    }

    void OnMouseDown()
    {
        
        if (jojoController != null && GetComponent<Collider2D>().enabled)
        {
            jojoController.OnFridgeTapped();
            helperController?.ResetTimer();
        }
    }

}
