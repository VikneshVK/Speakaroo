using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShutterController : MonoBehaviour
{
    // Reference to the audio clip and audio source for the camera sound
    public AudioClip shutterSound;
    private AudioSource audioSource;

    // Flash effect: a white image that covers the screen
    public Image flashImage;
    public float flashDuration = 0.2f; // Duration of the flash

    // The name of the screenshot file
    public string screenshotFileName = "screenshot.png";

    void Start()
    {
        // Get or add the AudioSource component
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        HandleTouchInput();
    }

    // Handle touch input to detect when the shutter sprite is touched
    void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Check if the touch began this frame
            if (touch.phase == TouchPhase.Began)
            {
                // Convert the touch position to world space
                Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                touchPosition.z = 0f; // Make sure the z-axis is 0 for 2D collision

                // Check if the touch is on the shutter GameObject
                RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);

                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    // If the shutter is touched, call the TakePhoto function
                    StartCoroutine(TakePhoto());
                }
            }
        }
    }

    // Coroutine to take a photo with flash effect and audio
    IEnumerator TakePhoto()
    {
        // Play the shutter sound effect
        if (shutterSound != null)
        {
            audioSource.PlayOneShot(shutterSound);
        }

        // Trigger the flash effect
        if (flashImage != null)
        {
            // Make the flash image fully opaque
            flashImage.color = new Color(1, 1, 1, 1);

            // Lerp the alpha of the flash image to create a fade-out effect
            float elapsedTime = 0f;
            while (elapsedTime < flashDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1, 0, elapsedTime / flashDuration);
                flashImage.color = new Color(1, 1, 1, alpha);
                yield return null;
            }

            // Ensure the flash is fully transparent after the effect
            flashImage.color = new Color(1, 1, 1, 0);
        }

        // Capture the screenshot
        ScreenCapture.CaptureScreenshot(screenshotFileName);

        // Wait for a frame to ensure the screenshot is captured before the function ends
        yield return new WaitForEndOfFrame();

        Debug.Log("Photo Taken and saved as " + screenshotFileName);
    }
}
