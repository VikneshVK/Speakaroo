using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleWithScreenSize_Collider : MonoBehaviour
{
    private Vector2 originalSize;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSize = spriteRenderer.bounds.size;
        AdjustScale();
    }

    void Update()
    {
        AdjustScale();
    }

    void AdjustScale()
    {
        Vector2 screenDimensions = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        Vector3 newScale = transform.localScale;
        newScale.x = screenDimensions.x / originalSize.x;
        newScale.y = screenDimensions.y / originalSize.y;

        float aspectRatio = Mathf.Min(newScale.x, newScale.y);
        newScale.x = aspectRatio;
        newScale.y = aspectRatio;

        transform.localScale = newScale;

        // Adjust the collider size if needed
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            if (collider is BoxCollider2D)
            {
                BoxCollider2D boxCollider = (BoxCollider2D)collider;
                boxCollider.size = new Vector2(originalSize.x * newScale.x, originalSize.y * newScale.y);
            }
            // Add similar adjustments for other collider types if necessary
        }
    }

}
