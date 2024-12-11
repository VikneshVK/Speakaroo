using UnityEngine;
using System.Collections;

public class Lvl3Sc3ToysdragHandler : MonoBehaviour
{
    public bool isDry = false;
    private Collider2D myCollider;
    public Sprite driedSprite;
    public Transform[] toyBasketPositions;
    public Collider2D toyBasketCollider;
    public Lvl3Sc3DragManager dragManager;
    public Animator kikiAnimator;
    public Animator jojoAnimator;
    public bool IsDragging { get; private set; }

    public AudioSource fedbackAudio1;
    public AudioSource fedbackAudio2;

    public static int toysDropped = 0;
    private bool eveningProcessed = false;
    public bool isdropped;
    private Vector3 resetPosition;

    public GameObject glowPrefab; // Assign your Glow prefab in the inspector
    private GameObject currentGlow;

    private void Start()
    {
        myCollider = GetComponent<Collider2D>();
        Debug.Log($"{name} Collider Enabled: {myCollider.enabled}");
        toyBasketCollider.enabled = false;
        isdropped = false;
    }

    private void Update()
    {
        if (dragManager.isEvening && !eveningProcessed)
        {
            // Change sprite to dried sprite if it's evening
            GetComponent<SpriteRenderer>().sprite = driedSprite;

            // Destroy WaterDroplets if it exists
            Transform waterDroplets = transform.Find("WaterDroplets");
            if (waterDroplets != null)
            {
                Destroy(waterDroplets.gameObject);
            }
            toyBasketCollider.enabled = true;
            eveningProcessed = true;
        }
    }

    public void EnableCollider()
    {
        if (myCollider != null)
        {
            myCollider.enabled = true;
            Debug.Log($"{name} collider enabled.");
        }
    }
    private void OnMouseDown()
    {
        resetPosition = transform.position;
    }

    private void OnMouseDrag()
    {
        IsDragging = true;
        Debug.Log($"{name} is being dragged.");
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePosition.x, mousePosition.y, 0);

        // Spawn glow at the correct destination
        if (currentGlow == null)
        {
            if (isDry)
            {
                // Glow at toy basket
                currentGlow = Instantiate(glowPrefab, toyBasketCollider.transform.position, Quaternion.identity);
            }
            else
            {
                // Glow at the first available hanger
                Transform availableHanger = dragManager.GetAvailableHangers()[0]; // Assuming GetAvailableHangers() returns a list of hangers
                if (availableHanger != null)
                {
                    currentGlow = Instantiate(glowPrefab, availableHanger.position, Quaternion.identity);
                }
            }

            // Tween the glow's scale to 8
            if (currentGlow != null)
            {
                currentGlow.transform.localScale = Vector3.zero;
                LeanTween.scale(currentGlow, Vector3.one * 8, 0.5f).setEaseOutBounce();
            }
        }
    }

    private void OnMouseUp()
    {
        IsDragging = false;
        StartCoroutine(HandleDrop());
        if (currentGlow != null)
        {
            LeanTween.scale(currentGlow, Vector3.zero, 0.5f).setOnComplete(() =>
            {
                Destroy(currentGlow);
            });
        }

    }

    private IEnumerator HandleDrop()
    {
        // Disable all colliders attached to game objects with this script
        DisableAllColliders();

        if (isDry && CheckToyBasketCollision())
        {
            TriggerRightDrop();
            yield return new WaitForSeconds(1f);
            EnableAllColliders();
            DropInToyBasket();             
        }
        else if (!isDry && CheckHangerCollision())
        {
            TriggerRightDrop();
            yield return new WaitForSeconds(1f);
            EnableAllColliders(); // Correct drop feedback
            DropOnHanger();            
        }
        else
        {
            TriggerWrongDrop(); // Incorrect drop feedback
            StartCoroutine(BlinkRedAndResetPosition());
        }
    }


    private bool CheckToyBasketCollision()
    {
        return toyBasketCollider.OverlapPoint(transform.position);
    }

    private bool CheckHangerCollision()
    {
        foreach (var hanger in dragManager.GetAvailableHangers())
        {
            Collider2D hangerCollider = hanger.GetComponent<Collider2D>();
            if (hangerCollider != null && hangerCollider.OverlapPoint(transform.position))
            {
                transform.position = hangerCollider.bounds.center;
                hangerCollider.enabled = false;
                transform.SetParent(hanger);
                myCollider.enabled = false;
                Debug.Log($"Toy '{name}' dropped on hanger '{hanger.name}'.");
                return true;
            }
        }
        Debug.Log($"Toy '{name}' could not find an available hanger.");
        return false;
    }
    private void DisableAllColliders()
    {
        Lvl3Sc3ToysdragHandler[] handlers = FindObjectsOfType<Lvl3Sc3ToysdragHandler>();
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
        Lvl3Sc3ToysdragHandler[] handlers = FindObjectsOfType<Lvl3Sc3ToysdragHandler>();
        foreach (var handler in handlers)
        {
            if (handler.myCollider != null && !handler.isdropped)
            {
                // Enable collider only if the toy has not been dropped
                handler.myCollider.enabled = true;
            }
        }
    }


    private void DropInToyBasket()
    {
        Transform position = toyBasketPositions[toysDropped];
        LeanTween.move(gameObject, position.position, 0.5f);
        toysDropped++;
        myCollider.enabled = false;
        isdropped = true;

        dragManager.ObjectsOnLine--;
        dragManager.CheckLevelComplete();
        dragManager.StartHelperTimerForNextObject();
    }

    private void DropOnHanger()
    {
        myCollider.enabled = false;
        isdropped = true;
        dragManager.OnToyDropped(transform.parent);        
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
        Color originalColor = spriteRenderer.color;

        for (int i = 0; i < 2; i++) // Blink red twice
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.2f);
        }

        // Reset position after blinking
        transform.position = resetPosition;
        EnableAllColliders();
    }
}
