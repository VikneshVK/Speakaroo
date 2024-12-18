using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonScaleEffect : MonoBehaviour
{
    // Scale multiplier
    public float scaleMultiplier = 1.2f;

    // Duration of the scale animation
    public float animationDuration = 0.2f;

    // Public function to be called from Button's OnClick event, passing the button's GameObject
    public void ScaleButtonOnClick(GameObject clickedButton)
    {
        // Get the RectTransform of the clicked button
        RectTransform targetButtonTransform = clickedButton.GetComponent<RectTransform>();
        if (targetButtonTransform)
        {
            // Start the scale coroutine when button is clicked
            StartCoroutine(ScaleUpAndDown(targetButtonTransform));
        }
        else
        {
            Debug.LogError("Clicked object is not a Button with a RectTransform.");
        }
    }

    IEnumerator ScaleUpAndDown(RectTransform targetButtonTransform)
    {
        Vector3 initialScale = targetButtonTransform.localScale;

        // Scale up
        float elapsedTime = 0;
        Vector3 startScale = initialScale;
        Vector3 endScale = initialScale * scaleMultiplier;
        while (elapsedTime < animationDuration)
        {
            targetButtonTransform.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        targetButtonTransform.localScale = endScale; // Ensure end scale is reached

        // Scale down
        elapsedTime = 0;
        startScale = endScale;
        endScale = initialScale;
        while (elapsedTime < animationDuration)
        {
            targetButtonTransform.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        targetButtonTransform.localScale = initialScale; // Reset to initial scale
    }
}
