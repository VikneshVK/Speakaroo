using UnityEngine;

public class ScaleWithScreenSize : MonoBehaviour
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
        newScale.x = (screenDimensions.x / originalSize.x)*2;
        newScale.y = (screenDimensions.y / originalSize.y)*2;

       

        float aspectRatio = Mathf.Min(newScale.x, newScale.y);
        newScale.x = aspectRatio;
        newScale.y = aspectRatio;

        transform.localScale = newScale;

        Debug.Log($"Screen Dimensions: {screenDimensions}");
        Debug.Log($"Original Size: {originalSize}");
        Debug.Log($"New Scale: {newScale}");
    }
}
