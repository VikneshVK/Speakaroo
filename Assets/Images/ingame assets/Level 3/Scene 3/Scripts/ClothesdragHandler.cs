using UnityEngine;
using System.Collections;

public class ClothesdragHandler : MonoBehaviour
{
    public bool isDry = false;
    public Sprite sprite1, sprite2, sprite3;
    public SpriteRenderer clothesBasketRenderer;
    public Sprite driedSprite;
    public Animator kikiAnimator;
    public Animator jojoAnimator;
    private static int clothestaken ;
    private bool eveningProcessed = false;
    public Collider2D myCollider;
    public Lvl3Sc3DragManager dragManager;
    public AudioSource fedbackAudio1;
    public AudioSource fedbackAudio2;
    public bool IsDragging { get; private set; }

    private Vector3 resetPosition;

    private void Start()
    {
        myCollider = GetComponent<Collider2D>();
        clothestaken = 0;
    }

    private void Update()
    {
        if (dragManager.isEvening && !eveningProcessed)
        {
            if (name == "wet_socK")
            {
                // Special handling for wet_socK's children (Sock_R and Sock_L)
                Transform sockR = transform.Find("Sock_R");
                Transform sockL = transform.Find("Sock_L");
                if (sockR != null) sockR.GetComponent<SpriteRenderer>().sprite = driedSprite;
                if (sockL != null) sockL.GetComponent<SpriteRenderer>().sprite = driedSprite;
            }
            else
            {
                // Change sprite to driedSprite for other cloth objects
                GetComponent<SpriteRenderer>().sprite = driedSprite;
            }

            // Destroy WaterDroplets if it exists
            Transform waterDroplets = transform.Find("WaterDroplets");
            if (waterDroplets != null)
            {
                Destroy(waterDroplets.gameObject);
            }

            eveningProcessed = true;
        }
    }

    public void EnableCollider()
    {
        if (myCollider != null)
        {
            myCollider.enabled = true;
        }
    }
    private void OnMouseDown()
    {
        resetPosition = transform.position;
    }

    private void OnMouseDrag()
    {
        if (isDry)
        {
            IsDragging = true;            
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x, mousePosition.y, 0);
        }
    }

    private void OnMouseUp()
    {
        IsDragging = false;
        StartCoroutine(HandleDrop());
    }

    private IEnumerator HandleDrop()
    {
        // Disable all colliders attached to game objects with this script
        DisableAllColliders();

        if (isDry && CheckBasketCollision())
        {
            TriggerRightDrop();
            yield return new WaitForSeconds(1f);
            DropInBasket();  
            
        }
        else if (!CheckBasketCollision())
        {
            TriggerWrongDrop(); // Incorrect drop feedback
            yield return BlinkRedAndResetPosition();
            yield return new WaitForSeconds(1f);
            EnableAllColliders();
        }
    }

    private void DisableAllColliders()
    {
        ClothesdragHandler[] handlers = FindObjectsOfType<ClothesdragHandler>();
        foreach (var handler in handlers)
        {
            if (handler.myCollider != null)
            {
                handler.myCollider.enabled = false;
            }
        }
    }

    private void EnableAllColliders()
    {
        ClothesdragHandler[] handlers = FindObjectsOfType<ClothesdragHandler>();
        foreach (var handler in handlers)
        {
            if (handler.myCollider != null && handler.isDry)
            {
                handler.myCollider.enabled = true;
            }
        }
    }

    private bool CheckBasketCollision()
    {
        return clothesBasketRenderer.bounds.Contains(transform.position);
    }

    private void DropInBasket()
    {
        clothestaken++;
        Transform hanger = transform.parent; // Get hanger before destruction
        UpdateBasketSprite();

        dragManager.RemoveClothHandler(this);
        dragManager.EnableAllColliders();
        dragManager.OnClothDropped(hanger);                
        Destroy(gameObject);
    }


    private void UpdateBasketSprite()
    {
        if (clothestaken == 1)
        {
            clothesBasketRenderer.sprite = sprite1;
        }
        else if (clothestaken == 3)
        {
            clothesBasketRenderer.sprite = sprite2;
        }
        else if (clothestaken >= 5)
        {
            clothesBasketRenderer.sprite = sprite3;
        }
    }

    private void TriggerRightDrop()
    {
        if (kikiAnimator != null)
        {
            kikiAnimator.SetTrigger("rightDrop");
        }
        if (jojoAnimator != null)
        {
            jojoAnimator.SetTrigger("rightDrop");
        }
        fedbackAudio1.Play();
    }

    private void TriggerWrongDrop()
    {
        if (kikiAnimator != null)
        {
            kikiAnimator.SetTrigger("wrongDrop");
        }
        if (jojoAnimator != null)
        {
            jojoAnimator.SetTrigger("wrongDrop");
        }
        fedbackAudio2.Play();
    }

    private IEnumerator BlinkRedAndResetPosition()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            
            Color originalColor = spriteRenderer.color;

            for (int i = 0; i < 2; i++) // Blink red twice
            {
                spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(0.2f);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(0.2f);
            }
        }
        else
        {
            Transform sockR = transform.Find("Sock_R");
            Transform sockL = transform.Find("Sock_L");

            if (sockR != null && sockL != null)
            {
                SpriteRenderer sockRSprite = sockR.GetComponent<SpriteRenderer>();
                SpriteRenderer sockLSprite = sockL.GetComponent<SpriteRenderer>();

                if (sockRSprite != null && sockLSprite != null)
                {
                    Color originalColorR = sockRSprite.color;
                    Color originalColorL = sockLSprite.color;

                    for (int i = 0; i < 2; i++) // Blink red twice
                    {
                        sockRSprite.color = Color.red;
                        sockLSprite.color = Color.red;
                        yield return new WaitForSeconds(0.2f);
                        sockRSprite.color = originalColorR;
                        sockLSprite.color = originalColorL;
                        yield return new WaitForSeconds(0.2f);
                    }
                }
            }
        }

        transform.position = resetPosition;
    }

}
