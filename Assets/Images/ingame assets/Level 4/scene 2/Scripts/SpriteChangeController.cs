using UnityEngine;
using System.Collections.Generic;
using System;

public class SpriteChangeController : MonoBehaviour
{
    private SpriteRenderer blenderJarSpriteRenderer;
    private List<string> fruitsInBlender = new List<string>();
    private Sprite activeBlenderSprite;  // Active while dragging
    private Collider2D blenderJarCollider;
    private Collider2D blenderCollider;
    public bool kikisJuice = false;
    public bool KikisJuice
    {
        get => kikisJuice;
        set
        {
            if (kikisJuice != value)
            {
                kikisJuice = value;
                OnKikisJuiceChanged?.Invoke(kikisJuice);
            }
        }
    }

    // Define an event to notify changes
    public event Action<bool> OnKikisJuiceChanged;

    void Start()
    {
        blenderJarSpriteRenderer = GameObject.FindGameObjectWithTag("Blender_Jar").GetComponent<SpriteRenderer>();
        blenderJarCollider = GameObject.FindGameObjectWithTag("Blender_Jar").GetComponent<Collider2D>();
        blenderCollider = GameObject.FindGameObjectWithTag("Blender").GetComponent<Collider2D>();
        LoadSprites();
    }

    private void LoadSprites()
    {
        activeBlenderSprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/blender_active");
        if (activeBlenderSprite == null)
        {
            Debug.LogError("Failed to load blender_active sprite");
        }
    }

    public void ActivateBlenderSprite()
    {
        if (activeBlenderSprite != null)
        {
            blenderJarSpriteRenderer.sprite = activeBlenderSprite;
        }
    }

    public void UpdateBlenderJarSprite(string fruitTag)
    {
        fruitsInBlender.Add(fruitTag);
        Debug.Log("Fruit added: " + fruitTag + ". Total fruits in blender: " + fruitsInBlender.Count);  // Debug log to check the number of fruits added
        string spriteName = DetermineSpriteName();
        Sprite newBlenderJarSprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/" + spriteName);
        if (newBlenderJarSprite != null)
        {
            blenderJarSpriteRenderer.sprite = newBlenderJarSprite;
            ManageColliders();  // Manage colliders based on current state
        }
        else
        {
            Debug.LogError("Sprite not found for combination: " + spriteName);
            ResetBlender();  // Reset to default if the specific sprite isn't found
        }
    }

    private string DetermineSpriteName()
    {
        fruitsInBlender.Sort();  // Sort to ensure the correct combination string
        if (fruitsInBlender.Count == 2)
        {
            return fruitsInBlender[0].ToLower() + fruitsInBlender[1] + "_blender";  // for combinations of two fruits
        }
        return fruitsInBlender[0].ToLower() + "_blender";  // for single fruit
    }

    public void ResetBlender()
    {
        Debug.Log("Resetting blender. Clearing all fruits.");  // Debug log for resetting blender
        fruitsInBlender.Clear();
        blenderJarSpriteRenderer.sprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/default_blender");
        blenderJarCollider.enabled = true; // Re-enable the jar collider
        blenderCollider.enabled = false;  // Disable the blender collider by default
    }

    private void ManageColliders()
    {
        if (KikisJuice)
        {
            if (fruitsInBlender.Count == 2)
            {
                blenderJarCollider.enabled = false;
                blenderCollider.enabled = true;
            }
        }
        else
        {
            if (fruitsInBlender.Count == 1)
            {
                blenderJarCollider.enabled = false;
                blenderCollider.enabled = true;
            }
        }
    }
}