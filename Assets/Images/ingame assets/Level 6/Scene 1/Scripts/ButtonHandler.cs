using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonHandler : MonoBehaviour
{
    public AudioSource buttonAudioSource; // AudioSource attached to the button
    public GameObject childTextObject;    // The TextMeshPro child object to enable and scale

    private void Start()
    {
        // Make sure the childTextObject is initially inactive
        if (childTextObject != null)
        {
            childTextObject.SetActive(false);
            childTextObject.transform.localScale = Vector3.zero; // Set initial scale to zero
        }

        // Get the button component and add a click listener
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        // Enable the child text object and scale it to (1, 1, 1)
        if (childTextObject != null)
        {
            childTextObject.SetActive(true);
            LeanTween.scale(childTextObject, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);
        }

        // Play the audio associated with the button
        if (buttonAudioSource != null)
        {
            buttonAudioSource.Play();
        }
    }
}

