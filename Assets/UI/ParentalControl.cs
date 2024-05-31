using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParentalControl : MonoBehaviour
{
    public Slider sessionTimeSlider; // Reference to the Slider UI element
    public sceneManager sceneManager; // Reference to the scene manager script
    public TMP_Text sliderValue;

    // Start is called before the first frame update
    void Start()
    {
        // Set min and max values for the slider
        sessionTimeSlider.minValue = 0;
        sessionTimeSlider.maxValue = 4;

        // Ensure the sessionTimeSlider is initialized and its value reflects the current session time
        if (sessionTimeSlider != null && sceneManager != null)
        {
            UpdateSliderValueText();
            sceneManager.sessionTime = GetSessionTimeFromSliderValue(sessionTimeSlider.value);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update the session time variable in the scene manager based on the Slider value
        if (sessionTimeSlider != null && sceneManager != null)
        {
            sceneManager.sessionTime = GetSessionTimeFromSliderValue(sessionTimeSlider.value);
            UpdateSliderValueText();
        }
    }

    // Update the text displaying the slider value
    void UpdateSliderValueText()
    {
        int sliderValueInt = Mathf.RoundToInt(sessionTimeSlider.value);
        int sessionTime = GetSessionTimeFromSliderValue(sliderValueInt);
        int minutes = sessionTime / 60;
        int seconds = sessionTime % 60;

        if (minutes > 0 && seconds > 0)
        {
            sliderValue.text = $"{minutes} mins {seconds} secs";
        }
        else if (minutes > 0)
        {
            sliderValue.text = $"{minutes} mins";
        }
        else
        {
            sliderValue.text = $"{seconds} secs";
        }
    }

    // Map slider value to session time
    int GetSessionTimeFromSliderValue(float sliderValue)
    {
        switch (Mathf.RoundToInt(sliderValue))
        {
            case 0:
                return 0;
            case 1:
                return 2;
            case 2:
                return 600; // 10 mins = 10 * 60 secs
            case 3:
                return 1200; // 20 mins = 20 * 60 secs
            case 4:
                return 1800; // 30 mins = 30 * 60 secs
            default:
                return 0;
        }
    }
}
