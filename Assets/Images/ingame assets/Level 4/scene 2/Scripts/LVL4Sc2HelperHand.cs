using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LVL4Sc2HelperHand : MonoBehaviour
{
    public static LVL4Sc2HelperHand Instance { get; private set; }

    public GameObject helperHandPrefab;
    public float delayTime = 5f;
    public TweeningController tweeningController;

    private GameObject spawnedHelperHand;
    private Coroutine delayTimerCoroutine;
    
    private JuiceManager juiceManager; // Reference to JuiceManager
    private bool isBlenderInteracted = false;
    private bool isBlenderJarInteracted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        juiceManager = FindObjectOfType<JuiceManager>(); // Initialize JuiceManager
    }

    // 1) Start the delay timer
    public void StartDelayTimer()
    {
        if (delayTimerCoroutine != null)
        {
            StopCoroutine(delayTimerCoroutine);
        }
        Debug.Log("Helper hand delay timer started.");
        delayTimerCoroutine = StartCoroutine(DelayTimerCoroutine());
    }

    private IEnumerator DelayTimerCoroutine()
    {
        yield return new WaitForSeconds(delayTime);
        Debug.Log("Helper hand delay timer finished.");

        if (tweeningController != null && tweeningController.isSecondTime)
        {
            Debug.Log("Spawning helper hand based on fruit requirements.");
            SpawnHelperHandBasedOnRequirements();
        }
        else
        {
            Debug.Log("isSecondTime is false, helper hand will not spawn.");
        }
    }

    private void SpawnHelperHandBasedOnRequirements()
    {
        if (juiceManager == null || juiceManager.requiredFruits == null || juiceManager.requiredFruits.Count == 0)
        {
            return;
        }

        // Spawn helper hand based on the required fruits
        if (juiceManager.requiredFruits.Count == 1)
        {
            Debug.Log("Spawning helper hand for single fruit.");
            SpawnAndTweenHelperHandForSingleFruit(juiceManager.requiredFruits[0]);
        }
        else if (juiceManager.requiredFruits.Count == 2)
        {
            Debug.Log("Spawning helper hand for multiple fruits.");
            StartCoroutine(SpawnAndTweenHelperHandForMultipleFruits(juiceManager.requiredFruits));
        }
    }

    private void SpawnAndTweenHelperHandForSingleFruit(string fruitTag)
    {
        // Find the fruit position based on tag
        GameObject fruit = GameObject.FindGameObjectWithTag(fruitTag);
        if (fruit != null)
        {
            Vector3 spawnPosition = fruit.transform.position;
            Transform targetPosition = GameObject.FindGameObjectWithTag("Blender").transform;
            Debug.Log($"Spawning helper hand at {spawnPosition} for fruit {fruitTag}.");
            SpawnAndTweenHelperHand(spawnPosition, targetPosition);
        }
        else
        {
            Debug.LogError($"Fruit with tag {fruitTag} not found.");
        }
    }

    private IEnumerator SpawnAndTweenHelperHandForMultipleFruits(List<string> fruitTags)
    {
        foreach (string fruitTag in fruitTags)
        {
            GameObject fruit = GameObject.FindGameObjectWithTag(fruitTag);
            if (fruit != null)
            {
                Vector3 spawnPosition = fruit.transform.position;
                Transform targetPosition = GameObject.FindGameObjectWithTag("Blender").transform;
                Debug.Log($"Spawning helper hand at {spawnPosition} for fruit {fruitTag}.");
                SpawnAndTweenHelperHand(spawnPosition, targetPosition);
                yield return new WaitForSeconds(2f); // Wait for a short duration before spawning for the next fruit
            }
            else
            {
                Debug.LogError($"Fruit with tag {fruitTag} not found.");
            }
        }
    }

    public void SpawnAndTweenHelperHand(Vector3 spawnPosition, Transform targetPosition)
    {
        if (helperHandPrefab == null || targetPosition == null) return;

        DestroySpawnedHelperHand();

        spawnedHelperHand = Instantiate(helperHandPrefab, spawnPosition, Quaternion.identity);

        LeanTween.move(spawnedHelperHand, targetPosition.position, 1f).setLoopClamp();
        Debug.Log($"Helper hand spawned at {spawnPosition}, moving to {targetPosition.position}.");
    }

    public void DestroySpawnedHelperHand()
    {
        if (spawnedHelperHand != null)
        {
            Destroy(spawnedHelperHand);
            spawnedHelperHand = null;
            Debug.Log("Helper hand destroyed.");
        }
    }

    public void ResetAndStartDelayTimer()
    {
        if (delayTimerCoroutine != null)
        {
            StopCoroutine(delayTimerCoroutine);
        }
        StartDelayTimer();
    }

    public bool IsHelperHandActive()
    {
        return spawnedHelperHand != null;
    }

    // Call this method when the player interacts with the correct fruit
    public void OnFruitInteraction()
    {
        DestroySpawnedHelperHand();
        ResetAndStartDelayTimer();
    }

    // Call this method when the player interacts with the blender
    public void OnBlenderInteraction()
    {
        isBlenderInteracted = true;
        DestroySpawnedHelperHand();
        ResetAndStartDelayTimer();
    }

    // Call this method when the player interacts with the blender jar
    public void OnBlenderJarInteraction()
    {
        isBlenderJarInteracted = true;
        DestroySpawnedHelperHand();
        ResetAndStartDelayTimer();
    }
}
