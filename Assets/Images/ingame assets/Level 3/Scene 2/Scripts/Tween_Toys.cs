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
    private Helper_PointerController helperPointerController;
    private int currentToyIndex = 0;
    private bool hasTweened = false;

    void Start()
    {
        tapControl = pipe.GetComponent<TapControl>();
        helperPointerController = FindObjectOfType<Helper_PointerController>();

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
                    StartCoroutine(HandleHelperHandForCurrentToy());
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
                EnableCollider(teddy);
                break;
            case 1:
                EnableCollider(dino);
                break;
            case 2:
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

    private IEnumerator HandleHelperHandForCurrentToy()
    {
        yield return new WaitForSeconds(helperPointerController.toyDragDelay);

        GameObject currentToy = GetCurrentToy();
        drag_Toys toyScript = currentToy.GetComponent<drag_Toys>();

        if (!toyScript.IsInteracted())
        {
            // Trigger the glow effect on the current toy's position
            helperPointerController.PauseAndSpawnGlow(currentToy.transform.position);

            // Wait for the glow effect to complete (glowInstance should be null after it completes)
            while (helperPointerController.glowInstance != null)
            {
                yield return null;
            }

            // Now spawn the helper hand after the glow effect is done
            helperPointerController.SpawnHelperHand(currentToy.transform.position, false);
            helperPointerController.TweenHelperHandToParticlesPosition(currentToyIndex);
        }

        while (!toyScript.IsInteracted())
        {
            yield return null;
        }

        // Reset helper hand after interaction
        helperPointerController.ResetHelperHand();

        currentToyIndex++;
        if (currentToyIndex < 3)
        {
            EnableColliderForCurrentToy();
            StartCoroutine(HandleHelperHandForCurrentToy());
        }
    }



    private GameObject GetCurrentToy()
    {
        switch (currentToyIndex)
        {
            case 0: return teddy;
            case 1: return dino;
            case 2: return bunny;
            default: return null;
        }
    }
}
