using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class Lvl6QuestManager : MonoBehaviour
{
    [Header("Items to be Tracked")]
    public int ItemstobeFound = 6;
    public int itemsFound = 0;

    [Header("Shell Game Objects")]
    public GameObject whiteShell;
    public GameObject redShell;
    public GameObject yellowShell;

    [Header("Sea Animal Game Objects")]
    public GameObject crab;
    public GameObject starfish;
    public GameObject turtle;

    [Header("Spawn Positions")]
    public Transform position1;
    public Transform position2;
    public Transform position3;
    public Transform position4;
    public Transform position5;
    public Transform position6;

    [Header("KIKI")]
    public GameObject Kiki;

    [Header("Description Canvas")]
    public GameObject descriptionCanvas;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Helper Hand")]
    public LVL6Sc2Helperhand helperhand;

    [Header("Quest Audio Clips")]
    public AudioClip whiteShellAudio;
    public AudioClip crabAudio;
    public AudioClip yellowShellAudio;
    public AudioClip starfishAudio;
    public AudioClip redShellAudio;
    public AudioClip babyTurtleAudio;

    public GameObject BlackoutPanel;

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private LVL6Sc2KikiController kikiController;
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();


    private void Start()
    {
        kikiController = Kiki.GetComponent<LVL6Sc2KikiController>();

        Debug.Log("Spawning Items. ItemstobeFound: " + ItemstobeFound);

    }

    
    public void SpawnItems()
    {
        ClearSpawnedObjects();

        kikiController.TweenBirdandback("FirstTalk");

        // Create lists for items and positions
        List<GameObject> shells = new List<GameObject> { whiteShell, redShell, yellowShell };
        List<GameObject> seaAnimals = new List<GameObject> { crab, starfish, turtle };

        List<Transform> oddPositions = new List<Transform> { position2, position4, position6 };
        List<Transform> evenPositions = new List<Transform> { position1, position3, position5 };

       
        ShuffleList(shells);
        ShuffleList(seaAnimals);

        // Spawn items based on the quest type (even or odd)
        if (ItemstobeFound % 2 == 0) 
        {
            for (int i = 0; i < evenPositions.Count; i++)
            {
                GameObject spawnedObject = Instantiate(shells[i], evenPositions[i]);
                spawnedObject.transform.localPosition = Vector3.zero; 
                spawnedObjects.Add(spawnedObject);

                originalScales[spawnedObject] = spawnedObject.transform.localScale;

                EnableSpecificCollider(evenPositions[i]);
            }
        }
        else 
        {
            for (int i = 0; i < oddPositions.Count; i++)
            {
                GameObject spawnedObject = Instantiate(seaAnimals[i], oddPositions[i]);
                spawnedObject.transform.localPosition = Vector3.zero; 
                spawnedObjects.Add(spawnedObject);

              
                originalScales[spawnedObject] = spawnedObject.transform.localScale;
                
                EnableSpecificCollider(oddPositions[i]);
            }
        }

       
        string questItemName = QuestGiver();

        
        GameObject currentQuestItem = spawnedObjects.Find(item => item.name.Contains(questItemName));

        
        AudioClip questAudio = GetQuestAudio();
        kikiController.PlayQuestAudio(questAudio);

        
        if (currentQuestItem != null)
        {
            helperhand.StartDelayTimer(currentQuestItem);
        }
        else
        {
            Debug.LogWarning("Quest item not found among spawned objects.");
        }
    }


    // Enable the collider of the specified position and disable others
    private void EnableSpecificCollider(Transform position)
    {
        BoxCollider2D collider = position.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }



    // Clears all spawned objects
    private void ClearSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            Destroy(obj);
        }
        spawnedObjects.Clear();
    }

    // Provides quest based on the ItemstobeFound variable
    public string QuestGiver()
    {
        switch (ItemstobeFound)
        {
            case 6:
                return "White_shell";
            case 5:
                return "crabe";
            case 4:
                return "Yellow_Shell";
            case 3:
                return "start-fish";
            case 2:
                return "Red_Shell";
            case 1:
                return "baby-turtle";
            default:
                return "No Quest Available";
        }
    }

    private AudioClip GetQuestAudio()
    {
        switch (ItemstobeFound)
        {
            case 6:
                return whiteShellAudio;
            case 5:
                return crabAudio;
            case 4:
                return yellowShellAudio;
            case 3:
                return starfishAudio;
            case 2:
                return redShellAudio;
            case 1:
                return babyTurtleAudio;
            default:
                return null;
        }
    }

    // Checks for changes to ItemstobeFound and updates the spawned items accordingly
    public void QuestValidation(string itemName, GameObject clickedObject)
    {
        string expectedItem = QuestGiver();

        if (itemName == expectedItem)
        {
            
            DisableAllColliders();
            EnableDescriptionCanvas();
            kikiController.TweenBirdandback("RightTalk");

            
            itemsFound++;
            ItemstobeFound--;

            
            StartCoroutine(HandleCorrectItem());
        }
        else
        {
            
            kikiController.TweenBirdandback("WrongTalk");
            
            BoxCollider2D collider = clickedObject.GetComponentInParent<BoxCollider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            
            EnableOtherColliders(clickedObject.transform);

            StartCoroutine(HandleIncorrectItem(clickedObject));
        }
    }


    // Coroutine for handling correct item
    private IEnumerator HandleCorrectItem()
    {
        yield return new WaitForSeconds(4f);

        BlackoutPanel.SetActive(false);

        yield return StartCoroutine(ResetItemPositionAndScale());

        DisableParticleEffects();

        DisableDescriptionCanvas();

        EnablePositionChildren();

        yield return new WaitForSeconds(0.5f); // Small buffer time to ensure everything is ready

        helperhand.DestroyAndResetTimer();

        if (itemsFound >= 6)
        {            
            kikiController.TweenBirdandback("FinalTalk");
        }
        else
        {            
            SpawnItems();
        }

    }

    private IEnumerator HandleIncorrectItem(GameObject clickedObject)
    {
        yield return new WaitForSeconds(3f);

        BlackoutPanel.SetActive(false);

        DisableParticleEffects();

        DisableDescriptionCanvas();

        EnablePositionChildren();

        yield return StartCoroutine(ResetItemPositionAndScale());

        EnableAllCollidersExcept(clickedObject);
    }

    private void EnableOtherColliders(Transform clickedPosition)
    {
        foreach (Transform pos in new List<Transform> { position1, position2, position3, position4, position5, position6 })
        {
            BoxCollider2D collider = pos.GetComponent<BoxCollider2D>();
            if (collider != null && pos != clickedPosition)
            {
                collider.enabled = true;
            }
        }
    }

    private void EnableAllCollidersExcept(GameObject clickedObject)
    {
        foreach (Transform pos in new List<Transform> { position1, position2, position3, position4, position5, position6 })
        {
            BoxCollider2D collider = pos.GetComponent<BoxCollider2D>();

            // Enable collider if it's not the clicked object and it's not null
            if (collider != null && pos.gameObject != clickedObject)
            {
                collider.enabled = true;
            }
        }
    }

    public void DisableOtherColliders(Transform clickedPosition)
    {
        foreach (Transform pos in new List<Transform> { position1, position2, position3, position4, position5, position6 })
        {
            BoxCollider2D collider = pos.GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.enabled = (pos == clickedPosition);
            }
        }
    }




    private void EnablePositionChildren()
    {
        foreach (Transform pos in new List<Transform> { position1, position2, position3, position4, position5, position6 })
        {
            // Check if the position has at least one child
            if (pos.childCount > 0)
            {
                pos.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    private void DisableDescriptionCanvas()
    {
        descriptionCanvas.SetActive(false);
    }


    private void DisableAllColliders()
    {
        foreach (Transform pos in new List<Transform> { position1, position2, position3, position4, position5, position6 })
        {
            BoxCollider2D collider = pos.GetComponent<BoxCollider2D>();
            if (collider != null && collider.enabled)
            {
                collider.enabled = false;
            }
        }
    }



    private void EnableDescriptionCanvas()
    {
        // Assuming descriptionCanvas is a GameObject in your scene
        descriptionCanvas.SetActive(true);

        // Get the title from QuestGiver()
        string questTitle = QuestGiver();

        // Set the title and description based on the quest item
        switch (questTitle)
        {
            case "White_shell":
                titleText.text = "White Seashell";
                descriptionText.text = "A smooth white shell you might find by the shore.";
                break;
            case "crabe":
                titleText.text = "Crab";
                descriptionText.text = "A small sea creature with claws that walks sideways.";
                break;
            case "Yellow_Shell":
                titleText.text = "Yellow Seashell";
                descriptionText.text = "A bright yellow shell you can find on the beach.";
                break;
            case "start-fish":
                titleText.text = "Starfish";
                descriptionText.text = "A sea creature shaped like a star that lives in the water.";
                break;
            case "Red_Shell":
                titleText.text = "Red Seashell";
                descriptionText.text = "A red shell that's pretty and stands out in the sand.";
                break;
            case "baby-turtle":
                titleText.text = "Sea Turtle";
                descriptionText.text = "A gentle turtle that swims in the ocean.";
                break;
            default:
                titleText.text = "Unknown";
                descriptionText.text = "No description available.";
                break;
        }
    }


    private void DisableParticleEffects()
    {
        foreach (GameObject item in spawnedObjects)
        {
            ParticleSystem ps = item.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop();
            }
        }
    }

    private IEnumerator ResetItemPositionAndScale()
    {
        // Reset each item to its original position and scale with tweens
        foreach (GameObject item in spawnedObjects)
        {
            if (originalScales.TryGetValue(item, out Vector3 originalScale))
            {
                LeanTween.move(item, item.transform.parent.position, 1.0f);
                LeanTween.scale(item, originalScale, 1.0f);
            }
            else
            {
                Debug.LogWarning("Original scale not found for item: " + item.name);
            }
        }

        yield return new WaitForSeconds(1.1f);
    }




    // Utility method to shuffle a list
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}