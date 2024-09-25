//using UnityEngine;
//using UnityEngine.UI; // Required for working with UI elements
//using TMPro;

//public class VariableTweaker : MonoBehaviour
//{
//    public Animal_Feed_Smooth_Drag_Handler dragHandler; // Reference to the script containing public variables

//    public TMP_InputField proximityThresholdInput;
//    public TMP_InputField dragOffsetYInput;
//    public TMP_InputField dragSpeedInput; // Input field for adjusting drag speed

//    void Start()
//    {
//        // Initialize input fields with the current values
//        proximityThresholdInput.text = dragHandler.dropZoneProximityThreshold.ToString();
//        dragOffsetYInput.text = dragHandler.dragOffsetY.ToString();
//        dragSpeedInput.text = dragHandler.dragSpeed.ToString(); // Initialize drag speed input field

//        // Add listeners to detect when the input changes
//        proximityThresholdInput.onEndEdit.AddListener(OnProximityThresholdChanged);
//        dragOffsetYInput.onEndEdit.AddListener(OnDragOffsetYChanged);
//        dragSpeedInput.onEndEdit.AddListener(OnDragSpeedChanged); // Listen for drag speed changes
//    }

//    // Called when the proximity threshold value is changed
//    public void OnProximityThresholdChanged(string value)
//    {
//        float newThreshold;
//        if (float.TryParse(value, out newThreshold))
//        {
//            dragHandler.dropZoneProximityThreshold = newThreshold;
//            Debug.Log("Proximity Threshold updated to: " + newThreshold);
//        }
//        else
//        {
//            Debug.LogWarning("Invalid input for Proximity Threshold.");
//        }
//    }

//    // Called when the drag offset Y value is changed
//    public void OnDragOffsetYChanged(string value)
//    {
//        float newOffsetY;
//        if (float.TryParse(value, out newOffsetY))
//        {
//            dragHandler.dragOffsetY = newOffsetY;
//            Debug.Log("Drag Offset Y updated to: " + newOffsetY);
//        }
//        else
//        {
//            Debug.LogWarning("Invalid input for Drag Offset Y.");
//        }
//    }

//    // Called when the drag speed value is changed
//    public void OnDragSpeedChanged(string value)
//    {
//        float newDragSpeed;
//        if (float.TryParse(value, out newDragSpeed))
//        {
//            dragHandler.dragSpeed = newDragSpeed;
//            Debug.Log("Drag Speed updated to: " + newDragSpeed);
//        }
//        else
//        {
//            Debug.LogWarning("Invalid input for Drag Speed.");
//        }
//    }
//}
