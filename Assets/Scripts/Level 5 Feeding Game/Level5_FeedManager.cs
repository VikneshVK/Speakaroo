using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public enum AnimalType
{
    None,
    Hippo,
    Croc,
    Panda,
    Lion,
    Monkey,
    Tiger
}

public class Level5_FeedManager : MonoBehaviour
{
    [Header("Drag Reference")]
    public Food_Feed_Dragger m_food_Feed_Dragger;

    [Header("Animal States")]
    public AnimalType currentAnimal = AnimalType.None;

    [Header("Hippo's References")]
    public GameObject Hippo_Rig;
    public GameObject HippoCorrectFood;
    public GameObject HippoWrongFood;
    public Vector3 HippoCorrectFoodTargetPosition; 
    public Vector3 HippoWrongFoodTargetPosition;
    public GameObject HippoDropZone;  // Hippo's drop zone with BoxCollider2D
    [SerializeField]
    private bool isHippoFinishedEating;


    [Header("Crocs's References")]
    public GameObject Croc_Rig;
    public GameObject CrocCorrectFood;
    public GameObject CorcWrongFood;
    [SerializeField]
    private bool isCrocFinishedEating;

    [Header("Panda's References")]
    public GameObject Panda_Rig;
    public GameObject PandaCorrectFood;
    public GameObject PandaWrongFood;
    [SerializeField]
    private bool isPandaFinishedEating;

    [Header("Lion References")]
    public GameObject Lion_Rig;
    public GameObject LionCorrectFood;
    public GameObject LionWrongFood;
    [SerializeField]
    private bool isLionFinishedEating;

    [Header("Monkey References")]
    public GameObject Monkey_Rig;
    public GameObject MonkeyCorrectFood;
    public GameObject MonkeyWrongFood;
    [SerializeField]
    private bool isMonkeyFinishedEating;

    [Header("Tiger References")]
    public GameObject Tiger_Rig;
    public GameObject TigerCorrectFood;
    public GameObject TigerWrongFood;
    [SerializeField]
    private bool isTigerFinishedEating;

    [Header("End game Check")]
    [SerializeField]
    private bool allAnimalsFed;

    [Header("Game References")]
    public TextMeshProUGUI QuestText;

    [Header("Hippo Helper")]
    public GameObject HelperHand;
    public GameObject H_Blink;

    [SerializeField]
    private GameObject currentCorrectFood;
    [SerializeField]
    private GameObject currentWrongFood;
    [SerializeField]
    public GameObject currentDropZone;
    [SerializeField]
    private GameObject currentAnimalRig;

    private Vector3 wrongFoodOriginalPosition;
    private Vector3 correctFoodOrignalPosition;

    private bool isDraggingFood = false;

    private void Start()
    {
        Debug.Log("Game starts");
        QuestText.text = "Quest Starts now";

        Invoke("HippoGameplay", 2.4f);
        Debug.Log("Hippo Gameplay start");

    }

    public void HippoGameplay()
    {
       
        currentDropZone = HippoDropZone;

        currentAnimal = AnimalType.Hippo;
        currentCorrectFood = HippoCorrectFood;
        currentWrongFood = HippoWrongFood;
        currentDropZone = HippoDropZone;
        currentAnimalRig = Hippo_Rig;

        Debug.Log("the correct hippo food is" + currentCorrectFood);
        Debug.Log("The wrong hippo food is" + currentWrongFood);

        Hippo_Rig.GetComponent<Animator>().SetTrigger("Idle");

        currentAnimal = AnimalType.Hippo;
        Debug.Log("Hippo gameplay started");

        LeanTween.moveLocal(HippoCorrectFood, HippoCorrectFoodTargetPosition, 1).setEase(LeanTweenType.easeInOutQuad);

        // Move the wrong food to the target local position
        LeanTween.moveLocal(HippoWrongFood, HippoWrongFoodTargetPosition, 1).setEase(LeanTweenType.easeInOutQuad);
        wrongFoodOriginalPosition = HippoWrongFoodTargetPosition;
        correctFoodOrignalPosition = HippoCorrectFoodTargetPosition;
        Debug.Log(HippoWrongFoodTargetPosition);
        Debug.Log(HippoCorrectFoodTargetPosition);

        QuestText.text = "Feed Hippo now";

    }

    public void CrocGameplay()
    {
        Debug.Log("Fed Hippo and moving to croc");
        isHippoFinishedEating = true;
        QuestText.text = "Feed Croc now";
        Hippo_Rig.SetActive(false);
    }

    public void PandaGameplay()
    {

    }

    public void LionGameplay()
    {

    }

    public void MonkeyGameplay()
    {

    }

    public void TigerGameplay()
    {

    }

    public void CongratulatorySequence()
    {

    }

    public void EndGameSequence()
    {

    }

