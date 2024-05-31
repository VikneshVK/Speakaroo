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
        // Ensure the sessionTimeSlider is initialized and its value reflects the current session time
        if (sessionTimeSlider != null && sceneManager != null)
        {
            UpdateSliderValueText();
            sessionTimeSlider.value = sceneManager.sessionTime;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update the session time variable in the scene manager based on the Slider value
        if (sessionTimeSlider != null && sceneManager != null)
        {
            sceneManager.sessionTime = sessionTimeSlider.value;
            UpdateSliderValueText();
        }
    }

    // Update the text displaying the slider value
    void UpdateSliderValueText()
    {
        float sessionTime = sessionTimeSlider.value;
        int minutes = Mathf.FloorToInt(sessionTime / 60);
        int seconds = Mathf.FloorToInt(sessionTime % 60);

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
}
