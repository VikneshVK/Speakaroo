using UnityEngine;
using System.Collections;

public class Lvl1Sc3HelperController : MonoBehaviour
{
    public GameObject glowPrefab; // Reference to the glow prefab
    public GameObject helperHandPrefab; // Reference to the helper hand prefab
    public GameObject jojo; // Reference to the Jojo game object

    private GameObject currentHelperHand; // Track the current helper hand instance
    private GameObject currentGlow; // Track the current glow instance


    public IEnumerator SpawnGlow(Vector3 spawnLocation)
    {
        if (glowPrefab == null)
        {
            Debug.LogError("Glow prefab is not assigned!");
            yield break;
        }

        GameObject currentGlow = Instantiate(glowPrefab, spawnLocation, Quaternion.identity);

        LeanTween.scale(currentGlow, Vector3.one * 10, 0.5f).setEase(LeanTweenType.easeOutBack).setOnComplete(() =>
        {
            // Wait for 2 seconds before scaling down
            LeanTween.delayedCall(1f, () =>
            {
                LeanTween.scale(currentGlow, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInBack).setOnComplete(() =>
                {
                    Destroy(currentGlow); // Destroy the glow object
                });
            });
        });

        // Total time for the tweening and waiting
        yield return new WaitForSeconds(2f);
    }


    public void SpawnHelperHand(Vector3 spawnLocation)
    {
        if (helperHandPrefab == null || jojo == null)
        {
            Debug.LogError("Helper hand prefab or Jojo reference is not assigned!");
            return;
        }

        // Destroy existing helper hand before spawning a new one
        DestroyHelperHand();

        currentHelperHand = Instantiate(helperHandPrefab, spawnLocation, Quaternion.identity);

        // Looping tween logic
        void TweenToJojo()
        {
            LeanTween.move(currentHelperHand, jojo.transform.position, 2f).setEase(LeanTweenType.linear).setOnComplete(() =>
            {
                // Teleport back to spawn location and repeat
                currentHelperHand.transform.position = spawnLocation;
                TweenToJojo();
            });
        }

        TweenToJojo();
    }

    
    public void DestroyHelperHand()
    {
        if (currentHelperHand != null)
        {
            Destroy(currentHelperHand);
            currentHelperHand = null;
        }
    }
}
