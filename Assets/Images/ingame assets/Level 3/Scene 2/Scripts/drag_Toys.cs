using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drag_Toys : MonoBehaviour
{
    public ParticleSystem waterParticleSystem; // Reference to the water particle system
    public float hitDuration = 2f; // Duration the object must be hit by particles to change the sprite
    public Sprite newSprite; // The new sprite to switch to

    private bool isDragging = false;
    private Vector3 offset;
    private float hitTimer = 0f;
    private bool spriteChanged = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        offset = transform.position - worldPosition;
        isDragging = true;
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
        // In this case, IsBeingHitByParticles() could be used to add additional checks
        // or logic if needed. For now, it simply indicates if the timer is running.
        return hitTimer > 0;
    }

    private void ChangeSprite()
    {
        spriteRenderer.sprite = newSprite;
        spriteChanged = true;
    }
}
