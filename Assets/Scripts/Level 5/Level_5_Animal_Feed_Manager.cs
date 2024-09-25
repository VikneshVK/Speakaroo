using UnityEngine;
using System.Collections;
using System.Linq;

public class Level_5_Animal_Feed_Manager : MonoBehaviour
{
    [Header("Hippo Gameplay References")]
    public GameObject hippo;  // Hippo GameObject
    public GameObject watermelon;  // Correct food (Watermelon)
    public GameObject fish;  // Incorrect food (Fish)

    public Transform hippoDropZone;  // Hippo's drop zone (mouth area)
    public Animal_Feed_Smooth_Drag_Handler dragHandler;

    private Animator hippoAnimator;

    private void Start()
    {
        // Set up the drag handler for the hippo test
        dragHandler.correctFood = watermelon;
        dragHandler.incorrectFood = fish;
        dragHandler.dropZone = hippoDropZone;

        // Activate the Hippo
        hippo.SetActive(true);

        // Get the Animator component from Hippo
        hippoAnimator = hippo.GetComponent<Animator>();
    }

    public void OnCorrectFoodDropped()
    {
        // Start the coroutine to handle sequential animation
        StartCoroutine(PlayCorrectFoodAnimations());
    }

    private IEnumerator PlayCorrectFoodAnimations()
    {
        hippoAnimator.SetBool("OpenMouth", false);
        // Trigger Eating animation
        hippoAnimator.SetTrigger("Eating");
        // Wait for the Eating animation to complete (assuming it's 1 second long; adjust as needed)
        yield return new WaitForSeconds(GetAnimationLength("Eating"));

        // Trigger Happy animation
        hippoAnimator.SetTrigger("Happy");
        // Wait for the Happy animation to complete (assuming it's 1 second long; adjust as needed)
        yield return new WaitForSeconds(GetAnimationLength("Happy"));

        // Set CanEnd to true
        hippoAnimator.SetBool("CanEnd", true);

        Debug.Log("Correct food (Watermelon) was dropped! Hippo is happy.");
    }

    public void OnIncorrectFoodDropped()
    {
        hippoAnimator.SetBool("CanEnd", false);
        hippoAnimator.SetTrigger("Sad");
        Debug.Log("Incorrect food (Fish) was dropped! Hippo is sad.");
        fish.SetActive(false);
        // You could play the sad animation here
    }

    public void OnFoodDragStarted()
    {
        hippoAnimator.SetBool("OpenMouth", true);
        Debug.Log("Dragging food! Hippo should open its mouth.");
        // Trigger "Open Mouth" animation here
    }

    public void OnFoodDragEnded(bool success)
    {
        if (!success)
        {
            hippoAnimator.SetBool("OpenMouth", false);
            hippoAnimator.SetTrigger("Idle");
            Debug.Log("Food wasn't dropped in the correct area. Hippo returns to idle.");
        }
    }

    private float GetAnimationLength(string animationName)
    {
        // Return the length of the animation based on its name
        AnimationClip clip = hippoAnimator.runtimeAnimatorController.animationClips
            .FirstOrDefault(c => c.name == animationName);
        return clip != null ? clip.length : 0f;
    }
}
