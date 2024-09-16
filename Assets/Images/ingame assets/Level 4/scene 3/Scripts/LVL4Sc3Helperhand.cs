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
        if (helperRoutine != null)
        {
            StopCoroutine(helperRoutine);
        }
        helperRoutine = StartCoroutine(HelperHandRoutine(spawnPosition, tweenTargetPosition));
    }



    private IEnumerator HelperHandRoutine(Vector3 spawnPosition, Vector3 tweenTargetPosition)
    {

        yield return new WaitForSeconds(helperHandDelay);


        if (currentHelperHand != null)
        {
            Destroy(currentHelperHand);
        }


        currentHelperHand = Instantiate(helperHandPrefab, spawnPosition, Quaternion.identity);


        yield return new WaitForSeconds(delayBeforeTween);


        LeanTween.move(currentHelperHand, tweenTargetPosition, tweenDuration).setLoopClamp();
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
