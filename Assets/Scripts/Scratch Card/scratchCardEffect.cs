using UnityEngine;

public class ScratchCardEffect : MonoBehaviour
{
    public GameObject maskPrefab;
    private GameObject instantiatedMask; // To keep track of the instantiated mask
    private bool timerStarted = false;
    private bool audioPlayed = false; // Flag to track if audio has been played

    void Update()
    {
        // Check if the left mouse button is pressed
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            // Check if the raycast hit this specific GameObject and it has the correct tag
            if (hit.collider != null && hit.collider.gameObject == gameObject &&
                (gameObject.tag == "Card_1" || gameObject.tag == "Card_2"))
            {
                Vector3 spawnPos = new Vector3(hit.point.x, hit.point.y, hit.collider.transform.position.z);

                if (!AlreadyHasMaskAtPoint(spawnPos, hit.collider.transform))
                {
                    instantiatedMask = Instantiate(maskPrefab, spawnPos, Quaternion.identity);
                    instantiatedMask.transform.SetParent(hit.collider.transform);

                    if (!timerStarted)
                    {
                        timerStarted = true;
                        Invoke("DisableSpriteAndPlayAudio", 5f); // Schedule the method call
                    }
                }
            }
        }
    }

    private void DisableSpriteAndPlayAudio()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        AudioSource audioSource = GetComponent<AudioSource>();

        if (sr != null)
        {
            sr.enabled = false;
        }

        if (audioSource != null && !audioPlayed && audioSource.clip != null)
        {
            audioSource.Play();
            audioPlayed = true; // Mark as played to prevent further playback
        }

        timerStarted = false; // Reset timer
    }

    private bool AlreadyHasMaskAtPoint(Vector3 point, Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.gameObject.CompareTag("Mask"))
            {
                float distance = Vector3.Distance(child.position, point);
                if (distance < 0.01f) // Adjust as necessary
                    return true;
            }
        }
        return false;
    }
}
