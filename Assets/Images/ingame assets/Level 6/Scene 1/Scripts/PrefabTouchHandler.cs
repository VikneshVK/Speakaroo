using UnityEngine;
using System;

public class PrefabTouchHandler : MonoBehaviour
{
    private GameObject stCanvasPrefab; // Reference to the ST Canvas prefab
    private RectTransform panelToScale; // Reference to the panel inside the ST Canvas
    public Action OnPrefabTapped; // Event to handle when prefab is tapped

    // Initialize with the ST Canvas prefab and panel reference
    public void Initialize(GameObject stCanvasPrefab)
    {
        this.stCanvasPrefab = stCanvasPrefab;

        // Assuming the panel to scale is the first child of the canvas
        panelToScale = stCanvasPrefab.transform.GetChild(0).GetComponent<RectTransform>();
    }

    // Detect mouse or tap input
    private void OnMouseDown()
    {
        // Trigger the OnPrefabTapped event if it's assigned
        OnPrefabTapped?.Invoke();

        if (stCanvasPrefab != null && panelToScale != null)
        {
            // Activate the ST_Canvas
            stCanvasPrefab.SetActive(true);

            // Set the initial scale of the panel to zero before tweening it up
            panelToScale.localScale = Vector3.zero;

            // Use LeanTween to smoothly scale up the panel to (1, 1, 1) with a nice easing effect
            LeanTween.scale(panelToScale, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);
        }

        // Destroy the speech bubble prefab after interaction
        Destroy(gameObject);
    }
}
