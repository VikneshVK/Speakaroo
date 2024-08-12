using UnityEngine;
using System.Collections;

public class DraggingController : MonoBehaviour
{
    private Vector3 offset;
    private Vector3 startPosition;
    private bool isDragging = false;
    private Vector3 originalScale;
    private SpriteChangeController spriteChangeController;
    private JuiceManager juiceManager;

    void Start()
    {
        originalScale = transform.localScale;
        spriteChangeController = FindObjectOfType<SpriteChangeController>();
        juiceManager = FindObjectOfType<JuiceManager>();
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x + offset.x, mousePosition.y + offset.y, transform.position.z);
            transform.localScale = originalScale * 1.1f;  // Scale up by 10%

            if (gameObject.tag != "Blender_Jar")
            {
                spriteChangeController.ActivateBlenderSprite(true);
            }
        }
        else
        {
            transform.localScale = originalScale;
        }
    }

    void OnMouseDown()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - mousePosition;
        startPosition = transform.position;
        isDragging = true;
    }

    void OnMouseUp()
    {
        isDragging = false;

        Collider2D glassCollider = FindOverlappingGlass();
        if (glassCollider != null)
        {
            HandleDropOnGlass(glassCollider);
            transform.position = startPosition;
            transform.rotation = Quaternion.identity;
        }
        else
        {
            transform.position = startPosition;
            transform.rotation = Quaternion.identity;
        }

        if (gameObject.tag != "Blender_Jar")
        {
            if (!gameObject.CompareTag("Kiwi") && !gameObject.CompareTag("SB") && !gameObject.CompareTag("BB"))
            {
                spriteChangeController.ResetBlender();
            }
        }

        // Pass the current object's collider to IsOverlappingBlenderJar
        Collider2D fruitCollider = GetComponent<Collider2D>();
        if (spriteChangeController.IsOverlappingBlenderJar(fruitCollider) && gameObject.tag != "Blender_Jar")
        {
            Debug.Log("IsOverlappingBlenderJar returned true and gameObject tag is: " + gameObject.tag);
            spriteChangeController.UpdateBlenderJarSprite(gameObject.tag, gameObject);
        }
        else
        {
            Debug.Log("Condition not met: IsOverlappingBlenderJar returned " + spriteChangeController.IsOverlappingBlenderJar(fruitCollider) + " or gameObject tag is: " + gameObject.tag);
        }
    }



    private Collider2D FindOverlappingGlass()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f); // Adjust the radius as needed
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Glass1") || collider.CompareTag("Glass2"))
            {
                return collider;
            }
        }
        return null;
    }

    public void OnGlassCollision(GlassController glassController)
    {
        // Rotate the jar and change the glass sprite
        if (glassController.CompareTag("Glass1"))
        {
            transform.Rotate(new Vector3(0, 0, 100)); // Rotate 100 degrees on z-axis
        }
        else if (glassController.CompareTag("Glass2"))
        {
            transform.Rotate(new Vector3(0, 0, -100)); // Rotate -100 degrees on z-axis
        }

        string glassSpriteName = spriteChangeController.GetJuiceSpriteName();
        Sprite newGlassSprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/" + glassSpriteName);
        glassController.GetComponent<SpriteRenderer>().sprite = newGlassSprite;
        Debug.Log($"Dropped on {glassController.name}, changed sprite to {glassSpriteName}");
        spriteChangeController.ResetBlenderJarSprite();
        transform.rotation = Quaternion.identity;
        StartCoroutine(StartBirdTweenSequence(glassController.CompareTag("Glass2")));
    }

    private void HandleDropOnGlass(Collider2D glassCollider)
    {
        // Notify the glass collider that it has been interacted with
        GlassController glassController = glassCollider.GetComponent<GlassController>();
        if (glassController != null)
        {
            OnGlassCollision(glassController);
        }
    }

    private IEnumerator StartBirdTweenSequence(bool isGlass2)
    {
        LeanTween.move(spriteChangeController.bird, spriteChangeController.birdEndPosition.position, 1f).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(1f);

        spriteChangeController.birdAnimator.SetTrigger("canTalk");
        yield return new WaitForSeconds(2f);

        LeanTween.move(spriteChangeController.bird, spriteChangeController.birdInitialPosition, 1f).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(1f);

        if (isGlass2 && juiceManager.isKikiJuice)
        {
            juiceManager.sceneEnded = true;
            juiceManager.fruitRequirementsText.gameObject.SetActive(false);
            Debug.Log("Scene ended. Juice making is complete.");
        }
        else
        {
            juiceManager.isKikiJuice = true;
            Debug.Log("isKikiJuice set to true");

            spriteChangeController.ResetBlender();
            juiceManager.UpdateFruitRequirements(true);
        }
    }
}