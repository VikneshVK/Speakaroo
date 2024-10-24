using UnityEngine;
using UnityEngine.UI;

public class PizzaDrag : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 startPosition;
    private Collider2D pizzacollider;
    public GameObject pizzaDropLocation;  // The location where pizza should be dropped
    public Transform cameraFinalPoint;    // The point where the camera should pan
    public Transform pizzaPoint1;         // The first position the pizza should tween to
    public Transform pizzaPoint2;         // The second position the pizza should tween to
    public Camera mainCamera;             // Reference to the main camera
    public GameObject pizzaBox;           // The pizza box that needs to be tapped
    public GameObject pizzaEatingPanel;   // Reference to the pizza eating panel

    public Lvl7Sc2QuestManager questManager;  // Reference to the quest manager

    public Sprite sprite1;  // Sprite for pizzaMade == 0
    public Sprite sprite2;  // Sprite for pizzaMade == 1
    public Sprite sprite3;  // Sprite for pizzaMade == 2
    public Sprite defaultSprite;  // Default sprite for resetting
    public GameObject heat;

    public bool pizzaDropped = false;
    private bool canTapPizzaBox = false;
    public bool canTapPizzaImage = false;
    private Collider2D pizzaBoxCollider;
    private Image pizzaImage;  // Reference to the Image component for the pizza
    private int imagesTappedCount = 0;
    private bool[] tappedImages = new bool[4];
    private int requiredTaps;  // Number of required taps (random between 3 and 6)
    private SpriteRenderer pizzasprite;
    // Counter to track how many toppings have been dropped
    private int toppingsDropped = 0;


    [Header("Pizza Piece")]
    public Sprite Piece1;
    public Sprite Piece2;
    public Sprite Piece3;
    public Sprite Piece4;
    public Sprite Piece5;
    public Sprite Piece6;
    public Sprite Piece7;
    public Sprite Piece8;
    public Sprite Piece9;
    public Sprite Piece10;
    public Sprite Piece11;
    public Sprite Piece12;

    void Start()
    {
        pizzacollider = GetComponent<Collider2D>();
        pizzaBoxCollider = pizzaBox.GetComponent<Collider2D>();
        startPosition = transform.position;  // Store the initial position of the pizza
        pizzasprite = GetComponent<SpriteRenderer>();

        // Disable pizza box collider initially
        if (pizzaBoxCollider != null)
        {
            pizzaBoxCollider.enabled = false;
        }
        // Initialize the tapped images array
        for (int i = 0; i < tappedImages.Length; i++)
        {
            tappedImages[i] = false;
        }


    }

    void Update()
    {
        // Handle dragging
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;  // Set z to 0 to ensure it stays on the same plane
            transform.position = mousePosition;
        }

        // Check if player taps the pizza box after the pizza reaches its final point
        if (canTapPizzaBox && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == pizzaBox)
            {
                TapPizzaBox();
            }
        }
    }

    void OnMouseDown()
    {
        if (!pizzaDropped)
        {
            isDragging = true;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
        if (IsDroppedOnTarget())
        {
            pizzaDropped = true;
            pizzacollider.enabled = false;
            PanCameraAndMovePizza();
        }
        else
        {
            transform.position = startPosition;
        }
    }

    private bool IsDroppedOnTarget()
    {
        Collider2D dropLocationCollider = pizzaDropLocation.GetComponent<Collider2D>();
        return pizzacollider.bounds.Intersects(dropLocationCollider.bounds);  // Check if bounds intersect
    }

    private void PanCameraAndMovePizza()
    {
        LeanTween.moveX(mainCamera.gameObject, cameraFinalPoint.position.x, 1.0f).setOnComplete(() =>
        {
            TweenPizzaToPoints();
        });
    }

    private void TweenPizzaToPoints()
    {
        LeanTween.move(gameObject, pizzaPoint1.position, 0.7f).setOnComplete(() =>
        {
            if (pizzaBoxCollider != null)
            {
                pizzaBoxCollider.enabled = true;
            }
            LeanTween.delayedCall(0.3f, () =>
            {
                heat.GetComponent<SpriteRenderer>().enabled = true;

                LeanTween.delayedCall(1.0f, () =>
                {
                    heat.GetComponent<SpriteRenderer>().enabled = false;

                    UpdatePizzaSprite();

                    LeanTween.delayedCall(0.3f, () =>
                    {
                        LeanTween.move(gameObject, pizzaPoint2.position, 0.7f).setOnComplete(() =>
                        {
                            canTapPizzaBox = true;
                        });
                    });

                });
            });
        });
    }

    // Method to track toppings dropped and enable the pizza collider
    public void EnablePizzaCollider()
    {
        pizzacollider.enabled = true;
    }


    private void TapPizzaBox()
    {
        canTapPizzaBox = false;

        UpdatePizzaImage();

        LeanTween.scale(pizzaEatingPanel, Vector3.one, 0.4f).setEase(LeanTweenType.easeOutBounce).setOnComplete(() =>
        {
            foreach (Transform child in pizzaEatingPanel.transform)
            {
                LeanTween.scale(child.gameObject, Vector3.one, 0.4f).setEase(LeanTweenType.easeInOutQuad);
            }
            canTapPizzaImage = true;
        });
    }

    public void CompletePizzaSequence()
    {
        mainCamera.transform.position = new Vector3(0, 0, mainCamera.transform.position.z);
        transform.position = startPosition;
        GetComponent<SpriteRenderer>().sprite = defaultSprite;
        LeanTween.scale(pizzaEatingPanel, Vector3.zero, 0.4f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
        {
            questManager.MakePizza();
            questManager.UpdateQuestDisplay();
            imagesTappedCount = 0;

            Image[] pizzaImages = new Image[4];
            pizzaImages[0] = pizzaEatingPanel.transform.Find("Image 1").GetComponent<Image>();
            pizzaImages[1] = pizzaEatingPanel.transform.Find("Image 2").GetComponent<Image>();
            pizzaImages[2] = pizzaEatingPanel.transform.Find("Image 3").GetComponent<Image>();
            pizzaImages[3] = pizzaEatingPanel.transform.Find("Image 4").GetComponent<Image>();

            for (int i = 0; i < pizzaImages.Length; i++)
            {
                tappedImages[i] = false;
            }
            
            Lvl7Sc2DragManager dragManager = FindObjectOfType<Lvl7Sc2DragManager>();
            if (dragManager != null)
            {
                dragManager.UpdateColliders();
            }
            else
            {
                Debug.LogError("DragManager not found!");
            }
        });
    }


    private void UpdatePizzaImage()
    {
        if (questManager == null)
        {
            Debug.LogError("Quest Manager is not assigned.");
            return;
        }

        if (pizzaEatingPanel == null)
        {
            Debug.LogError("Pizza Eating Panel is not assigned.");
            return;
        }

        // Find all the Image components under the pizzaEatingPanel
        Image[] pizzaImages = new Image[4];
        pizzaImages[0] = pizzaEatingPanel.transform.Find("Image 1")?.GetComponent<Image>();
        pizzaImages[1] = pizzaEatingPanel.transform.Find("Image 2")?.GetComponent<Image>();
        pizzaImages[2] = pizzaEatingPanel.transform.Find("Image 3")?.GetComponent<Image>();
        pizzaImages[3] = pizzaEatingPanel.transform.Find("Image 4")?.GetComponent<Image>();

        // Check if any of the images were not found or don't have Image components
        for (int i = 0; i < pizzaImages.Length; i++)
        {
            if (pizzaImages[i] == null)
            {
                Debug.LogError($"Pizza Image {i + 1} is missing or doesn't have an Image component.");
                return;
            }
        }

        // Update the sprites based on the PizzaMade value
        switch (questManager.PizzaMade)
        {
            case 0:
                pizzaImages[0].sprite = Piece1;
                pizzaImages[1].sprite = Piece2;
                pizzaImages[2].sprite = Piece3;
                pizzaImages[3].sprite = Piece4;
                break;
            case 1:
                pizzaImages[0].sprite = Piece5;
                pizzaImages[1].sprite = Piece6;
                pizzaImages[2].sprite = Piece7;
                pizzaImages[3].sprite = Piece8;
                break;
            case 2:
                pizzaImages[0].sprite = Piece9;
                pizzaImages[1].sprite = Piece10;
                pizzaImages[2].sprite = Piece11;
                pizzaImages[3].sprite = Piece12;
                break;
            default:
                Debug.LogWarning("Invalid PizzaMade value in Quest Manager.");
                break;
        }
    }


    private void UpdatePizzaSprite()
    {
        if (questManager != null)
        {
            switch (questManager.PizzaMade)
            {
                case 0:
                    pizzasprite.sprite = sprite1;  // Set the pizza image to sprite1
                    break;
                case 1:
                    pizzasprite.sprite = sprite2;  // Set the pizza image to sprite2
                    break;
                case 2:
                    pizzasprite.sprite = sprite3;  // Set the pizza image to sprite3
                    break;
                default:
                    Debug.LogWarning("Invalid PizzaMade value in Quest Manager.");
                    break;
            }
        }
        else
        {
            Debug.LogWarning("Quest Manager is not assigned.");
        }
    }

    public void OnPizzaImageTapped(GameObject tappedImage)
    {
        // Find which image was tapped by comparing the tappedImage with the pizzaEatingPanel children
        Image[] pizzaImages = new Image[4];
        pizzaImages[0] = pizzaEatingPanel.transform.Find("Image 1").GetComponent<Image>();
        pizzaImages[1] = pizzaEatingPanel.transform.Find("Image 2").GetComponent<Image>();
        pizzaImages[2] = pizzaEatingPanel.transform.Find("Image 3").GetComponent<Image>();
        pizzaImages[3] = pizzaEatingPanel.transform.Find("Image 4").GetComponent<Image>();

        // Iterate through the images and check which one was tapped
        for (int i = 0; i < pizzaImages.Length; i++)
        {
            if (tappedImage == pizzaImages[i].gameObject && !tappedImages[i])
            {
                // Scale down the tapped image
                LeanTween.scale(pizzaImages[i].gameObject, Vector3.zero, 0.3f).setEase(LeanTweenType.easeInOutQuad);

                // Mark this image as tapped
                tappedImages[i] = true;
                imagesTappedCount++;

                // If all images are tapped, complete the pizza sequence
                if (imagesTappedCount >= 4)
                {
                    CompletePizzaSequence();
                }

                break;
            }
        }
    }
}
