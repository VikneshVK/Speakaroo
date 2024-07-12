using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
 

public class ShowerController : MonoBehaviour
{
    public List<GameObject> foamObjects = new List<GameObject>(); // List to store foam objects
    public Animator hotTapAnimator, coldTapAnimator; // References to the tap animators
    public ParticleSystem showerParticles; // Reference to the water particles
    public Collider2D hotTapCollider, coldTapCollider; // Colliders for the hot and cold taps for interaction
    public Animator boyAnimator; // Reference to the boy's Animator
    public GameObject boyGameObject; // Reference to the boy GameObject

    private bool tapsOn = false; // To track if taps are currently turned on
    private Scene_Manager sceneManager;
    private bool foamDestroyed = false;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            if (hotTapCollider.OverlapPoint(mousePos2D) || coldTapCollider.OverlapPoint(mousePos2D))
            {
                OnTapClicked();
            }
        }

        // Check if the "end shine" animation is completed
        if (boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("end shine") &&
            boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            // Once the animation completes, move to the next scene
            sceneManager.LoadLevel("Level 3");
        }
    }

    public void AddFoamObject(GameObject foam)
    {
        foamObjects.Add(foam);
    }

    public void OnTapClicked()
    {
        if (!tapsOn) // Ensure taps turn on only if they are currently off
        {
            tapsOn = true;
            hotTapAnimator.SetTrigger("TapOn");
            coldTapAnimator.SetTrigger("TapOn");
            showerParticles.Play();
           

            StartCoroutine(DestroyFoamObjects());
        }
        else // If taps are already on, turn them off
        {
            tapsOn = false;
            hotTapAnimator.SetTrigger("TapOff");
            coldTapAnimator.SetTrigger("TapOff");
            showerParticles.Stop();
        }
        if (foamDestroyed && tapsOn == false )
        {
            boyAnimator.SetBool("showerDone", true);
        }
    }

    public IEnumerator DestroyFoamObjects()
    {
        yield return new WaitForSeconds(1); // Wait a moment before starting to destroy foam

        foreach (GameObject foam in foamObjects)
        {
            LeanTween.scale(foam, Vector3.zero, 0.5f).setOnComplete(() => Destroy(foam));
        }

        foamObjects.Clear();
        yield return new WaitForSeconds(0.5f); 
        foamDestroyed = true;
    }
}
