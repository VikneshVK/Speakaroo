using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.LookDev;
using Unity.VisualScripting;

public class Lvl3Sc3ToysdragHandler : MonoBehaviour
{
    public bool isDry = false;
    public Collider2D myCollider;
    public Sprite driedSprite;
    public Transform[] toyBasketPositions;
    public Collider2D toyBasketCollider;
    public Lvl3Sc3DragManager dragManager;
    public Animator kikiAnimator;
    public Animator jojoAnimator;
    public bool IsDragging { get; private set; }

    public AudioSource fedbackAudio1;
    public AudioSource fedbackAudio2;
    public GameObject sun;
    public GameObject sky;
    public Lvl3sc3HelperHand helperHand;

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
        helperHand.ResetHelperHand();
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
        yield return new WaitForSeconds(2f);
        // Spawn the glow effect at the reset position
        if (glowPrefab != null)
        {
            // Trigger "helper1" on Kiki's animator
            if (kikiAnimator != null)
            {
                kikiAnimator.SetTrigger("helper2");
            }

            if (!isDry)
            {
                AudioSource skyAudio = sky.GetComponent<AudioSource>();
                if (skyAudio != null)
                {
                    skyAudio.Play();
                }
            }
            else 
            {
                AudioSource sunAudio = sun.GetComponent<AudioSource>();
                if (sunAudio != null)
                {
                    sunAudio.Play();
                }
            }
           
            GameObject glow = Instantiate(glowPrefab, resetPosition, Quaternion.identity);
            glow.transform.localScale = Vector3.zero;

            // Tween the glow's scale to 8
            LeanTween.scale(glow, Vector3.one * 8, 0.5f).setEaseOutQuad();

            // Wait for 2 seconds
            yield return new WaitForSeconds(2f);

            // Fade out the glow
            SpriteRenderer glowRenderer = glow.GetComponent<SpriteRenderer>();
            if (glowRenderer != null)
            {
                Color originalGlowColor = glowRenderer.color;
                float fadeDuration = 0.5f;
                float elapsedTime = 0f;

                while (elapsedTime < fadeDuration)
                {
                    elapsedTime += Time.deltaTime;
                    glowRenderer.color = new Color(originalGlowColor.r, originalGlowColor.g, originalGlowColor.b, 1 - (elapsedTime / fadeDuration));
                    yield return null;
                }
            }

            // Destroy the glow object
            Destroy(glow);
        }

        EnableAllColliders();
    }

}
