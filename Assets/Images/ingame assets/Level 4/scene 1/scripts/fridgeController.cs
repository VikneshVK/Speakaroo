using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fridgeController : MonoBehaviour
{
    public GameObject Boy;
    private Lvl4Sc1JojoController jojoController;
    private LVL4Sc1HelperController helperController;
    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;

    void Start()
    {
        GameObject helperHandObject = GameObject.FindGameObjectWithTag("HelperHand");
        if (helperHandObject != null)
        {
            helperController = helperHandObject.GetComponent<LVL4Sc1HelperController>();
        }
        else
        {
            Debug.Log("helperhand not found");
        }
        jojoController = Boy.GetComponent<Lvl4Sc1JojoController>();
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
    }

    void OnMouseDown()
    {
        
        if (jojoController != null && GetComponent<Collider2D>().enabled)
        {
            if (SfxAudioSource != null)
            {
                SfxAudioSource.loop = false;
                SfxAudioSource.PlayOneShot(SfxAudio1);
            }
            jojoController.OnFridgeTapped();
            helperController?.ResetTimer();
        }
    }

}
