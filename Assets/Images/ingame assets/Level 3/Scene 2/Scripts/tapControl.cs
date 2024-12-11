using System.Collections;
using TMPro;
using UnityEngine;

public class TapControl : MonoBehaviour
{
    public ParticleSystem waterParticleSystem;
    public float duration = 10f;  // Duration to play the particle system
    public bool isFirstTime = true;
    public GameObject hose;
    public Transform targetPosition;
    public float resetDelay = 2f; // Delay before resetting position
    public float gravityModifierValue = 1f; // Gravity modifier to apply when conditions are met
    public Animator boyAnimator;
   
    public AudioClip SfxAudio1;
    public AudioSource SfxAudioSource;
    
    private Collider2D hoseCollider;
    private Collider2D tapCollider;
    private DragScript dragScript;

    private void Start()
    {
        if (waterParticleSystem != null)
        {
            waterParticleSystem.Stop();
        }

        if (hose != null)
        {
            hoseCollider = hose.GetComponent<Collider2D>();
            tapCollider = GetComponent<Collider2D>();
            dragScript = hose.GetComponent<DragScript>();
        }
        /*SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();*/
    }

    private void OnMouseDown()
    {
        if (waterParticleSystem != null)
        {
            if (isFirstTime)
            {
                tapCollider.enabled = false;
                hoseCollider.enabled = true;
                StartCoroutine(PlayWaterEffectForDuration());

                // Reset the helper hand after the tap is interacted with
                Helper_PointerController helperController = FindObjectOfType<Helper_PointerController>();
                if (helperController != null)
                {
                    helperController.ResetHelperHand();
                }
            }
            else
            {
                ToggleWaterEffect();
            }
        }
    }

    private IEnumerator PlayWaterEffectForDuration()
    {
        Debug.Log("Tap interacted, setting isFirstTime to false");
        waterParticleSystem.Play();
        if (SfxAudioSource != null)
        {
            SfxAudioSource.clip = SfxAudio1;
            SfxAudioSource.loop = true;
            SfxAudioSource.Play();
        }
        yield return new WaitForSeconds(duration);

        if (hoseCollider != null)
        {
            hoseCollider.enabled = false;
        }

        if (targetPosition != null && hose != null)
        {
            if (dragScript != null)
            {
                dragScript.canDrag = false;
                dragScript.enabled = false;
            }

            yield return new WaitForSeconds(resetDelay);

            // Move the hose (tap_gun) to the target position
            hose.transform.position = targetPosition.position;            
            // Change the gravity modifier of the water particle system here
            var mainModule = waterParticleSystem.main;
            mainModule.gravityModifier = gravityModifierValue; // Set gravity modifier
        }

        
        isFirstTime = false;
    }

    private void ToggleWaterEffect()
    {
        if (waterParticleSystem.isPlaying)
        {
            waterParticleSystem.Stop();
        }
        else
        {
            waterParticleSystem.Play();
            if (SfxAudioSource != null)
            {
                SfxAudioSource.clip = SfxAudio1;
                SfxAudioSource.loop = true;
                SfxAudioSource.Play();
            }
        }
    }

   
}
