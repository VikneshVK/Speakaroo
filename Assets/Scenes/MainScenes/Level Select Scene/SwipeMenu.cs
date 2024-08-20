using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeMenu : MonoBehaviour
{
    public GameObject scrollbar;
    private float scroll_pos = 0;
    private float[] pos;

    void Start()
    {
        // Initialize positions array
        pos = new float[transform.childCount];
        // Calculate the distance between each position
        float distance = 1f / (pos.Length - 1f);
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
        }
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
            // Snap to the closest position when the mouse is released
            for (int i = 0; i < pos.Length; i++)
            {
                if (scroll_pos < pos[i] + (1f / (pos.Length - 1f) / 2) && scroll_pos > pos[i] - (1f / (pos.Length - 1f) / 2))
                {
                    scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[i], 0.1f);
                }
            }
        }

        // Handle the scaling of the child elements
        for (int i = 0; i < pos.Length; i++)
        {
            if (scroll_pos < pos[i] + (1f / (pos.Length - 1f) / 2) && scroll_pos > pos[i] - (1f / (pos.Length - 1f) / 2))
            {
                // Scale up the currently selected element
                transform.GetChild(i).localScale = Vector2.Lerp(transform.GetChild(i).localScale, new Vector2(1f, 1f), 0.1f);

                // Scale down the other elements
                for (int a = 0; a < pos.Length; a++)
                {
                    if (a != i)
                    {
                        transform.GetChild(a).localScale = Vector2.Lerp(transform.GetChild(a).localScale, new Vector2(0.8f, 0.8f), 0.1f);
                    }
                }
            }
        }
    }
}
