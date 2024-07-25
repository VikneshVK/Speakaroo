using System.Collections;
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

    private Collider2D hoseCollider;
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
            dragScript = hose.GetComponent<DragScript>();
        }
    }

    private void OnMouseDown()
    {
        if (waterParticleSystem != null)
        {
            if (isFirstTime)
            {
                StartCoroutine(PlayWaterEffectForDuration());
            }
            else
            {
                ToggleWaterEffect();
            }
        }
    }

    private IEnumerator PlayWaterEffectForDuration()
    {
        waterParticleSystem.Play();
        yield return new WaitForSeconds(duration);
        waterParticleSystem.Stop();

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
            var mainModule = waterParticleSystem.main;
            mainModule.gravityModifier = isFirstTime ? 0f : gravityModifierValue; // Set gravity modifier
            waterParticleSystem.Play();
        }
    }
}
