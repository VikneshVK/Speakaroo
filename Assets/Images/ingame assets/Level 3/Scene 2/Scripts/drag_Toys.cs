using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drag_Toys : MonoBehaviour
{
    public ParticleSystem waterParticleSystem; // Reference to the water particle system
    public float hitDuration = 2f; // Duration the object must be hit by particles to change the sprite
    public Transform targetTransform; // Target transform for position and rotation
    public float moveTime = 1f; // Duration for the move animation
    public float scaleTime = 0.5f; // Duration for the scale animation
    public float scaleDownFactor = 0.85f; // Scale down factor for squeaky effect
    public static int completedTweens = 0; // Counter for completed tweens

    private bool isDragging = false;
    private Vector3 offset;
    private float hitTimer = 0f;
    private bool spriteChanged = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D toysCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        toysCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        // Reset the timer if the object is not being hit by particles
        if (hitTimer > 0 && !IsBeingHitByParticles())
        {
            hitTimer = 0f;
        }
    }

    private void OnMouseDown()
    {
        if (!spriteChanged) // Allow dragging only if the sprite hasn't been changed
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            offset = transform.position - worldPosition;
            isDragging = true;
        }
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition) + offset;

            transform.position = worldPosition;
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }

    private void OnParticleCollision(GameObject other)
    {
        // Check if the collided particle system is the water particle system
        if (other.gameObject == waterParticleSystem.gameObject)
        {
            hitTimer += Time.deltaTime;

            if (hitTimer >= hitDuration && !spriteChanged)
            {
                ChangeSprite();
            }
        }
    }

    private bool IsBeingHitByParticles()
    {
        return hitTimer > 0;
    }

    private void ChangeSprite()
    {
        // Determine the sprite to load based on the tag
        string spriteName = "";
        switch (gameObject.tag)
        {
            case "Teddy":
                spriteName = "wet kuma";
                break;
            case "Dino":
                spriteName = "wet dino";
                break;
            case "Bunny":
                spriteName = "wet bunny";
                break;
        }

        // Load the sprite from the Resources/Images/Lvl 3/Scene 2 folder
        string path = $"Images/Lvl 3/Scene 2/{spriteName}";
        Sprite newSprite = Resources.Load<Sprite>(path);
        if (newSprite != null)
        {
            spriteRenderer.sprite = newSprite;
            spriteChanged = true;
            isDragging = false;
            toysCollider.enabled = false; // Disable the collider

            // LeanTween move and rotate to target position and rotation with ease out back
            LeanTween.move(gameObject, targetTransform.position, moveTime)
                     .setEase(LeanTweenType.easeOutBack)
                     .setOnComplete(() =>
                     {
                         // Scale down and then back to original for squeaky effect
                         LeanTween.scale(gameObject, transform.localScale * scaleDownFactor, scaleTime)
                                  .setEase(LeanTweenType.easeOutBack)
                                  .setOnComplete(() =>
                                  {
                                      LeanTween.scale(gameObject, transform.localScale, scaleTime)
                                               .setEase(LeanTweenType.easeOutBack)
                                               .setOnComplete(() =>
                                               {
                                                   // Increment the counter and check if the particles should be stopped
                                                   completedTweens++;
                                                   if (completedTweens >= 3)
                                                   {
                                                       waterParticleSystem.Stop();
                                                   }
                                               });
                                  });
                     });

            // Apply the target rotation
            LeanTween.rotate(gameObject, targetTransform.eulerAngles, moveTime).setEase(LeanTweenType.easeOutBack);
        }
        else
        {
            Debug.LogError("Sprite not found for tag: " + gameObject.tag);
        }
    }
}
