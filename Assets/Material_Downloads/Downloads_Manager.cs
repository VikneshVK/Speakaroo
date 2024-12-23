using System.Collections.Generic;
using UnityEngine;

public class Downloads_Manager : MonoBehaviour
{
    public List<GameObject> Openbuttons; // Drag all buttons into this list in the Inspector
    public GameObject blackoutPanel;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Screen.width);
        Debug.Log(Screen.height);
        AdjustButtonPositions();
    }

    void AdjustButtonPositions()
    {
        foreach (GameObject button in Openbuttons)
        {
            RectTransform buttonRect = button.GetComponent<RectTransform>();

            // Check resolution and adjust positions
            if (Screen.width == 2732 && Screen.height == 2048) // iPad Pro 2018 resolution
            {
                buttonRect.anchoredPosition = new Vector2(-422, 280); // Example position
            }
            else if (Screen.width == 960 && Screen.height == 540) // Phone resolution
            {
                buttonRect.anchoredPosition = new Vector2(-422, 295); // Example position
            }
            else if (Screen.width == 3040 && Screen.height == 1440) // Phone resolution
            {
                buttonRect.anchoredPosition = new Vector2(-422, 265); // Example position
            }
            else if (Screen.width == 1920 && Screen.height == 1200) // Phone resolution
            {
                buttonRect.anchoredPosition = new Vector2(-422, 300); // Example position
            }

            else if (Screen.width == 1280 && Screen.height == 720) // Phone resolution
            {
                buttonRect.anchoredPosition = new Vector2(-422, 300); // Example position
            }

            else if (Screen.width == 2360 && Screen.height == 1640) // Phone resolution
            {
                buttonRect.anchoredPosition = new Vector2(-422, 175); // Example position
            }

            else if (Screen.width == 1920 && Screen.height == 1080) // Phone resolution
            {
                buttonRect.anchoredPosition = new Vector2(-422, 335); // Example position
            }

            else if (Screen.width == 2048 && Screen.height == 1536) // Phone resolution
            {
                buttonRect.anchoredPosition = new Vector2(-422, 150); // Example position
            }

            else if (Screen.width == 2160 && Screen.height == 1620) // Phone resolution
            {
                buttonRect.anchoredPosition = new Vector2(-422, 120); // Example position
            }

            else if (Screen.width == 2224 && Screen.height == 1668) // Phone resolution
            {
                buttonRect.anchoredPosition = new Vector2(-422, 130); // Example position
            }

            else if (Screen.width == 2388 && Screen.height == 1668) // Phone resolution
            {
                buttonRect.anchoredPosition = new Vector2(-422, 170); // Example position
            }

            else if (Screen.width == 1280 && Screen.height == 768) // Phone resolution
            {
                buttonRect.anchoredPosition = new Vector2(-422, 260); // Example position
            }

            else if (Screen.width == 1600 && Screen.height == 2560) // Phone resolution
            {
                buttonRect.anchoredPosition = new Vector2(-422, 250); // Example position
            }

            else if (Screen.width == 1200 && Screen.height == 2000) // Phone resolution
            {
                buttonRect.anchoredPosition = new Vector2(-422, 250); // Example position
            }

            else if (Screen.width == 2772 && Screen.height == 1240 ) // Phone resolution
            {
                buttonRect.anchoredPosition = new Vector2(-422, 476); // Example position
            }
            else
            {
                buttonRect.anchoredPosition = new Vector2(-422, 480); // Example position
            }
        }
    }

    public void TurnOnBlackout()
    {
        blackoutPanel.SetActive(true);
    }

    public void TurnOffBlackout()
    {
        blackoutPanel.SetActive(false);
    }
}

