using UnityEngine;
using System.Collections.Generic; // Required for Dictionary

public class SpeechBubble : MonoBehaviour
{
    private GameObject mechanicsPrefab;
    private DragAndDropController controller;
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();

    public void Setup(GameObject prefab, DragAndDropController dragAndDropController)
    {
        mechanicsPrefab = prefab;
        controller = dragAndDropController;
    }

    void OnMouseDown()
    {
        SpawnAndAnimateMechanicsPrefab();
        Destroy(gameObject); // Destroy the speech bubble after spawning the prefab
    }

    void SpawnAndAnimateMechanicsPrefab()
    {
        // Instantiate the mechanics prefab at the desired position (modify as needed)
        GameObject instantiatedPrefab = Instantiate(mechanicsPrefab, Vector3.zero, Quaternion.identity);

        // Save original scales and set scale to zero
        SaveAndResetScales(instantiatedPrefab);

        // Start animation of parent prefab
        LeanTween.scale(instantiatedPrefab, originalScales[instantiatedPrefab.transform], 0.5f)
                 .setEase(LeanTweenType.easeOutBack)
                 .setOnComplete(() => {
                     AnimateChildren(instantiatedPrefab.transform);
                 });
    }

    void SaveAndResetScales(GameObject root)
    {
        originalScales[root.transform] = root.transform.localScale;
        root.transform.localScale = Vector3.zero;

        foreach (Transform child in root.transform)
        {
            originalScales[child] = child.localScale;
            child.localScale = Vector3.zero;
        }
    }

    void AnimateChildren(Transform parentTransform)
    {
        foreach (Transform child in parentTransform)
        {
            if (originalScales.ContainsKey(child)) // Check if the original scale was stored
            {
                LeanTween.scale(child.gameObject, originalScales[child], 0.5f).setEase(LeanTweenType.easeOutBack);
            }
        }
    }
}
