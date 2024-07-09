using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TapController : MonoBehaviour
{
    public ParticleSystem showerParticleSystem; // Reference to the shower particle system
    public Animator tapAnimator; // Reference to the tap animator
    public string tapOnTrigger = "TapOn"; // Trigger name for turning on the tap
    public string tapOffTrigger = "TapOff"; // Trigger name for turning off the tap
    public ShowerController showerController; // Reference to the ShowerController
    public GameObject showerScreen; // Reference to the shower screen
    public Transform showerScreenTargetPosition; // Target position for the shower screen
    private bool isInteractable = true; // Whether the tap is interactable

    private void OnMouseDown()
    {
        if (isInteractable)
        {
            if (showerParticleSystem.isPlaying)
            {
                HandleTapOffInteraction();
            }
            else
            {
                StartCoroutine(HandleTapOnInteraction());
            }
        }
    }

    private IEnumerator HandleTapOnInteraction()
    {
        isInteractable = false; // Make the tap non-interactable
        tapAnimator.SetTrigger(tapOnTrigger); // Trigger the tap on animation
        yield return new WaitForSeconds(tapAnimator.GetCurrentAnimatorStateInfo(0).length); // Wait for the animation to finish

        showerParticleSystem.gameObject.SetActive(true);
        showerParticleSystem.Play(); // Turn on the particle system

        yield return StartCoroutine(showerController.DestroyFoamObjects()); // Wait for all foam objects to be destroyed

        isInteractable = true; // Make the tap interactable again
    }

    private void HandleTapOffInteraction()
    {
        StartCoroutine(TurnOffShower());
    }

    private IEnumerator TurnOffShower()
    {
        isInteractable = false; // Make the tap non-interactable
        tapAnimator.SetTrigger(tapOffTrigger); // Trigger the tap off animation
        yield return new WaitForSeconds(tapAnimator.GetCurrentAnimatorStateInfo(0).length); // Wait for the animation to finish

        // Tween the position of the showerScreen to its target position
        

        yield return new WaitForSeconds(1f); // Wait for the tween to complete

        showerParticleSystem.Stop();
        showerParticleSystem.gameObject.SetActive(false); // Turn off the particle system
        Scene_Manager sceneManager = GameObject.Find("Scene_Manager").GetComponent<Scene_Manager>();
        if (sceneManager != null)
        {
            sceneManager.LoadLevel("Scene 3");
        }
        else
        {
            Debug.LogError("SceneManager not found or Scene_Manager script not attached.");
        }
        /*  LeanTween.move(showerScreen, showerScreenTargetPosition.position, 1f).setEase(LeanTweenType.easeInOutQuad);*/

        isInteractable = true; // Make the tap interactable again
    }
}
