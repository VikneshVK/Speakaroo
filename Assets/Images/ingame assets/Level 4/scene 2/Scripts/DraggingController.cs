using UnityEngine;
using UnityEngine.UI;

public class DraggingController : MonoBehaviour
{
    private Vector3 offset;
    private Vector3 startPosition;
    private bool isDragging;
    private Vector3 originalScale;
    private SpriteChangeController spriteChangeController;
    private JuiceManager juiceManager;
    public TweeningController tweeningController;
    public JuiceController juiceController;
    private bool helperTimerStarted;
    private bool InitialPositionConfirimed;
    public LVL4Sc2HelperHand helperHandInstance;
    private int originalSortingOrder;
    private SpriteRenderer spriteRenderer;

    // References for UI images and sprites
    public Image Image1;
    public Image Image2;
    public Sprite Sprite1; // Kiwi
    public Sprite Sprite2; // Strawberry
    public Sprite Sprite3; // Blueberry

    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;

    void Start()
    {
        originalScale = transform.localScale;
        spriteChangeController = FindObjectOfType<SpriteChangeController>();
        juiceManager = FindObjectOfType<JuiceManager>();
        
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
        helperTimerStarted = false;
        isDragging = false;
        InitialPositionConfirimed = false;
        // Get the SpriteRenderer and store its original sorting order
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }
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

        if (!InitialPositionConfirimed && tweeningController.JuiceTweenCompleted)
        {
            Debug.Log("initial position is set");
            InitialPositionConfirimed = true;
            startPosition = transform.position;
        }
    }

    void OnMouseDown()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - mousePosition;       
        isDragging = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 10;
        }
        // Destroy helper hand and reset timer for inactivity guidance
        helperHandInstance.DestroySpawnedHelperHand();
        helperHandInstance.ResetAndStartDelayTimer();

    }

    void OnMouseUp()
    {
        isDragging = false;
        transform.position = startPosition;
        Collider2D fruitCollider = GetComponent<Collider2D>();
        if (spriteChangeController.IsOverlappingBlenderJar(fruitCollider) && gameObject.tag != "Blender_Jar")
        {
            if (SfxAudioSource != null)
            {
                SfxAudioSource.loop = false;
                SfxAudioSource.PlayOneShot(SfxAudio1);
            }

            spriteChangeController.UpdateBlenderJarSprite(gameObject.tag, gameObject);
            transform.position = startPosition;
            transform.rotation = Quaternion.identity;
            UpdateImageSpritesOnDrop(gameObject.tag); // Update the image sprites based on the dragged fruit
            helperHandInstance.DestroySpawnedHelperHand();
        }

        if (juiceManager.requiredFruits.Contains(gameObject.tag))
        {
            helperHandInstance.OnFruitCollected(gameObject.tag); // Notify HelperHand of fruit collection            
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
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
