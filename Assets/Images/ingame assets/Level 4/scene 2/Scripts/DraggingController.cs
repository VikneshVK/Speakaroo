using UnityEngine;
using UnityEngine.UI;

public class DraggingController : MonoBehaviour
{
    private Vector3 offset;
    private Vector3 startPosition;
    private bool isDragging = false;
    private Vector3 originalScale;
    private SpriteChangeController spriteChangeController;
    private JuiceManager juiceManager;
    public TweeningController tweeningController;
    public JuiceController juiceController;
    private bool helperTimerStarted = false;
    private LVL4Sc2HelperHand helperHandInstance;

    // References for UI images and sprites
    public Image Image1;
    public Image Image2;
    public Sprite Sprite1; // Kiwi
    public Sprite Sprite2; // Strawberry
    public Sprite Sprite3; // Blueberry

    void Start()
    {
        originalScale = transform.localScale;
        spriteChangeController = FindObjectOfType<SpriteChangeController>();
        juiceManager = FindObjectOfType<JuiceManager>();
        helperHandInstance = LVL4Sc2HelperHand.Instance;
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x + offset.x, mousePosition.y + offset.y, transform.position.z);
            transform.localScale = originalScale * 1.1f;

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

        // Destroy helper hand and reset timer for inactivity guidance
        helperHandInstance.DestroySpawnedHelperHand();
        helperHandInstance.ResetAndStartDelayTimer();
    }

    void OnMouseUp()
    {
        isDragging = false;

        Collider2D fruitCollider = GetComponent<Collider2D>();
        if (spriteChangeController.IsOverlappingBlenderJar(fruitCollider) && gameObject.tag != "Blender_Jar")
        {
            spriteChangeController.UpdateBlenderJarSprite(gameObject.tag, gameObject);
            transform.position = startPosition;
            transform.rotation = Quaternion.identity;
            helperHandInstance.DestroySpawnedHelperHand();
        }

        if (juiceManager.requiredFruits.Contains(gameObject.tag))
        {
            helperHandInstance.OnFruitCollected(gameObject.tag); // Notify HelperHand of fruit collection
            UpdateImageSpritesOnDrop(gameObject.tag); // Update the image sprites based on the dragged fruit
        }
    }

    // Function to update image sprites based on the dragged and dropped fruit
    private void UpdateImageSpritesOnDrop(string fruitTag)
    {
        if (!juiceManager.isKikiJuice)
        {
            // If only one fruit is required, set Image1 based on the fruit tag
            if (fruitTag == "Kiwi")
            {
                Image1.sprite = Sprite1;
            }
            else if (fruitTag == "SB")
            {
                Image1.sprite = Sprite2;
            }
            else if (fruitTag == "BB")
            {
                Image1.sprite = Sprite3;
            }
        }
        else
        {
            // For Kiki's juice with two required fruits, set Image1 and Image2 based on the tags and order
            if (juiceManager.requiredFruits.Contains("Kiwi") && juiceManager.requiredFruits.Contains("SB"))
            {
                if (fruitTag == "Kiwi")
                {
                    Image1.sprite = Sprite1;
                }
                else if (fruitTag == "SB")
                {
                    Image2.sprite = Sprite2;
                }
            }
            else if (juiceManager.requiredFruits.Contains("Kiwi") && juiceManager.requiredFruits.Contains("BB"))
            {
                if (fruitTag == "Kiwi")
                {
                    Image1.sprite = Sprite1;
                }
                else if (fruitTag == "BB")
                {
                    Image2.sprite = Sprite3;
                }
            }
            else if (juiceManager.requiredFruits.Contains("SB") && juiceManager.requiredFruits.Contains("BB"))
            {
                if (fruitTag == "SB")
                {
                    Image1.sprite = Sprite2;
                }
                else if (fruitTag == "BB")
                {
                    Image2.sprite = Sprite3;
                }
            }
        }
    }
}
