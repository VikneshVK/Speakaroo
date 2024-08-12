using UnityEngine;
using System.Collections;

public class BlenderController : MonoBehaviour
{
    private JuiceController juiceController;
    private SpriteChangeController spriteChangeController;
    private SpriteRenderer jarSpriteRenderer;

    void Start()
    {
        juiceController = FindObjectOfType<JuiceController>();
        spriteChangeController = FindObjectOfType<SpriteChangeController>();
        jarSpriteRenderer = GameObject.FindGameObjectWithTag("Blender_Jar").GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        if (GetComponent<Collider2D>().enabled)
        {
            // Disable the jar's sprite renderer
            jarSpriteRenderer.enabled = false;

            // Trigger the appropriate animation
            if (juiceController.juiceManager.isKikiJuice)
            {
                juiceController.TriggerBlenderAnimationForKiki(spriteChangeController.fruitsInBlender);
            }
            else
            {
                string fruitTag = spriteChangeController.fruitsInBlender[0];
                juiceController.TriggerBlenderAnimation(fruitTag);
            }

            juiceController.DisableBlenderCollider();
            StartCoroutine(EnableJarSpriteAfterAnimation());
        }
    }

    private IEnumerator EnableJarSpriteAfterAnimation()
    {
        // Wait for the animation to complete
        yield return new WaitForSeconds(1.7f);  // Adjust based on animation length

        // Enable the jar's sprite renderer
        jarSpriteRenderer.enabled = true;
        juiceController.EnableJarCollider();
        spriteChangeController.UpdateJuiceSprite();
    }
}