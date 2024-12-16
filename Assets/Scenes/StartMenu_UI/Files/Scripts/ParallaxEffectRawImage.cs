using UnityEngine;
using UnityEngine.UI;

public class ParallaxEffectRawImage : MonoBehaviour
{
    public RectTransform scrollContent; // Reference to the ScrollView content
    public float parallaxFactor = 0.5f; // Adjust for parallax intensity (0 = no movement, 1 = same as content)

    private RawImage rawImage; // Reference to the RawImage component
    private Vector2 initialUVOffset; // Initial UV offset

    void Start()
    {
        // Get the RawImage component
        rawImage = GetComponent<RawImage>();
        if (rawImage == null)
        {
            Debug.LogError("ParallaxEffectRawImage: No RawImage component found!");
            enabled = false;
            return;
        }

        // Store the initial UV offset
        initialUVOffset = rawImage.uvRect.position;
    }

    void Update()
    {
        // Calculate the scroll offset (invert the effect by using negative multiplication)
        float scrollOffset = scrollContent.anchoredPosition.x;
        float uvOffsetX = initialUVOffset.x - (scrollOffset * parallaxFactor) / scrollContent.rect.width;

        // Apply the UV offset back to the RawImage
        rawImage.uvRect = new Rect(new Vector2(uvOffsetX, initialUVOffset.y), rawImage.uvRect.size);
    }
}