    public void OnFoodDragged(GameObject draggedObject)
    {
        //Debug.Log("Object is dragging");
        // This method will be called when any food is dragged.
        if (currentAnimal == AnimalType.Hippo)
        {
            
            Hippo_Rig.GetComponent<Animator>().SetBool("OpenMouth",true);
            Hippo_Rig.GetComponent<Animator>().SetBool("Sad", false);
            Hippo_Rig.GetComponent<Animator>().SetBool("Idle", false);
            isDraggingFood = true; // Set dragging state to true
            Debug.Log("Hippo's OpenMouth animation triggered.");
            
        }
        else if (currentAnimal == AnimalType.Croc)
        {
            // Trigger Croc's open mouth animation
            if (isDraggingFood) // Only trigger if not already dragging
            {
                Croc_Rig.GetComponent<Animator>().SetBool("OpenMouth",true);
                isDraggingFood = true; // Set dragging state to true
                Debug.Log("Croc's OpenMouth animation triggered.");
            }
        }
    }

    public void OnFoodDropped(GameObject foodObject)
    {
        // Reset dragging state
        isDraggingFood = false; // Set dragging state back to false

        // Check if the food dropped was correct or incorrect
        if (IsDroppedInCorrectZone(foodObject) == currentCorrectFood)
        {
            OnCorrectFoodDropped(foodObject);
        }
        else
        {
            OnIncorrectFoodDropped(foodObject);
        }
    }


    public bool IsDroppedInCorrectZone(GameObject draggedObject)
    {
        // Get the position of the food object
        Vector2 foodPosition = draggedObject.transform.position;

        // Define the ray direction (downwards)
        Vector2 rayDirection = Vector2.down;

        // Perform a raycast from the food position downwards
        RaycastHit2D hit = Physics2D.Raycast(foodPosition, rayDirection, 10f); // Increased distance

        // Check if the ray hits the drop zone
        if (hit.collider != null && hit.collider.CompareTag("DropZone_Feeding"))
        {
            Debug.Log("Raycast hit the drop zone with: " + draggedObject.name);
            Debug.Log("Dragging: " + draggedObject.name + " | Current Correct Food: " + currentCorrectFood.name);

            if (draggedObject == currentCorrectFood)
            {
                Debug.Log("Correct animal food dropped in animal's mouth.");
                return true;  // Correct food drop
            }

            Debug.Log("Food item dropped in the drop zone but it's not the correct food.");
            return false;  // It's in the drop zone but not correct
        }

        Debug.Log("Raycast missed the drop zone with: " + draggedObject.name);
        StartCoroutine(ResetFoodPosition());
        return false;  // Incorrect drop
    }



    public void OnCorrectFoodDropped(GameObject foodObject)
    {
        if (foodObject == currentCorrectFood)
        {
            Debug.Log("Correct food is Drop");

            // Set the proper animation states
            currentAnimalRig.GetComponent<Animator>().SetBool("Idle", false);
            currentAnimalRig.GetComponent<Animator>().SetBool("OpenMouth", false);
            currentAnimalRig.GetComponent<Animator>().SetBool("Sad", false); // Ensure sadness is not triggered
            currentAnimalRig.GetComponent<Animator>().SetBool("Eat", true); // Trigger the eat animation
            Debug.Log(currentAnimal + " is eating!");

            // Stop dragging state
            isDraggingFood = false;

            // Delay the next steps to ensure the eating animation plays fully
            StartCoroutine(HandlePostEatingSequence());
        }
    }

    public void OnIncorrectFoodDropped(GameObject foodObject)
    {
        if (foodObject == currentWrongFood)
        {
            Debug.Log("Wrong food is dropped");
            currentAnimalRig.GetComponent<Animator>().SetBool("Sad",true);
            Debug.Log(currentAnimal + " is sad!");
            isDraggingFood = false;
            //foodObject.SetActive(false);  // Deactivate the wrong food
            StartCoroutine(ResetFoodPosition());
        }
    }

    private void ProgressToNextAnimal()
    {
        switch (currentAnimal)
        {
            case AnimalType.Hippo:
                CrocGameplay();
                break;
            case AnimalType.Croc:
                PandaGameplay();
                break;
            // Add cases for other animals here...
            default:
                Debug.Log("All animals have been fed!");
                break;
        }
    }

    public IEnumerator ResetFoodPosition()
    {
        yield return new WaitForSeconds(1f); // Wait for a moment before resetting

        // Reset the wrong food to its original position
        LeanTween.moveLocal(HippoWrongFood, wrongFoodOriginalPosition, 1).setEase(LeanTweenType.easeInOutQuad);

        // Reset the correct food to its target position
        LeanTween.moveLocal(HippoCorrectFood, HippoCorrectFoodTargetPosition, 1).setEase(LeanTweenType.easeInOutQuad);

        // Reset the animal's state back to idle
        currentAnimalRig.GetComponent<Animator>().SetBool("Idle", true);
        currentAnimalRig.GetComponent<Animator>().SetBool("OpenMouth", false);

        Debug.Log("Correct and wrong foods have been reset to their respective positions.");
    }


    private IEnumerator HandlePostEatingSequence()
    {
        // Wait for 2 seconds to allow the eating animation to complete (adjust the time as per your animation length)
        yield return new WaitForSeconds(2f);

        // Stop eating animation and trigger the happy animation or return to idle
        currentAnimalRig.GetComponent<Animator>().SetBool("Eat", false);
        currentAnimalRig.GetComponent<Animator>().SetBool("Idle", true); // Return to idle

        // After the animal has finished eating, trigger the next gameplay
        ProgressToNextAnimal();
    }
}
