using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class Lvl3Sc3DragManager : MonoBehaviour
{
    public int ObjectsOnLine;
    public ClothesdragHandler[] clothesHandlers;
    public Lvl3Sc3ToysdragHandler[] toysHandlers;
    public Transform[] availableHangers;
    public GameObject sun; // Reference to the sun GameObject
    public Transform sunFinalPosition; // Final position for the sun
    public GameObject sky; // Reference to the sky GameObject
    public bool levelComplete = false;
    public bool isEvening = false;

    public Jojo_action1 jojo; // Reference to Jojo's script
    public Kiki_actions kiki; // Reference to Kiki's script
    public TextMeshProUGUI subtitleText;

    // Helper hand functionality
    public Lvl3sc3HelperHand helperHand; // Reference to HelperHand script
    private int currentIndex = 0; // Tracks the current object being monitored by helper hand
    private List<GameObject> interactableObjects = new List<GameObject>(); // List of objects to interact with

    private List<Transform> activeHangers = new List<Transform>();

    private void Start()
    {
        InitializeDryClothes();
        activeHangers.AddRange(availableHangers);
        ObjectsOnLine = 6;
    }

    private void InitializeDryClothes()
    {
        int enabledCount = 0;
        foreach (var handler in clothesHandlers)
        {
            if (handler.isDry)
            {
                handler.EnableCollider();
                enabledCount++;
                if (enabledCount == 3) break;
            }
        }
    }

    public void OnClothDropped(Transform hanger)
    {
        if (hanger == null) return; // Null check to ensure the hanger still exists

        ObjectsOnLine--;
        activeHangers.Add(hanger);

        if (ObjectsOnLine == 3 && !isEvening)
        {
            StartCoroutine(HandleHelperAnimationAndAudio());
        }

        CheckLevelComplete();

        // Notify helper hand to reset and find the next object
        OnObjectInteracted();
    }

    private IEnumerator HandleHelperAnimationAndAudio()
    {
        yield return new WaitForSeconds(2f);

        if (kiki != null && kiki.GetComponent<Animator>() != null)
        {
            kiki.GetComponent<Animator>().SetTrigger("helper2");
        }

        AudioSource skyAudio = sky.GetComponent<AudioSource>();
        if (skyAudio != null)
        {
            skyAudio.Play();
        }

        yield return new WaitForSeconds(3f);

        EnableToyColliders();
        EnableHangerColliders();
        StartHelperTimerForNextObject();
    }

    private void EnableHangerColliders()
    {
        foreach (var hanger in activeHangers)
        {
            Collider2D hangerCollider = hanger.GetComponent<Collider2D>();
            if (hangerCollider != null)
            {
                hangerCollider.enabled = true;
            }
        }
    }

    private void EnableToyColliders()
    {
        foreach (var handler in toysHandlers)
        {
            handler.EnableCollider();
        }
    }

    public void OnToyDropped(Transform hanger)
    {
        ObjectsOnLine++;
        activeHangers.Remove(hanger);
        Debug.Log($"Toy dropped on hanger '{hanger.name}'. This hanger is now occupied and unavailable for other toys.");
        if (ObjectsOnLine == 6)
        {
            StartCoroutine(StartOffScreenSequence());
        }

        OnObjectInteracted();
    }

    private IEnumerator StartOffScreenSequence()
    {
        yield return new WaitForSeconds(2.5f);

        AudioSource kikiAudioSource = kiki.GetComponent<AudioSource>();
        if (kikiAudioSource != null)
        {
            kikiAudioSource.Play();
        }
        StartCoroutine(RevealTextWordByWord("In the morning, the sun will dry the clothes", 0.5f));

        Animator kikiAnimator = kiki.GetComponent<Animator>();
        if (kikiAnimator != null)
        {
            kikiAnimator.SetTrigger("1stHalf");
        }

        yield return new WaitForSeconds(4f);

        jojo.MoveOffScreen();
        kiki.MoveOffScreen();

        yield return new WaitForSeconds(3f);

        LeanTween.move(sun, sunFinalPosition.position, 10f);
        LeanTween.color(sky, new Color32(229, 137, 93, 255), 10f);

        yield return new WaitForSeconds(10f);

        isEvening = true;

        jojo.ReturnToStopPosition();
        kiki.ReturnToStopPosition();
        

        yield return new WaitForSeconds(5f);
        SetObjectsToDryAndEnable();
        StartHelperTimerForNextObject();
    }

    private void SetObjectsToDryAndEnable()
    {
        foreach (var handler in clothesHandlers)
        {
            handler.isDry = true;
            handler.EnableCollider();
        }

        foreach (var handler in toysHandlers)
        {
            handler.isDry = true;
            handler.EnableCollider();
            handler.isdropped = false;
        }
    }

    public void CheckLevelComplete()
    {
        if (ObjectsOnLine == 0)
        {
            levelComplete = true;
            helperHand.ResetHelperHand();
        }
    }
    public void RemoveClothHandler(ClothesdragHandler handler)
    {
        // Create a new list excluding the destroyed handler and update clothesHandlers
        clothesHandlers = clothesHandlers.Where(h => h != handler).ToArray();
    }
    public void EnableAllColliders()
    {
        ClothesdragHandler[] handlers = FindObjectsOfType<ClothesdragHandler>();
        foreach (var handler in handlers)
        {
            if (handler.myCollider != null && handler.isDry)
            {
                handler.myCollider.enabled = true;
            }
        }
    }


    public List<Transform> GetAvailableHangers()
    {
        foreach (var hanger in activeHangers)
        {
            Debug.Log($"Available hanger: '{hanger.name}'");
        }
        return activeHangers;
    }

    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(true);

        string[] words = fullText.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            subtitleText.text = string.Join(" ", words, 0, i + 1);
            yield return new WaitForSeconds(delayBetweenWords);
        }
        subtitleText.text = "";
    }

    // Helper hand functions

    public void StartHelperTimerForNextObject()
    {
        Debug.Log("Starting helper timer for the next object...");

        interactableObjects.Clear(); // Clear list each time the method is called

        // Add active clothes and toys handlers to interactableObjects
        foreach (var handler in clothesHandlers)
        {
            if (handler != null && handler.isActiveAndEnabled)
            {
                interactableObjects.Add(handler.gameObject);
                Debug.Log($"Added to interactableObjects: {handler.gameObject.name}");
            }
            else
            {
                Debug.Log($"Skipped adding handler: {handler?.gameObject?.name} (Null or inactive)");
            }
        }

        foreach (var handler in toysHandlers)
        {
            if (handler != null && handler.isActiveAndEnabled)
            {
                interactableObjects.Add(handler.gameObject);
                Debug.Log($"Added to interactableObjects: {handler.gameObject.name}");
            }
            else
            {
                Debug.Log($"Skipped adding handler: {handler?.gameObject?.name} (Null or inactive)");
            }
        }

        // Iterate through interactableObjects to find the first object with an enabled collider
        foreach (var obj in interactableObjects)
        {
            if (obj != null)
            {
                Debug.Log($"Checking object: {obj.name}");

                Collider2D collider = obj.GetComponent<Collider2D>();
                if (collider != null)
                {
                    Debug.Log($"Object {obj.name} has a collider component.");

                    if (collider.enabled)
                    {
                        Debug.Log($"Object {obj.name} has an active collider. Starting helper hand timer.");
                        helperHand.StartDelayTimer(obj);
                        return;
                    }
                    else
                    {
                        Debug.Log($"Collider for object {obj.name} is disabled.");
                    }
                }
                else
                {
                    Debug.Log($"Object {obj.name} does not have a Collider2D component.");
                }
            }
            else
            {
                Debug.Log("Encountered a null object in interactableObjects list.");
            }
        }

        Debug.Log("No interactable object with an active collider found.");
    }





    public void OnObjectInteracted()
    {
        helperHand.ResetHelperHand();
        StartHelperTimerForNextObject();
    }
}