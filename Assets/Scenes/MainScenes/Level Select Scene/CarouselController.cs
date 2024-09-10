using UnityEngine;
using UnityEngine.UI;

public class CarouselController : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform content;
    public float snapSpeed = 10f;
    private bool isSnapping = false;
    private Vector2 targetPosition;

    void Update()
    {
        if (isSnapping)
        {
            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, targetPosition, Time.deltaTime * snapSpeed);
            if (Vector2.Distance(content.anchoredPosition, targetPosition) < 0.1f)
            {
                isSnapping = false;
            }
        }
    }

    public void OnScrollEnd()
    {
        // Find the closest button to the center of the scroll view
        RectTransform closestButton = null;
        float closestDistance = Mathf.Infinity;

        foreach (RectTransform button in content)
        {
            float distance = Mathf.Abs(button.anchoredPosition.x);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestButton = button;
            }
        }

        // Set target position to center the closest button
        if (closestButton != null)
        {
            targetPosition = new Vector2(-closestButton.anchoredPosition.x, content.anchoredPosition.y);
            isSnapping = true;
        }
    }
}

