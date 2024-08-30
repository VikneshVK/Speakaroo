using UnityEngine;

public class ButtonClickSound : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayClickSound()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}
