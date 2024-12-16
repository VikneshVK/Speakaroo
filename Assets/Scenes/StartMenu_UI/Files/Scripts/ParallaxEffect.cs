using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    public RectTransform scrollContent; // Reference to the ScrollView content
    public float parallaxFactor = 0.5f; // Adjust for parallax intensity (0 = no movement, 1 = same as content)

    private float contentWidth; // Total width of the content
    private Vector2 initialPosition; // Initial position of the background layer

    void Start()
    {
        // Store the initial position of the background
        initialPosition = transform.localPosition;

        // Calculate the width of the content
        contentWidth = scrollContent.rect.width;
    }

    void Update()
    {
        // Calculate the scrolling offset
        float scrollOffset = scrollContent.anchoredPosition.x;

        // Apply parallax effect
        float parallaxOffset = scrollOffset * parallaxFactor;

        // Reposition the background
        transform.localPosition = new Vector2(initialPosition.x + parallaxOffset % contentWidth, initialPosition.y);
    }
}
