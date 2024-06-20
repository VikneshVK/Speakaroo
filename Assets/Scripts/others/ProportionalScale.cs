using UnityEngine;

public class ProportionalScale : MonoBehaviour
{
    // Reference resolution you designed your game for (e.g., 1920x1080)
    public Vector2 referenceResolution = new Vector2(1920, 1080);

    // Original scale at the reference resolution
    public Vector3 originalScale;

    private void Start()
    {
        // Save the original scale of the GameObject
        originalScale = transform.localScale;

        // Scale the GameObject proportionally based on the current screen resolution
        ScaleGameObject();
    }

    private void Update()
    {
        // Scale the GameObject proportionally based on the current screen resolution
        ScaleGameObject();
    }

    private void ScaleGameObject()
    {
        // Get the current screen resolution
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Calculate the aspect ratio of the reference resolution and the current screen
        float referenceAspectRatio = referenceResolution.x / referenceResolution.y;
        float currentAspectRatio = screenWidth / screenHeight;

        // Calculate the scaling factor relative to the reference resolution
        float scaleFactorX = screenWidth / referenceResolution.x;
        float scaleFactorY = screenHeight / referenceResolution.y;

        // Adjust scaling factor to maintain aspect ratio
        float scaleFactor;
        if (currentAspectRatio > referenceAspectRatio)
        {
            // Screen is wider than the reference aspect ratio
            scaleFactor = scaleFactorY * 0.9f;
        }
        else
        {
            // Screen is taller or same aspect ratio as the reference
            scaleFactor = scaleFactorX * 0.9f;
        }

        // Apply the proportional scaling factor to the original scale
        transform.localScale = new Vector3(
            originalScale.x * scaleFactor,
            originalScale.y * scaleFactor,
            originalScale.z * scaleFactor
        );
    }
}
