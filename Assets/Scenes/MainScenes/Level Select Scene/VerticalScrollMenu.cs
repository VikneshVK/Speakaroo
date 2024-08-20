using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VerticalScrollWithHorizontalContent : MonoBehaviour
{
    public GameObject scrollbar;
    private float scroll_pos = 0;
    private float[] pos;
    private List<Transform> scrollableContents;

    void Start()
    {
        // Filter out only the scrollable contents, skipping the TMP texts
        scrollableContents = new List<Transform>();

        foreach (Transform child in transform)
        {
            if (child.GetComponent<HorizontalLayoutGroup>() != null || child.GetComponent<ScrollRect>() != null)
            {
                scrollableContents.Add(child);
            }
        }

        // Initialize positions array based on scrollable contents
        pos = new float[scrollableContents.Count];
        float distance = 1f / (pos.Length - 1f);
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
        }

        // Center the closest content at the start
        CenterClosestContent();
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            // Capture the current scrollbar value when the mouse is being held down (dragging)
            scroll_pos = scrollbar.GetComponent<Scrollbar>().value;
        }
        else
        {
            // Snap to the closest scrollable content (skipping TMP texts)
            CenterClosestContent();
        }
    }

    void CenterClosestContent()
    {
        for (int i = 0; i < pos.Length; i++)
        {
            if (scroll_pos < pos[i] + (1f / (pos.Length - 1f) / 2) && scroll_pos > pos[i] - (1f / (pos.Length - 1f) / 2))
            {
                scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[i], 0.1f);
            }
        }
    }
}
