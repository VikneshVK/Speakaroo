using System.Collections;
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
    private int currentToyIndex = 0;
    private bool hasTweened = false;

    void Start()
    {
        tapControl = pipe.GetComponent<TapControl>();
        DisableAllColliders();
    }

    void Update()
    {
        if (!tapControl.isFirstTime && !hasTweened)
        {
            TweenGameObjects();
            hasTweened = true;
        }
    }

    private void TweenGameObjects()
    {
        LeanTween.move(teddy, teddyFinalPosition.position, 1f).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.move(dino, dinoFinalPosition.position, 1f).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.move(bunny, bunnyFinalPosition.position, 1f).setEase(LeanTweenType.easeInOutQuad);
        LeanTween.move(basket, basketFinalPosition.position, 1f).setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() =>
                {
                    EnableColliderForCurrentToy();
                    FindObjectOfType<Helper_PointerController>().EnableHelperHandForToy(currentToyIndex);
                });
    }

    private void DisableAllColliders()
    {
        DisableCollider(teddy);
        DisableCollider(dino);
        DisableCollider(bunny);
        DisableCollider(basket);
    }

    private void EnableColliderForCurrentToy()
    {
        DisableAllColliders();

        switch (currentToyIndex)
        {
            case 0:
                Debug.Log("Enabling Teddy’s collider.");
                EnableCollider(teddy);
                break;
            case 1:
                Debug.Log("Enabling Dino’s collider.");
                EnableCollider(dino);
                break;
            case 2:
                Debug.Log("Enabling Bunny’s collider.");
                EnableCollider(bunny);
                break;
        }
    }


    private void DisableCollider(GameObject obj)
    {
        Collider2D collider = obj.GetComponent<Collider2D>();
        collider.enabled = false;
    }

    private void EnableCollider(GameObject obj)
    {
        Collider2D collider = obj.GetComponent<Collider2D>();
        collider.enabled = true;
    }

    public void MoveToNextToy()
    {
        currentToyIndex++;
        if (currentToyIndex < 3)
        {
            EnableColliderForCurrentToy();
            FindObjectOfType<Helper_PointerController>().EnableHelperHandForToy(currentToyIndex);
        }
    }
}
