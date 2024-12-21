using System.Collections;
using UnityEngine;

public class LVL4Sc3HelperHand : MonoBehaviour
{
    public GameObject helperHandPrefab; 
    public float tweenDuration = 1.0f; 
    public float delayBeforeTween = 1.0f; 
    public float helperHandDelay = 5.0f; 

    private GameObject currentHelperHand; 
    private Coroutine helperRoutine;


    public void SpawnHelperHand(Vector3 spawnPosition, Vector3 tweenTargetPosition)
    {
        Debug.Log($"Spawning helper hand at {spawnPosition}, moving to {tweenTargetPosition}");

        if (helperRoutine != null)
        {
            StopCoroutine(helperRoutine);
        }

        helperRoutine = StartCoroutine(HelperHandRoutine(spawnPosition, tweenTargetPosition));
    }



    private IEnumerator HelperHandRoutine(Vector3 spawnPosition, Vector3 tweenTargetPosition)
    {
        if (currentHelperHand != null)
        {
            Destroy(currentHelperHand);
        }

        currentHelperHand = Instantiate(helperHandPrefab, spawnPosition, Quaternion.identity);
        Debug.Log("Helper hand instantiated.");

        yield return new WaitForSeconds(delayBeforeTween);

        if (currentHelperHand != null)
        {
            LeanTween.move(currentHelperHand, tweenTargetPosition, tweenDuration).setLoopClamp();
            Debug.Log("Helper hand moving with LeanTween.");
        }
        else
        {
            Debug.LogError("Helper hand is null when attempting to move.");
        }
    }



    public void StopHelperHand()
    {
        if (helperRoutine != null)
        {
            StopCoroutine(helperRoutine);
            helperRoutine = null;
        }

        
        if (currentHelperHand != null)
        {
            Destroy(currentHelperHand);
        }
    }
}
