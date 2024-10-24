
using UnityEngine;
using System;
using UnityEngine.UI;

public class PrefabTouchHandler2 : MonoBehaviour
{
    private RectTransform panelToScale; // Reference to the panel to scale
    public Action OnPrefabTapped; // Event to handle when prefab is tapped    
   

    public void Initialize(RectTransform panelToScale)
    {
        this.panelToScale = panelToScale; // Assign the panel reference
    }

    private void OnMouseDown()
    {
        OnPrefabTapped?.Invoke();

        // Change the sprite of the panel based on currentStopIndex from Lvl7Sc1JojoController
        

        if (panelToScale != null)
        {
            panelToScale.gameObject.SetActive(true);

            panelToScale.localScale = Vector3.zero;

            LeanTween.scale(panelToScale, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);
        }
        Destroy(gameObject);
    }

    // Function to change the sprite based on currentStopIndex
   
}
