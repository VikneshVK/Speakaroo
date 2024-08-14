using UnityEngine;
using System.Collections;

public class DraggingController1 : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;
    private SpriteRenderer spriteRenderer;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float initialZ;

    private static bool isMilkDropped = false;
    private static bool isCerealDropped = false;

    private Sprite newSprite;
    private Sprite milkSprite;
    private Sprite cerealSprite;
    private Sprite milkAndCerealSprite;
    private Sprite finalBowlSprite;

    private BoxCollider2D cherryCollider;

    private GameObject bird;
    private Animator birdAnimator;
    private Vector3 birdInitialPosition;
    public Transform birdEndPosition;

    public Transform cerealInitialPosition;
    public Transform milkInitialPosition;
    public Transform cherryInitialPosition;

    private BoxCollider2D cerealCollider;
    private BoxCollider2D milkCollider;

    public GameObject tweenManager;
    private TweeningController tweeningController;
    private bool startSecondTime = false;
    private bool finalBowlDisabled = false;

    public GameObject maskPrefab;
    public GameObject finalBowlSpriteObject;
    public GameObject eatenBowl;

    private Animator objectAnimator;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialZ = transform.position.z;

        objectAnimator = GetComponent<Animator>();

        if (gameObject.CompareTag("Milk"))
        {
            newSprite = Resources.Load<Sprite>("Images/Lvl 4 Scene 2/milk-Open");
        }
        else if (gameObject.CompareTag("Cereal"))
        {
            newSprite = Resources.Load<Sprite>("Images/Lvl 4 Scene 2/cereal-Opened");
        }

        milkSprite = Resources.Load<Sprite>("Images/Lvl 4 Scene 2/c-Bowl");
        cerealSprite = Resources.Load<Sprite>("Images/Lvl 4 Scene 2/c-Bowl");
        milkAndCerealSprite = Resources.Load<Sprite>("Images/Lvl 4 Scene 2/mc-Bowl");
        finalBowlSprite = Resources.Load<Sprite>("Images/Lvl 4 Scene 2/mcc-Bowl");

        GameObject cherry = GameObject.FindGameObjectWithTag("Cherry");
        if (cherry != null)
        {
            cherryCollider = cherry.GetComponent<BoxCollider2D>();
            if (cherryCollider != null)
            {
                cherryCollider.enabled = false;
            }
        }

        bird = GameObject.FindGameObjectWithTag("Bird");
        if (bird != null)
        {
            birdAnimator = bird.GetComponent<Animator>();
            birdInitialPosition = bird.transform.position;
        }

        cerealCollider = GameObject.FindGameObjectWithTag("Cereal")?.GetComponent<BoxCollider2D>();
        milkCollider = GameObject.FindGameObjectWithTag("Milk")?.GetComponent<BoxCollider2D>();

        if (cerealCollider != null) cerealCollider.enabled = false;
        if (milkCollider != null) milkCollider.enabled = false;

        if (tweenManager != null)
        {
            tweeningController = tweenManager.GetComponent<TweeningController>();
        }

        StartBirdTweenSequence();
    }

    private void OnMouseDown()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - mousePosition;
        offset.z = 0;
        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
            Vector3 targetPosition = mousePosition + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);

            Collider2D bowlCollider = GameObject.FindGameObjectWithTag("EmptyBowl")?.GetComponent<Collider2D>();
            if (bowlCollider != null && bowlCollider.bounds.Contains(transform.position))
            {
                if (gameObject.CompareTag("Milk"))
                {
                    spriteRenderer.sprite = newSprite;
                    transform.rotation = Quaternion.Euler(0, 0, -100);
                }
                else if (gameObject.CompareTag("Cereal"))
                {
                    spriteRenderer.sprite = newSprite;
                    transform.rotation = Quaternion.Euler(0, 0, 100);
                }
            }
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        Collider2D bowlCollider = GameObject.FindGameObjectWithTag("EmptyBowl")?.GetComponent<Collider2D>();
        transform.rotation = Quaternion.identity;
        if (bowlCollider != null && bowlCollider.bounds.Contains(transform.position))
        {
            SpriteRenderer bowlSpriteRenderer = bowlCollider.GetComponent<SpriteRenderer>();

            // Trigger "isPouring" animation
            if (objectAnimator != null)
            {
                objectAnimator.SetTrigger("isPouring");
            }

            // Reset position and change sprite after animation ends through events
        }
        else
        {
            ResetPositionAndRotation();
        }
    }

    private void ResetPositionAndRotation()
    {
        if (gameObject.CompareTag("Milk") && milkInitialPosition != null)
        {
            transform.position = milkInitialPosition.position;
        }
        else if (gameObject.CompareTag("Cereal") && cerealInitialPosition != null)
        {
            transform.position = cerealInitialPosition.position;
        }
        else if (gameObject.CompareTag("Cherry") && cherryInitialPosition != null)
        {
            transform.position = cherryInitialPosition.position;
        }
        transform.rotation = initialRotation;
        Debug.Log("Position Reset");
    }

    private void ResetPositionAndChangeSprite(string tag)
    {
        SpriteRenderer bowlSpriteRenderer = GameObject.FindGameObjectWithTag("EmptyBowl")?.GetComponent<SpriteRenderer>();

        if (tag == "Cereal")
        {
            isCerealDropped = true;
            if (bowlSpriteRenderer != null)
                bowlSpriteRenderer.sprite = cerealSprite;

            transform.position = cerealInitialPosition.position;
            Debug.Log("Cereal Position Reset and Sprite Changed");
        }
        else if (tag == "Milk")
        {
            isMilkDropped = true;
            if (bowlSpriteRenderer != null)
                bowlSpriteRenderer.sprite = milkAndCerealSprite;

            transform.position = milkInitialPosition.position;
            Debug.Log("Milk Position Reset and Sprite Changed");
        }
        else if (tag == "Cherry")
        {
            if (isMilkDropped && isCerealDropped)
            {
                if (bowlSpriteRenderer != null)
                    bowlSpriteRenderer.sprite = finalBowlSprite;
                StartCoroutine(ExecuteBirdTweenAndReveal());
            }

            transform.position = cherryInitialPosition.position;
            Debug.Log("Cherry Position Reset and Sprite Changed");
        }

        transform.rotation = initialRotation;

        // Enable the collider for the next item
        if (tag == "Cereal" && milkCollider != null)
            milkCollider.enabled = true;
        else if (tag == "Milk" && cherryCollider != null)
            cherryCollider.enabled = true;
    }

    private void CerealAnimationComplete()
    {
        ResetPositionAndChangeSprite("Cereal");
        StartBirdTweenSequence();
    }

    private void MilkAnimationComplete()
    {
        ResetPositionAndChangeSprite("Milk");
        StartBirdTweenSequence();
    }

    private void CherryAnimationComplete()
    {
        ResetPositionAndChangeSprite("Cherry");
        StartBirdTweenSequence();
    }

    private IEnumerator ExecuteBirdTweenAndReveal()
    {
        yield return StartCoroutine(WaitAndTweenBack());
        yield return StartCoroutine(SpawnMasksAndReveal());
    }

    private IEnumerator SpawnMasksAndReveal()
    {
        float elapsedTime = 0f;
        float maskInterval = 1f;

        while (elapsedTime < 3f)
        {
            InstantiateMaskWithinBounds();
            elapsedTime += maskInterval;
            yield return new WaitForSeconds(maskInterval);
        }

        if (finalBowlSpriteObject != null)
        {
            finalBowlSpriteObject.GetComponent<SpriteRenderer>().enabled = false;
            finalBowlDisabled = true;
        }
        startSecondTime = true;
        StartBirdTweenSequence();
    }

    private void InstantiateMaskWithinBounds()
    {
        if (finalBowlSpriteObject == null)
            return;

        Collider2D bowlCollider = finalBowlSpriteObject.GetComponent<Collider2D>();
        if (bowlCollider == null)
            return;

        Bounds bounds = bowlCollider.bounds;
        Vector2 randomPosition;
        bool positionValid;

        do
        {
            randomPosition = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );

            positionValid = IsPositionWithinBounds(randomPosition, bowlCollider) && !IsPositionInsideMaskCollider(randomPosition);
        } while (!positionValid);

        Instantiate(maskPrefab, randomPosition, Quaternion.identity);
    }

    private bool IsPositionWithinBounds(Vector2 position, Collider2D collider)
    {
        return collider.bounds.Contains(position);
    }

    private bool IsPositionInsideMaskCollider(Vector2 position)
    {
        CircleCollider2D maskCollider = maskPrefab.GetComponent<CircleCollider2D>();
        if (maskCollider == null) return false;

        Vector2 maskCenter = maskCollider.transform.position;
        float radius = maskCollider.radius * maskCollider.transform.lossyScale.x;

        return Vector2.Distance(position, maskCenter) <= radius;
    }

    private void StartBirdTweenSequence()
    {
        if (bird != null && birdEndPosition != null)
        {
            LeanTween.move(bird, birdEndPosition.position, 1f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
            {
                birdAnimator.SetTrigger("canTalk");
                StartCoroutine(WaitAndTweenBack());
            });
        }
    }

    private IEnumerator WaitAndTweenBack()
    {
        yield return new WaitForSeconds(2f);

        LeanTween.move(bird, birdInitialPosition, 1f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
        {
            if (!isCerealDropped && cerealCollider != null)
            {
                cerealCollider.enabled = true;
            }
            else if (!isMilkDropped && milkCollider != null)
            {
                milkCollider.enabled = true;
            }
            else if (cherryCollider != null)
            {
                cherryCollider.enabled = true;
            }

            if (startSecondTime && tweeningController != null)
            {
                tweeningController.isSecondTime = true;
            }
        });

        yield return new WaitForSeconds(1f);
    }
}
