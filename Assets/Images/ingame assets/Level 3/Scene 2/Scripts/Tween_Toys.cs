using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tween_Toys : MonoBehaviour
{
    public GameObject teddy;
    public GameObject dino;
    public GameObject bunny;
    public GameObject basket;
    public GameObject pipe;

    public Transform teddyFinalPosition;
    public Transform dinoFinalPosition;
    public Transform bunnyFinalPosition;
    public Transform basketFinalPosition;

    private TapControl tapControl;
    private bool hasTweened = false;

    void Start()
    {
        tapControl = pipe.GetComponent<TapControl>();

        // Initially disable the colliders of the GameObjects
        DisableColliders(teddy);
        DisableColliders(dino);
        DisableColliders(bunny);
        DisableColliders(basket);
    }

    void Update()
    {
        if (!tapControl.isFirstTime && !hasTweened)
        {
            TweenGameObjects();
            EnableColliders(teddy);
            EnableColliders(dino);
            EnableColliders(bunny);
            EnableColliders(basket);
            hasTweened = true;
        }
    }

    private void TweenGameObjects()
    {
        LeanTween.move(teddy, teddyFinalPosition.position, 1f).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.move(dino, dinoFinalPosition.position, 1f).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.move(bunny, bunnyFinalPosition.position, 1f).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.move(basket, basketFinalPosition.position, 1f).setEase(LeanTweenType.easeInOutQuad);
    }

    private void DisableColliders(GameObject obj)
    {
        Collider2D collider = obj.GetComponent<Collider2D>();
        collider.enabled = false;
    }

    private void EnableColliders(GameObject obj)
    {
        Collider2D collider = obj.GetComponent<Collider2D>();
        collider.enabled = true;
    }
}
