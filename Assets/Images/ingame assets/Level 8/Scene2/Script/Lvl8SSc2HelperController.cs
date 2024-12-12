using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lvl8SSc2HelperController : MonoBehaviour
{
    public GameObject glowPrefab; // Glow prefab to spawn
    public float glowScale = 10f; // Final scale for the glow effect
    public float glowDuration = 2f; // Duration to wait before destroying the glow
    private Collider2D[] testTubeColliders;
    private GameObject SpawnedGlow;

    public void DisableAllTestTubeColliders(GameObject leftStand, GameObject rightStand)
    {
        // Disable all colliders in the left and right stands
        testTubeColliders = leftStand.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider in testTubeColliders)
        {
            collider.enabled = false;
        }

        testTubeColliders = rightStand.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider in testTubeColliders)
        {
            collider.enabled = false;
        }
    }

    public void EnableTestTubeCollider(GameObject testTube)
    {
        Collider2D collider = testTube.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }

    public void SpawnGlow(GameObject target)
    {
        SpawnedGlow = Instantiate(glowPrefab, target.transform.position, Quaternion.identity);
        LeanTween.scale(SpawnedGlow, Vector3.one * glowScale, 0.5f).setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() =>
            {
                LeanTween.scale(SpawnedGlow, Vector3.zero, 0.5f).setDelay(glowDuration);                    
            });
    }

    public void ResetGlow()
    {
        if(SpawnedGlow != null)
        {
            Destroy(SpawnedGlow);
        }
    }
}
