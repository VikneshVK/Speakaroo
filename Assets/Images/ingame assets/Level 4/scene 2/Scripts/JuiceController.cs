using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class JuiceController : MonoBehaviour
{
    public List<string> requiredFruits = new List<string>();
    private SpriteChangeController spriteChangeController;
    public JuiceManager juiceManager;
    public BlenderController blenderController;
    private Coroutine helperHandTimerCoroutine;
    private float delay;
    
    private bool blenderInteractable = false;

    void Start()
    {
        spriteChangeController = FindObjectOfType<SpriteChangeController>();
        juiceManager = FindObjectOfType<JuiceManager>();
        DisableJarCollider();
        delay = 10;
    }

    public bool ValidateFruit(List<string> fruitsInBlender)
    {
        fruitsInBlender.Sort();
        juiceManager.requiredFruits.Sort();
        bool isValid = juiceManager.requiredFruits.SequenceEqual(fruitsInBlender);
        Debug.Log($"Validating fruits: {string.Join(", ", fruitsInBlender)}, RequiredFruits: {string.Join(", ", juiceManager.requiredFruits)}, IsValid: {isValid}");
        
        return isValid;
    }

    // Called when the player clicks the blender
    public void OnBlenderClick()
    {
        blenderController.isBlenderClicked = true;
        delay = 0;
        LVL4Sc2HelperHand.Instance.DestroySpawnedHelperHand();
        Debug.Log("Blender clicked, helper hand destroyed if active.");
    }

    // Called to start monitoring for player interaction with the blender
    public void StartBlenderInteractionTimer()
    {
        if (blenderInteractable) 
        {
            if (helperHandTimerCoroutine != null)
            {
                StopCoroutine(helperHandTimerCoroutine);
            }
            helperHandTimerCoroutine = StartCoroutine(HelperHandInteractionTimer());
        }
        
    }

    private IEnumerator HelperHandInteractionTimer()
    { 

        yield return new WaitForSeconds(delay);

        if(!blenderController.isBlenderClicked && blenderInteractable)
        {
            Vector3 outsideViewportPosition = new Vector3(-10, -10, 0); 
            Transform blenderPosition = GameObject.FindGameObjectWithTag("Blender").transform;
           
            LVL4Sc2HelperHand.Instance.SpawnAndTweenHelperHand(outsideViewportPosition, blenderPosition);
            Debug.Log("Helper hand spawned to guide the player to click the blender.");
        }
    }

    public void EnableBlenderCollider()
    {
        Collider2D blenderCollider = GameObject.FindGameObjectWithTag("Blender").GetComponent<Collider2D>();
        blenderCollider.enabled = true;
        blenderInteractable = true;
    }

    public void DisableBlenderCollider()
    {
        Collider2D blenderCollider = GameObject.FindGameObjectWithTag("Blender").GetComponent<Collider2D>();
        blenderCollider.enabled = false;
        blenderInteractable = false;
    }

    public void EnableJarCollider()
    {
        Collider2D jarCollider = GameObject.FindGameObjectWithTag("Blender_Jar").GetComponent<Collider2D>();
        jarCollider.enabled = true;
    }

    public void DisableJarCollider()
    {
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