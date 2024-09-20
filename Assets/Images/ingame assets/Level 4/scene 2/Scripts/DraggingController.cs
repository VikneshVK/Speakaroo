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
    public TweeningController tweeningController; // Reference to TweeningController
    public JuiceController juiceController;
    private bool helperTimerStarted = false; // To track if the helper hand timer has already started

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

        if (tweeningController.isSecondTime && !helperTimerStarted)
        {
            Debug.Log("Starting helper hand delay timer in DraggingController for the second time.");
            LVL4Sc2HelperHand.Instance.StartDelayTimer();
            helperTimerStarted = true; // Ensure the timer is only started once
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
            transform.rotation = Quaternion.identity;  // Reset rotation after drop
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

        Collider2D fruitCollider = GetComponent<Collider2D>();
        if (spriteChangeController.IsOverlappingBlenderJar(fruitCollider) && gameObject.tag != "Blender_Jar")
        {
            spriteChangeController.UpdateBlenderJarSprite(gameObject.tag, gameObject);
            LVL4Sc2HelperHand.Instance.DestroySpawnedHelperHand();
        }

        // Handle fruit and blender interactions based on the second time logic
        if (tweeningController.isSecondTime)
        {
            if (gameObject.CompareTag("Kiwi") || gameObject.CompareTag("SB") || gameObject.CompareTag("BB"))
            {
                juiceController.StartBlenderInteractionTimer();
            }

            // Check interaction with blender
            if (gameObject.CompareTag("Blender"))
            {
                juiceController.OnBlenderClick();
            }

            // Check interaction with blender jar
            if (gameObject.CompareTag("Blender_Jar"))
            {
                LVL4Sc2HelperHand.Instance.OnBlenderJarInteraction();
            }
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

    private void HandleDropOnGlass(Collider2D glassCollider)
    {
        GlassController glassController = glassCollider.GetComponent<GlassController>();
        if (glassController != null)
        {
            OnGlassCollision(glassController);
        }
    }

    public void OnGlassCollision(GlassController glassController)
    {
        // Only update sprites when the jar is dropped
        string glassSpriteName = spriteChangeController.GetJuiceSpriteName();
        Sprite newGlassSprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/" + glassSpriteName);
        glassController.GetComponent<SpriteRenderer>().sprite = newGlassSprite;

        spriteChangeController.ResetBlenderJarSprite();
        StartCoroutine(StartBirdTweenSequence(glassController.CompareTag("Glass2")));
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
        }
        else
        {
            juiceManager.isKikiJuice = true;
            spriteChangeController.ResetBlender();
            juiceManager.UpdateFruitRequirements(true);
        }
    }
}
