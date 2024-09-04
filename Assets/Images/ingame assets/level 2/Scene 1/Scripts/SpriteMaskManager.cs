using UnityEngine;

public class SpriteMaskManager : MonoBehaviour
{
    // Singleton instance
    public static SpriteMaskManager Instance { get; private set; }

    private GameObject leftEyeMask;
    private GameObject rightEyeMask;

    private void Awake()
    {
        // Ensure there is only one instance of SpriteMaskManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keeps the object alive across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        
        GameObject eyelidsL = GameObject.Find("eyelids l");
        GameObject eyelidsR = GameObject.Find("eyelids R");

        
        if (eyelidsL != null)
        {
            leftEyeMask = eyelidsL.transform.Find("Sprite Mask").gameObject;
        }
        if (eyelidsR != null)
        {
            rightEyeMask = eyelidsR.transform.Find("Sprite Mask").gameObject;
        }
    }

    public void DeactivateMasks()
    {        
        if (leftEyeMask != null) leftEyeMask.SetActive(false);
        if (rightEyeMask != null) rightEyeMask.SetActive(false);
    }

    public void ActivateMasks()
    {        
        if (leftEyeMask != null) leftEyeMask.SetActive(true);
        if (rightEyeMask != null) rightEyeMask.SetActive(true);
    }
}
