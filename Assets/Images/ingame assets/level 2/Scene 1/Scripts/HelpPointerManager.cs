using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HelpPointerManager : MonoBehaviour
{
    public static HelpPointerManager Instance;
    public GameObject pointer; // Helper hand pointer
    private AudioSource audioSource;
    public GameObject activeHelperHand = null;

    public GameObject bus;
    public GameObject whale;
    public GameObject block;

    private float timerBus = 10f;
    private float timerWhale = 10f;
    private float timerBlock = 10f;

    private bool isHelpActive = false;
    private GameObject currentHelpObject = null;
    private Coroutine activeHelperCoroutine;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Check if colliders are enabled, and start looping the helper hand if necessary
        if (activeHelperHand == null)
        {
            if (!InteractableObject.isBusDropped && bus.GetComponent<Collider2D>().enabled)
            {
                timerBus -= Time.deltaTime;
                if (timerBus <= 0)
                {
                    activeHelperCoroutine = StartCoroutine(LoopHelperHand(bus));
                }                   
            }
            else if (!InteractableObject.isWhaleDropped && whale.GetComponent<Collider2D>().enabled && InteractableObject.isBusDropped)
            {
                timerWhale -= Time.deltaTime;
                if (timerWhale <= 0)
                {
                    activeHelperCoroutine = StartCoroutine(LoopHelperHand(whale));
                }                   
            }
            else if (!InteractableObject.isBlockDropped && block.GetComponent<Collider2D>().enabled && InteractableObject.isBusDropped && InteractableObject.isWhaleDropped)
            {
                timerBlock -= Time.deltaTime;
                if (timerBlock <= 0)
                {
                    activeHelperCoroutine = StartCoroutine(LoopHelperHand(block));
                }
                    
            }
        }
    }


    private IEnumerator LoopHelperHand(GameObject targetObject)
    {
        // Spawn the helper hand at the object's position
        activeHelperHand = Instantiate(pointer, targetObject.transform.position, Quaternion.identity);

        // Move the helper hand to the drop position and set it to loop
        Vector3 dropPosition = targetObject.GetComponent<InteractableObject>().GetDropLocation();
        LeanTween.move(activeHelperHand, dropPosition, 2f)
            .setLoopClamp()  // Loop the tween indefinitely
            .setEase(LeanTweenType.easeInOutSine);

        // Wait until the object is interacted with
        while (!targetObject.GetComponent<InteractableObject>().isInteracted)
        {
            yield return null;  // Yield control back to Unity until the condition is met
        }

        // Destroy the active helper hand after interaction is complete
        Destroy(activeHelperHand);
        activeHelperHand = null;
    }

    public void ResetTimerForObject(GameObject targetObject)
    {
        if (targetObject == bus || targetObject == whale || targetObject == block)
        {
            timerBus = 10f;
            timerWhale = 10f;
            timerBlock = 10f;
        }       
    }

    public void StopHelpPointer()
    {
        if (activeHelperHand != null)
        {
            Destroy(activeHelperHand);
            activeHelperHand = null;
        }

        if (activeHelperCoroutine != null)
        {
            StopCoroutine(activeHelperCoroutine);
            activeHelperCoroutine = null;
        }
    }
}
