using UnityEngine;
using UnityEngine.UI; // Add UI namespace for Button
using UnityEngine.Events;

public class BubblePopImage : MonoBehaviour
{
    private AudioSource audioSource;
    private Animator animator;
    private Button button; // Button component to handle click

    void Start()
    {
        // Get the necessary components
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        button = GetComponent<Button>();

        // Ensure the button is interactable
        if (button != null)
        {
            button.onClick.AddListener(OnClick);  // Add the click listener
        }
    }

    // Called when the image (button) is clicked
    public void OnClick()
    {
        Debug.Log("Bubble clicked!");
        if (audioSource != null)
        {
            audioSource.Play();
        }

        if (animator != null)
        {
            animator.SetTrigger("Pop");
        }

        Destroy(gameObject, 1f);
    }

}
