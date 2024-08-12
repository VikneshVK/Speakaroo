using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class JuiceController : MonoBehaviour
{
    public List<string> requiredFruits = new List<string>();
    private SpriteChangeController spriteChangeController;
    public JuiceManager juiceManager;

    void Start()
    {
        spriteChangeController = FindObjectOfType<SpriteChangeController>();
        juiceManager = FindObjectOfType<JuiceManager>();
        DisableJarCollider();
    }

    public bool ValidateFruit(List<string> fruitsInBlender)
    {
        fruitsInBlender.Sort();
        juiceManager.requiredFruits.Sort();
        bool isValid = juiceManager.requiredFruits.SequenceEqual(fruitsInBlender);
        Debug.Log($"Validating fruits: {string.Join(", ", fruitsInBlender)}, RequiredFruits: {string.Join(", ", juiceManager.requiredFruits)}, IsValid: {isValid}");
        return isValid;
    }

    public void EnableBlenderCollider()
    {
        // Enable blender collider for clicking
        Collider2D blenderCollider = GameObject.FindGameObjectWithTag("Blender").GetComponent<Collider2D>();
        blenderCollider.enabled = true;
    }

    public void DisableBlenderCollider()
    {
        // Disable blender collider after clicking
        Collider2D blenderCollider = GameObject.FindGameObjectWithTag("Blender").GetComponent<Collider2D>();
        blenderCollider.enabled = false;
    }

    public void EnableJarCollider()
    {
        // Enable jar collider for dragging
        Collider2D jarCollider = GameObject.FindGameObjectWithTag("Blender_Jar").GetComponent<Collider2D>();
        jarCollider.enabled = true;
    }

    public void DisableJarCollider()
    {
        // Disable jar collider after dragging
        Collider2D jarCollider = GameObject.FindGameObjectWithTag("Blender_Jar").GetComponent<Collider2D>();
        jarCollider.enabled = false;
    }

    public void TriggerBlenderAnimation(string fruitTag)
    {
        Animator blenderAnimator = GameObject.FindGameObjectWithTag("Blender").GetComponent<Animator>();
        blenderAnimator.SetTrigger(fruitTag);
    }

    public void TriggerBlenderAnimationForKiki(List<string> fruitsInBlender)
    {
        fruitsInBlender.Sort();
        string animationTrigger = string.Join("", fruitsInBlender);
        Animator blenderAnimator = GameObject.FindGameObjectWithTag("Blender").GetComponent<Animator>();
        blenderAnimator.SetTrigger(animationTrigger);
    }
}