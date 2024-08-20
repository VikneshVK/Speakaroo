using UnityEngine;
using System.Collections;

public class HelperPointerController : MonoBehaviour
{
    public GameObject helperHand; // Reference to the helper hand GameObject
    public float delayTime = 5.0f; // Time delay before the helper hand appears
    public Transform dropZone; // Drop zone position for the helper hand to move towards

    private Vector3 startPosition;
    private GameObject currentObject;
    private Coroutine helpCoroutine;

    void Start()
    {
        helperHand.SetActive(false); // Ensure the helper hand is inactive initially
    }

    public void StartHelper(GameObject interactableObject, Transform targetDropZone)
    {
        // If there's an ongoing coroutine, stop it
        if (helpCoroutine != null)
        {
            StopCoroutine(helpCoroutine);
        }

        currentObject = interactableObject;
        dropZone = targetDropZone;
        startPosition = currentObject.transform.position;

        // Start the helper hand timer
        helpCoroutine = StartCoroutine(HelperHandTimer());
    }

    private IEnumerator HelperHandTimer()
    {
        yield return new WaitForSeconds(delayTime);

        // Activate and move the helper hand before drag-and-drop interaction
        helperHand.SetActive(true);
        helperHand.transform.position = startPosition;
        LeanTween.move(helperHand, dropZone.position, 1f).setLoopPingPong(); // Tween the helper hand between start and drop position
    }

    public void OnObjectInteracted(bool correctDrop)
    {
        if (helpCoroutine != null)
        {
            StopCoroutine(helpCoroutine);
        }

        // Deactivate helper hand after interaction
        helperHand.SetActive(false);

        if (correctDrop)
        {
            // Handle the next object (you might call the StartHelper again for the next object)
        }
        else
        {
            // Restart the helper hand for the same object if the drop was incorrect
            helpCoroutine = StartCoroutine(HelperHandTimer());
        }
    }
}
