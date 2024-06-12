using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleManager : MonoBehaviour
{
    // Reference resolution (e.g., 1920x1080)
    public Vector2 referenceResolution = new Vector2(1920, 1080);

    void Start()
    {
        // Scale all child objects
        ScaleAllChildren();
    }

    void ScaleAllChildren()
    {
        // Calculate the scaling factor based on the screen resolution
        float screenRatio = (float)Screen.width / Screen.height;
        float referenceRatio = referenceResolution.x / referenceResolution.y;

        float scaleX = Screen.width / referenceResolution.x;
        float scaleY = Screen.height / referenceResolution.y;

        if (screenRatio > referenceRatio)
        {
            // Screen is wider than reference
            scaleX = scaleY;
        }
        else
        {
            // Screen is taller than reference
            scaleY = scaleX;
        }

        // Iterate over all child objects and scale them
        foreach (Transform child in transform)
        {
            ScaleObject(child, scaleX, scaleY);
        }
    }

    void ScaleObject(Transform obj, float scaleX, float scaleY)
    {
        // Save the initial scale and position
        Vector3 initialScale = obj.localScale;
        Vector3 initialPosition = obj.position;

        // Apply the scaling factor to the game object's scale
        Vector3 newScale = new Vector3(initialScale.x * scaleX, initialScale.y * scaleY, initialScale.z);
        obj.localScale = newScale;

        // Optionally, adjust the position to maintain the aspect ratio
        Vector3 newPosition = new Vector3(initialPosition.x * scaleX, initialPosition.y * scaleY, initialPosition.z);
        obj.position = newPosition;

        // Apply scaling to all children recursively
        foreach (Transform child in obj)
        {
            ScaleObject(child, scaleX, scaleY);
        }
    }
}
