using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LVL4Sc2HelperHand : MonoBehaviour
{
    public static LVL4Sc2HelperHand Instance { get; private set; }

    public GameObject helperHandPrefab;
    public GameObject glowPrefab;
    public float delayTime; // Total delay time
    public TweeningController tweeningController;

    private GameObject spawnedHelperHand;
    private GameObject spawnedGlow;
    private Coroutine delayTimerCoroutine;
    private JuiceManager juiceManager;

    // Track collected status of required fruits
    private Dictionary<string, bool> requiredFruitStatus = new Dictionary<string, bool>();

    

    private void Start()
    {
        juiceManager = FindObjectOfType<JuiceManager>();
        
    }

    public void InitializeRequiredFruitStatus()
    {
        requiredFruitStatus.Clear();
        if (tweeningController.isSecondTime) 
        {
            foreach (string fruit in juiceManager.requiredFruits)
            {
                requiredFruitStatus[fruit] = false; // Set each required fruit as not collected
                Debug.Log("Initialized required fruit: " + fruit + " as uncollected.");
            }
        }
        
    }


    public void StartDelayTimer()
    {
        if (delayTimerCoroutine != null)
        {
            StopCoroutine(delayTimerCoroutine);
        }
        delayTimerCoroutine = StartCoroutine(DelayTimerCoroutine());
        Debug.Log("Timer started.");
    }

    private IEnumerator DelayTimerCoroutine()
    {
        yield return new WaitForSeconds(delayTime / 2);

        // Spawn glow on the required fruit
        string nextTarget = GetCurrentTarget();
        if (!string.IsNullOrEmpty(nextTarget))
        {
            Debug.Log("Attempting to spawn and animate glow for: " + nextTarget);
            yield return StartCoroutine(SpawnAndAnimateGlow(nextTarget));
        }
        else
        {
            Debug.Log("No target found for glow.");
        }

        yield return new WaitForSeconds(delayTime / 2);
        Debug.Log("Timer ended.");

        // After timer, spawn helper hand on the next target
        SpawnHelperHandForNextTarget();
    }


    private string GetCurrentTarget()
    {
        if (juiceManager.requiredFruits.Count > 0)
        {
            foreach (string fruit in juiceManager.requiredFruits)
            {
                if (requiredFruitStatus.ContainsKey(fruit) && !requiredFruitStatus[fruit])
                {
                    Debug.Log("Next target for glow: " + fruit);
                    return fruit; // Return the first uncollected required fruit
                }
            }
        }
        Debug.Log("No more fruits to collect.");
        return null; // No more fruits to collect
    }


    private void SpawnHelperHandForNextTarget()
    {
        
        string nextTarget = GetCurrentTarget();

        if (string.IsNullOrEmpty(nextTarget))
            return;

        GameObject fruitObject = GameObject.FindGameObjectWithTag(nextTarget);
        if (fruitObject == null)
        {
            Debug.Log("Fruit with tag '" + nextTarget + "' not found.");
            return;
        }
        Vector3 spawnPosition = fruitObject.transform.position; // Get spawn position from fruit's location

        GameObject blenderObject = GameObject.FindGameObjectWithTag("Blender");
        if (blenderObject == null)
        {
            Debug.Log("Blender not found in the scene.");
            return;
        }
        Transform targetPosition = blenderObject.transform; 

        Debug.Log("Spawning helper hand from " + nextTarget + " to Blender.");
        SpawnAndTweenHelperHand(spawnPosition, targetPosition);
    }


    private IEnumerator SpawnAndAnimateGlow(string fruitTag)
    {
        if (glowPrefab == null)
        {
            Debug.Log("Glow prefab is not assigned.");
            yield break;
        }

        GameObject fruit = GameObject.FindGameObjectWithTag(fruitTag);
        if (fruit != null)
        {
            Debug.Log("Fruit found for glow: " + fruitTag);
            spawnedGlow = Instantiate(glowPrefab, fruit.transform.position, Quaternion.identity);
            spawnedGlow.transform.localScale = Vector3.zero;

            if (spawnedGlow != null)
            {
                LeanTween.scale(spawnedGlow, Vector3.one * 15, 1f).setOnComplete(() =>
                {
                    Debug.Log("Glow scale up completed for: " + fruitTag);
                });
            }
                
            yield return new WaitForSeconds(2f);

            if (spawnedGlow != null) 
            {
                LeanTween.scale(spawnedGlow, Vector3.zero, 1f).setOnComplete(() =>
                {
                    Debug.Log("Glow scale down completed for: " + fruitTag);
                    Destroy(spawnedGlow);
                });
            }
            
        }
        else
        {
            Debug.Log("Fruit with tag '" + fruitTag + "' not found in the scene.");
        }
    }



    public void SpawnAndTweenHelperHand(Vector3 spawnPosition, Transform targetPosition)
    {
        if (helperHandPrefab == null || targetPosition == null) return;

        DestroySpawnedHelperHand();

        spawnedHelperHand = Instantiate(helperHandPrefab, spawnPosition, Quaternion.identity);
        LeanTween.move(spawnedHelperHand, targetPosition.position, 1f).setLoopClamp();
    }

    public void DestroySpawnedHelperHand()
    {
        if (spawnedHelperHand != null)
        {
            Destroy(spawnedHelperHand);
            spawnedHelperHand = null;
        }
        if (spawnedGlow != null)
        {
            Destroy(spawnedGlow);
            spawnedGlow = null;
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

    public void OnFruitCollected(string fruitTag)
    {
        if (requiredFruitStatus.ContainsKey(fruitTag))
        {
            requiredFruitStatus[fruitTag] = true; // Mark as collected
            DestroySpawnedHelperHand();
            Debug.Log("Fruit collected: " + fruitTag);

            // Trigger next target (next fruit if available)
            StartDelayTimer();
        }
    }

    private bool AllFruitsCollected()
    {
        return requiredFruitStatus.Values.All(collected => collected);
    }
    public void OnBlenderJarInteraction()
    {
        // If all fruits are collected, this should be the final step
        if (AllFruitsCollected())
        {
            DestroySpawnedHelperHand();
            ResetAndStartDelayTimer(); // Restart delay for next round if needed
            Debug.Log("Blender jar interaction detected, helper hand and glow reset.");
        }
    }
}
