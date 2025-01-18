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
    public TextMeshProUGUI subtitleText;

    [Header("Description Canvas")]
    public GameObject descriptionCanvas;
    public TextMeshProUGUI titleText;
    /*public TextMeshProUGUI descriptionText;*/

    [Header("Helper Hand")]
    public LVL6Sc2Helperhand helperhand;

    [Header("Quest Audio Clips")]
    public AudioClip whiteShellAudio;
    public AudioClip crabAudio;
    public AudioClip yellowShellAudio;
    public AudioClip starfishAudio;
    public AudioClip redShellAudio;
    public AudioClip babyTurtleAudio;
    public AudioClip rightDropAudio;
    public AudioClip wrongDropAudio;
    public AudioClip finalAudio;
    public SubtitleManager subtitleManager;

    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;

    public GameObject BlackoutPanel;

    public GameObject glowPrefab;

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private LVL6Sc2KikiController kikiController;
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();


    private void Start()
    {
        kikiController = Kiki.GetComponent<LVL6Sc2KikiController>();
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
        Debug.Log("Spawning Items. ItemstobeFound: " + ItemstobeFound);

    }
    public void SpawnItems()
    {
        ClearSpawnedObjects();

        kikiController.TriggerAnimation("Pointing");

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

                // Spawn glow effect
                SpawnGlowEffect(evenPositions[i].position);
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

                // Spawn glow effect
                SpawnGlowEffect(oddPositions[i].position);
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

    private void SpawnGlowEffect(Vector3 position)
    {
        // Instantiate the glow prefab at the specified position
        GameObject glowInstance = Instantiate(glowPrefab, position, Quaternion.identity);

        // Ensure the glow starts with its default scale
        glowInstance.transform.localScale = Vector3.zero;

        // Tween the glow to scale up, wait, fade out, and destroy
        LeanTween.scale(glowInstance, Vector3.one * 10f, 0.5f) // Scale up to 8 over 0.5 seconds
            .setOnComplete(() =>
            {
                StartCoroutine(HandleGlowFadeOut(glowInstance));
            });
    }

    private IEnumerator HandleGlowFadeOut(GameObject glowInstance)
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds before fading out

        // Fade out the glow
        SpriteRenderer glowRenderer = glowInstance.GetComponent<SpriteRenderer>();
        if (glowRenderer != null)
        {
            LeanTween.alpha(glowRenderer.gameObject, 0f, 0.5f) // Fade out over 0.5 seconds
                .setOnComplete(() =>
                {
                    Destroy(glowInstance); // Destroy the glow instance after fading out
                });
        }
        else
        {
            Destroy(glowInstance); // Fallback: destroy if no SpriteRenderer is found
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
        AudioClip selectedAudio = null;
        string subtitleText = "";

        // Set the audio and subtitle text based on the current quest item
        switch (ItemstobeFound)
        {
            case 6:
                selectedAudio = whiteShellAudio;
                subtitleText = "Lets find a White Shell";
                break;
            case 5:
                selectedAudio = crabAudio;
                subtitleText = "Lets find the Crab";
                break;
            case 4:
                selectedAudio = yellowShellAudio;
                subtitleText = "Lets find a Yellow Shell";
                break;
            case 3:
                selectedAudio = starfishAudio;
                subtitleText = "Lets find the Starfish";
                break;
            case 2:
                selectedAudio = redShellAudio;
                subtitleText = "Lets find a Red Shell";
                break;
            case 1:
                selectedAudio = babyTurtleAudio;
                subtitleText = "Lets find the Turtle";
                break;
            default:
                Debug.LogWarning("No audio clip available for the current quest.");
                return null;
        }

        // Start the subtitle coroutine with the specific subtitle text
        /*StartCoroutine(RevealTextWordByWord(subtitleText, 0.5f));*/
        subtitleManager.DisplaySubtitle(subtitleText, "Kiki", selectedAudio);

        return selectedAudio;
    }


    // Checks for changes to ItemstobeFound and updates the spawned items accordingly
    public void QuestValidation(string itemName, GameObject clickedObject)
    {
        string expectedItem = QuestGiver();

        if (itemName == expectedItem)
        {
            if (SfxAudioSource != null)
            {
                SfxAudioSource.loop = false;
                SfxAudioSource.PlayOneShot(SfxAudio1);
            }
            DisableAllColliders();
            EnableDescriptionCanvas();
            kikiController.TriggerAnimation("RightTalk");
            kikiController.PlayQuestAudio(rightDropAudio);


            itemsFound++;
            ItemstobeFound--;


            StartCoroutine(HandleCorrectItem());
        }
        else
        {

            kikiController.TriggerAnimation("WrongTalk");
            kikiController.PlayQuestAudio(wrongDropAudio);

            BoxCollider2D collider = clickedObject.GetComponentInParent<BoxCollider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

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
            kikiController.TriggerAnimation("FinalTalk");
            kikiController.PlayQuestAudio(finalAudio);
            subtitleManager.DisplaySubtitle("Playing in the sand was super fun", "Kiki", finalAudio);
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
        
        kikiController.TriggerAnimation("Pointing");
        AudioClip currentQuestAudio = GetQuestAudio();
        kikiController.PlayQuestAudio(currentQuestAudio);

        EnablePositionChildren();

        yield return StartCoroutine(ResetItemPositionAndScale());

        EnableAllCollidersExcept(clickedObject);

        string questItemName = QuestGiver();

        GameObject currentQuestItem = spawnedObjects.Find(item => item.name.Contains(questItemName));
        if (currentQuestItem != null)
        {
            helperhand.StartDelayTimer(currentQuestItem);
        }
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
                SpriteRenderer spriteRenderer = pos.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = 1.0f; // Set alpha to full opacity
                    spriteRenderer.color = color; // Assign the modified color back
                }
                else
                {
                    Debug.LogWarning("SpriteRenderer is null. Cannot reset alpha.");
                }
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
                /*descriptionText.text = "A smooth white shell you might find by the shore.";*/
                break;
            case "crabe":
                titleText.text = "Crab";
                /*descriptionText.text = "A small sea creature with claws that walks sideways.";*/
                break;
            case "Yellow_Shell":
                titleText.text = "Yellow Seashell";
                /*descriptionText.text = "A bright yellow shell you can find on the beach.";*/
                break;
            case "start-fish":
                titleText.text = "Starfish";
                /*descriptionText.text = "A sea creature shaped like a star that lives in the water.";*/
                break;
            case "Red_Shell":
                titleText.text = "Red Seashell";
                /*descriptionText.text = "A red shell that's pretty and stands out in the sand.";*/
                break;
            case "baby-turtle":
                titleText.text = "Sea Turtle";
                /*descriptionText.text = "A gentle turtle that swims in the ocean.";*/
                break;
            default:
                titleText.text = "Unknown";
                /*descriptionText.text = "No description available.";*/
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
}
