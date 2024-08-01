using UnityEngine;

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

    public Transform cerealInitialPosition; // Set in inspector
    public Transform milkInitialPosition; // Set in inspector
    public Transform cherryInitialPosition; // Set in inspector

    private BoxCollider2D cerealCollider;
    private BoxCollider2D milkCollider;

    public GameObject tweenManager;
    private TweeningController tweeningController;
    private bool startSecondTime = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialPosition = transform.position;
        initialRotation = transform.rotation; // Store the initial rotation
        initialZ = transform.position.z; // Store the initial z-axis value

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

        if (cerealCollider != null) cerealCollider.enabled = true;
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
                    transform.rotation = Quaternion.Euler(0, 0, -100); // Rotate the milk object
                }
                else if (gameObject.CompareTag("Cereal"))
                {
                    spriteRenderer.sprite = newSprite;
                    transform.rotation = Quaternion.Euler(0, 0, 100); // Rotate the cereal object
                }
            }
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        Collider2D bowlCollider = GameObject.FindGameObjectWithTag("EmptyBowl")?.GetComponent<Collider2D>();

        if (bowlCollider != null && bowlCollider.bounds.Contains(transform.position))
        {
            SpriteRenderer bowlSpriteRenderer = bowlCollider.GetComponent<SpriteRenderer>();

            if (gameObject.CompareTag("Milk"))
            {
                isMilkDropped = true;
                bowlSpriteRenderer.sprite = milkSprite;
            }
            else if (gameObject.CompareTag("Cereal"))
            {
                isCerealDropped = true;
                bowlSpriteRenderer.sprite = cerealSprite;
            }

            if (isMilkDropped && isCerealDropped && !gameObject.CompareTag("Cherry"))
            {
                bowlSpriteRenderer.sprite = milkAndCerealSprite;

                if (cherryCollider != null)
                {
                    cherryCollider.enabled = true;
                }
            }

            if (gameObject.CompareTag("Cherry"))
            {
                bowlSpriteRenderer.sprite = finalBowlSprite;
                startSecondTime = true;
            }

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

            StartBirdTweenSequence();
        }
        else
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
        }


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

    private System.Collections.IEnumerator WaitAndTweenBack()
    {
        yield return new WaitForSeconds(2f);

        LeanTween.move(bird, birdInitialPosition, 1f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
        {
            GameObject activeGameObject = null;
            if (!isCerealDropped)
            {
                if (cerealCollider != null)
                {
                    cerealCollider.enabled = true;
                    activeGameObject = cerealCollider.gameObject;
                }
            }
            else if (!isMilkDropped)
            {
                if (milkCollider != null)
                {
                    milkCollider.enabled = true;
                    activeGameObject = milkCollider.gameObject;
                }
            }
            else if (cherryCollider != null)
            {
                cherryCollider.enabled = true;
                activeGameObject = cherryCollider.gameObject;
            }

            if (activeGameObject != null)
            {
                LeanTween.scale(activeGameObject, activeGameObject.transform.localScale * 1.15f, 0.5f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
                {
                    LeanTween.scale(activeGameObject, activeGameObject.transform.localScale / 1.15f, 0.5f).setEase(LeanTweenType.easeInOutQuad);
                });
            }

            if (startSecondTime && tweeningController != null)
            {
                tweeningController.isSecondTime = true;
            }
        });
    }
}